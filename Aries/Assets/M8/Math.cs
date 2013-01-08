using UnityEngine;

namespace M8 {
	public struct Math {
		public enum Side {
			None,
			Left,
			Right
		}
		
		//-------------- 2D --------------
		
		public static void Limit(ref Vector2 v, float limit) {
			float d = v.magnitude;
			if(d > limit) {
				v /= d;
				v *= limit;
			}
		}
		
		public static Vector2 Limit(Vector2 v, float limit) {
			Limit(ref v, limit);
			return v;
		}
		
		public static Vector2 Reflect(Vector2 v, Vector2 n) {
			return v - (2.0f*Vector2.Dot(v, n))*n;
		}
		
		public static Vector2 Slide(Vector2 v, Vector2 n) {
			return v - Vector2.Dot(v, n)*n;
		}
										
		public static float CheckSideSign(Vector2 up1, Vector2 up2) {
			return Cross(up1, up2) < 0 ? -1 : 1;
		}
		
		/// <summary>
		/// Checks which side up1 is in relation to up2
		/// </summary>
		public static Side CheckSide(Vector2 up1, Vector2 up2) {
			float s = Cross(up1, up2);
			return s == 0 ? Side.None : s < 0 ? Side.Right : Side.Left;
		}
		
		public static Vector2 Rotate(Vector2 v, float r) {
			float c = Mathf.Cos(r);
			float s = Mathf.Sin(r);
			
			return new Vector2(v.x*c+v.y*s, -v.x*s+v.y*c);
		}
		
		public static float Cross(Vector2 v1, Vector2 v2) {
			return (v1.x*v2.y) - (v1.y*v2.x);
		}
		
		/// <summary>
		/// Caps given destDir with angleLimit (degree) on either side of srcDir, returns which side the destDir is capped relative to srcDir.
		/// </summary>
		/// <returns>
		/// The side destDir is relative to srcDir. (-1 or 1)
		/// </returns>
		public static float DirCap(Vector2 srcDir, ref Vector2 destDir, float angleLimit) {
			
			float side = CheckSideSign(srcDir, destDir);
			
			float angle = Mathf.Acos(Vector2.Dot(srcDir, destDir));
			
			float limitAngle = angleLimit*Mathf.Deg2Rad;
			
			if(angle > limitAngle) {
				destDir = Rotate(srcDir, -side*limitAngle);
			}
			
			return side;
		}
		
		//-------------- 3D --------------
		
		public static void Limit(ref Vector3 v, float limit) {
			float d = v.magnitude;
			if(d > limit) {
				v /= d;
				v *= limit;
			}
		}
		
		public static Vector3 Limit(Vector3 v, float limit) {
			Limit(ref v, limit);
			return v;
		}
	}
}
