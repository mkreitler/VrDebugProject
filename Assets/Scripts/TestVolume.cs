using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

namespace PalletBuilder {
    public class TestVolume : MonoBehaviour {
        private int Overlaps {
            get;
            set;
        }

        private int OldOverlaps {
            get;
            set;
        }

        private Material material;

        private Renderer localRenderer = null;

        private List<Collider> overlaps = new List<Collider>();

        private bool initialized = false;

        // Start is called before the first frame update
        void Start() {
            if (!initialized) {
                Init();
            }
        }

        void OnEnable() {
            if (!initialized) {
                Init();
            }

            OldOverlaps = 0;
            Overlaps = 0;
        }

        // Update is called once per frame
        void Update() {
            if (OVRInput.Get(OVRInput.Button.One) ||
                OVRInput.Get(OVRInput.Button.Three) ||
                OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ||
                OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) {
                Color color = material.color;
                if (Overlaps > 0) {
                    color.r = 1.0f;
                    color.g = 0.0f;
                    color.b = 0.0f;
                }
                else {
                    color.r = 0.0f;
                    color.g = 1.0f;
                    color.b = 0.0f;
                }

                material.color = color;
                localRenderer.enabled = true;
            }
            else {
                localRenderer.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!overlaps.Contains(other)) {
                Overlaps += 1;
                overlaps.Add(other);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (overlaps.Contains(other)) {
                overlaps.Remove(other);
                Overlaps -= 1;
            }
        }

        private void Init() {
            if (localRenderer == null) {
                localRenderer = gameObject.GetComponent<Renderer>();
            }

            Assert.Test(localRenderer != null, "No tester localRenderer defined!");

            material = localRenderer.materials[0];
            localRenderer.enabled = false;

            Assert.Test(material != null, "No tester material defined!");

            initialized = true;
        }
    }
}
