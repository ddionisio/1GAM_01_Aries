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

using Holoville.HOEditorGUIFramework;
using Holoville.HOEditorGUIFramework.Utils;
using Holoville.HOTween;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HOTweenComponent))]
public class HOTweenComponentInspector : Editor, IHOTweenPanel
{
    HOTweenComponent src;
    HOEditorUndoManager undoManager;

    const int LabelsWidth = 130;
    const int FieldsWidth = 60;

    // ===================================================================================
    // MONOBEHAVIOUR METHODS -------------------------------------------------------------

    void OnEnable()
    {
        src = target as HOTweenComponent;
        undoManager = new HOEditorUndoManager(src, "HOTweenComponent");
        HOTweenEditorGUI.panelMode = HOTweenPanelMode.Default;
    }

    override public void OnInspectorGUI()
    {
        HOTweenEditorGUI.GUIStart(this, src, undoManager, LabelsWidth, FieldsWidth);

        GUILayout.Space(4);
        GUILayout.Label("HOTween v" + HOTween.VERSION + " | Editor v" + HOTweenEditor.VERSION, HOGUIStyle.miniLabel);
        GUILayout.Space(2);
        
        HOTweenEditorGUI.DrawPanel();

        HOTweenEditorGUI.GUIEnd();
    }
}