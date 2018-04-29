using System;
using System.Collections.Generic;
using UnityEngine;
namespace GK{
	public class Matrix3f
	{
		public float[] m_elements=new float[9];
		//INITIALISE THE MATRIX
		public Matrix3f(float fill = 0.0f)
		{

			for (int i = 0; i < 9; ++i)
			{

				m_elements[i] = fill;
			}

		}
		public Matrix3f(float m00, float m01, float m02,
			float m10, float m11, float m12,
			float m20, float m21, float m22)
		{

			m_elements[0] = m00;
			m_elements[1] = m10;
			m_elements[2] = m20;
			m_elements[3] = m01;
			m_elements[4] = m11;
			m_elements[5] = m21;

			m_elements[6] = m02;
			m_elements[7] = m12;
			m_elements[8] = m22;
		}

		public Matrix3f( Matrix3f rm )
		{

			for (int i = 0; i < 9; i++)
			{
				m_elements[i] = rm.getElement(i);
			}

		}
		public Matrix3f( Vector3 v)
		{

			for( int i = 0; i < 9; ++i )
			{

				m_elements[ i ] = 0;
			}
			m_elements [0] = v.x;
			m_elements [4] = v.y;
			m_elements [8] = v.z;


		}

		public float[] getElements(){
			return m_elements;
		}
		public float getElement(int i ){
			return m_elements[i];
		}
		public float getElement(int i,int j ){
			return m_elements[j*3+i];
		}

		//OPERATORS
		//returns the actual position of the matrix value in a float 
		public int GetIndex (int i, int j)
		{
			return j * 3 + i;
		}
		//Sets the value of the element at (i,J) index
		public void setMELEMENT(int i,int j, float value)
		{
			m_elements[GetIndex(i, j)] = value;
		}
		public static Vector3 operator *(Matrix3f m, Vector3 v)
		{
			Vector3 output = new Vector3();

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					output[i] += m.getElement(i,j)* v[j];
				}
			}

			return output;
		}       // static
		public static float determinant2x2( float m00, float m01,
			float m10, float m11 )
		{
			return( m00 * m11 - m01 * m10 );
		}
		//
		// static
		public static float determinant3x3( float m00, float m01, float m02,
			float m10, float m11, float m12,
			float m20, float m21, float m22 )
		{
			return
				(
					m00 * ( m11 * m22 - m12 * m21 )
					- m01 * ( m10 * m22 - m12 * m20 )
					+ m02 * ( m10 * m21 - m11 * m20 )
				);
		}

		public float determinant()
		{
			return determinant3x3
				(
					m_elements[ 0 ], m_elements[ 3 ], m_elements[ 6 ],
					m_elements[ 1 ], m_elements[ 4 ], m_elements[ 7 ],
					m_elements[ 2 ], m_elements[ 5 ], m_elements[ 8 ]
				);
		}


		public Matrix3f inverse(float epsilon=0.0001F )
		{

			float m00 = m_elements[ 0 ];
			float m10 = m_elements[ 1 ];
			float m20 = m_elements[ 2 ];

			float m01 = m_elements[ 3 ];
			float m11 = m_elements[ 4 ];
			float m21 = m_elements[ 5 ];

			float m02 = m_elements[ 6 ];
			float m12 = m_elements[ 7 ];
			float m22 = m_elements[ 8 ];

			float cofactor00 =  determinant2x2( m11, m12, m21, m22 );
			float cofactor01 = -determinant2x2( m10, m12, m20, m22 );
			float cofactor02 =  determinant2x2( m10, m11, m20, m21 );

			float cofactor10 = -determinant2x2( m01, m02, m21, m22 );
			float cofactor11 =  determinant2x2( m00, m02, m20, m22 );
			float cofactor12 = -determinant2x2( m00, m01, m20, m21 );

			float cofactor20 =  determinant2x2( m01, m02, m11, m12 );
			float cofactor21 = -determinant2x2( m00, m02, m10, m12 );
			float cofactor22 =  determinant2x2( m00, m01, m10, m11 );

			float determinant = m00 * cofactor00 + m01 * cofactor01 + m02 * cofactor02;

			bool isSingular = ( Math.Abs( determinant ) < epsilon );
			if( isSingular )
			{
				//              if( pbIsSingular != null )
				//              {
				//                  pbIsSingular = true;
				//              }
				return new Matrix3f();
			}
			else
			{
				//              if( pbIsSingular != null )
				//              {
				//                  pbIsSingular = false;
				//              }

				float reciprocalDeterminant = 1.0f / determinant;

				return new Matrix3f
					(
						cofactor00 * reciprocalDeterminant, cofactor10 * reciprocalDeterminant, cofactor20 * reciprocalDeterminant,
						cofactor01 * reciprocalDeterminant, cofactor11 * reciprocalDeterminant, cofactor21 * reciprocalDeterminant,
						cofactor02 * reciprocalDeterminant, cofactor12 * reciprocalDeterminant, cofactor22 * reciprocalDeterminant
					);
			}
		}


		public static Matrix3f operator *(Matrix3f x, Matrix3f y)
		{
			Matrix3f product = new Matrix3f(); // zeroes

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					for (int k = 0; k < 3; ++k)
					{

						product.m_elements[product.GetIndex(i,k)] += (x.getElement(i,j ) * y.getElement(j,k )); 
						//product.setElement(i, k, product.getElement(i, k) + x.getElement(i, j) * y.getElement(j, k));
					}
				}
			}

			return product;
		}
		public static Matrix3f operator +(Matrix3f x, Matrix3f y)
		{
			Matrix3f product = new Matrix3f(); // zeroes

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{


					product.m_elements[product.GetIndex(i, j)] = (x.getElement(i, j) + y.getElement(i,j));
					//product.setElement(i, k, product.getElement(i, k) + x.getElement(i, j) * y.getElement(j, k));

				}
			}

			return product;
		}

		public static Matrix3f operator *(Matrix3f x, float y)
		{
			Matrix3f product = new Matrix3f(); // zeroes


			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					for (int k = 0; k < 3; ++k)
					{

						product.m_elements[product.GetIndex(i,k)] *= y;
						//product.setElement(i, k, product.getElement(i, k) + x.getElement(i, j) * y.getElement(j, k));
					}
				}
			}

			return product;
		}

		// FUNCTIONS
		Vector3 GetRow(int i)
		{
			return new Vector3
				(
					m_elements[i],
					m_elements[i + 3],
					m_elements[i + 6]

				);
		}

		void SetRow(int i, Vector3 v)
		{
			m_elements[i] = v.x;
			m_elements[i + 3] = v.y;
			m_elements[i + 6] = v.z;
		}
		public Matrix3f( Vector3 v0, Vector3 v1, Vector3 v2, bool setColumns=false )
		{
			m_elements = new float[9];
			if( setColumns )
			{

				m_elements[0] = v0.x;
				m_elements[1] = v0.y;
				m_elements[2] = v0.z;
				m_elements[3] = v1.x;
				m_elements[4] = v1.y;
				m_elements[5] = v1.z;
				m_elements[6] = v2.x;
				m_elements[7] = v2.y;
				m_elements[8] = v2.z;

			}
			else
			{
				m_elements[0] = v0.x;
				m_elements[1] = v1.x;
				m_elements[2] = v2.x;
				m_elements[3] = v0.y;
				m_elements[4] = v1.y;
				m_elements[5] = v2.y;
				m_elements[6] = v0.z;
				m_elements[7] = v1.z;
				m_elements[8] = v2.z;
			}
		}
		Vector3 GetCol(int j)
		{

			int colStart = 3 * j;

			return new Vector3
				(
					m_elements[colStart],
					m_elements[colStart + 1],
					m_elements[colStart + 2]

				);
		}

		public void SetCol(int j, Vector3 v)
		{
			int colStart = 3 * j;

			m_elements[colStart] = v.x;
			m_elements[colStart + 1] = v.y;
			m_elements[colStart + 2] = v.z;
		}

		public Matrix3f Transposed()
		{
			Matrix3f m = new Matrix3f();

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{

					m.getElements()[GetIndex(j, i)] = this.getElements()[GetIndex(i, j)]; 

				}
			}

			return m;
		}

		// static
		public Matrix3f Ones()
		{
			Matrix3f m = new Matrix3f();
			for (int i = 0; i < 9; ++i)
			{
				m.getElements()[i] = 1;
			}

			return m;
		}

		public Matrix3f Identity()
		{
			Matrix3f m = new Matrix3f();

			m.getElements()[GetIndex(0, 0)]=1.0f;
			m.getElements()[GetIndex(1, 1)] = 1.0f;
			m.getElements()[GetIndex(2, 2)] = 1.0f;

			return m;
		}
		// static
		Matrix3f rotateX(float radians)
		{
			float c = Mathf.Cos(radians);
			float s = Mathf.Sin(radians);

			return new Matrix3f
				(
					1, 0, 0,
					0, c, -s,
					0, s, c
				);
		}

		// static
		Matrix3f rotateY(float radians)
		{
			float c = Mathf.Cos(radians);
			float s = Mathf.Sin(radians);

			return new Matrix3f
				(
					c, 0, s,
					0, 1, 0,
					-s, 0, c
				);
		}

		// static
		Matrix3f rotateZ(float radians)
		{
			float c = Mathf.Cos(radians);
			float s = Mathf.Sin(radians);

			return new Matrix3f
				(
					c, -s, 0,
					s, c, 0,
					0, 0, 1
				);
		}

		public Matrix3f SkewSymmetry(Vector3 v)
		{

			Matrix3f m = new Matrix3f();

			m.m_elements[GetIndex(0, 0)] = 0.0f; m.m_elements[GetIndex(0, 1)] = -v.z; m.m_elements[GetIndex(0, 2)] = v.y;
			m.m_elements[GetIndex(1, 0)] = v.z; m.m_elements[GetIndex(1, 1)] = 0.0f; m.m_elements[GetIndex(1, 2)] = -v.x;
			m.m_elements[GetIndex(2, 0)] = -v.y; m.m_elements[GetIndex(2, 1)] = v.x; m.m_elements[GetIndex(2, 2)] = 0.0f;

			return m;
		}

		public void OrthonormalizeOrientation( )
		{
			Vector3 X= new Vector3(this.m_elements[GetIndex(0,0)], this.m_elements[GetIndex(1, 0)], this.m_elements[GetIndex(2, 0)]);
			Vector3 Y = new Vector3 (this.m_elements[GetIndex(0, 1)], this.m_elements[GetIndex(1, 1)], this.m_elements[GetIndex(2, 1)]);
			Vector3 Z;

			X.Normalize();

			Z = Vector3.Cross(X, Y);
			Z.Normalize(); 
			Y = Vector3.Cross(Z, X);
			Y.Normalize(); 

			this.m_elements[GetIndex(0, 0)] = X.x; this.m_elements[GetIndex(0, 1)] = Y.x; this.m_elements[GetIndex(0, 2)] = Z.x;
			this.m_elements[GetIndex(1, 0)] = X.y; this.m_elements[GetIndex(1, 1)] = Y.y; this.m_elements[GetIndex(1, 2)] = Z.y;
			this.m_elements[GetIndex(2, 0)] = X.z; this.m_elements[GetIndex(2,1)] = Y.z; this.m_elements[GetIndex(2, 2)] = Z.z;
		}
		public void print (){
			Debug.Log(m_elements[ 0 ]+" "+m_elements[ 3 ]+" "+m_elements[ 6 ]+" "+
				m_elements[ 1 ]+" "+m_elements[ 4 ]+" "+m_elements[ 7 ]+" "+
				m_elements[ 2 ]+" "+ m_elements[ 5 ]+" "+m_elements[ 8 ] );

		}
		// static
		public static Matrix3f rotation(Quaternion rq )
		{
			//					Quaternion q = rq;
			float abs = (float)Math.Sqrt ((rq.w) * (rq.w) + (rq.x) * (rq.x) + (rq.y) * (rq.y) + (rq.z) * (rq.z));
			Quaternion q=new Quaternion(rq.x/abs,rq.y/abs,rq.z/abs,rq.w/abs);


			//need to normalize?


			float xx = q.x * q.x;
			float yy = q.y * q.y;
			float zz = q.z * q.z;

			float xy = q.x * q.y;
			float zw = q.z * q.w;

			float xz = q.x * q.z;
			float yw = q.y * q.w;

			float yz = q.y * q.z;
			float xw = q.x * q.w;

			return new Matrix3f
				(
					1.0f - 2.0f * ( yy + zz ),        2.0f * ( xy - zw ),                2.0f * ( xz + yw ),
					2.0f * ( xy + zw ),                1.0f - 2.0f * ( xx + zz ),        2.0f * ( yz - xw ),
					2.0f * ( xz - yw ),                2.0f * ( yz + xw ),                1.0f - 2.0f * ( xx + yy )
				);
		}
		public Quaternion matrixs3fToQuaternion(Matrix3f m)
		{
			Quaternion q = new Quaternion();
			float trace = m.m_elements[GetIndex(0,0)] + m.m_elements[GetIndex(1, 1)] + m.m_elements[GetIndex(2, 2)]; 
			if (trace > 0)
			{
				float s = 0.5f / Mathf.Sqrt(trace + 1.0f);
				q.w = 0.25f / s;
				q.x = (m.m_elements[GetIndex(2, 1)] - m.m_elements[GetIndex(1, 2)]) * s;
				q.y = (m.m_elements[GetIndex(0, 2)] - m.m_elements[GetIndex(2, 0)]) * s;
				q.z = (m.m_elements[GetIndex(1, 0)] - m.m_elements[GetIndex(0, 1)]) * s;
			}
			else
			{
				if (m.m_elements[GetIndex(0, 0)] > m.m_elements[GetIndex(1, 1)] && m.m_elements[GetIndex(0, 0)] > m.m_elements[GetIndex(2, 2)])
				{
					float s = 2.0f * Mathf.Sqrt(1.0f + m.m_elements[GetIndex(0, 0)] - m.m_elements[GetIndex(1, 1)] - m.m_elements[GetIndex(2, 2)]);
					q.w = (m.m_elements[GetIndex(2, 1)] - m.m_elements[GetIndex(1, 2)]) / s;
					q.x = 0.25f * s;
					q.y = (m.m_elements[GetIndex(0, 1)] + m.m_elements[GetIndex(1, 0)]) / s;
					q.z = (m.m_elements[GetIndex(0, 2)] + m.m_elements[GetIndex(2, 0)]) / s;
				}
				else if (m.m_elements[GetIndex(1, 1)] > m.m_elements[GetIndex(2, 2)])
				{
					float s = 2.0f * Mathf.Sqrt(1.0f + m.m_elements[GetIndex(1, 1)] - m.m_elements[GetIndex(0, 0)] - m.m_elements[GetIndex(2, 2)]);
					q.w = (m.m_elements[GetIndex(0, 2)] - m.m_elements[GetIndex(2, 0)]) / s;
					q.x = (m.m_elements[GetIndex(0, 1)] + m.m_elements[GetIndex(1, 0)]) / s;
					q.y = 0.25f * s;
					q.z = (m.m_elements[GetIndex(1, 2)] + m.m_elements[GetIndex(2, 1)]) / s;
				}
				else
				{
					float s = 2.0f * Mathf.Sqrt(1.0f + m.m_elements[GetIndex(2, 2)] - m.m_elements[GetIndex(0, 0)] - m.m_elements[GetIndex(1, 1)]);
					q.w = (m.m_elements[GetIndex(1, 0)] - m.m_elements[GetIndex(0, 1)]) / s;
					q.x = (m.m_elements[GetIndex(0, 2)] + m.m_elements[GetIndex(2, 0)]) / s;
					q.y = (m.m_elements[GetIndex(1, 2)] + m.m_elements[GetIndex(2, 1)]) / s;
					q.z = 0.25f * s;
				}
			}
			return q;
		}
		//code adapted from https://www.learnopencv.com/rotation-matrix-to-euler-angles/ 
		public Matrix3f QuaterniontoMatrix3(Quaternion q)
		{
			Matrix3f m = new Matrix3f();
			float sqw = q.w * q.w;
			float sqx = q.x * q.x;
			float sqy = q.y * q.y;
			float sqz = q.z * q.z;

			float inverse = 1 / (sqw + sqy + sqz + sqw);

			m.setMELEMENT(0, 0, (sqx - sqy - sqz + sqw) * inverse);
			m.setMELEMENT(1, 1, (-sqx + sqy - sqz + sqw) * inverse);
			m.setMELEMENT(2, 2, (-sqx - sqy + sqz + sqw) * inverse);

			float temp1 = q.x * q.y;
			float temp2 = q.z * q.w;

			m.setMELEMENT(1,0 , 2.0f * (temp1 + temp2) * inverse);
			m.setMELEMENT(0,1, 2.0f * (temp1 - temp2) * inverse);

			float temp3 = q.x * q.z;
			float temp4 = q.y * q.w;
			m.setMELEMENT(2,0, 2.0f * (temp3 - temp4) * inverse);
			m.setMELEMENT(0, 2, 2.0f * (temp1 + temp2) * inverse);

			float temp5 = q.y * q.z;
			float temp6 = q.x * q.w;
			m.setMELEMENT(2,1, 2.0f * (temp5 + temp6) * inverse);
			m.setMELEMENT(1,2, 2.0f * (temp5 - temp6) * inverse);

			return m; 
		}
		//	public static void Main(string[] args) {
		//			Matrix3f m = new Matrix3f ();
		//			Debug.Log ("Variable num for s1: ");
		//
		//
		//
		////
		////			Console.WriteLine("Variable num for s1: ");
		////			Console.WriteLine("Variable num for s2:");
		//
		//		}
	}

}
