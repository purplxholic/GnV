using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GK
{
	public class MHit
	{
		public float t;
		public Vector3 n;
		public MHit ()
		{
			t = float.PositiveInfinity;
		}
		public MHit(Vector3 normal, float tmin){
			t = tmin;
			n = normal;
		}
		public void set( float _t,  Vector3 _n )
		{
			t = _t;
			n = _n;
		}
	}
}

