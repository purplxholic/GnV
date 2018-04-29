using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTrianglesV3 : MonoBehaviour
{
    [Tooltip("Physics Material of object.")]
    public PhysicMaterial pMat;

    [Tooltip("Limit of force propagation.")]
    public float limit = 50.0f;

    [Tooltip("Minimal force spread reduction for no angle difference.")]
    public float spreadReduction = 1.3f;

    [Tooltip("Coroutine shattering delay.")]
    public float waitTime = 2.0f;

    [Tooltip("Colour of force propagation (y/n)")]
    public bool spreadColour = false;

    //Child Triangle Pieces
    private GameObject[] pieces;

    //Vertex Triangle Dictionary & Adjacent Triangle Dictionary
    private Dictionary<GameObject, List<GameObject>> adjacentPieces = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, List<GameObject>> vertexPieces = new Dictionary<GameObject, List<GameObject>>();

    //Break triangle piece
    private Dictionary<GameObject, bool> breakState = new Dictionary<GameObject, bool>();   //break state of piece
    private List<GameObject> breakItem = new List<GameObject>();                            //list of break pieces (already broken & to be broken)
    private List<float> breakForce = new List<float>();                                     //break pieces and their force
    int breakCount = 0;                                                                     //breakItem index for last broken piece

    //Construct triangle pieces from mesh
    private bool constructTriangles = true;
    private bool constructed = false;

    [Tooltip("Recalculate sections upon collision (y/n)")]
    public bool recalculation = false;

    [Tooltip("Colour of recalculated sections (y/n)")]
    public bool recalculateColour = false;

    [Tooltip("Freeze object position")]
    public bool freezePosition = false;

    [Tooltip("Freeze object rotation")]
    public bool freezeRotation = false;

    //Mesh recalculation thread-lock semaphore
    private int recalculater = -1;
    private List<bool> recalculating = new List<bool>();

    //Render Smooth Mesh (incomplete)
    private Mesh mainMesh;
    //doubly-linked dictionary of triangles
    //private Dictionary<GameObject, int> pieces_ObjIndex = new Dictionary<GameObject, int>();
    //private Dictionary<int, GameObject> pieces_IndexObj = new Dictionary<int, GameObject>();

    //for time debugging
    private float time = 0f;


    private void Start() {
        if (freezePosition && freezeRotation) { GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; }
        else if (freezePosition) { GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition; }
        else if (freezeRotation) { GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation; }
    }

    void Update() {
        if (constructTriangles && !constructed) startConstruct();
    }

    public void setUp(float limit2, float spreadReduction2, float waitTime2, PhysicMaterial pMat2, Mesh mainMesh2, GameObject[] pieces2, Dictionary<GameObject, List<GameObject>> adjacentPieces2, Dictionary<GameObject, List<GameObject>> vertexPieces2, Dictionary<GameObject, bool> breakState2, bool recalculation2, bool recalculateColour2, bool spreadColour2) {
        constructed = true; recalculation = recalculation2; limit = limit2; spreadReduction = spreadReduction2; waitTime = waitTime2; pMat = pMat2; mainMesh = mainMesh2; pieces = pieces2; adjacentPieces = adjacentPieces2; vertexPieces = vertexPieces2; breakState = breakState2; recalculateColour = recalculateColour2; spreadColour = spreadColour2;
    }

    public void startConstruct()
    {
        constructed = true;
        //get mesh & triangles
        mainMesh = GetComponent<MeshFilter>().mesh;
        pieces = new GameObject[mainMesh.triangles.Length / 3];

        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(1, 1, 1);
        Vector3 oldPos = transform.localPosition;
        transform.localPosition = new Vector3(1, 1, 1);

        //Debug.Log(transform.name + ": creating triangles...");
        time = Time.realtimeSinceStartup;

        //Rescale and Rerender Smooth Mesh (incomplete)
        //List<Vector3> newMainVertices = new List<Vector3>();
        //for (int i = 0; i < mainMesh.vertexCount; i ++) newMainVertices.Add(Vector3.Scale(mainMesh.vertices[i], oldScale));
        //mainMesh.SetVertices(newMainVertices);

        //Rescale & create individual triangles
        for (int i = 0; i < mainMesh.triangles.Length; i += 3)
        {
            GameObject cur = new GameObject();
            Mesh newMesh = new Mesh();
            newMesh.vertices = new Vector3[]
            {
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 0]], oldScale),
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 1]], oldScale),
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 2]], oldScale),
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 0]], oldScale) - mainMesh.normals[mainMesh.triangles[i+0]] * 0.01f, //turn this plane 3D
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 1]], oldScale) - mainMesh.normals[mainMesh.triangles[i+1]] * 0.01f,
                Vector3.Scale(mainMesh.vertices[mainMesh.triangles[i + 2]], oldScale) - mainMesh.normals[mainMesh.triangles[i+2]] * 0.01f
            };
            //Rerender Smooth Mesh Version (incomplete)
            //newMesh.vertices = new Vector3[]
            //{        
            //    mainMesh.vertices[mainMesh.triangles[i + 0]],
            //    mainMesh.vertices[mainMesh.triangles[i + 1]],
            //    mainMesh.vertices[mainMesh.triangles[i + 2]],
            //    mainMesh.vertices[mainMesh.triangles[i + 0]] - mainMesh.normals[mainMesh.triangles[i+0]] * 0.01f, //turn this plane 3D
            //    mainMesh.vertices[mainMesh.triangles[i + 1]] - mainMesh.normals[mainMesh.triangles[i+1]] * 0.01f,
            //    mainMesh.vertices[mainMesh.triangles[i + 2]] - mainMesh.normals[mainMesh.triangles[i+2]] * 0.01f
            //};
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
            cur.name = (i / 3).ToString();
            cur.tag = "piece";
			cur.layer = 8;

            cur.transform.parent = transform;
            cur.transform.localPosition = Vector3.zero;
            cur.transform.localRotation = Quaternion.identity;
            //cur.transform.GetComponent<MeshRenderer>().enabled = false;

            pieces[i / 3] = cur;

            //Rerender Smooth Mesh Code (incomplete)
            //pieces_IndexObj[i / 3] = cur;
            //pieces_ObjIndex[cur] = i / 3;
        }

        transform.GetComponent<MeshRenderer>().enabled = false;

        transform.localPosition = oldPos;

        time = Time.realtimeSinceStartup - time;
        //Debug.Log(transform.name + ": created triangles: " + time);
        time = Time.realtimeSinceStartup;
        //Debug.Log(transform.name + ": computing adjacents...");

        //calculate adjacent triangles
        calculateAdjacent();

        time = Time.realtimeSinceStartup - time;
        //Debug.Log(transform.name + ": computed adjacents: " + time);
    }

    private void calculateAdjacent()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            adjacentPieces[pieces[i]] = new List<GameObject>();
            vertexPieces[pieces[i]] = new List<GameObject>();
            breakState[pieces[i]] = false;
        }

        for (int i = 0; i < pieces.Length; i++)
        {
            //get current piece
            GameObject cur = pieces[i];
            Mesh mesh = cur.GetComponent<MeshFilter>().mesh;
            List<Vector3> vert = new List<Vector3> { mesh.vertices[0], mesh.vertices[1], mesh.vertices[2] };

            for (int j = i + 1; j < pieces.Length; j++)
            {
                //get next next piece
                GameObject next = pieces[j];
                Mesh meshP = next.GetComponent<MeshFilter>().mesh;
                List<Vector3> vertP = new List<Vector3> { meshP.vertices[0], meshP.vertices[1], meshP.vertices[2] };

                //compare
                int adjacent = vertPairCheck(vert, vertP);
                //add relationship
                if (adjacent == 2) //vertex triangles
                {
                    adjacentPieces[cur].Add(next);
                    adjacentPieces[next].Add(cur);
                }
                if (adjacent > 0) //adajacent triangles
                {
                    vertexPieces[cur].Add(next);
                    vertexPieces[next].Add(cur);
                }
            }
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        Vector3 collisionForce = coll.impulse / Time.fixedDeltaTime;
        float magnitude = collisionForce.magnitude;

        if (coll.gameObject.tag == "lock") return;

        if (magnitude > limit && coll.gameObject.tag != "piece")
        {
            foreach (ContactPoint c in coll.contacts)
            {
                float newForce = magnitude;
                newForce += newForce * (Random.Range(0, 2) * 2 - 1) * (Random.value / 10);
                //Debug.Log(transform.name+ " collision: "+newForce);
                GameObject cur = c.thisCollider.gameObject;
                if (!breakState[cur] && newForce > limit)
                {
                    breakItem.Add(c.thisCollider.gameObject);
                    breakForce.Add(newForce);

                    Material myNewMaterial = new Material(Shader.Find("Standard"));
                    myNewMaterial.color = new Color(Random.value, Random.value, Random.value, 1.0f);

                    StartCoroutine(ContinuedBreak(0, myNewMaterial));
                }
            }
        }
    }

    IEnumerator ContinuedBreak(int spread=0, Material myNewMaterial = null) 
    {
        Material myNewMaterial2 = new Material(Shader.Find("Standard"));
        myNewMaterial2.color = new Color(Random.value, Random.value, Random.value, 1.0f);

        Dictionary<GameObject, bool> border = new Dictionary<GameObject, bool>();

        List<GameObject> newBreakItem = new List<GameObject>();
        List<float> newBreakForce = new List<float>();

        for (int i = breakCount; i < breakItem.Count; i++)
        {

            GameObject cur = breakItem[i];
            float force = breakForce[i];
            breakCount++;

            if (!breakState[cur])
            {
                removePiece(cur); //seperate piece out

                Material myNewMaterial3 = new Material(Shader.Find("Standard"));
                myNewMaterial3.color = new Color(force / 400, force / 400, force / 400, 1.0f);
                if (spreadColour) cur.GetComponent<MeshRenderer>().material = myNewMaterial3;

                Vector3 normal = cur.transform.forward;
                float area = getArea(cur.GetComponent<MeshFilter>().mesh.vertices);

                bool noBreak = true;
                foreach (GameObject next in adjacentPieces[cur])
                {
                    Vector3 normalNext = next.transform.forward;
                    float normalDiff = Vector3.Angle(normal, normalNext);
                    float areaNext = getArea(next.GetComponent<MeshFilter>().mesh.vertices);

                    Debug.Log(normalDiff/180);
                    float newForce = force / (spreadReduction+normalDiff/180 + areaNext/area/2);
                    if (newForce > limit && !breakState[next])
                    {
                        if (noBreak) noBreak = false;
                        newBreakItem.Add(next);
                        newBreakForce.Add(newForce);
                        if (border.ContainsKey(next)) border.Remove(next);
                    }
                }
                if (noBreak) {
                  foreach (GameObject next in adjacentPieces[cur])
                  {
                        if (!breakState[next] && !border.ContainsKey(next)) {
                            border[next] = true;
                            //if (spreadColour) next.GetComponent<MeshRenderer>().material = myNewMaterial;
                        }
                  }
                }
            }
        }

        for (int i = 0; i < newBreakItem.Count; i++)
        {
            breakItem.Add(newBreakItem[i]);
            breakForce.Add(newBreakForce[i]);
        }

        yield return null;

        StartCoroutine(RecalculatePieces(border,++spread, myNewMaterial));
    }

    IEnumerator RecalculatePieces(Dictionary<GameObject,bool> border, int spread, Material myNewMaterial = null)
    {
        if(spread >0) yield return new WaitForSeconds(waitTime * 1/(spread*5));

        if (breakItem.Count > breakCount) //not all broken yet
        {
            StartCoroutine(ContinuedBreak(spread, myNewMaterial));
        }
        else if (border.Count > 0 && !recalculation) // recalculation is set to off
        {
            //check edge pieces (check for unaccounted pieces)
            var buffer = new List<GameObject>(border.Keys);
            foreach (GameObject key in buffer) {
                if (!breakState[key]) {
                    bool test = true;
                    foreach (GameObject next in adjacentPieces[key]) {
                        if (!breakState[next]) { test = false; break; }
                    }
                    if (test) removePiece(key); //separate piece out
                }
            }
        }
        else if (border.Count > 0 && recalculation) { // recalculation is set to on

            int index = recalculater + 1;
            recalculating.Add(true);
            recalculater++;
            Debug.Log("recalculator " + index + " waiting");
            if (recalculater > 0) {
                while (recalculating[recalculater - 1]) { } //wait
            }

            Debug.Log("recalculator " + index + " running");

            //check edge pieces (check for unaccounted pieces)
            var buffer = new List<GameObject>(border.Keys);
            foreach (GameObject key in buffer)
            {
                if (!breakState[key])
                {
                    bool test = true;
                    foreach (GameObject next in adjacentPieces[key])
                    {
                        if (!breakState[next]) { test = false; break; }
                    }
                    if (test) removePiece(key); //seperate piece out
                }
            }

            //calculate sections
            List<List<GameObject>> sections = new List<List<GameObject>>();
            Dictionary<GameObject, List<GameObject>> pieceSection = new Dictionary<GameObject, List<GameObject>>();

            buffer = new List<GameObject>(border.Keys);
            foreach (GameObject key in buffer)
            {
                if (!pieceSection.ContainsKey(key)) {
                    List<GameObject> section = new List<GameObject>(); //create new section
                    sections.Add(section);
                    section.Add(key);
                    pieceSection[key] = section;

                    //Material myNewMaterial2 = new Material(Shader.Find("Standard"));
                    //myNewMaterial2.color = new Color(Random.value, Random.value, Random.value, 1.0f);
                    //if (recalculateColour) key.GetComponent<MeshRenderer>().material = myNewMaterial2;

                    calculateBorders(key, section, border, pieceSection);
                }
            }

            if (sections.Count > 1)
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    GameObject newParent = new GameObject();
                    newParent.transform.localPosition = transform.localPosition;
                    newParent.transform.localRotation = transform.localRotation;
                    newParent.transform.localEulerAngles = transform.localEulerAngles;

                    Material myNewMaterial2 = new Material(Shader.Find("Standard"));
                    myNewMaterial2.color = new Color(Random.value, Random.value, Random.value, 1.0f);

                    GameObject piece = sections[i][0];
                    piece.transform.parent = newParent.transform;
                    
                    if (recalculateColour) piece.GetComponent<MeshRenderer>().material = myNewMaterial2;

                    List<GameObject> totalPieces = new List<GameObject>() { piece };
                    reassignParents(piece, newParent.transform, ref totalPieces, myNewMaterial2);

                    Rigidbody rb = newParent.AddComponent<Rigidbody>();
                    rb.mass = GetComponent<Rigidbody>().mass;
                    GetTrianglesV3 triangleCode = newParent.AddComponent<GetTrianglesV3>();
                    triangleCode.setUp(limit, spreadReduction, waitTime, pMat, mainMesh, totalPieces.ToArray(), adjacentPieces, vertexPieces, breakState, recalculation, recalculateColour, spreadColour);

                }
            }

            recalculating[index] = false;
        }
    }

    private void removePiece(GameObject cur)
    {
        breakState[cur] = true;

        Rigidbody rb = cur.AddComponent<Rigidbody>();
        cur.transform.parent = transform.parent;

        //Rerender Smooth Mesh Code (incomplete)
        //cur.GetComponent<MeshRenderer>().enabled = true;
        //List<int> triangles = new List<int>(mainMesh.triangles);
        //triangles.RemoveRange(pieces_ObjIndex[cur], 3);
        //mainMesh.SetTriangles(triangles,0);
        //mainMesh.RecalculateNormals();
    }

    private float getArea(Vector3[] verts) //calculate area of triangle
    {
        float AB = (verts[0] - verts[1]).magnitude;
        float AC = (verts[0] - verts[2]).magnitude;
        float dot = Vector3.Dot(verts[0] - verts[1], verts[0] - verts[2]);
        float area = 0.5f * AB * AC * Mathf.Sin(dot);
        return area;
    }

    private void calculateBorders(GameObject piece, List<GameObject> section, Dictionary<GameObject, bool> border, Dictionary<GameObject, List<GameObject>> pieceSection)
    {
        foreach (GameObject next in vertexPieces[piece]) {
            if (border.ContainsKey(next) && !section.Contains(next) && !breakState[next]) {
                section.Add(next);
                pieceSection[next] = section;
                //if (recalculateColour) next.GetComponent<MeshRenderer>().material = myNewMaterial;
                calculateBorders(next, section, border, pieceSection);
            }
        }
    }

    private void reassignParents(GameObject piece, Transform newParent, ref List<GameObject> totalPieces, Material myNewMaterial)
    {
        foreach (GameObject next in adjacentPieces[piece]) {
            if (next.transform.parent == transform) {
                next.transform.parent = newParent.transform;
                if (recalculateColour) next.GetComponent<MeshRenderer>().material = myNewMaterial;
                totalPieces.Add(next);
                reassignParents(next, newParent, ref totalPieces, myNewMaterial);
            }
        }
    }

    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.1;
    }

    public int vertPairCheck(List<Vector3> vList1, List<Vector3> vList2)
    {
        int count = 0;
        Vector3 a = Vector3.zero;
        foreach (Vector3 vert in vList1) {
            foreach (Vector3 vert2 in vList2) {
                if (count == 0 && vert == vert2)
                {
                    a = vert2;
                    count++;
                }
                else if (count == 1)
                {
                    if (vert2 != a && vert == vert2)
                    {
                        count++;
                        return count;
                    }
                }
            }
        }
        return count;
    }
}


// A    B   B   E   E
// B    A   E/A B   D
// B/A  D   C   E   A
// E/B  D/C A/B E   C
// B/D  A   C   B   D/B
// E    B   E   A   E

//A:6.5 B:8.5 C:3.5 D:3.5 E:8  