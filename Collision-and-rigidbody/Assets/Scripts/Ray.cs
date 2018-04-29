using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace GK
{
	public class MRay 
	{
		public Vector3 origin;
		public Vector3 direction;
		public MRay (Vector3 morigin, Vector3 mdirection)
		{
			origin = morigin;
			direction = mdirection;
		}
		public Vector3 pointAt(float t){
			return origin + direction * t;
		}
	}
}

