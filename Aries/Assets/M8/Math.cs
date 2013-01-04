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
	}
}
