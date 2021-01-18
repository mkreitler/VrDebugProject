using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

namespace PalletBuilder {
    public class PlayerLadder : MonoBehaviour {
        [SerializeField]
        private CharacterController charController = null;

        // Start is called before the first frame update
        void Start() {
            Assert.Test(charController != null, "No character controller found!");
        }

        private void FixedUpdate() {
            if (charController != null) {
                if (OVRInput.Get(OVRInput.Button.Three) || OVRInput.Get(OVRInput.Button.Four)) {
                    charController.height = 3.0f;
                }
                else {
                    charController.height = 1.0f;
                }
            }
        }
    }
}
