using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getTrianglesV2 : MonoBehaviour
{

    public float limit = 50.0f;

    public PhysicMaterial pMat;
    public GameObject copy;
    public GameObject[] pieces;

    public Dictionary<GameObject, List<GameObject>> adjacentPieces = new Dictionary<GameObject, List<GameObject>>();
    public Dictionary<GameObject, bool> breakState = new Dictionary<GameObject, bool>();

    private List<GameObject> breakItem = new List<GameObject>();
    private List<float> breakForce = new List<float>();
    int breakCount = 0;

    // Use this for initialization
    void Start()
    {
        Explode();
    }

    // Update is called once per frame
    void Update()
    {
        //if (breakItem.Count > breakCount) ContinuedBreak();
    }

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
            cur.AddComponent<Rigidbody>();
            cur.AddComponent<triangleCollision>().limit = limit;
            cur.name = (i / 3).ToString();

            cur.transform.parent = this.transform;
            cur.transform.localPosition = Vector3.zero;
            cur.transform.localRotation = Quaternion.identity;

            pieces[i / 3] = cur;
        }

        //hide parent object
        transform.GetComponent<MeshRenderer>().enabled = false;
        transform.localScale = oldScale;
        transform.localPosition = oldPos;

        Debug.Log(transform.name + ": created triangles.");
        Debug.Log(transform.name + ": computing adjacents...");

        //calculate adjacent triangles
        calculateAdjacent();

        Debug.Log(transform.name + ": computed adjacents.");
    }

    private void calculateAdjacent()
    {
        Dictionary<Vector3, List<GameObject>> jointDict = new Dictionary<Vector3, List<GameObject>>();

        for (int i = 0; i < pieces.Length; i++)
        {
            //get current piece
            GameObject cur = pieces[i];
            triangleCollision curCollision = cur.GetComponent<triangleCollision>();
            Mesh mesh = cur.GetComponent<MeshFilter>().mesh;
            List<Vector3> vert = new List<Vector3> { mesh.vertices[0], mesh.vertices[1], mesh.vertices[2] };

            for (int j = i + 1; j < pieces.Length; j++)
            {
                //get next next piece
                GameObject next = pieces[j];
                triangleCollision nextCollision = cur.GetComponent<triangleCollision>();
                Mesh meshP = next.GetComponent<MeshFilter>().mesh;
                List<Vector3> vertP = new List<Vector3> { meshP.vertices[0], meshP.vertices[1], meshP.vertices[2] };

                //compare
                bool adjacent = false;
                Vector3 point = Vector3.zero;
                foreach (Vector3 v in mesh.vertices)
                {
                    foreach (Vector3 v2 in meshP.vertices)
                    {
                        adjacent = (V3Equal(v, v2));
                        if (adjacent) { point = v; break; }
                    }
                    if (adjacent) break;
                }

                //add relationship
                if (adjacent)
                {
                    //curCollision.adjacent.Add(next);
                    //nextCollision.adjacent.Add(cur);

                    //FixedJoint joint = cur.AddComponent<FixedJoint>();
                    //joint.connectedBody = next.GetComponent<Rigidbody>();
                    //joint.anchor = point;
                    //joint.connectedAnchor = point;
                    //joint.enableCollision = true;
                    //joint.breakForce = limit;
                    //curCollision.limit = limit;
                    //nextCollision.limit = limit;

                    if (!jointDict.ContainsKey(point)) jointDict[point] = new List<GameObject>();

                    List<GameObject> jointList = jointDict[point];

                    if (!jointList.Contains(cur)) jointList.Add(cur);
                    if (!jointList.Contains(next)) jointList.Add(next);
                }
            }
        }
        foreach (KeyValuePair<Vector3, List<GameObject>> joint in jointDict)
        {
            GameObject jointBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointBall.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            //jointBall.GetComponent<MeshRenderer>().enabled = false;
            jointBall.transform.parent = transform;
            jointBall.AddComponent<Rigidbody>();
            jointBall.transform.localPosition = joint.Key;
            
            foreach (GameObject obj in joint.Value)
            {
                HingeJoint j = jointBall.AddComponent<HingeJoint>();
                j.connectedBody = obj.GetComponent<Rigidbody>();
            }
        }
    }

    void ContinuedBreak()
    {
        for (int i = breakCount; i < breakItem.Count; i++)
        {

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
                float newForce = force / 3;

                if (newForce > limit)
                {
                    foreach (GameObject next in adjacentPieces[cur])
                    {
                        if (!breakState[next])
                        {
                            breakItem.Add(next);
                            breakForce.Add(newForce);
                        }
                    }
                }
            }
        }

    }

    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.1;
    }
}
