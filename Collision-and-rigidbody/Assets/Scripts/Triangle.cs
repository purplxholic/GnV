using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GK
{
	public class Triangle
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		Vector3 a,b,c;

		public Triangle (Vector3[] m_vertices, Vector3[] m_normal)
		{
			vertices = m_vertices;
			a = vertices [0];
			b = vertices [1];
			c = vertices [2];
			normals = m_normal;


		}
		public bool intersect(MRay ray,  MHit hit , float tmin){
			Vector3 R0=ray.origin;
			Vector3 Rd=ray.direction;
			Vector3 e1=b-a;
			Vector3 e2=c-a;
			//R0-a=beta*e1+gamma*e2-t*Rd
			Matrix3f A= new Matrix3f(-e1,-e2,Rd,true);
			Matrix3f betaM= new Matrix3f(a-R0,-e2,Rd,true);
			Matrix3f gammaM= new Matrix3f(-e1,a-R0,Rd,true);
			Matrix3f tM= new Matrix3f(-e1,-e2,a-R0,true);
			float beta=betaM.determinant()/A.determinant();
			float gamma=gammaM.determinant()/A.determinant();
			float result_t=tM.determinant()/A.determinant();
			float alpha=1.0f-beta-gamma;
			if (beta+gamma>1||beta<0 ||gamma<0){return false;}
			if(result_t>=tmin && result_t< hit.t){
				Vector3 newNormal = (alpha*normals[0] + beta*normals[1] + gamma*normals[2]).normalized;
				hit.set(result_t, newNormal);
				return true;
			}
				
			return false;
		}
	}
}

