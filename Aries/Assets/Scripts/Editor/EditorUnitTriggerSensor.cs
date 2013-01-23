using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UnitTriggerSensor))]
public class EditorUnitTriggerSensor : Editor {
	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();
		/*public string fsmToVar = "trigger"; //which variable to give this crap
	public UnitTriggerEvent data;*/
		
		//serializedObject.Update();
		UnitTriggerSensor item = target as UnitTriggerSensor;
		
		item.fsmToVar = EditorGUILayout.TextField("FSM Variable", item.fsmToVar);
		
		item.iValue = EditorGUILayout.IntField("Value(int)", item.iValue);
		item.fValue = EditorGUILayout.FloatField("Value(float)", item.fValue);
		item.sValue = EditorGUILayout.TextField("Value(string)", item.sValue);
		
		item.filterFlags = EditorGUILayout.MaskField("Filter Types", item.filterFlags, System.Enum.GetNames(typeof(UnitType)));
	}
}
