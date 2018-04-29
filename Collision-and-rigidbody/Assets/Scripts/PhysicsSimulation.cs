using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * OVERLORD CODE THAT COMPUTES THE INDIVIDUAL RIGIDBODY FORCES 
 */
public class PhysicsSimulation : MonoBehaviour
{

    public Vector3 gravity = new Vector3(0, -1, 0);

    enum CollisionState
    {
        Penetrating,
        Colliding,
        Clear
    }

    private CollisionState collisionState;
    //intialise walls 
    //ref code treats it as a plane 
    public struct Plane
    {
        public Vector3 Normal; // inward pointing
        public float d;  // ax + by + cz + d = 0 
    }

    public Plane groundFloor;

    Vector3 CollisionNormal;
    int CollidingBodyIndex;
    int CollidingCornerIndex;

    int SourceConfigurationIndex;
    int TargetConfigurationIndex;

    //capture all the bodies in the scene 
    List<GameObject> allBodies = new List<GameObject>(); //holds all the rigidbodies 
    List<GameObject> allPlanes = new List<GameObject>(); //holds all the planes in the world
    public int NumberOfBodies = 0; //no. of bodies in the scene
    public int noOfplanes = 0; // no. of planes to hit in the world space 

    private CollisionState Penetrating { get; set; }
    private CollisionState Colliding { get; set; }

    void IntialiseBodies()
    {
        GameObject[] go_allBodies = GameObject.FindGameObjectsWithTag("Object");
        NumberOfBodies = go_allBodies.Length;
        for (int i = 0; i < NumberOfBodies; i++)
        {
           
            allBodies.Add(go_allBodies[i]);
        }
        Debug.Log("Found" + allBodies.Capacity + " bodies");
        GameObject[] go_allFloorandWall = GameObject.FindGameObjectsWithTag("Stationary");
        noOfplanes = go_allFloorandWall.Length;
        for (int i = 0; i < noOfplanes; i++)
        {
            allPlanes.Add(go_allFloorandWall[i]);
        }
        Debug.Log("Found" + noOfplanes + " planes");
        //now set all the stuff for the rigidbodies 
        for (int bodyIndex = 0; bodyIndex < NumberOfBodies; bodyIndex++)
        {
            //create a rigidbody 
            //RigidBody rb = new RigidBody();
            //get the gameobject 
            GameObject gameObject = (GameObject)allBodies[bodyIndex];
            //add a rigidbody
            gameObject.AddComponent<RigidBody>();

            RigidBody rigidBody = gameObject.GetComponent<RigidBody>();
            rigidBody.ns_position = gameObject.transform.position;
            float halfX = gameObject.GetComponent<Renderer>().bounds.extents.x;
            float halfY = gameObject.GetComponent<Renderer>().bounds.extents.y;
            float halfZ = gameObject.GetComponent<Renderer>().bounds.extents.z;

            rigidBody.BodyInertiaInverseTensor.setMELEMENT(0, 0, 3.0f / (rigidBody.Mass * (halfY * halfY + halfZ * halfZ)));
            rigidBody.BodyInertiaInverseTensor.setMELEMENT(1, 1, 3.0f / (rigidBody.Mass * (halfX * halfX + halfZ * halfZ)));
            rigidBody.BodyInertiaInverseTensor.setMELEMENT(2, 2, 3.0f / (rigidBody.Mass * (halfX * halfX + halfY * halfY)));


        }
        Debug.Log("Finished set up");

    }
    public float Gravity = -9.8f;  //follows Physics.gravity s
    private float rho = 1.2f; // Density of air. 
    private float C_d = 1.05f;  // Drag for ball is 0.47. Cube is 1.05
    public float Kda = 0.01f;

    void ComputeForces()
    {

        for (int counter = 0; counter < NumberOfBodies; counter++)
        {
            //get the gameobject 

            GameObject gameObject = (GameObject) allBodies[counter];
            //get the rigidbody  of the object 
            RigidBody rigidBody = gameObject.GetComponent<RigidBody>();

            //clear forces 
            float A;// Surface area for cube is 6a^2
            rigidBody.Torque = Vector3.zero;
            rigidBody.forces = Vector3.zero;

            A = gameObject.GetComponent<Renderer>().bounds.size.x * gameObject.GetComponent<Renderer>().bounds.size.y;
            if (gameObject.tag != "Stationary")
            {
                rigidBody.forces += new Vector3(0, rigidBody.Mass * Gravity, 0);

                /*
             * Drag will determine how air resistance would be 
             */
                Debug.Log(0.5f * 1.2f * 0.47f * rigidBody.m_AngularVelocity.x * rigidBody.m_AngularVelocity.x);
                //this.forces += ((Mathf.Pow(m_linearVelocity.magnitude, 2) * Drag) * -m_linearVelocity);
                if (rigidBody.Drag == 0) //if  user does not define drag 
                {
                    rigidBody.Drag = 0.00001f; //default to a small value 
                }
                //damping / drag force 

                rigidBody.forces.x += 0.5f * rho * C_d * A * rigidBody.m_linearVelocity.x * rigidBody.m_linearVelocity.x * rigidBody.Drag;
                rigidBody.forces.y += 0.5f * rho * C_d * A * rigidBody.m_linearVelocity.y * rigidBody.m_linearVelocity.y * rigidBody.Drag;
                rigidBody.forces.z += 0.5f * rho * C_d * A * rigidBody.m_linearVelocity.z * rigidBody.m_linearVelocity.z * rigidBody.Drag;

                rigidBody.Torque += -Kda * rigidBody.m_AngularVelocity;

            }
        }
    }

    void Integrate()
    {
        for (int counter = 0; counter < NumberOfBodies; counter++)
        {
            //get the gameobject 
            GameObject gameObject = (GameObject)allBodies[counter];
            //get the rigidbody  of the object 
            RigidBody rigidBody = gameObject.GetComponent<RigidBody>();

            //linear
            //set position first 
            if(rigidBody.position != rigidBody.transform.position)
            {
                rigidBody.position = rigidBody.transform.position; 
            }
            rigidBody.ns_position = rigidBody.position + rigidBody.m_linearVelocity * Time.fixedDeltaTime;
            //set the velocity 
            rigidBody.ns_m_linearVelocity = rigidBody.m_linearVelocity + rigidBody.inv_mass * rigidBody.forces * Time.fixedDeltaTime;

            //set the acceleration 
            //rigidBody.ns_m_linearAcceleration += rigidBody.forces * rigidBody.inv_mass;



            //rotational 

            GK.Matrix3f skewedAngurlar = new GK.Matrix3f();
            skewedAngurlar = skewedAngurlar.SkewSymmetry(rigidBody.m_AngularVelocity);

            rigidBody.ns_orientation = rigidBody.orientation + skewedAngurlar * Time.fixedDeltaTime * rigidBody.orientation;

            rigidBody.ns_AngularMomentum = rigidBody.AngularMomentum + Time.fixedDeltaTime * rigidBody.Torque;

            GK.Matrix3f transposedOrientation = new GK.Matrix3f();
            transposedOrientation = rigidBody.ns_orientation.Transposed();
            rigidBody.ns_WorldInertiaInverseTensor = (rigidBody.ns_orientation * rigidBody.BodyInertiaInverseTensor) * transposedOrientation;

            rigidBody.ns_orientation.OrthonormalizeOrientation();

            rigidBody.ns_m_AngularVelocity = rigidBody.ns_WorldInertiaInverseTensor * rigidBody.ns_AngularMomentum;
        }
    }

    void SetVariables()
    {
        for (int counter = 0; counter < NumberOfBodies; counter++)
        {
            //get the gameobject 
            GameObject gameObject = (GameObject)allBodies[counter];
            //get the rigidbody  of the object 
            RigidBody rigidBody = gameObject.GetComponent<RigidBody>();
            
            rigidBody.position = rigidBody.ns_position;
            //rigidBody.m_linearAcceleration = rigidBody.ns_m_linearAcceleration;
            //position = transform.position;
            rigidBody.orientation = rigidBody.ns_orientation;
            rigidBody.m_linearVelocity = rigidBody.ns_m_linearVelocity;
            rigidBody.m_AngularVelocity = rigidBody.ns_m_AngularVelocity;
            rigidBody.Torque = rigidBody.ns_Torque;
            rigidBody.AngularMomentum = rigidBody.ns_AngularMomentum;

            //rigidBody.LinearMomentum = rigidBody.ns_LinearMomentum;
            rigidBody.m_AngularAccelration = rigidBody.ns_AngularMomentum;
            rigidBody.WorldInertiaInverseTensor = rigidBody.ns_WorldInertiaInverseTensor;

        }

           


    }

    CollisionState CheckForCollision(int ConfigurationIndex)
    {
        return new CollisionState();
    }

    void ResolveCollisions(int ConfigurationIndex)
    {
        //zou 
    }

    void CalculateVertices(int ConfigurationIndex)
    {
        //for (int counter = 0; counter < NumberOfBodies; counter++)
        //{
        //    //get the configuration of the rb 
        //    RigidBody objectRB = gameObject.GetComponent<RigidBody>();
        //    RigidBody.Configuration objectRBConfig = objectRB.RBConfiguration;

        //    GK.Matrix3x3 A = objectRBConfig.Orientation;
        //    Vector3 R = objectRBConfig.CMPosition;

        //    for (int i = 0; i < objectRB.NumberOfBoundingVertices; i++)
        //    {
        //        objectRBConfig.aBoundingVertices = R + A * objectRBConfig.aBoundingVertices[i];
        //    }
        //}
    }
    // do the frame thing 
    private void Simulate(float DeltaTime)
    {
        //Debug.Log("Simulate");
        //float CurrentTime = 0.0f;
        //float TargetTime = DeltaTime;
        //while (CurrentTime < DeltaTime)
        //{
        //    ComputeForces();

        //    Integrate(TargetTime - CurrentTime);

        //    CalculateVertices(TargetConfigurationIndex);

        //    CheckForCollision(TargetConfigurationIndex);

            //if (collisionState == Penetrating)
            //{
            //    TargetTime = (CurrentTime + TargetTime) / 2.0f;

            //}
            //else
            //{
            //    //colliding or clear
            //    //@TODO FIX COLLINDG == 
            //    if (collisionState == Colliding)
            //    {
            //        int Counter = 0;
            //        do
            //        {
            //            ResolveCollisions(TargetConfigurationIndex);
            //            Counter++;
            //        } while ((CheckForCollision(TargetConfigurationIndex) == Colliding) && (Counter < 100));
            //    }

            // we made a successful step, so swap configurations
            // to "save" the data for the next step

        //    CurrentTime = TargetTime;
        //    TargetTime = DeltaTime;

        //    //}
        //}
    }

    
    // Use this for initialization
    void Start()
    {
        Debug.Log("initialising");
        IntialiseBodies(); //called once 

    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetPosition()
    {
        for (int counter = 0; counter < NumberOfBodies; counter++)
        {
            //get the gameobject 

            GameObject gameObject = (GameObject)allBodies[counter];
            //get the rigidbody  of the object 
            RigidBody rigidBody = gameObject.GetComponent<RigidBody>();
            rigidBody.transform.position = rigidBody.position; 
        }
    }
    //deal rigidbody stuff here 
    void FixedUpdate()
    {
        SetVariables();
        SetPosition();
        ComputeForces();
        Integrate();
        
        //float MaxTimeStep = 0.01f;
        //float LastTime = 0;
        //Debug.Log(MaxTimeStep);
        //Debug.Log(LastTime);
        //while (LastTime < Time.fixedDeltaTime)
        //{

        //    float DeltaTime = Time.fixedDeltaTime - LastTime;
        //    if (DeltaTime > MaxTimeStep)
        //    {
        //        DeltaTime = MaxTimeStep;
        //    }
        //    Debug.Log("Calling simulate");
        //    Simulate(DeltaTime);
        //    LastTime += DeltaTime;
        //}
        //LastTime = Time.fixedDeltaTime;

        ////Simulate(Time.fixedDeltaTime);
    }
}
