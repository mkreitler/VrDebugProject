using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

namespace PalletBuilder {
    public class ScenarioData : MonoBehaviour {
        [SerializeField]
        private Pallet _pallet = null;

        [SerializeField]
        private ScenarioData next = null;

        [SerializeField]
        private ScenarioData prev = null;

        [SerializeField]
        private GameObject[] testVolumes = null;

        public Pallet pallet {
            get {
                return _pallet;
            }
        }

        // Start is called before the first frame update
        void Start() {
            Assert.Test(testVolumes != null, "No test volume array defined!");
            Assert.Test(testVolumes.Length > 0, "No test volumes found!");
            Assert.Test(pallet != null, "No pallet floor defined!");
            Assert.Test(next != null, "No 'next' found!");
            Assert.Test(prev != null, "No 'prev' found!");
        }

        public ScenarioData EnableNext() {
            next.gameObject.SetActive(true);
            gameObject.SetActive(false);

            Disable();
            next.Enable();

            return next;
        }

        public ScenarioData EnablePrev() {
            prev.gameObject.SetActive(true);
            gameObject.SetActive(false);

            Disable();
            prev.Enable();

            return prev;
        }

        public void GetContainedCargo(Cargo[] cargoIn, ArrayList cargoOut) {
            Assert.Test(cargoIn != null, "Invalid input cargo list!");
            Assert.Test(cargoOut != null, "Invalid output cargo list!");

            BoxCollider container = pallet.testTrigger;

            Assert.Test(container != null, "Invalid container!");

            cargoOut.Clear();

            int ignoreMask = -(1 << LayerMask.NameToLayer("ContainmentTest"));

            foreach (Cargo cargo in cargoIn) {
                BoxCollider boxCol = cargo.gameObject.GetComponent<BoxCollider>();

                if (boxCol != null) {
                    if (Physics.CheckBox(boxCol.transform.position, boxCol.bounds.extents, boxCol.transform.rotation, ignoreMask, QueryTriggerInteraction.Collide)) {
                        cargoOut.Add(cargo);
                    }
                }
            }
        }

        public void Enable() {
            pallet.gameObject.SetActive(true);

            foreach (GameObject testVolume in testVolumes) {
                testVolume.SetActive(true);
            }
        }

        public void Disable() {
            pallet.gameObject.SetActive(false);

            foreach (GameObject testVolume in testVolumes) {
                testVolume.SetActive(false);
            }
        }
    }
}
