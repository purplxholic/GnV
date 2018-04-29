using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseShoot : MonoBehaviour {

    public PhysicMaterial bulletMat;
    public Transform cam;
    public float speed = 2.5f;

    private GameObject bullet;

	// Use this for initialization
	void Start () {
        bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        SphereCollider collider = bullet.AddComponent<SphereCollider>();
        collider.material = bulletMat;
        Rigidbody rigidbody = bullet.AddComponent<Rigidbody>();
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        bullet.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) OnMouseUp();
    }

    void OnMouseUp()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 3.0f;
        pos = Camera.main.ScreenToWorldPoint(pos);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            bullet.SetActive(true);
            bullet.transform.position = pos;
            Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.velocity = (hit.point - pos).normalized * speed;
        }
    }
}
