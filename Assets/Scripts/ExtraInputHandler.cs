using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;
using TMPro;

namespace PalletBuilder {
    public class ExtraInputHandler : MonoBehaviour {
        [SerializeField]
        private OVRManager ovrManager = null;

        [SerializeField]
        private TextMeshProUGUI textMesh = null;

        [SerializeField]
        private OVRPlayerController playerController = null;

        [SerializeField]
        private GameObject goPlayer = null;

        [SerializeField]
        private float ladderHeight = 1.5f;

        [SerializeField]
        private ScenarioManager scenarioManager = null;

        private bool primaryTriggerWasDown = false;

        private bool secondaryTriggerWasDown = false;

        private bool isUp = false;

        // Start is called before the first frame update
        void Start() {
            Assert.Test(ovrManager != null, "No OVR Manager defined!");
            Assert.Test(textMesh != null, "No text mesh defined!");
            Assert.Test(playerController != null, "No player controller defined!");
            Assert.Test(goPlayer != null, "No player game object defined!");
            Assert.Test(ladderHeight > 0.0f, "Ladder height must be greater than 0!");
            Assert.Test(scenarioManager != null, "No scene manager found!");
        }

        private void FixedUpdate() {
            OVRInput.FixedUpdate();

            float rightStickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;
            if (rightStickY > 0.5f && !isUp) {
                isUp = true;
                playerController.EnableLinearMovement = false;
                playerController.GravityModifier = 0;

                Vector3 vNewPos = Vector3.zero;
                vNewPos.Set(goPlayer.transform.position.x, goPlayer.transform.position.y + ladderHeight, goPlayer.transform.position.z);
                goPlayer.transform.position = vNewPos;
            }
            else if (rightStickY < -0.5f && isUp) {
                isUp = false;
                playerController.EnableLinearMovement = true;
                playerController.GravityModifier = 1;

                Vector3 vNewPos = Vector3.zero;
                vNewPos.Set(goPlayer.transform.position.x, goPlayer.transform.position.y - ladderHeight, goPlayer.transform.position.z);
                goPlayer.transform.position = vNewPos;
            }

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)) {
                if (!primaryTriggerWasDown) {
                    // Trigger pressed.
                    scenarioManager.NextScenario();
                }

                primaryTriggerWasDown = true;
            }
            else {
                primaryTriggerWasDown = false;
            }

            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) {
                if (!secondaryTriggerWasDown) {
                    // Trigger pressed.
                    scenarioManager.PrevScenario();
                }

                secondaryTriggerWasDown = true;
            }
            else {
                secondaryTriggerWasDown = false;
            }
        }
    }
}
