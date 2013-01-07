using UnityEngine;

namespace M8 {
	public struct Math {
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
