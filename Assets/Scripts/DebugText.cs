using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PalletBuilder
{
    public class DebugText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMesh = null;

        // Start is called before the first frame update
        void Awake()
        {
            Switchboard.Listen("SetText", this, SetText);
        }

        private void Start() {
            Switchboard.Broadcast("SetText", "Debug started...");
        }

        // Update is called once per frame
        void Update()
        {

        }

        public object SetText(object args)
        {
            if (textMesh != null)
            {
                string strArg = args as string;

                textMesh.text = strArg != null ? strArg : "";
            }

            return null;
        }
    }
}
