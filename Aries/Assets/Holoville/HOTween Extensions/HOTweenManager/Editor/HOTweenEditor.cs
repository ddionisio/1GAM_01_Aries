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

// Required folders:
// - Holoville/HOTween Extensions
// - Holoville/HOUnityEditor
// - Gizmos folder

using Holoville.HOEditorGUIFramework;
using Holoville.HOEditorGUIFramework.Utils;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Holoville.HOTween;

/// <summary>
/// Visual editor window for click-creating HOTweens.
/// </summary>
public class HOTweenEditor : EditorWindow, IHOTweenPanel
{
    [MenuItem("Window/HOTween Visual Editor")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(HOTweenEditor), false, "HOTweenEditor");
    }

    [MenuItem("GameObject/Create Other/HOTween/HOTweenManager", false, 5000)]
    static void Create()
    {
        Undo.RegisterSceneUndo("Create HOTweenManager");
        GameObject srcGO = new GameObject("HOTweenManager");
        srcGO.AddComponent<HOTweenManager>();
    }

    public const string VERSION = "1.1.010";

    // ENUMS //////////////////////////////////////////////////

    private enum DisabledReason
    {
        NotInitialized,
        NoManager,
        TooManyManagers
    }

    // VARS ///////////////////////////////////////////////////

    static private bool _enabled;
    static private bool hasStarted;
    static private DisabledReason disabledReason;

    static private int labelsWidth = 75;
    static private int fieldsWidth = 130;
    static private Vector2 winScroll = Vector2.zero;

    // REFERENCES /////////////////////////////////////////////

    static private HOTweenManager src;
    static private HOEditorUndoManager undoManager;
    static private GameObject[] twManagersGOs;

    // GETS/SETS //////////////////////////////////////////////

    static private bool enabled
    {
        get { return _enabled; }
        set { _enabled = value; }
    }


    // ===================================================================================
    // UNITY METHODS ---------------------------------------------------------------------

    private void OnEnable()
    {
        Setup();
    }

    private void OnFocus()
    {
        Setup();
        Repaint();
    }

    private void OnHierarchyChange()
    {
        Setup();
        Repaint();
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void OnGUI()
    {
        if (!enabled) {
            DrawDisabled();
            return;
        }

        HOTweenEditorGUI.GUIStart(this, src, undoManager, labelsWidth, fieldsWidth);

        GUILayout.Space(4);
        GUILayout.Label("HOTween v" + HOTween.VERSION + " | Editor v" + HOTweenEditor.VERSION, HOGUIStyle.miniLabel);
        winScroll = GUILayout.BeginScrollView(winScroll);
        GUILayout.Space(2);

        HOTweenEditorGUI.DrawPanel();

        GUILayout.EndScrollView();
        GUILayout.Space(20); // Needed to avoid bottom Unity bar to hide something (stack panels bug).

        HOTweenEditorGUI.GUIEnd();
    }

    // ===================================================================================
    // GUI METHODS -----------------------------------------------------------------------

    private void DrawDisabled()
    {
        GUILayout.Space(20);
        switch (disabledReason) {
            case DisabledReason.NotInitialized:
                GUILayout.Space(20);
                GUILayout.Label("HOTween Editor\nv" + VERSION + "\n\nClick to initialize", HOGUIStyle.wordWrapCenteredLabelBold);
                break;
            case DisabledReason.NoManager:
                GUILayout.Label("HOTween Editor\nv" + VERSION + "\n\nIn order to use the editor, an HOTweenManager instance needs to be added in your scene", HOGUIStyle.wordWrapCenteredLabelBold);
                if (GUILayout.Button("Add HOTweenManager")) InstantiateManager();
                break;
            case DisabledReason.TooManyManagers:
                GUILayout.Label("Beware!\nThere are " + twManagersGOs.Length + " HOTweenManagers in this scene.\nYou should delete all but one.\n", HOGUIStyle.wordWrapCenteredLabelBold);
                if (GUILayout.Button("Select All HOTweenManagers")) {
                    Selection.objects = twManagersGOs;
                    EditorWindow h = HOTweenEditorGUI.GetHierarchyPanel();
                    if (h != null) h.Focus(); // Focus Hierarchy panel.
                }
                break;
        }
    }

    // ===================================================================================
    // PRIVATE METHODS -------------------------------------------------------------------

    private void Setup()
    {
        if (!hasStarted && FindObjectOfType(typeof(GameObject)) == null) {
            // Project is starting up.
            hasStarted = true;
            Disable(DisabledReason.NotInitialized);
        } else {
            HOTweenManager[] tms = FindSceneObjectsOfType(typeof(HOTweenManager)) as HOTweenManager[];
            if (tms.Length > 0) {
                if (IsSingleManager()) {
                    Enable(tms[0]);
                } else {
                    Disable(DisabledReason.TooManyManagers);
                }
            } else {
                Disable(DisabledReason.NoManager);
            }
        }
    }

    private void Disable(DisabledReason p_reason)
    {
        enabled = false;
        disabledReason = p_reason;
        src = null;
        undoManager = null;
    }

    private void Enable(HOTweenManager p_source)
    {
        enabled = true;
        src = p_source;
        HOTweenEditorGUI.panelMode = HOTweenPanelMode.Default;
        undoManager = new HOEditorUndoManager(src, "HOTween Editor");

        if (src.tweenDatas == null) src.tweenDatas = new List<HOTweenManager.HOTweenData>();
    }

    /// <summary>
    /// Returns TRUE if there is only one HOTweenManager in the scene.
    /// </summary>
    private bool IsSingleManager()
    {
        HOTweenManager[] tms = FindSceneObjectsOfType(typeof(HOTweenManager)) as HOTweenManager[];
        if (tms == null) return false;
        if (tms.Length > 1) {
            twManagersGOs = new GameObject[tms.Length];
            for (int i = 0; i < tms.Length; ++i) twManagersGOs[i] = tms[i].gameObject;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Instantiates a new HOTweenManager in the scene.
    /// </summary>
    private void InstantiateManager()
    {
        Undo.RegisterSceneUndo("Create HOTweenManager");
        GameObject srcGO = new GameObject("HOTweenManager");
        srcGO.AddComponent<HOTweenManager>();
        Setup();
    }
}
