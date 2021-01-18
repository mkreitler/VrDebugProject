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

        [SerializeField]
        private Renderer renderer;

        private List<Collider> overlaps = new List<Collider>();

        // Start is called before the first frame update
        void Start() {
            OldOverlaps = 0;
            Overlaps = 0;

            if (renderer == null) {
                renderer = gameObject.GetComponent<Renderer>();
            }

            Assert.Test(renderer != null, "No tester renderer defined!");

            material = renderer.materials[0];
            renderer.enabled = false;

            Assert.Test(material != null, "No tester material defined!");
        }

        // Update is called once per frame
        void Update() {
            if (OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three)) {
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
                renderer.enabled = true;
                Switchboard.Broadcast("SetText", "Overlaps: " + Overlaps);
            }
            else {
                renderer.enabled = false;
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
    }
}
