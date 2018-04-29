using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GK;

public class mouseShoot : MonoBehaviour {

	public PhysicMaterial bulletMat;
	public Transform cam;
	public float speed = 2.5f;

	private GameObject bullet;
	bool created = false;

	void createBullet () {
		created = true;
		bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		bullet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);


		bullet.SetActive(false);
		bullet.name = "bullet";
	}

	void destroyBullet () {
		Destroy (bullet);
		created = false;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) OnMouseUp();
	}

	void OnMouseUp()
	{
		if (created)
			destroyBullet ();
		createBullet ();

		Vector3 pos = Input.mousePosition;

//		pos.z = 3.0f;
		//bullet.GetComponent<RigidBody> ().ns_position = pos; 
		pos = Camera.main.ScreenToWorldPoint(pos);

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			bullet.SetActive(true);
			bullet.transform.position = pos;
			bullet.AddComponent<Rigidbody>();
			bullet.AddComponent<GK.Collider>();
			Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
			rigidbody.velocity = (hit.point - pos).normalized * speed;
		}
	}
}