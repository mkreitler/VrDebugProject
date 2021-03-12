using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

namespace PalletBuilder {
    public class ScenarioManager : MonoBehaviour {
        private Cargo[] cargoList = null;

        private ScenarioData activeTestVolume = null;
        private bool didTest = true;

        [SerializeField]
        private Cargo[] testCargo;

        private class SupportTest {
            public Cargo cargo = null;
            public float weight = 0.0f;
            public Bounds bounds;
            public float overlapArea = 0.0f;

            public SupportTest(Cargo cargo, float weight, Bounds bound) {
                this.cargo = cargo;
                this.weight = weight;
                this.bounds = bound;
            }
        }

        // Start is called before the first frame update
        void Start() {
            cargoList = GetComponentsInChildren<Cargo>();

            ScenarioData[] testVolumes = GetComponentsInChildren<ScenarioData>();
            Assert.Test(testVolumes.Length > 0, "No test volumes found!");

            foreach (ScenarioData testVolume in testVolumes) {
                if (testVolume.isActiveAndEnabled) {
                    activeTestVolume = testVolume;
                    break;
                }
            }

            Assert.Test(activeTestVolume != null, "No active test volume found!");

            showInstructions();

            testCrushErrors();
        }

        private void Update() {
            if (OVRInput.Get(OVRInput.Button.One) ||
                OVRInput.Get(OVRInput.Button.Three)) {

                if (!didTest) {
                    didTest = true;
                    int thisSideUpErrors = testThisSideUpErrors();
                    int crushErrors = testCrushErrors();

                    Switchboard.Broadcast("SetText", "Stacking Errors: " + (thisSideUpErrors + crushErrors));
                }
            }
            else {
                if (didTest) {
                    showInstructions();
                }

                didTest = false;
            }
        }

        private int testThisSideUpErrors() {
            int errors = 0;
            ArrayList containedCargo = new ArrayList();

            activeTestVolume.GetContainedCargo(cargoList, containedCargo);

            foreach (Cargo cargo in containedCargo) {
                errors += cargo.isRightSideUp() ? 0 : 1;
            }

            return errors;
        }

        private int testCrushErrors() {
            int errors = 0;
            ArrayList containedCargo = new ArrayList();
            ArrayList testCargo = new ArrayList();

            activeTestVolume.GetContainedCargo(cargoList, containedCargo);

            // DEBUG FUNCTION: REMOVE!!!
            foreach(Cargo cargo in testCargo) {
                containedCargo.Add(cargo);
            }

            // Generate a list of axis-aligned axis-aligned bounding boxes, sorted by the
            // height of the top surface.

            // Step 1: build an assoc list of cargo and their bounds.
            foreach (Cargo cargo in containedCargo) {
                bool alreadyInList = false;

                foreach(SupportTest support in testCargo) {
                    if (cargo == support.cargo) {
                        alreadyInList = true;
                        break;
                    }
                }

                if (!alreadyInList) {
                    GameObject go = cargo.gameObject;
                    Renderer r = go.GetComponent<Renderer>();
                    Bounds b = r.bounds;
                    SupportTest testData = new SupportTest(cargo, cargo.Weight, b);
                    testCargo.Add(testData);
                    testData.cargo.ResetSupport();
                }

                // Pull in cargo that supports this cargo.
                foreach(Cargo supporter in cargo.SupportedBy) {
                    alreadyInList = false;

                    foreach (Cargo contained in containedCargo) {
                        if (contained == supporter) {
                            alreadyInList = true;
                            break;
                        }
                    }

                    // This modifies the list through which we are
                    // iterating, but it should be OK in this case.
                    if (!alreadyInList) {
                        containedCargo.Add(supporter);
                    }
                }
            }

            // Step 2: sort test cargo according to height of the top surface.
            for (int iOuter=0; iOuter<testCargo.Count - 1; ++iOuter) {
                SupportTest testData = testCargo[iOuter] as SupportTest;
                Bounds bOuter = testData.bounds;
                float top = bOuter.max.y;
                int iTop = iOuter;

                for (int iInner=iOuter+1; iInner< testCargo.Count; ++iInner) {
                    SupportTest otherTestData = testCargo[iInner] as SupportTest;
                    Bounds bInner = otherTestData.bounds;
                    float testTop = bInner.max.y;

                    if (testTop > top) {
                        top = testTop;
                        iTop = iInner;
                    }
                }

                if (iTop != iOuter) {
                    object temp = testCargo[iTop];
                    testCargo[iTop] = testCargo[iOuter];
                    testCargo[iOuter] = temp;
                }
            }

            // Step 3: iterate through test cargo from highest to lowest,
            // distributing weight onto lower supports.
            ArrayList supports = new ArrayList();
            foreach(SupportTest top in testCargo) {
                supports.Clear();
                Bounds bTop = top.bounds;
                Rect topRect = new Rect(bTop.min.x, bTop.min.z, bTop.size.x, bTop.size.z);

                foreach (Cargo support in top.cargo.SupportedBy) {
                    foreach(SupportTest testData in testCargo) {
                        if (testData.cargo == support) {
                            supports.Add(testData);
                            break;
                        }
                    }
                }

                float totalArea = 0.0f;
                foreach(SupportTest support in supports) {
                    Bounds bSupport = support.bounds;
                    Rect supportRect = new Rect(bSupport.min.x, bSupport.min.z, bSupport.size.x, bSupport.size.z);

                    Assertions.Assert.Test(topRect.Overlaps(supportRect), "Supprting rect doesn't overlap bounds!");

                    float minInterX = Mathf.Max(topRect.min.x, supportRect.min.x);
                    float maxInterX = Mathf.Min(topRect.max.x, supportRect.max.x);
                    float minInterY = Mathf.Max(topRect.min.y, supportRect.min.y);
                    float maxInterY = Mathf.Min(topRect.max.y, supportRect.max.y);
                    float sizeX = Mathf.Abs(maxInterX - minInterX);
                    float sizeY = Mathf.Abs(maxInterY - minInterY);

                    float interArea = sizeX * sizeY;
                    totalArea += interArea;
                    support.overlapArea = sizeX * sizeY;
                }

                foreach(SupportTest support in supports) {
                    support.cargo.SupportedWeight = top.cargo.Weight * support.overlapArea / totalArea;
                }
            }

            // Step 4: check for crushing.
            foreach (SupportTest crushCargo in testCargo) {
                if (crushCargo.cargo.SupportedWeight > crushCargo.cargo.CrushWeight) {
                    crushCargo.cargo.Crush();
                    errors += 1;
                }
            }

            return errors;
        }

        private void showInstructions() {
            Switchboard.Broadcast("SetText", "Move: left stick\nTurn/Ladder: right stick\nGrab: hand trigger\nTest: 'A' button\nSwitch: index trigger");
        }

        public void NextScenario() {
            activeTestVolume = activeTestVolume.EnableNext();
            ResetCargo();
        }

        public void PrevScenario() {
            activeTestVolume = activeTestVolume.EnablePrev();
            ResetCargo();
        }

        private void ResetCargo() {
            foreach (Cargo cargo in cargoList) {
                cargo.Reset();
            }
        }
    }
}
