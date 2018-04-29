using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testFace : MonoBehaviour {

    private Mesh mesh;
    private float MinImpactToBreak = 0.05f;

    // Use this for initialization
    void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
	}
	
    private void readFace()
    {
    }


	// Update is called once per frame
	void Update () {
		
	}

}
