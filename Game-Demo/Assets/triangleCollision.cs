using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triangleCollision : MonoBehaviour { 

    public bool breakState = false;
    public List<GameObject> adjacent = new List<GameObject>();
    public float limit;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	//void Update () {
		
	//}
    
    //void OnCollisionEnter(Collision coll)
    void OnCol(Collision coll)
    {
        Vector3 collisionForce = coll.impulse / Time.fixedDeltaTime;
        float magnitude = collisionForce.magnitude;

        if (magnitude > limit)
        {
            foreach (ContactPoint c in coll.contacts)
            {
                GameObject cur = c.thisCollider.gameObject;
                //if (!breakState[cur])
                //{ //if obj is 'broken'
                //    breakItem.Add(c.thisCollider.gameObject);
                //    breakForce.Add(collisionForce.magnitude);
                //}
            }
        }
    }
}
