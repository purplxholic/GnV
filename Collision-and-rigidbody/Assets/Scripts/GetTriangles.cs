using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK
{

public class GetTriangles : MonoBehaviour {

    public GameObject copy;
    public GameObject[] pieces;
    //public List<int>[] adjacentPieces;
    public Dictionary<GameObject, List<GameObject>> adjacentPieces;

    private List<GameObject> breakItem = new List<GameObject>();
    private List<float> breakForce = new List<float>();
	public float startTime; 

	// Use this for initialization
	void Start () {
        Explode();
			print ("tri");
			print (pieces [0].name);
			startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
//        if (breakItem.Count > 0) ContinuedBreak();
//		CheckForCollission ();

	}

    public void Explode()
    {
        //get mesh & triangles
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        pieces = new GameObject[mesh.triangles.Length / 3];
        //adjacentPieces = new List<int>[mesh.triangles.Length / 3];
        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(1, 1, 1);

        //new holder object
        //copy = new GameObject();
        //copy.AddComponent<Rigidbody>();
        //copy.transform.localPosition = transform.localPosition;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            GameObject go = new GameObject();
            Mesh newMesh = new Mesh();
            newMesh.vertices = new Vector3[]
            {
                mesh.vertices[mesh.triangles[i+0]],
                mesh.vertices[mesh.triangles[i+1]],
                mesh.vertices[mesh.triangles[i+2]],
                mesh.vertices[mesh.triangles[i+0]] - mesh.normals[mesh.triangles[i+0]] * 0.01f, //need to turn this plane 3D
                mesh.vertices[mesh.triangles[i+1]] - mesh.normals[mesh.triangles[i+1]] * 0.01f,
                mesh.vertices[mesh.triangles[i+2]] - mesh.normals[mesh.triangles[i+2]] * 0.01f
            };
            newMesh.uv = new Vector2[]
            {
                mesh.uv[mesh.triangles[i+0]],
                mesh.uv[mesh.triangles[i+1]],
                mesh.uv[mesh.triangles[i+2]],
                mesh.uv[mesh.triangles[i+0]],
                mesh.uv[mesh.triangles[i+1]],
                mesh.uv[mesh.triangles[i+2]]
            };
            newMesh.triangles = new int[]
            {
                0, 2, 3,
                2, 5, 3,
                0, 3, 1,
                1, 3, 4,
                1, 4, 2,
                2, 4, 5,
                2, 0, 1,
                5, 4, 3
            };
            newMesh.RecalculateNormals();

            go.AddComponent<MeshFilter>().mesh = newMesh;
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = newMesh;
            mc.convex = true;
            go.AddComponent<MeshRenderer>().material = filter.transform.GetComponent<MeshRenderer>().material;
//            go.AddComponent<TriangleMesh>();
            go.name = (i/3).ToString();

            //go.transform.parent = copy.transform;
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            pieces[i / 3] = go;
        }

        mesh.triangles = new int[0];
        mesh.vertices = new Vector3[0];
        mesh.uv = new Vector2[0];
        mesh.normals = new Vector3[0];
        transform.localScale = oldScale;

//        calculateAdjacent();
    }

//    private void calculateAdjacent()
//    {
//        for (int i = 0; i < pieces.Length; i++) adjacentPieces[pieces[i]] = new List<GameObject>();
//
//        for (int i= 0; i< pieces.Length; i++)
//        {
//            //get current piece
//            GameObject obj = pieces[i];
//            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
//            List<Vector3> vert = new List<Vector3> { mesh.vertices[0], mesh.vertices[1], mesh.vertices[2] };
//
//            for (int j= i+1; j < pieces.Length; j++)
//            {
//                //checking piece
//                GameObject piece = pieces[j];
//                if (!adjacentPieces[obj].Contains(piece))
//                {
//                    Mesh meshP = piece.GetComponent<MeshFilter>().mesh;
//                    List<Vector3> vertP = new List<Vector3> { meshP.vertices[0], meshP.vertices[1], meshP.vertices[2] };
//                    bool same = false;
//                    foreach (Vector3 v in vert) {
//                        foreach (Vector3 v2 in vertP) {
//                            same = (V3Equal(v, v2));
//                            if (same) break;
//                        }
//                        if (same) break;
//                    }
//                    if (same) {
//                        adjacentPieces[obj].Add(piece);
//                        adjacentPieces[piece].Add(obj);
//                    }
//                }
//            }
//            //Debug.Log(i+1 + " "+adjacentPieces[i].Count);
//        }
//    }

    void OnCollisionEnter(Collision coll)
    {
        Vector3 collisionForce = coll.impulse / Time.fixedDeltaTime;
        if (collisionForce.magnitude > 100)
        {
            foreach (ContactPoint c in coll.contacts)
            {
                Debug.Log(collisionForce + " " + c.thisCollider.name);
                //breaking.Add(breakVal);
                breakItem.Add(c.thisCollider.gameObject);
                breakForce.Add(collisionForce.magnitude);

            }
        }
    }

//    void ContinuedBreak()
//    {
//        for (int i = 0; i<breakItem.Count; i++)
//        {
//            GameObject go = breakItem[i];
//            float force = breakForce[i];
//
//            if (force > 100)
//            {
//                go.AddComponent<Rigidbody>();
//                go.transform.parent = transform.parent;
//
//                List<Vector3> verts = new List<Vector3>(go.GetComponent<MeshFilter>().mesh.vertices);
//                float AB = (verts[0] - verts[1]).magnitude;
//                float AC = (verts[0] - verts[2]).magnitude;
//                float dot = Vector3.Dot(verts[0] - verts[1], verts[0] - verts[2]);
//                float area = 0.5f * AB * AC * Mathf.Sin(dot);
//                Debug.Log(area);
//
//                float newForce = force / area;
//
//                //if (force > 100)
//                //{
//                //    foreach (int index2 in adjacentPieces[index])
//                //    {
//                //        Debug.Log(index2);
//                //    }
//                //}
//            }
//        }
//
//        breakItem.Clear();
//        //breaking = newBreaking;
//        //GameObject go = c.thisCollider.gameObject;
//        //go.AddComponent<Rigidbody>();
//        //go.transform.parent = transform.parent;
//    }

    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.1;
    }
	public bool BoundaryIntersect(GameObject go1, GameObject go2){
		Mesh mesh1 = go1.GetComponent<MeshFilter>().mesh;
		Mesh mesh2 = go2.GetComponent<MeshFilter>().mesh;
		Bounds b1 = mesh1.bounds;
		Bounds b2 = mesh2.bounds;
		//need to code this out 
		return b1.Intersects(b2);
	}
	public GameObject hitAt(GameObject go){
		Rigidbody rb = go.GetComponent<Rigidbody>();
		Vector3 goVelocity = rb.velocity; 
		Ray ray = new Ray (go.transform.position, goVelocity);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 100.0f)) {
			Transform prefab = hit.transform;
			GameObject tobehitgo = prefab.gameObject;
			return tobehitgo;
		} else
			return null;	
	}
//		public void CheckForCollission(GameObject go){
//			
//			//for every pair of rigid bodies, try to check the distance between them 
////			for(int i=0; i<pieces.Length;i++){
////				GameObject go = pieces [i];
////				GameObject target = hitAt (go);
////				print ("checkhit");
//
////			}
//
//		}

//	public void CollisionResponce(float e,float ma,float mb, Matrix3x3 Ia,Matrix3x3 Ib,Vector3 ra,Vector3 rb,Vector3 n,
//		Vector3 vai, Vector3 vbi, Vector3 wai, Vector3 wbi, Vector3 vaf, Vector3 vbf, Vector3 waf, Vector3 wbf) {
//		Matrix3x3 IaInverse = Ia.inverse();
//		Vector3 normal = n.normalized;
//		Vector3 angularVelChangea  = normal; // start calculating the change in abgular rotation of a
//		angularVelChangea=Vector3.Cross(angularVelChangea,ra);
//
//		Vector3 vaLinDueToR = Vector3.Cross(IaInverse*angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
//		float scalar = 1/ma + Vector3.Dot(vaLinDueToR,normal);
//		Matrix3x3 IbInverse = Ib.inverse();
//		Vector3 angularVelChangeb = normal; // start calculating the change in abgular rotation of b
//		angularVelChangeb=Vector3.Cross(angularVelChangeb, rb);
//
//		Vector3 vbLinDueToR = Vector3.Cross(IbInverse*angularVelChangeb, rb); // calculate the linear velocity of collision point on b due to rotation of b
//		scalar += 1/mb +Vector3.Dot(vbLinDueToR,normal);
//		float Jmod = (e+1)*(vai-vbi).magnitude/scalar;
//		Vector3 J = normal*Jmod;
//		vaf = vai - J*(1/ma);
//		vbf = vbi - J*(1/mb);
//		waf = wai - angularVelChangea;
//		wbf = wbi - angularVelChangeb;
//	}


}
}