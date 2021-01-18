using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;
using TMPro;

public class ExtraInputHandler : MonoBehaviour
{
    [SerializeField]
    private OVRManager ovrManager = null;

    [SerializeField]
    private TextMeshProUGUI textMesh = null;

    [SerializeField]
    private OVRPlayerController playerController = null;

    [SerializeField]
    private Rigidbody playerRigidbody = null;

    [SerializeField]
    private GameObject goPlayer = null;

    [SerializeField]
    private float ladderHeight = 1.5f;

    private bool isUp = false;

    // Start is called before the first frame update
    void Start()
    {
        Assert.Test(ovrManager != null, "No OVR Manager defined!");
        Assert.Test(textMesh != null, "No text mesh defined!");
        Assert.Test(playerController != null, "No player controller defined!");
        Assert.Test(goPlayer != null, "No player game object defined!");
        Assert.Test(ladderHeight > 0.0f, "Ladder height must be greater than 0!")
    }

    private void FixedUpdate() {
        OVRInput.FixedUpdate();

        float rightStickY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).y;
        if (rightStickY > 0.5f && !isUp) {
            textMesh.text = "Up!";
            isUp = true;
            playerController.EnableLinearMovement = false;
            playerController.GravityModifier = 0;

            Vector3 vNewPos = Vector3.zero;
            vNewPos.Set(goPlayer.transform.position.x, goPlayer.transform.position.y + ladderHeight, goPlayer.transform.position.z);
            goPlayer.transform.position = vNewPos;
        }
        else if (rightStickY < -0.5f && isUp) {
            textMesh.text = "Down!";
            isUp = false;
            playerController.EnableLinearMovement = true;
            playerController.GravityModifier = 1;

            Vector3 vNewPos = Vector3.zero;
            vNewPos.Set(goPlayer.transform.position.x, goPlayer.transform.position.y -ladderHeight, goPlayer.transform.position.z);
            goPlayer.transform.position = vNewPos;
        }
    }
}
