// 
// HOTweenEditor.cs
//  
// Author: Daniele Giardini
// 
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEditor;
using UnityEngine;
using System;

[CustomEditor( typeof( HOTweenManager ) )]
public class HOTweenManagerInspector : Editor
{
	// VARS ///////////////////////////////////////////////////
	
	private		bool					enabled = true;
	private		bool					isActive;
	private		int						labelsWidth = 130;
	private		int						fieldsWidth = 60;
	
	private		bool					stylesSet;
	private		GUIStyle				messageStyle;
	
	// REFERENCES /////////////////////////////////////////////
	
	private		HOTweenManager			src;
	private		GameObject				srcGO;
	private		GameObject[]			twManagersGOs;
	
	// ===================================================================================
	// UNITY METHODS ---------------------------------------------------------------------
	
	private void OnEnable()
	{
		src = target as HOTweenManager;
		srcGO = src.gameObject;
		isActive = ( srcGO.activeInHierarchy == true );
		enabled = IsSingleManager();
	}
	
	override public void OnInspectorGUI()
	{
		EditorGUIUtility.LookLikeControls( labelsWidth, fieldsWidth );
		
		if ( srcGO.activeInHierarchy != isActive ) {
			isActive = !isActive;
			enabled = IsSingleManager();
		}
		
		if ( !stylesSet ) {
			stylesSet = true;
			messageStyle = new GUIStyle( GUI.skin.label );
			messageStyle.normal.textColor = new Color( 0.9960784f, 0.509804f, 0f, 1f );
			messageStyle.wordWrap = true;
			messageStyle.padding = new RectOffset( 10, 10, 10, 10 );
		}
		if ( !enabled ) {
			// Too many managers in the scene.
			// Warn and allow to select them.
			GUILayout.Label( "Beware!\n\nThere are " + twManagersGOs.Length + " HOTweenManagers in this scene.\n\nYou should delete all but one.\n", messageStyle );
			GUILayout.BeginHorizontal();
			GUILayout.Space( 15 );
			if ( GUILayout.Button( "Select All HOTweenManagers" ) ) {
				Selection.objects = twManagersGOs;
				EditorWindow h = GetHierarchyPanel();
				if ( h != null )		h.Focus(); // Focus Hierarchy panel.
			}
			GUILayout.Space( 15 );
			GUILayout.EndHorizontal();
			return;
		}
		
		GUILayout.BeginVertical();
		GUILayout.Space( 15 );
		GUILayout.BeginHorizontal();
		GUILayout.Space( 15 );
		if ( GUILayout.Button( "Open HOTween Editor" ) )		EditorWindow.GetWindow( typeof( HOTweenEditor ), false, "HOTweenEditor" );
		GUILayout.Space( 15 );
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}
	
	// ===================================================================================
	// PRIVATE METHODS -------------------------------------------------------------------
	
	private bool IsSingleManager()
	{
		HOTweenManager[] tms = FindSceneObjectsOfType( typeof( HOTweenManager ) ) as HOTweenManager[];
		if ( tms != null && tms.Length > 1 ) {
			twManagersGOs = new GameObject[tms.Length];
			for ( int i = 0; i < tms.Length; ++i )		twManagersGOs[i] = tms[i].gameObject;
			return false;
		}
		return true;
	}
	
	// ===================================================================================
	// HELPERS ---------------------------------------------------------------------------
	
	/// <summary>
	/// Returns a reference to the Hierarchy panel.
	/// </summary>
	private EditorWindow GetHierarchyPanel()
	{
		EditorWindow[] wins = Resources.FindObjectsOfTypeAll( typeof( EditorWindow ) ) as EditorWindow[];
		foreach ( EditorWindow win in wins ) {
			if ( win.GetType().ToString() == "UnityEditor.HierarchyWindow" )		return win;
		}
		return null;
	}
}
