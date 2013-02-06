using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(StatBase))]
public class EditorStatBase : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        //serializedObject.Update();
        StatBase item = target as StatBase;

        item.resistFlags = (UnitDamageType)EditorGUILayout.EnumMaskField("Resistance", item.resistFlags);
        item.resistDamageMod = EditorGUILayout.FloatField("Resist Damage Mod", item.resistDamageMod);

        item.spellImmuneFlags = (SpellFlag)EditorGUILayout.EnumMaskField("Spell Immune", item.spellImmuneFlags);
    }
}