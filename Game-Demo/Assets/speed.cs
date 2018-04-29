using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speed : MonoBehaviour {

    public float spd = 10.0f;
    public Transform dir;

    // Use this for initialization
    void Start () {
        //Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        //rigidbody.velocity = (dir.forward).normalized * spd;

        foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.velocity = (dir.forward).normalized * spd;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
