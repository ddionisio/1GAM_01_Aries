using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FlockFilter))]
public class EditorFlockFilter : Editor {
	public override void OnInspectorGUI() {
		//serializedObject.Update();
		FlockFilter item = target as FlockFilter;
		
		item.type = (FlockType)EditorGUILayout.EnumPopup("Type", item.type);
		item.avoidTypeFilter = EditorGUILayout.MaskField("Avoid", item.avoidTypeFilter, System.Enum.GetNames(typeof(FlockType)));
	}
}
