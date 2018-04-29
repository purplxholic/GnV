using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * DEFINITION OF CUSTOM RIGIDBODY 
 */
public class RigidBody : MonoBehaviour {

    //variables 
    
    public bool DampingActive = false;

    public float CoefficientOfRestitution =1.0f;
    public float Mass = 1.0f;
    public float inv_mass;
    public bool UseGravity = true;
    public float Drag = 1.0f;
    public float AngularDrag = 0.05f;


    //Current Step
    public Vector3 position;
    public GK.Matrix3f orientation; // A
    public Vector3 m_linearVelocity; // v
    public Vector3 m_AngularVelocity; // omega
    public Vector3 Torque;
    public Vector3 AngularMomentum; // L
    public Vector3 forces; // F
    //public Vector3 LinearMomentum; // p 
    public Vector3 m_AngularAccelration; //alpha 
    public GK.Matrix3f WorldInertiaInverseTensor;
    public GK.Matrix3f BodyInertiaInverseTensor; 
    //next step 
    public Vector3 ns_position;
    public GK.Matrix3f ns_orientation; // A
    public Vector3 ns_m_linearVelocity; // v
    public Vector3 ns_m_AngularVelocity; // omega
    public Vector3 ns_Torque;
    public Vector3 ns_AngularMomentum; // L
    public Vector3 ns_forces; // F
    //public Vector3 ns_LinearMomentum; // p 
    public Vector3 ns_m_AngularAccelration; //alpha 
    public GK.Matrix3f ns_WorldInertiaInverseTensor;
    // Use this for initialization
    //define the intital values here 

    public RigidBody()
    {
        inv_mass = 1.0f / this.Mass;
        //position = transform.position;
        orientation = new GK.Matrix3f();
        m_linearVelocity = new Vector3();
        m_AngularVelocity = new Vector3();
        Torque = new Vector3();
        AngularMomentum = new Vector3();
        forces = new Vector3();
        //LinearMomentum = new Vector3();
        m_AngularAccelration = new Vector3();
        WorldInertiaInverseTensor = new GK.Matrix3f();
        BodyInertiaInverseTensor = new GK.Matrix3f();

        //position = transform.position;
        ns_orientation = new GK.Matrix3f();
        ns_m_linearVelocity = new Vector3();
        ns_m_AngularVelocity = new Vector3();
        ns_Torque = new Vector3();
        ns_AngularMomentum = new Vector3();
        ns_forces = new Vector3();
        //ns_LinearMomentum = new Vector3();
        ns_m_AngularAccelration = new Vector3();
        ns_WorldInertiaInverseTensor = new GK.Matrix3f();
    }
	private Mesh mesh; 
	private Vector3[] vertices;
    // UNITY DEFINED FUNCTIONS 
    void Start()
    {
        //initialise the initial conditions 
        /*
         * InverseInertiaTensor 
         *  
         */
        ns_position = transform.position;
        ns_orientation = orientation.QuaterniontoMatrix3(transform.rotation); 
        float halfX = GetComponent<Renderer>().bounds.extents.x;
        float halfY = GetComponent<Renderer>().bounds.extents.y;
        float halfZ = GetComponent<Renderer>().bounds.extents.z;

        BodyInertiaInverseTensor.setMELEMENT(0, 0, 12f / (Mass * (halfY * halfY + halfZ * halfZ)));
        BodyInertiaInverseTensor.setMELEMENT(1, 1, 12f / (Mass * (halfX * halfX + halfZ * halfZ)));
        BodyInertiaInverseTensor.setMELEMENT(2, 2, 12f / (Mass * (halfX * halfX + halfY * halfY)));

    }

    // Update is called once per frame
    void Update()
    {

       

    }

    private void FixedUpdate()
    {
        ns_position = transform.position;
        //SetVariables(); //update to next time data , not initialising 

        //ComputeForces(); //calculate the force 
        //Integrate(); //find out the next time step 
        //SetPosition(position);
        //SetRotation(orientation); 
    }

    // RIGIDBODY FUNCTIONS 
    public void SetVariables()
    {
        position = ns_position;
        //m_linearAcceleration = ns_m_linearAcceleration;
        //position = transform.position;
        orientation = ns_orientation;
        m_linearVelocity = ns_m_linearVelocity;
        m_AngularVelocity = ns_m_AngularVelocity;
        Torque = ns_Torque;
        AngularMomentum = ns_AngularMomentum;

        //LinearMomentum = ns_LinearMomentum;
        m_AngularAccelration = ns_AngularMomentum;
        WorldInertiaInverseTensor = ns_WorldInertiaInverseTensor;


    }
    public float Gravity = -9.8f;  //follows Physics.gravity 
    private float rho = 1.2f; // Density of air. 
    private float C_d = 1.05f;  // Drag for ball is 0.47. Cube is 1.05
    private float A;// Surface area for cube is 6a^2
    public float Kda = 0.01f;
    public void ComputeForces()
    {
        this.forces = Vector3.zero; //clear forces 
        this.Torque = Vector3.zero; //clear torque 
        //Debug.Log(Physics.gravity); 
        A = GetComponent<Renderer>().bounds.size.x * GetComponent<Renderer>().bounds.size.y;
        if (gameObject.tag != "Stationary")
        {
            this.forces += new Vector3(0, Mass * Gravity, 0);

            /*
         * Drag will determine how air resistance would be 
         */
           // Debug.Log(0.5f * 1.2f * 0.47f * m_AngularVelocity.x * m_AngularVelocity.x);
            //this.forces += ((Mathf.Pow(m_linearVelocity.magnitude, 2) * Drag) * -m_linearVelocity);
            if (Drag == 0) //if  user does not define drag 
            {
                Drag = 0.00001f; //default to a small value 
            }
            //damping / drag force 

            this.forces.x += 0.5f * rho * C_d * A * m_linearVelocity.x * m_linearVelocity.x * Drag;
            this.forces.y += 0.5f * rho * C_d * A * m_linearVelocity.y * m_linearVelocity.y * Drag;
            this.forces.z += 0.5f * rho * C_d * A * m_linearVelocity.z * m_linearVelocity.z * Drag;

            this.Torque += -Kda * m_AngularVelocity;

        }




    }

    //TODO: cross linear and rotational 
    public void Integrate()
    {
        //linear
        //set position first 
        ns_position = position + m_linearVelocity * Time.fixedDeltaTime;
        //set the velocity 
        ns_m_linearVelocity = m_linearVelocity + inv_mass * this.forces * Time.fixedDeltaTime;

        //set the acceleration 
        //ns_m_linearAcceleration += forces * inv_mass;



        //rotational 

        GK.Matrix3f skewedAngurlar = new GK.Matrix3f();
        skewedAngurlar = skewedAngurlar.SkewSymmetry(m_AngularVelocity);

        ns_orientation = orientation + skewedAngurlar * Time.fixedDeltaTime * orientation;
        Debug.Log("Orientation");
        ns_orientation.print();

        ns_AngularMomentum = AngularMomentum + Time.fixedDeltaTime * Torque;

        GK.Matrix3f transposedOrientation = new GK.Matrix3f();
        transposedOrientation = ns_orientation.Transposed();
        ns_WorldInertiaInverseTensor = (ns_orientation * BodyInertiaInverseTensor) * transposedOrientation;
        Debug.Log("WorldInertia");
        ns_WorldInertiaInverseTensor.print();
        ns_orientation.OrthonormalizeOrientation();

        ns_m_AngularVelocity = ns_WorldInertiaInverseTensor * ns_AngularMomentum;



    }

    // returns diagonal of InertiaTensorMatrix
    public Vector3 GetInertiaTensor()
    {
        return new Vector3(BodyInertiaInverseTensor.getElement(0, 0), BodyInertiaInverseTensor.getElement(1, 1), BodyInertiaInverseTensor.getElement(2, 2));
    }
    Quaternion normalise(Quaternion q)
    {
        Quaternion result = new Quaternion();
        float distance = Mathf.Sqrt(Mathf.Pow(q.w, q.w) + Mathf.Pow(q.x, q.x) + Mathf.Pow(q.y, q.y) + Mathf.Pow(q.z, q.z));
        result = new Quaternion(q.x * distance, q.y * distance, q.z * distance, q.w * distance);
        return result;


    }

    Quaternion FloatTimesQuat(float a, Quaternion q)
    {
        q.x *= a;
        q.y *= a;
        q.z *= a;
        q.w *= a;
        return q;
    }

    //RK4 implementation 
    //tutorial, set acceleration as a damping system 
    Vector3 SetAccelration(Vector3 stateP, Vector3 stateV, float t)
    {
        float k = 15.0f;
        float b = 0.1f;
        return -k * stateP - b * stateV;
    }

    //evaluates the output velocity and acceleartion 
    List<Vector3> Evaluate(Vector3 initialV, Vector3 initialP, float t, float dt, Vector3 derivedV, Vector3 derivedA)
    {
        List<Vector3> derivative = new List<Vector3>();
        Vector3 stateP = initialP + derivedV * dt;
        Vector3 stateV = initialV + derivedV * dt;

        //Derivative output;
        //output.dx = state.v;
        //output.dv = acceleration(state, t + dt);
        //return output;

        derivative.Add(stateV);
        derivative.Add(SetAccelration(stateP, stateV, t + dt));

        return derivative;


    }

    //get and set functions 
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetRotation(GK.Matrix3f Orientation)
    {
        
        float sy = Mathf.Sqrt(Orientation.getElement(0, 0) * Orientation.getElement(0, 0) + Orientation.getElement(1, 0) * Orientation.getElement(1, 0));

        bool singular = sy < 1e-6; // If

        float x, y, z;
        if (!singular)
        {
            x = Mathf.Atan2(Orientation.getElement(2, 1), Orientation.getElement(2, 2));
            y = Mathf.Atan2(-Orientation.getElement(2, 0), sy);
            z = Mathf.Atan2(Orientation.getElement(1, 0), Orientation.getElement(0, 0));
        }
        else
        {
            x = Mathf.Atan2(-Orientation.getElement(1, 2), Orientation.getElement(1, 1));
            y = Mathf.Atan2(-Orientation.getElement(2, 0), sy);
            z = 0;
        }
        Debug.Log("euler" + new Vector3(x, y, z));
        transform.Rotate(new Vector3(x, y, z));
    }

    public Quaternion GetRotation()
    {
        return transform.rotation;
    }
}
