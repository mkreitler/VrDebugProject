using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PalletBuilder
{
    public class DebugSphere : MonoBehaviour
    {
        [SerializeField]
        private GameObject debugSphere = null;

        [SerializeField]
        private GameObject forwardTransform = null;

        private Vector3 startScale = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            if (debugSphere != null)
            {
                startScale = debugSphere.transform.localScale;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (debugSphere != null && forwardTransform != null)
            {
                debugSphere.transform.position = gameObject.transform.position + gameObject.transform.forward * 10.0f;

                Vector3 vScale = startScale + startScale * 0.5f * Mathf.Sin(Time.time * 2.0f * Mathf.PI / 1.0f);
                debugSphere.transform.localScale = vScale;
            }
            else if (debugSphere != null)
            {
                debugSphere.GetComponent<Renderer>().enabled = false;
            }
        }
    }
}
