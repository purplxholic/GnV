using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flyCamera : MonoBehaviour {

    public Camera cam;
    public float mouseSensitivity = 3.8f;
    public float movementIncrement = 0.5f;

    // Update is called once per frame
    void Update () {

		//local position (WASD)
		if (Input.GetKey("w")) cam.transform.localPosition += cam.transform.forward * movementIncrement;

		if (Input.GetKey("s")) cam.transform.localPosition += cam.transform.forward * -movementIncrement;

		if (Input.GetKey("a")) cam.transform.localPosition += cam.transform.right * -movementIncrement;

		if (Input.GetKey("d")) cam.transform.localPosition += cam.transform.right * movementIncrement;

		//world position (QE up/down)
		if (Input.GetKey("q")) cam.transform.position += new Vector3(0f, -movementIncrement, 0f);

		if (Input.GetKey("e")) cam.transform.position += new Vector3(0f, movementIncrement, 0f);

        if (Input.GetMouseButton(1))
        {
            //local rotation
            cam.transform.localEulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * mouseSensitivity, 0f, 0f);
            cam.transform.localEulerAngles += new Vector3(0f, Input.GetAxis("Mouse X") * mouseSensitivity, 0f);


        }
    }
}
