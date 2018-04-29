using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK
{
	public class Collider : MonoBehaviour
	{
		GameObject go;
		Rigidbody grb,trb;
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
		void Start ()
		{
			go = gameObject;
			grb = go.GetComponent<Rigidbody> ();
			mesh = go.GetComponent<MeshFilter> ().mesh;

			
//		Vertices = mesh.vertices;
			minHitDis = (float)double.PositiveInfinity;
			collisiondetected = false;
//			Color[] colors = new Color[Vertices.Length];
//
//			for (int i = 0; i < Vertices.Length; i++)
//				colors[i] = Color.Lerp(Color.red, Color.green, Vertices[i].y);
//			mesh.colors = colors;


		
		}
	
		// Update is called once per frame
		void Update ()
		{
			Vertices = mesh.vertices;
			GameObject target = hitAt ();
			if (target != null) {
//				Debug.Log (target.name);
				Rigidbody trb=target.GetComponent<Rigidbody>();
				ChangeColor (target);
				higherThan (target);
//				print ("passing through:"+BoundaryIntersect ( target));
				intersect (target);
//				if(){grb.Sleep ();}
				if (collisiondetected && minHitDis < 1f) {
					CollisionResponce (grb, trb,collisionpoint, hitNormal);
				}

				Resetcol ();



//				if (BoundaryIntersect (go, target)) {//should be vertices intersect the wall, goal, need to find the intersection pt
//					if (hitNormal != null) {
//						CollisionResponce (grb);
//						//rest position fixed
//
////						if (grb.velocity.y<0.4) {
////							grb.Sleep ();
//////													print ("hello stop");
//////													float contactforce = 20F * grb.mass;
//////													Vector3 contact = new Vector3 (0, contactforce, 0);
//////													grb.AddForce (contact);
////						}
//
//					}
//
//				}

			}

		}

		void Resetcol ()
		{
			collisiondetected = false;
			minHitDis = (float)double.PositiveInfinity;

		}

		public GameObject hitAt ()
		{
			Vector3 goVelocity = grb.velocity; 
			Ray ray = new Ray (go.transform.position, goVelocity);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100.0f)) {
				Transform prefab = hit.transform;
				GameObject tobehitgo = prefab.gameObject;//it will be the overall object
				return tobehitgo;
			} else
				return null;	
		}
		//this doesnt work for slanted
		public void higherThan (GameObject go2)
		{
			if (hitNormal.y == 1.0f && hitNormal.x == 0.0f && hitNormal.z == 0.0f) {
				
				Bounds b2 = go2.GetComponent<Renderer> ().bounds;
				if (b1.min.y < b2.max.y) {
					grb.transform.position = new Vector3 (grb.transform.position.x, grb.transform.position.y + (b2.max.y - b1.min.y), grb.transform.position.z);
					grb.Sleep ();

				}
			}


		}

		public void vertexhitAt (Vector3 v, int i)
		{
			Vector3 goVelocity = grb.velocity; 
			Ray ray = new Ray (v, goVelocity);
			RaycastHit hit;
//			print ("v: "+v);
			if (Physics.Raycast (ray, out hit, 100.0f)) {
//				print (i + " is" + v);
//				print ("to collide");
				Transform prefab = hit.transform;
				Vector3 hitN = hit.normal;
				impactpt = hit.point;

//				int triangleindex = hit.triangleIndex;
				float hitdistance = hit.distance;
//				print (hitdistance);


				if (hitdistance < 0.5F) {
					float relativeVelocity = Vector3.Dot (hitN, grb.velocity);
//					if (relativeVelocity < 0) {
					if (hitdistance < minHitDis) {
						minHitDis = hitdistance;
//							print ("hitdistance:"+hitdistance+"hitnormal"+hitNormal);
						collisionpoint = v;
						hitNormal = hitN;
						collisionptidx = i;
						collisiondetected = true;
//							print ("smaller"+v);

//						}
					}



					
				}
	
			} 
		}

		public void ChangeColor (GameObject target)
		{
			target.GetComponent<MeshRenderer> ().material.color = Color.red;

		}


		public void CollisionResponce (Rigidbody grb, Rigidbody trb, Vector3 v, Vector3 hitNormal, float e = 0.3F)
		{
			//coefficient of restitution: ratio of speed after/before



	
			float ma = grb.mass;
			float mb = trb.mass;
			Vector3 n = hitNormal.normalized;
			Vector3 vai = grb.velocity;
			Vector3 vbi = trb.velocity;

			Vector3 wai = grb.angularVelocity;
			Vector3 wbi = trb.angularVelocity;

			Vector3 ita = grb.inertiaTensor;



			Quaternion itra = grb.inertiaTensorRotation;


			Matrix3f Qa = Matrix3f.rotation (grb.inertiaTensorRotation);
//			Matrix3f QaT = Qa.Transposed ();


			Matrix3f Ia = new Matrix3f (ita);
//			print (Aa+" ita:"+ita+" itra:"+itra+" Qa:"+Qa+" QaT:"+QaT);
//			Matrix3f Ia = Qa * Aa * QaT;
//			Ia.print();
			Matrix3f IaInverse = Ia.inverse ();

//			print ("Pos:"+v+" CM:" + grb.centerOfMass);
//			//need to change, this is point of impact
			Vector3 ra = (v - grb.worldCenterOfMass);

			Vector3 itb = trb.inertiaTensor;
			Matrix3f Ib = new Matrix3f (itb);
			Matrix3f IbInverse = Ib.inverse ();
			Vector3 rb = impactpt-trb.worldCenterOfMass;

			Vector3 normal = n.normalized;
			Vector3 angularVelChangea = Vector3.Cross (normal, ra);
			Vector3 vaLinDueToR = Vector3.Cross (IaInverse * angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
			Vector3 angularVelChangeb = Vector3.Cross (normal, rb);
			Vector3 vbLinDueToR = Vector3.Cross (IbInverse * angularVelChangeb, rb); 
//
			float scalar = 1 / ma +1/mb+ Vector3.Dot (vaLinDueToR, normal)+Vector3.Dot (vbLinDueToR, normal);


			float Jmod = -(e + 1) * (vai-vbi).magnitude / scalar;
			Vector3 J = normal * Jmod;
			Vector3 vaf = vai - J * (1 / ma);
			grb.velocity = vaf;
			trb.velocity = vbi+J *(1 / mb);
			Vector3 deltawa= IaInverse*Vector3.Cross (J, ra);
			Vector3 deltawb = IbInverse * Vector3.Cross (J, rb);
			grb.angularVelocity = wai-deltawa;
			trb.angularVelocity = wbi-deltawb;

		}
		//do pairwise boundary checking or use octree to check through the whole scene(octree for
		public bool BoundaryIntersect (GameObject go2)
		{
			b1 = go.GetComponent<Renderer> ().bounds;//bounding box of one object
			Bounds b2 = go2.GetComponent<Renderer> ().bounds;//boundingbox of another
			b1.Expand (0.05F);//expand the boundary slightly to check ahead 
			return b1.Intersects (b2);
		}
		//vertices on triangles
		public void intersect (GameObject go2)
		{
			//check for AABB first, 
			if (BoundaryIntersect (go2)) {
//				MeshFilter filter2 = go2.GetComponent<MeshFilter>();//target is a plane
//				Mesh mesh2 = filter2.mesh;
//				Vector3[] targetNormals = mesh2.normals;
//				int[] targetTriangles = mesh2.triangles;
				//each indivisual vertice shoot an ray given velocity, assume it is a large area wall h
				for (int i = 0; i < Vertices.Length; i++) {
					Vector3 v = transform.TransformPoint (Vertices [i]);

					vertexhitAt (v, i);


				}

			}

			//then check the vertices of the triangle/wall
			//from the triangle, get the normal, get the distance to vertices
			//the closest one will collide
			//initially can use bounds intersect ray
		}

	}
}