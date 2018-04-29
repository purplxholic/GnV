using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObject : MonoBehaviour
{
    //public GameObject shrapnelPrefab;
    //public AudioClip breakSound;
    public float lifespan = 10.0f;
    public float fadeTime = 0.5f;
    public float MinImpactToBreak = 0.05f;
    public bool voronoi = false;

    protected bool destroyed = false;
    protected float lifetime;
    protected GameObject[] pieces;

    public bool exploded = false;

    public virtual void Update()
    {
        if (destroyed)
        {
            lifetime -= Time.deltaTime;

            //out of time, destroy this object
            if (lifetime <= 0.0f)
            {
                //Object.Destroy(gameObject);
            }
            //fade out before destroying
            else if (lifetime <= fadeTime)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    Color c = pieces[i].GetComponent<Renderer>().material.color;
                    c.a = 1.0f - ((fadeTime - lifetime) / fadeTime);
                    pieces[i].GetComponent<MeshRenderer>().material.color = c;
                }
            }
        }

        if (!exploded)
        {
            Explode();
            exploded = !exploded;
        }
    }

    public void Explode()
    {
        if (destroyed) return;
        destroyed = true;
        lifetime = lifespan + fadeTime;

        //construct all the individual destructible pieces from our mesh
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        pieces = new GameObject[mesh.triangles.Length / 3];
        //a sneaky easy way to get the children to be sized correctly is to have a unit scale when spawning them, then restore it later
        Vector3 oldScale = transform.localScale;
        transform.localScale = new Vector3(1, 1, 1);

        GameObject copy = new GameObject();
        copy.AddComponent<Rigidbody>();
        copy.transform.localPosition = transform.localPosition;

        int count = 0;

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
            go.AddComponent<TriangleMesh>();
            go.name = count.ToString();
            //go.AddComponent<Rigidbody>();
            //if (voronoi) go.AddComponent<GK.BreakableSurface>();

            go.transform.parent = copy.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            pieces[i / 3] = go;
            Debug.Log("added 1 triangle");
            count++;
        }
        mesh.triangles = new int[0];
        mesh.vertices = new Vector3[0];
        mesh.uv = new Vector2[0];
        mesh.normals = new Vector3[0];
        transform.localScale = oldScale;
        copy.transform.localScale = oldScale;
        Object.Destroy(GetComponent<Collider>());

        //GetComponent<AudioSource>().PlayOneShot(breakSound);
    }

     void OnCollisionEnter(Collision coll)
    {

        Debug.Log("Collided");
        Debug.Log(coll.collider.name);

        foreach (ContactPoint c in coll.contacts)
        {
            Debug.Log(c.thisCollider.name);
        }
    }

    public GameObject[] GetPieces()
    {
        return pieces;
    }
}