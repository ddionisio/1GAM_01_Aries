using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FlockAntiField))]
public class EditorFlockAntiField : Editor {
	public override void OnInspectorGUI() {
		//serializedObject.Update();
		FlockAntiField item = target as FlockAntiField;
		
		item.type = (FlockAntiField.Type)EditorGUILayout.EnumPopup("Type", item.type);
		
		//item.layerMask = EditorGUILayoutx.LayerMaskField("Filter:", item.layerMask);
		
		item.angle = EditorGUILayout.Slider("Dir Angle", item.angle, 0.0f, 360.0f);
		
		item.force = EditorGUILayout.FloatField("Force", item.force);
		
		item.updateDelay = EditorGUILayout.FloatField("Update Delay", item.updateDelay);
	}
	
	
}
