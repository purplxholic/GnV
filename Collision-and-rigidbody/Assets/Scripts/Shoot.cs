using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace GK {
	public class Shoot : MonoBehaviour {

		public GameObject Projectile; 
		public float MinDelay = 0.25f;
		public float InitialSpeed = 10.0f;
		public Transform SpawnLocation;
		public Vector3 hitNormal;

		float lastShot = -1000.0f;

		void Update() {
			var shooting = CrossPlatformInputManager.GetButton("Fire1");
			if (shooting) {
				if (Time.time - lastShot >= MinDelay) {
					lastShot = Time.time;

					var go = Instantiate(Projectile, SpawnLocation.position, SpawnLocation.rotation);
					Rigidbody grb = go.GetComponent<Rigidbody> ();





					grb.velocity = InitialSpeed * Camera.main.transform.forward;
					GameObject target = hitAt (go);
					if (target != null) {
						Debug.Log (target.name);
						Rigidbody trb=target.GetComponent<Rigidbody>();
						ChangeColor (target);
						//float e,float ma,float mb, Matrix3x3 Ia,Matrix3x3 Ib,Vector3 ra,Vector3 rb,Vector3 n,
						//Vector3 vai, Vector3 vbi, Vector3 wai, Vector3 wbi, Vector3 vaf, Vector3 vbf, Vector3 waf, Vector3 wbf
						//find the meshes of the target --> get triangle vertices
						//check bounds first? go the closest vertices 
						//check 
						//					trb.inertiaTensor
						//asume the target is a wall, so the collison normal is the normal to the wall
						//stage one: test out the bounds first
						//stage two: use probabaly recursive search on AABB tree(?)
						if (BoundaryIntersect (go, target)) {
							CollisionResponce (grb,trb);


						}
						
					}






//					
				}
			} 
		}
		public bool BoundaryIntersect(GameObject go1, GameObject go2){
			Bounds b1 =  go1.GetComponent<Renderer>().bounds;
			Bounds b2 =go2.GetComponent<Renderer>().bounds;

			//need to code this out 
			return b1.Intersects(b2);
		}
		public void CheckforCollision(){
			
		}
		public GameObject hitAt(GameObject go){
					Rigidbody rb = go.GetComponent<Rigidbody>();
					Vector3 goVelocity = rb.velocity; 
					Ray ray = new Ray (go.transform.position, goVelocity);
					RaycastHit hit;
					if (Physics.Raycast (ray, out hit, 100.0f)) {
						Transform prefab = hit.transform;
						hitNormal = hit.normal;
						GameObject tobehitgo = prefab.gameObject;//it will be the overall object
						return tobehitgo;
					} else
						return null;	
				}
		public void ChangeColor(GameObject target){
					target.GetComponent<MeshRenderer> ().material.color = Color.blue;
					print ("change color");
				}

//		public void CollisionResponce(Rigidbody grb,float e=0.5F) {
//			//coefficient of restitution: ratio of speed after/before
//
//
//			//
//			//			vector_3 Velocity = Configuration.CMVelocity +
//			//				CrossProduct(Configuration.AngularVelocity,R);
//			//
//			//			real ImpulseNumerator = -(r(1) + Body.CoefficientOfRestitution) *
//			//				DotProduct(Velocity,CollisionNormal);
//			//
//			//			real ImpulseDenominator = Body.OneOverMass +
//			//				DotProduct(CrossProduct(Configuration.InverseWorldInertiaTensor *
//			//					CrossProduct(R,CollisionNormal),R),
//			//					CollisionNormal);
//			//
//			//			vector_3 Impulse = (ImpulseNumerator/ImpulseDenominator) * CollisionNormal;
//			//
//			//			// apply impulse to primary quantities
//			//			Configuration.CMVelocity += Body.OneOverMass * Impulse;
//			//			Configuration.AngularMomentum += CrossProduct(R,Impulse);
//			//
//			//			// compute affected auxiliary quantities
//			//			Configuration.AngularVelocity = Configuration.InverseWorldInertiaTensor *
//			//				Configuration.AngularMomentum;
//
//			float ma = grb.mass;
//			//manually set first
//
//			Vector3 n = hitNormal;
//
//			//			grb.velocity =Vector3.zero;
//			//			grb.angularVelocity = Vector3.zero;
//			//			grb.Sleep ();
//
//			//			print (grb.velocity);
//
//			Vector3 vai = grb.velocity;
//
//			Vector3 wai = grb.angularVelocity;
//
//			Vector3 ita = grb.inertiaTensor;
//
//			Quaternion itra = grb.inertiaTensorRotation;
//
//			Matrix3f Qa = Matrix3f.rotation (grb.inertiaTensorRotation);
//			Matrix3f QaT = Qa.Transposed ();
//
//
//			Matrix3f Ia = new Matrix3f (ita);
//			//			print (Aa+" ita:"+ita+" itra:"+itra+" Qa:"+Qa+" QaT:"+QaT);
//			//			Matrix3f Ia = Qa * Aa * QaT;
//			Ia.print();
//			Matrix3f IaInverse = Ia.inverse();
//			print ("Pos:"+grb.position+" CM:" + grb.centerOfMass);
//
//			Vector3 ra = grb.position - grb.worldCenterOfMass;
//			Vector3 velocity = vai + Vector3.Cross (wai, ra);
//			float impulsenumerator = -(e + 1) * Vector3.Dot (velocity, n);
//			float impulsedenominator = 1 / ma + Vector3.Dot (Vector3.Cross (IaInverse * Vector3.Cross (ra, n), ra), n);
//			Vector3 J = (impulsenumerator / impulsedenominator) * n;
//			Vector3 vaii = grb.velocity;
//			print ((1 / ma) * J);
//			grb.velocity += (1/ma)*J;
//			grb.angularVelocity = IaInverse * Vector3.Cross (ra,J);
//			Debug.Log ("initial is"+vaii+"final is"+grb.velocity);
//
//
//
//
//			////			IaInverse.print ();
//			//		Vector3 normal = n.normalized;
//			//		Vector3 angularVelChangea  = normal; // start calculating the change in abgular rotation of a
//			//		angularVelChangea=Vector3.Cross(angularVelChangea,ra);
//			//
//			//		Vector3 vaLinDueToR = Vector3.Cross(IaInverse*angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
//			//		float scalar = 1/ma + Vector3.Dot(vaLinDueToR,normal);
//			//
//			//
//			//		float Jmod = -(e+1)*(vai).magnitude/scalar;
//			//		Vector3 J = normal*Jmod;
//			//			Vector3 vaf = vai - J*(1/ma);
//			//			grb.velocity = vaf;
//			//			Debug.Log ("initial is"+vai+"final is"+vaf);
//			//		grb.angularVelocity = wai - angularVelChangea;
//		}
		        public void CollisionResponce(Rigidbody grb, Rigidbody trb,float e=0.5F) {
		            
		            float mb = trb.mass;
		            float ma = grb.mass;
		            //manually set first
			Vector3 n=hitNormal;

		
		            Vector3 vai = grb.velocity;
		            Vector3 vbi = trb.velocity;
		            Vector3 wai = grb.angularVelocity;
		            Vector3 wbi = trb.angularVelocity;
		            Vector3 ita = grb.inertiaTensor;
		            Vector3 itb = trb.inertiaTensor;
			Matrix3f Qa = Matrix3f.rotation (grb.inertiaTensorRotation);
			Matrix3f QaT = Qa.Transposed ();
			Matrix3f Qb = Matrix3f.rotation (trb.inertiaTensorRotation);
			Matrix3f QbT = Qb.Transposed ();
		            Matrix3f Aa = new Matrix3f (ita);
		            Matrix3f Ab = new Matrix3f (itb);
			Matrix3f Ia = Qa * Aa * QaT;
			Matrix3f Ib = Qb * Ab * QbT;
			Vector3 ra = grb.position - grb.worldCenterOfMass;
			Vector3 rb = trb.position - trb.worldCenterOfMass;
		
		            Matrix3f IaInverse = Ia.inverse();
		            Vector3 normal = n.normalized;
		            Vector3 angularVelChangea  = normal; // start calculating the change in abgular rotation of a
		            angularVelChangea=Vector3.Cross(angularVelChangea,ra);
		
		            Vector3 vaLinDueToR = Vector3.Cross(IaInverse*angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
		            float scalar = 1/ma + Vector3.Dot(vaLinDueToR,normal);
		            Matrix3f IbInverse = Ib.inverse();
		            Vector3 angularVelChangeb = normal; // start calculating the change in abgular rotation of b
		            angularVelChangeb=Vector3.Cross(angularVelChangeb, rb);
		
		            Vector3 vbLinDueToR = Vector3.Cross(IbInverse*angularVelChangeb, rb); // calculate the linear velocity of collision point on b due to rotation of b
		            scalar += 1/mb +Vector3.Dot(vbLinDueToR,normal);
		            float Jmod = (e+1)*(vai-vbi).magnitude/scalar;
		            Vector3 J = normal*Jmod;
		            Vector3 vaf = vai - J * (1 / ma);
		            grb.velocity = vaf;
		            Debug.Log ("initial is"+vai+"final is"+vaf);
		            trb.velocity = vbi - J*(1/mb);
		            grb.angularVelocity = wai - angularVelChangea;
		            trb.angularVelocity = wbi - angularVelChangeb;
		        }



//		public void CollisionResponce(float e,float ma,float mb, Matrix3x3 Ia,Matrix3x3 Ib,Vector3 ra,Vector3 rb,Vector3 n,
//			Vector3 vai, Vector3 vbi, Vector3 wai, Vector3 wbi, Vector3 vaf, Vector3 vbf, Vector3 waf, Vector3 wbf) {
//			Matrix3x3 IaInverse = Ia.inverse();
//			Vector3 normal = n.normalized;
//			Vector3 angularVelChangea  = normal; // start calculating the change in abgular rotation of a
//			angularVelChangea=Vector3.Cross(angularVelChangea,ra);
//
//			Vector3 vaLinDueToR = Vector3.Cross(IaInverse*angularVelChangea, ra);  // calculate the linear velocity of collision point on a due to rotation of a
//			float scalar = 1/ma + Vector3.Dot(vaLinDueToR,normal);
//			Matrix3x3 IbInverse = Ib.inverse();
//			Vector3 angularVelChangeb = normal; // start calculating the change in abgular rotation of b
//			angularVelChangeb=Vector3.Cross(angularVelChangeb, rb);
//
//			Vector3 vbLinDueToR = Vector3.Cross(IbInverse*angularVelChangeb, rb); // calculate the linear velocity of collision point on b due to rotation of b
//			scalar += 1/mb +Vector3.Dot(vbLinDueToR,normal);
//			float Jmod = (e+1)*(vai-vbi).magnitude/scalar;
//			Vector3 J = normal*Jmod;
//			vaf = vai - J*(1/ma);
//			vbf = vbi - J*(1/mb);
//			waf = wai - angularVelChangea;
//			wbf = wbi - angularVelChangeb;
//		}
	}
}
