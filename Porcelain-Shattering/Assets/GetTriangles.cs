using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTriangles : MonoBehaviour {

    public float limit = 50.0f;
    public float spreadReduction = 2.0f;
    public float waitTime = 2.0f;

    public PhysicMaterial pMat;
    public GameObject copy;
    public GameObject[] pieces;

    public Dictionary<GameObject, List<GameObject>> adjacentPieces = new Dictionary<GameObject, List<GameObject>>();
    public Dictionary<GameObject, bool> breakState = new Dictionary<GameObject, bool>();

    private List<GameObject> breakItem = new List<GameObject>();
    private List<float> breakForce = new List<float>();
    int breakCount = 0;
    
    private float time = 0f;

	// Use this for initialization
	void Start () {
        Explode();
	}
	
	// Update is called once per frame
	//void Update () {
    //    if (breakItem.Count > breakCount) ContinuedBreak();
	//}

    public void Explode()
    {
        //get mesh & triangles
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        pieces = new GameObject[mesh.triangles.Length / 3];

        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(1, 1, 1);
        Vector3 oldPos = transform.localPosition;
        transform.localPosition = new Vector3(1, 1, 1);

        Debug.Log(transform.name + ": creating triangles...");
        time = Time.realtimeSinceStartup;

        //create each triangle
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            GameObject cur = new GameObject();
            Mesh newMesh = new Mesh();
            newMesh.vertices = new Vector3[]
            {
                mesh.vertices[mesh.triangles[i+0]],
                mesh.vertices[mesh.triangles[i+1]],
                mesh.vertices[mesh.triangles[i+2]],
                mesh.vertices[mesh.triangles[i+0]] - mesh.normals[mesh.triangles[i+0]] * 0.01f, //turn this plane 3D
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

            cur.AddComponent<MeshFilter>().mesh = newMesh;
            MeshCollider mc = cur.AddComponent<MeshCollider>();
            mc.material = pMat;
            mc.sharedMesh = newMesh;
            mc.convex = true;
            cur.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
            cur.name = (i/3).ToString();

            cur.transform.parent = this.transform;
            cur.transform.localPosition = Vector3.zero;
            cur.transform.localRotation = Quaternion.identity;

            pieces[i / 3] = cur;
        }

        //hide parent object
        transform.GetComponent<MeshRenderer>().enabled = false;
        transform.localScale = oldScale;
        transform.localPosition = oldPos;

        time = Time.realtimeSinceStartup - time;
        Debug.Log(transform.name + ": created triangles: " +time);
        time = Time.realtimeSinceStartup;
        Debug.Log(transform.name + ": computing adjacents...");

        //calculate adjacent triangles
        calculateAdjacent();
        time = Time.realtimeSinceStartup - time;
        Debug.Log(transform.name + ": computed adjacents: "+time);
    }

    private void calculateAdjacent()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            adjacentPieces[pieces[i]] = new List<GameObject>();
            breakState[pieces[i]] = false;
        }

        for (int i= 0; i< pieces.Length; i++)
        {
            //get current piece
            GameObject cur = pieces[i];
            Mesh mesh = cur.GetComponent<MeshFilter>().mesh;
            List<Vector3> vert = new List<Vector3> { mesh.vertices[0], mesh.vertices[1], mesh.vertices[2] };

            for (int j= i+1; j < pieces.Length; j++)
            {
                //get next next piece
                GameObject next = pieces[j];
                Mesh meshP = next.GetComponent<MeshFilter>().mesh;
                List<Vector3> vertP = new List<Vector3> { meshP.vertices[0], meshP.vertices[1], meshP.vertices[2] };

                //compare
                bool adjacent = false;
                foreach (Vector3 v in mesh.vertices) {
                    foreach (Vector3 v2 in meshP.vertices) {
                        adjacent = (V3Equal(v, v2));
                        if (adjacent) break;
                    }
                    if (adjacent) break;
                }
                //add relationship
                if (adjacent) {
                    adjacentPieces[cur].Add(next);
                    adjacentPieces[next].Add(cur);
                }
            }
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Vector3 collisionForce = coll.impulse / Time.fixedDeltaTime;
        float magnitude = collisionForce.magnitude;

        if (magnitude > limit) {
            foreach (ContactPoint c in coll.contacts)
            {
                GameObject cur = c.thisCollider.gameObject;
                if (breakState.ContainsKey(cur))
                {
                    if (!breakState[cur])
                    { //if obj is 'broken'
                        breakItem.Add(c.thisCollider.gameObject);
                        breakForce.Add(collisionForce.magnitude);
                        StartCoroutine(ContinuedBreak());
                    }
                }
            }
        }
    }

    IEnumerator ContinuedBreak()
    {
        Material myNewMaterial = new Material(Shader.Find("Standard"));
        myNewMaterial.color = new Color(Random.value, Random.value, Random.value, 1.0f);

        List<GameObject> border = new List<GameObject>();

        List<GameObject> newBreakItem = new List<GameObject>();
        List<float> newBreakForce = new List<float>();

        for (int i = breakCount; i< breakItem.Count; i++) {

            GameObject cur = breakItem[i];
            float force = breakForce[i];
            breakCount++;

            if (!breakState[cur])
            {
                breakState[cur] = true;

                Rigidbody rb = cur.AddComponent<Rigidbody>();
                //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                cur.transform.parent = transform.parent; //seperate piece out

                //calculate area of triangle
                Vector3[] verts = cur.GetComponent<MeshFilter>().mesh.vertices;
                float AB = (verts[0] - verts[1]).magnitude;
                float AC = (verts[0] - verts[2]).magnitude;
                float dot = Vector3.Dot(verts[0] - verts[1], verts[0] - verts[2]);
                float area = 0.5f * AB * AC * Mathf.Sin(dot);

                //spread force to adjacent piece
                float newForce = force / spreadReduction;

                if (newForce > limit) {
                    foreach (GameObject next in adjacentPieces[cur])
                    {
                        if (!breakState[next]) {
                            newBreakItem.Add(next);
                            newBreakForce.Add(newForce);
                        }
                        //if (border.Contains(next)) border.Remove(next);
                    }
                } //else
                //{
                    //foreach (GameObject next in adjacentPieces[cur])
                    //{
                    //    if (!breakState[next] && !border.Contains(next)) border.Add(next);
                    //}
                //}
            }
        }

        for (int i = 0; i < newBreakItem.Count; i++)
        {
            breakItem.Add(newBreakItem[i]);
            breakForce.Add(newBreakForce[i]);
        }
        //foreach (GameObject next in border)
        //{
        //    next.GetComponent<MeshRenderer>().material = myNewMaterial;
        //}
        Debug.Log("shattered");

        yield return null;

        StartCoroutine(RecalculatePieces());
    }

    IEnumerator RecalculatePieces()
    {
        
        yield return new WaitForSeconds(waitTime);

        if (breakItem.Count > breakCount) StartCoroutine(ContinuedBreak());
    }

    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.1;
    }
}


// A    B   B   E   E
// B    A   E/A B   D
// B/A  D   C   E   A
// E/B  D/C A/B E   C
// B/D  A   C   B   D/B
// E    B   E   A   E

//A:6.5 B:8.5 C:3.5 D:3.5 E:8  