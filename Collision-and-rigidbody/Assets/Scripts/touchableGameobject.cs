using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchableGameobject : MonoBehaviour
{

	GameObject touchedObject;

	// Use this for initialization
	void Start ()
	{
		touchedObject = gameObject;
		print ("start mousemanager for "+touchedObject.name);
	}

	void OnMouseDown ()
	{
		print("Received a mouse down on "+touchedObject.name);
	}

	void Update ()
	{ 
	}

	public void Touched()
	{
		Instantiate(gameObject, gameObject.transform.parent.position, Quaternion.identity);
	}
}
