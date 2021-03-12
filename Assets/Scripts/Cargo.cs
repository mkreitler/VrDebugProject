using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{
    private static float THIS_SIDE_UP_TOLERANCE = 0.5f;

    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation = Quaternion.identity;
    private Vector3 startScale = Vector3.zero;

    [SerializeField]
    private bool wantsThisSideUp = false;

    [SerializeField]
    private float weight = 0.0f;

    [SerializeField]
    private float crushWeight = 50.0f;

    [SerializeField]
    private float crushAmount = 0.67f;

    private ArrayList supportedBy = new ArrayList();
    private ArrayList supporting = new ArrayList();
    private float supportedWeight = 0.0f;

    public float SupportedWeight {
        get {
            return supportedWeight;
        }
        set {
            supportedWeight = value;
        }
    }

    public float Weight {
        get {
            return weight + supportedWeight;
        }
    }

    public float CrushWeight {
        get {
            return crushWeight;
        }
    }

    public float CrushAmount {
        get {
            return Mathf.Clamp(crushAmount, 0.1f, 0.9f);
        }
    }

    public ArrayList SupportedBy {
        get {
            return supportedBy;
        }
    }

    public ArrayList Supporting {
        get {
            return supporting;
        }
    }

    public void Crush() {
        float localUpDot = Mathf.Abs(Vector3.Dot(gameObject.transform.up, Vector3.up));
        float localRightDot = Mathf.Abs(Vector3.Dot(gameObject.transform.right, Vector3.up));
        float localForwardDot = Mathf.Abs(Vector3.Dot(gameObject.transform.forward, Vector3.up));

        float crushScaleY = 1.0f - CrushAmount * localUpDot;
        float crushScaleX = 1.0f - CrushAmount * localRightDot;
        float crushScaleZ = 1.0f - CrushAmount * localForwardDot;

        Vector3 crushedScale = new Vector3(crushScaleX, crushScaleY, crushScaleZ);
        gameObject.transform.localScale = crushedScale;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = gameObject;

        startPosition.Set(go.transform.position.x, go.transform.position.y, go.transform.position.z);
        startRotation.Set(go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z, go.transform.rotation.w);
        startScale.Set(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
    }

    public void ResetSupport() {
        SupportedWeight = 0.0f;
    }

    public void Reset() {
        gameObject.transform.position = startPosition;
        gameObject.transform.rotation = startRotation;
        gameObject.transform.localScale = startScale;
    }

    public bool isRightSideUp() {
        bool rightSideUp = true;

        if (wantsThisSideUp) {
            rightSideUp = Vector3.Dot(gameObject.transform.up, Vector3.up) > THIS_SIDE_UP_TOLERANCE;
        }

        return rightSideUp;
    }

    private void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        Renderer otherRenderer = other.GetComponent<Renderer>();
        Cargo otherCargo = other.GetComponent<Cargo>();

        if (otherRenderer != null && otherCargo != null) {
            float hitAveY = 0.0f;
            foreach (ContactPoint contactPoint in collision.contacts) {
                hitAveY += contactPoint.point.y;
            }
            hitAveY /= Mathf.Max(1.0f, collision.contacts.Length);

            if (hitAveY < gameObject.transform.position.y) {
                // We are resting on the other object.
                if (!SupportedBy.Contains(otherCargo)) {
                    SupportedBy.Add(otherCargo);
                }
            }
            else {
                // The other object is resting on us.
                if (!Supporting.Contains(otherCargo)) {
                    Supporting.Add(otherCargo);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        GameObject other = collision.gameObject;
        Cargo otherCargo = other.GetComponent<Cargo>();

        if (SupportedBy.Contains(otherCargo)) {
            SupportedBy.Remove(otherCargo);
        }

        if (Supporting.Contains(otherCargo)) {
            Supporting.Remove(otherCargo);
        }
    }
}
