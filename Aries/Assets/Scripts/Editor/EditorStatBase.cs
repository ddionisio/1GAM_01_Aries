using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StatBase))]
public class EditorStatBase : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		
		//serializedObject.Update();
		StatBase item = target as StatBase;
		
		item.immuneFlags = (UnitDamageType)EditorGUILayout.EnumMaskField("Immunity", item.immuneFlags);
	}
}