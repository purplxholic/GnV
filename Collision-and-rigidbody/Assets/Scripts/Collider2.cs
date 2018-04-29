using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GK{
	public class Collider2 : MonoBehaviour {
		GameObject go;
		RigidBody grb,trb;
		Mesh mesh;
		Vector3 collisionpoint;
		Vector3 hitNormal;
		float minHitDis;
		//		Bounds b1;
		int collisionptidx;
		Vector3[] Vertices;
		bool collisiondetected;
        Bounds b1;
        Vector3 impactpt;



        // Use this for initialization
        void Start () {
			go = gameObject;
			grb = go.GetComponent<RigidBody> (); 
			//grb = go.GetComponent<RigidBody> ();
			mesh = go.GetComponent<MeshFilter>().mesh;

//			Vertices = mesh.vertices;
			minHitDis = (float)double.PositiveInfinity;
			collisiondetected = false;
//			Color[] colors = new Color[Vertices.Length];
//
//			for (int i = 0; i < Vertices.Length; i++)
//				colors[i] = Color.Lerp(Color.red, Color.green, Vertices[i].y);
//			mesh.colors = colors;
			ChangeColor (go);



		}

		// Update is called once per frame
		void FixedUpdate () {
            grb.SetVariables(); //update to next time data , not initialising 

            grb.ComputeForces(); //calculate the force 
            grb.Integrate(); //find out the next time step 
            grb.SetPosition(grb.position);
            grb.SetRotation(grb.orientation);
            Vertices = grb.gameObject.GetComponent<MeshFilter> ().mesh.vertices;
			//print ("transformposition of rigidbody"+grb.transform.position);

			GameObject target = hitAt ();

			if (target != null) {
                //				Debug.Log (target.name);
                Rigidbody trb = target.GetComponent<Rigidbody>();
                ChangeColor (target);
				higherThan (target);
				//				print ("passing through:"+BoundaryIntersect (go, target));
				intersect (target);
				//				if(){grb.Sleep ();}

				if ( collisiondetected &&minHitDis<1f) {
					//print (collisionpoint+"collision detected"+hitNormal);
					CollisionResponce (grb, trb,collisionpoint,hitNormal);
				}

				Resetcol ();

			}

		}
		void Resetcol(){
			collisiondetected=false;
			minHitDis = (float)double.PositiveInfinity;

		}
		public GameObject hitAt(){
			Vector3 goVelocity = grb.m_linearVelocity; 
			Ray ray = new Ray (grb.transform.position, goVelocity);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100.0f)) {
				Transform prefab = hit.transform;
				//			hitNormal = hit.normal;
				GameObject tobehitgo = prefab.gameObject;//it will be the overall object
				return tobehitgo;
			} else
				return null;	
		}
		//this doesnt work for slanted
		public void higherThan(GameObject go2){
			Bounds b1 =  go.GetComponent<Renderer>().bounds;
			Bounds b2 =go2.GetComponent<Renderer>().bounds;

			if (b1.min.y< b2.max.y) {
				//grb.stop = true; 
				//grb.UseGravity = false;
//				Debug.Log ("gravity disabled"); 
				//grb.forces -= grb.ns_forces;
				grb.ns_position = new Vector3 (grb.position.x, grb.position.y+(b2.max.y - b1.min.y), grb.position.z);


			}

		}
		public void vertexhitAt(Vector3 v, int i){
			Vector3 goVelocity = grb.m_linearVelocity; 
			Ray ray = new Ray (v, goVelocity);
			RaycastHit hit;
			//			print ("v: "+v);
			if (Physics.Raycast (ray, out hit, 100.0f)) {
				//print (i + " is" + v);
				//				//print ("to collide");
				Transform prefab = hit.transform;
				Vector3 hitN = hit.normal;
                impactpt = hit.point; 
				float hitdistance = hit.distance;
				//print ("hitdistance"+hitdistance);


				if (hitdistance < 0.5F) {
                    float relativeVelocity = Vector3.Dot(hitN, grb.m_linearVelocity);
                    //					if (relativeVelocity < 0) {
                    if (hitdistance < minHitDis) {
							minHitDis = hitdistance;
							//print ("hitdistance:"+hitdistance+"hitnormal"+hitN);
							collisionpoint = v;
							hitNormal = hitN;
							collisionptidx = i;
							collisiondetected = true;
//							//print ("smaller"+v);

						}
//					}




				}

			} 
		}
		public void ChangeColor(GameObject target){
			target.GetComponent<MeshRenderer> ().material.color = Color.red;

		}


		public void CollisionResponce(RigidBody grb,Rigidbody trb, Vector3 v,Vector3 hitNormal, float e=0.5F) {
			//coefficient of restitution: ratio of speed after/before

			Vector3 targetNormal = hitNormal.normalized;
            Vector3 n = hitNormal.normalized;
            //Debug.Log("targetnormal");
            //Debug.Log(targetNormal);
            //Vector3 ra = v - grb.position;
            //Debug.Log("ra");
            //Debug.Log(ra);
            //Vector3 Velocity = grb.m_linearVelocity + Vector3.Cross(grb.m_AngularVelocity, ra);
            //Debug.Log("velocity");
            //Debug.Log(Velocity);
            //float ImpulseNumerator = -(1.0f + e) * Vector3.Dot(Velocity, targetNormal);
            //Debug.Log("ImpulseNumerator");
            //Debug.Log(ImpulseNumerator);
            //float ImpulseDenominator = grb.inv_mass + Vector3.Dot(Vector3.Cross(grb.WorldInertiaInverseTensor * Vector3.Cross(ra, targetNormal), ra), hitNormal);
            //Debug.Log("ImpulseDenominator");
            //Debug.Log(ImpulseDenominator);
            //Vector3 J = (ImpulseNumerator * 1.0f / ImpulseDenominator * 1.0f) * targetNormal;
            //Debug.Log("J");
            //Debug.Log(J);
            //grb.m_linearVelocity += grb.inv_mass * J;
            //grb.AngularMomentum += Vector3.Cross(ra, J);

            //grb.m_AngularVelocity = grb.WorldInertiaInverseTensor * grb.AngularMomentum;

            float ma = grb.Mass;
            float mb = trb.mass;

            Vector3 vai = grb.m_linearVelocity;
            Vector3 vbi = trb.velocity;

            Vector3 wai = grb.m_AngularVelocity;
            Vector3 wbi = trb.angularVelocity;

            Vector3 ita = grb.GetInertiaTensor();


            Matrix3f IaInverse = grb.WorldInertiaInverseTensor;
            //need to change, this is point of impact
            Vector3 ra = (v - grb.ns_position);

            Vector3 itb = trb.inertiaTensor;
            Matrix3f Ib = new Matrix3f(itb);
            Matrix3f IbInverse = Ib.inverse();
            Vector3 rb = v - trb.worldCenterOfMass;

            Vector3 normal =  - n.normalized;
            Vector3 angularVelChangea = Vector3.Cross(normal, ra); // start calculating the change in angular rotation of a
            Vector3 vaLinDueToR = Vector3.Cross(IaInverse * angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
            Vector3 angularVelChangeb = Vector3.Cross(normal, rb);
            Vector3 vbLinDueToR = Vector3.Cross(IbInverse * angularVelChangeb, rb);
            // calculate the linear velocity of collision point on a due to rotation of a
            float scalar = 1 / ma + 1 / mb + Vector3.Dot(vaLinDueToR, normal) + Vector3.Dot(vbLinDueToR, normal);


            float Jmod = -(e + 1) * (vai).magnitude / scalar;
            Vector3 J = normal * Jmod;
            Vector3 vaf = vai - J * grb.inv_mass;
            grb.m_linearVelocity = vaf;
            trb.velocity = -vbi + J * (1 / mb);
            Vector3 deltawa = IaInverse * Vector3.Cross(J, ra);
            Vector3 deltawb = IbInverse * Vector3.Cross(J, rb);
            grb.m_AngularVelocity = wai - deltawa;
            trb.angularVelocity = wbi - deltawb;

            //float ma = grb.Mass;
            //float mb = trb.Mass;

            //Vector3 vai = grb.m_linearVelocity;
            //Vector3 vbi = trb.m_linearVelocity;

            //Vector3 wai = grb.m_AngularVelocity;
            //Vector3 wbi = trb.m_AngularVelocity;

            //Vector3 ita = grb.GetInertiaTensor();


            //Matrix3f IaInverse = grb.WorldInertiaInverseTensor;
            ////need to change, this is point of impact
            //Vector3 ra = (v - grb.ns_position);

            ////Vector3 itb = trb.World;
            ////Matrix3f Ib = new Matrix3f(itb);
            //Matrix3f IbInverse = trb.WorldInertiaInverseTensor;
            //Vector3 rb = v - trb.ns_position;

            //Vector3 normal = n.normalized;
            //Vector3 angularVelChangea = Vector3.Cross(normal, ra); // start calculating the change in angular rotation of a
            //Vector3 vaLinDueToR = Vector3.Cross(IaInverse * angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
            //Vector3 angularVelChangeb = Vector3.Cross(normal, rb);
            //Vector3 vbLinDueToR = Vector3.Cross(IbInverse * angularVelChangeb, rb);
            //// calculate the linear velocity of collision point on a due to rotation of a
            //float scalar = 1 / ma + 1 / mb + Vector3.Dot(vaLinDueToR, normal) + Vector3.Dot(vbLinDueToR, normal);


            //float Jmod = -(e + 1) * (vai).magnitude / scalar;
            //Vector3 J = normal * Jmod;
            //Vector3 vaf = vai - J * grb.inv_mass;
            //grb.m_linearVelocity = vaf;
            //trb.m_linearVelocity = -vbi + J * (1 / mb);
            //Vector3 deltawa = IaInverse * Vector3.Cross(J, ra);
            //Vector3 deltawb = IbInverse * Vector3.Cross(J, rb);
            //grb.m_AngularVelocity = wai - deltawa;
            //trb.m_AngularVelocity = wbi - deltawb;

        }
        //do pairwise boundary checking or use octree to check through the whole scene(octree for 
        public bool BoundaryIntersect(GameObject go2){
			Bounds b1 =  go.GetComponent<Renderer>().bounds;
			Bounds b2 =go2.GetComponent<Renderer>().bounds;
			//incase the bounds falls through
			b1.Expand(0.05f);
			//			print (b2.center);
			//need to code this out 
			return b1.Intersects(b2);
		}
		//vertices on triangles
		public void intersect(GameObject go2){
			//check for AABB first, 
			if (BoundaryIntersect (go2)) {
				//				MeshFilter filter2 = go2.GetComponent<MeshFilter>();//target is a plane
				//				Mesh mesh2 = filter2.mesh;
				//				Vector3[] targetNormals = mesh2.normals;
				//				int[] targetTriangles = mesh2.triangles;
				//each indivisual vertice shoot an ray given velocity, assume it is a large area wall h
				for(int i=0;i< Vertices.Length;i++){
					Vector3 v = transform.TransformPoint (Vertices [i]);

//					print (Vertices [i]);

					vertexhitAt (v,i);


				}

			}

			//then check the vertices of the triangle/wall
			//from the triangle, get the normal, get the distance to vertices
			//the closest one will collide
			//initially can use bounds intersect ray
		}

	}
}