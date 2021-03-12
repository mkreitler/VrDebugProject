using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assertions;

public class Pallet : MonoBehaviour {
    [SerializeField]
    private BoxCollider _testTrigger = null;

    public BoxCollider testTrigger {
        get {
            return _testTrigger;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Assert.Test(testTrigger != null, "Invalid test trigger!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
