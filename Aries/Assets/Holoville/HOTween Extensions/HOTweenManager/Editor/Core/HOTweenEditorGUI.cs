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

using System;
using System.Collections.Generic;
using System.Reflection;
using Holoville.HOEditorGUIFramework;
using Holoville.HOEditorGUIFramework.Utils;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using UnityEditor;
using UnityEngine;

public static class HOTweenEditorGUI
{
    public const int ControlsVPadding = 5;
    public const int BtIcoWidth = 20;
    public const int TinyToggleWidth = 15;

    public static readonly Type[] acceptedValueTypes = { typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Single), typeof(String), typeof(Color), typeof(Int32), typeof(Rect) };
    public static readonly Type[] invalidPlugins = { typeof(PlugVector3Path), typeof(PlugSetColor) };
    static readonly List<Type> invalidTargetTypes = new List<Type> {typeof(HOTweenComponent), typeof(HOTweenManager)};

    static bool initialized;
    static List<List<ObjectData>> objDatasCollection;
    static List<PluginData> pluginDatas;
    static List<Type> validPropTypes;
    static Dictionary<HOTweenManager.HOPropData, List<PluginData>> dcPropDataToValidPluginDatas;
    static Dictionary<HOTweenManager.HOPropData, string[]> dcPropDataToValidPluginsEnum;

    // GUI LOOP VARIABLED ///////////////////////////////////////////

    public static HOTweenPanelMode panelMode;

    static IHOTweenPanel panel;
    static ABSHOTweenEditorElement src;
    static HOTweenComponent componentSrc;
    static bool isManagerGUI;
    static HOEditorUndoManager undoManager;
    static int labelsWidth;
    static int fieldsWidth;
    static GameObject selection;
    static GameObject prevSelection;
    static UnityEngine.Object currentTarget;
    static string currentTargetPath;
    static GameObject currentTargetRoot;
    static HOTweenManager.HOTweenData currentTwData;

    // ***********************************************************************************
    // INIT + GUI START - END
    // ***********************************************************************************

    public static void GUIStart(IHOTweenPanel panel, HOTweenComponent src, HOEditorUndoManager undoManager, int labelsWidth, int fieldsWidth)
    {
        // Ensure that only one panel is in a state different from the default one
        if (isManagerGUI && src != null && panelMode != HOTweenPanelMode.Default) {
            panelMode = HOTweenPanelMode.Default;
            HOTweenEditorGUI.panel.Repaint();
        }

        isManagerGUI = false;
        HOTweenEditorGUI.src = componentSrc = src;
        GUIStart(panel, undoManager, labelsWidth, fieldsWidth);
    }
    public static void GUIStart(IHOTweenPanel panel, HOTweenManager src, HOEditorUndoManager undoManager, int labelsWidth, int fieldsWidth)
    {
        // Ensure that only one panel is in a state different from the default one
        if (!isManagerGUI && HOTweenEditorGUI.panel != null && panelMode != HOTweenPanelMode.Default) {
            panelMode = HOTweenPanelMode.Default;
            HOTweenEditorGUI.panel.Repaint();
        }

        isManagerGUI = true;
        HOTweenEditorGUI.src = src;
        GUIStart(panel, undoManager, labelsWidth, fieldsWidth);
    }
    static void GUIStart(IHOTweenPanel panel, HOEditorUndoManager undoManager, int labelsWidth, int fieldsWidth)
    {
        HOTweenEditorGUI.panel = panel;
        HOTweenEditorGUI.undoManager = undoManager;
        HOTweenEditorGUI.labelsWidth = labelsWidth;
        HOTweenEditorGUI.fieldsWidth = fieldsWidth;
        undoManager.CheckUndo();
        EditorGUIUtility.LookLikeControls(labelsWidth, fieldsWidth);

        if (pluginDatas == null) ReflectHOTween();
        dcPropDataToValidPluginDatas = new Dictionary<HOTweenManager.HOPropData, List<PluginData>>();
        dcPropDataToValidPluginsEnum = new Dictionary<HOTweenManager.HOPropData, string[]>();
        foreach (HOTweenManager.HOTweenData twData in src.tweenDatas) {
            foreach (HOTweenManager.HOPropData propData in twData.propDatas) {
                StoreValidPluginsFor(twData.targetType, propData);
            }
        }
    }

    public static void GUIEnd()
    {
        undoManager.CheckDirty();
        // Repaint after undo.
        if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") {
            panelMode = HOTweenPanelMode.Default;
            panel.Repaint();
            Event.current.Use();
        }
    }

    // ===================================================================================
    // GUI METHODS -----------------------------------------------------------------------

    public static void DrawPanel()
    {
        switch (panelMode) {
        case HOTweenPanelMode.Default:
            DrawDefaultMode();
            break;
        case HOTweenPanelMode.TargetSelection:
            DrawTargetSelectionMode();
            break;
        case HOTweenPanelMode.PropertySelection:
            DrawPropertySelectionMode();
            break;
        }
    }

    static void DrawTargetSelectionMode()
    {
        if (Selection.transforms.Length == 0 || Selection.transforms[0].gameObject != selection) {
            if (Event.current.type == EventType.Repaint) panelMode = HOTweenPanelMode.Default;
        }
        if (GUILayout.Button("Cancel", GUILayout.Height(24))) panelMode = HOTweenPanelMode.Default;
        GUILayout.Space(HOTweenEditorGUI.ControlsVPadding);
        List<ObjectData> objDatas = objDatasCollection[objDatasCollection.Count - 1];
        ObjectData o;
        for (int i = 0; i < objDatas.Count; ++i) {
            o = objDatas[i];
            if (i == 0) {
                HOGUILayout.Vertical(GUI.skin.box, ()=> {
                    GUILayout.Label("Select tween target...", HOGUIStyle.centeredLabel);
                    HOGUILayout.Horizontal(GUI.skin.box, ()=> {
                        GUILayout.Label(GetSelectionTargetPath(), HOGUIStyle.centeredLabelBold);
                    });
                    HOGUILayout.Horizontal(()=> {
                        if (objDatasCollection.Count > 1) {
                            if (GUILayout.Button("<- Back", HOGUIStyle.button, GUILayout.Width(60), GUILayout.Height(24))) objDatasCollection.RemoveAt(objDatasCollection.Count - 1);
                        }
                        if (HOGUILayout.ColoredButton("Tween this!", HOGUIStyle.button, HOTweenEditorVars.strongButtonBgColor, HOTweenEditorVars.strongButtonFontColor, GUILayout.Height(24))) {
                            undoManager.ForceDirty();
                            SelectTweenTarget(o);
                        }
                    });
                });
                GUILayout.Space(HOTweenEditorGUI.ControlsVPadding);
                GUILayout.BeginVertical();
                GUILayout.Label("...or search inner elements", HOGUIStyle.centeredLabel);
            } else {
                if (GUILayout.Button(o.name + "\n(" + o.type + ")", HOGUIStyle.menubarButton, GUILayout.Height(30))) {
                    objDatasCollection.Add(GetTargetAndSubTargets(o.obj, o.name));
                }
            }
        }
        GUILayout.EndVertical();
    }

    static void DrawPropertySelectionMode()
    {
        if (GUILayout.Button("Cancel", HOGUIStyle.button, GUILayout.Height(24))) panelMode = HOTweenPanelMode.Default;
        GUILayout.Space(HOTweenEditorGUI.ControlsVPadding);
        GUILayout.BeginVertical();
        GUILayout.Label("Select a property to tween:", HOGUIStyle.centeredLabel);

        List<ObjectData> objDatas = objDatasCollection[0];
        ObjectData o;
        HOTweenManager.HOPropData propData;
        for (int i = 0; i < objDatas.Count; ++i) {
            o = objDatas[i];
            if (GUILayout.Button(o.name + "\n(" + o.type + ")", HOGUIStyle.menubarButton, GUILayout.Height(30))) {
                undoManager.ForceDirty();
                propData = new HOTweenManager.HOPropData(o.name, o.type);
                currentTwData.propDatas.Add(propData);
                StoreValidPluginsFor(currentTwData.targetType, propData);
                currentTwData.propDatas.Sort(SortPropDatas);
                panelMode = HOTweenPanelMode.Default;
            }
        }

        GUILayout.EndVertical();
    }

    static void DrawDefaultMode()
    {
        objDatasCollection = null;

        Transform[] selTrans = Selection.transforms;
        
        HOGUILayout.Menubar(22, HOTweenEditorVars.mainToolbarColor, ()=> {
            int twCount = src.tweenDatas.Count;
            GUILayout.Label(twCount + (twCount == 0 || twCount > 1 ? " Tweens" : " Tween"), HOGUIStyle.labelBold);
            if (src.TotEmptyTweens() > 0) {
                GUILayout.Label("(" + src.TotEmptyTweens() + " empty)", HOGUIStyle.miniLabel);
            }
            GUILayout.FlexibleSpace();
            GUIContent addLabel = isManagerGUI ? new GUIContent("+ Tween Selection", "Add a tween to the currently selected object") : new GUIContent("+ Add Tween", "Add a tween to this object or one of its Components");
            HOGUILayout.DisabledGroup(isManagerGUI && (selTrans.Length == 0 || selTrans.Length > 1), () => {
                if (HOGUILayout.ColoredButton(addLabel, HOGUIStyle.menubarButton, HOTweenEditorVars.strongButtonBgColor, HOTweenEditorVars.strongButtonFontColor)) {
                    selection = isManagerGUI ? selTrans[0].gameObject : src.gameObject;
                    currentTargetRoot = selection;
                    objDatasCollection = new List<List<ObjectData>>();
                    objDatasCollection.Add(GetTargetAndSubTargets(selection, selection.name));
                    panelMode = HOTweenPanelMode.TargetSelection;
                }
            });
            if (isManagerGUI) {
                if (GUILayout.Button(new GUIContent("□", "Select all GameObjects that contain a tweened target"), HOGUIStyle.toolbarIconButton)) {
                    List<GameObject> tRoots = new List<GameObject>();
                    foreach (HOTweenManager.HOTweenData td in src.tweenDatas) {
                        if (td.targetRoot != null) tRoots.Add(td.targetRoot);
                    }
                    Selection.objects = tRoots.ToArray();
                    EditorWindow h = GetHierarchyPanel();
                    if (h != null) h.Focus(); // Focus Hierarchy panel.
                }
            } else {
                componentSrc.destroyAfterSetup = HOGUILayout.ToggleButton(componentSrc.destroyAfterSetup, new GUIContent("AutoDestroy", "If selected, destroys this Component after the tween has been setup"), HOGUIStyle.toolbarButton, HOTweenEditorVars.toggleBgColor, HOTweenEditorVars.toggleFontColor);
            }
            if (HOGUILayout.ToolbarExpandButton("Expand all foldouts")) {
                bool expanded = false;
                foreach (HOTweenManager.HOTweenData td in src.tweenDatas) {
                    if (!td.foldout) expanded = true;
                    td.foldout = true;
                }
                if (expanded) undoManager.ForceDirty();
            }
            if (HOGUILayout.ToolbarCollapseButton("Collapse all foldouts")) {
                bool collapsed = false;
                foreach (HOTweenManager.HOTweenData td in src.tweenDatas) {
                    if (td.foldout) collapsed = true;
                    td.foldout = false;
                }
                if (collapsed) undoManager.ForceDirty();
            }
            GUILayout.Space(6);
        });
        HOGUILayout.Vertical(HOGUIStyle.blankBox, HOTweenEditorVars.globalsBgColor, ()=> {
            HOGUILayout.Horizontal(() => {
                EditorGUIUtility.LookLikeControls(80, 50);
                src.globalDelay = EditorGUILayout.FloatField("Global Delay", src.globalDelay);
                if (src.globalDelay < 0) src.globalDelay = 0;
                EditorGUIUtility.LookLikeControls(104, 74);
                GUILayout.Space(10);
                src.globalTimeScale = EditorGUILayout.FloatField("GlobalTimeScale", src.globalTimeScale);
                if (src.globalTimeScale < 0) src.globalTimeScale = 0;
            });
            GUILayout.Space(4);
        });

        DrawTweens();
    }

    static void DrawTweens()
    {
        src.tweenDatas.Sort(SortTweenDatas);
        int tweensCount = src.tweenDatas.Count;
        for (int i = 0; i < tweensCount; ++i) {
            EditorGUIUtility.LookLikeControls(81);
            HOTweenManager.HOTweenData twData = src.tweenDatas[i];
            HOTweenManager.HOPropData propData;
            PluginData plugData = null;
            bool invalidTarget = twData.targetRoot == null;
            bool emptyTween = twData.propDatas == null || twData.propDatas.Count == 0;

            // Group everything to allow drag
            HOGUILayout.Vertical(() => {
                Color toolbarColor = invalidTarget ? HOTweenEditorVars.errorBgColor : emptyTween ? HOTweenEditorVars.warningBgColor : HOTweenEditorVars.tweenToolbarColor;
                HOGUILayout.Toolbar(toolbarColor, () => {
                    twData.isActive = HOGUILayout.ToolbarToggle(twData.isActive);
                    twData.foldout = HOGUILayout.MiniFoldout(twData.foldout, (twData.id == "" ? "" : "[" + twData.id + "] ") + (isManagerGUI ? twData.targetPath : twData.partialTargetPath), true);
                    if (HOGUILayout.ColoredButton(new GUIContent("+", "Add a property to tween"), HOGUIStyle.toolbarButton, HOTweenEditorVars.strongButtonBgColor, HOTweenEditorVars.strongButtonFontColor, GUILayout.Width(HOGUIStyle.IconButtonWidth * 2))) {
                        currentTwData = twData;
                        objDatasCollection = new List<List<ObjectData>>();
                        objDatasCollection.Add(GetProperties(twData.target));
                        panelMode = HOTweenPanelMode.PropertySelection;
                    }
                    if (GUILayout.Button(new GUIContent("╣", "Duplicate this tween"), HOGUIStyle.toolbarIconButton)) {
                        undoManager.ForceDirty();
                        HOTweenManager.HOTweenData twDataClone = twData.Clone(++src.creationTime);
                        foreach (HOTweenManager.HOPropData pd in twDataClone.propDatas) {
                            StoreValidPluginsFor(twDataClone.targetType, pd);
                        }
                        src.tweenDatas.Add(twDataClone);
                        ++tweensCount;
                    }
                    if (isManagerGUI) {
                        if (GUILayout.Button(new GUIContent("□", "Select GameObject"), HOGUIStyle.toolbarIconButton)) {
                            Selection.activeGameObject = twData.targetRoot;
                            EditorWindow h = GetHierarchyPanel();
                            if (h != null) h.Focus(); // Focus Hierarchy panel.
                        }
                    }
                    if (HOGUILayout.ColoredButton(new GUIContent("x", "Delete this tween"), HOGUIStyle.toolbarIconButton, Color.red, Color.white)) {
                        undoManager.ForceDirty();
                        src.tweenDatas.RemoveAt(i);
                        --tweensCount;
                    }
                });
                if (twData.foldout) {
                    if (invalidTarget) {
                        GUILayout.Label("This tween's target doesn't exist anymore", HOGUIStyle.wordWrapLabelBold);
                        GUILayout.Space(4);
                    } else {
                        HOGUILayout.DisabledGroup(!twData.isActive, () => {
                            // General options toolbar
                            HOGUILayout.Toolbar(() => {
                                GUILayout.Label("Id", HOGUIStyle.miniLabel);
                                twData.id = EditorGUILayout.TextField("", twData.id, HOGUIStyle.toolbarTextField, GUILayout.ExpandWidth(true));
                                twData.tweenFrom = HOGUILayout.ToggleButton(twData.tweenFrom, new GUIContent("From", "If selected the tween will animate FROM instead than TO the given value"), HOGUIStyle.toolbarButton, HOTweenEditorVars.toggleBgColor, HOTweenEditorVars.toggleFontColor, GUILayout.Width(40));
                                twData.paused = !HOGUILayout.ToggleButton(!twData.paused, new GUIContent("Autoplay"), HOGUIStyle.toolbarButton, HOTweenEditorVars.toggleBgColor, HOTweenEditorVars.toggleFontColor, GUILayout.Width(60));
                                twData.autoKill = HOGUILayout.ToggleButton(twData.autoKill, new GUIContent("Autokill", "If selected, destroys the tween after it reaches completion"), HOGUIStyle.toolbarButton, HOTweenEditorVars.toggleBgColor, HOTweenEditorVars.toggleFontColor, GUILayout.Width(60));
                            });
                            // Properties
                            int propDatasCount = twData.propDatas.Count;
                            for (int c = 0; c < propDatasCount; ++c) {
                                HOGUILayout.Vertical(GUI.skin.box, () => {
                                    propData = twData.propDatas[c];
                                    bool propDeleted = false;
                                    HOGUILayout.Horizontal(() => {
                                        propData.isActive = EditorGUILayout.Toggle(propData.isActive, GUILayout.Width(HOTweenEditorGUI.TinyToggleWidth));
                                        GUILayout.Label(propData.propName + " (" + propData.shortPropType + ")", HOGUIStyle.miniLabel);
                                        GUILayout.FlexibleSpace();
                                        if (GUILayout.Button(new GUIContent("╣", "Duplicate this property tween"), HOGUIStyle.iconButton)) {
                                            undoManager.ForceDirty();
                                            HOTweenManager.HOPropData propDataClone = propData.Clone();
                                            StoreValidPluginsFor(twData.targetType, propDataClone);
                                            twData.propDatas.Add(propDataClone);
                                        }
                                        if (HOGUILayout.ColoredButton(new GUIContent("x", "Delete this property tween"), HOGUIStyle.iconButton, Color.white, Color.red)) {
                                            undoManager.ForceDirty();
                                            dcPropDataToValidPluginDatas.Remove(propData);
                                            dcPropDataToValidPluginsEnum.Remove(propData);
                                            twData.propDatas.RemoveAt(c);
                                            --propDatasCount;
                                            propDeleted = true;
                                        }
                                    });
                                    if (!propDeleted) {
                                        HOGUILayout.DisabledGroup(!propData.isActive, () => {
                                            string[] propPluginsEnum = dcPropDataToValidPluginsEnum[propData];
                                            // Plugin type popup
                                            int ind = 0;
                                            // Get index of currently applied plugin.
                                            Type plugType = propData.pluginType;
                                            List<PluginData> propPlugins = dcPropDataToValidPluginDatas[propData];
                                            if (plugType != null) {
                                                for (int d = 0; d < propPlugins.Count; ++d) {
                                                    if (propPlugins[d].type == plugType) {
                                                        ind = d;
                                                        break;
                                                    }
                                                }
                                            } else {
                                                propData.pluginType = propPlugins[0].type;
                                            }
                                            // Show popup
                                            HOGUILayout.DisabledGroup(propPluginsEnum.Length <= 1, () => {
                                                int newInd = EditorGUILayout.Popup("Plugin Type", ind, propPluginsEnum);
                                                if (newInd != ind) {
                                                    propData.pluginType = propPlugins[newInd].type;
                                                    propData.valueType = null;
                                                }
                                            });
                                            // Property type popup.
                                            ind = 0;
                                            // Get reference to currently used plugin.
                                            foreach (PluginData pd in pluginDatas) {
                                                if (pd.type == propData.pluginType) {
                                                    plugData = pd;
                                                    break;
                                                }
                                            }
                                            if (plugData != null) {
                                                // Get index of currently used value type.
                                                Type valType = propData.valueType;
                                                if (valType != null) {
                                                    for (int d = 0; d < plugData.validValueTypes.Length; ++d) {
                                                        if (plugData.validValueTypes[d] == valType) {
                                                            ind = d;
                                                            break;
                                                        }
                                                    }
                                                } else {
                                                    propData.valueType = plugData.validValueTypes[0];
                                                }
                                                // Show popup
                                                HOGUILayout.DisabledGroup(plugData.validValueTypesEnum.Length <= 1, () => {
                                                    int newInd = EditorGUILayout.Popup("Value Type", ind, plugData.validValueTypesEnum);
                                                    if (newInd != ind) propData.valueType = plugData.validValueTypes[newInd];
                                                });
                                            }
                                            // Value field.
                                            string twSuffix = "TO";
                                            if (twData.tweenFrom) {
                                                twSuffix = (propData.isRelative ? "BY FROM" : "FROM");
                                            } else {
                                                twSuffix = (propData.isRelative ? "BY" : "TO");
                                            }
                                            if (propData.valueType == typeof(Vector2)) {
                                                propData.endVal = EditorGUILayout.Vector2Field("Tween " + twSuffix, (Vector2)propData.endVal);
                                            } else if (propData.valueType == typeof(Vector3)) {
                                                propData.endVal = EditorGUILayout.Vector3Field("Tween " + twSuffix, (Vector3)propData.endVal);
                                            } else if (propData.valueType == typeof(Quaternion)) {
                                                propData.endVal = EditorGUILayout.Vector4Field("Tween " + twSuffix, (Vector4)propData.endVal);
                                            } else if (propData.valueType == typeof(Color)) {
                                                propData.endVal = EditorGUILayout.ColorField("Tween " + twSuffix, (Color)propData.endVal);
                                            } else if (propData.valueType == typeof(Int32)) {
                                                propData.endVal = EditorGUILayout.IntField("Tween " + twSuffix, (int)propData.endVal);
                                            } else if (propData.valueType == typeof(String)) {
                                                propData.endVal = EditorGUILayout.TextField("Tween " + twSuffix, (string)propData.endVal);
                                            } else if (propData.valueType == typeof(Single)) {
                                                propData.endVal = EditorGUILayout.FloatField("Tween " + twSuffix, (float)propData.endVal);
                                            } else if (propData.valueType == typeof(Rect)) {
                                                propData.endVal = EditorGUILayout.RectField("Tween " + twSuffix, (Rect)propData.endVal);
                                            }
                                            // Is relative.
                                            EditorGUIUtility.LookLikeControls(81);
                                            propData.isRelative = EditorGUILayout.Toggle("Relative", propData.isRelative);
                                        });
                                    }
                                });
                            }
                            // General tween parameters
                            // Other parameters.
                            HOGUILayout.Horizontal(() => {
                                EditorGUIUtility.LookLikeControls(86, 70);
                                twData.duration = EditorGUILayout.FloatField("Duration", twData.duration);
                                if (twData.duration < 0) twData.duration = 0;
                                GUILayout.Space(10);
                                EditorGUIUtility.LookLikeControls(50, 40);
                                twData.delay = EditorGUILayout.FloatField("Delay", twData.delay);
                                if (twData.delay < 0) twData.delay = 0;
                            });
                            HOGUILayout.Horizontal(() => {
                                EditorGUIUtility.LookLikeControls(86, 70);
                                twData.timeScale = EditorGUILayout.FloatField("TimeScale", twData.timeScale);
                                if (twData.timeScale < 0) twData.timeScale = 0;
                                GUILayout.Space(10);
                                EditorGUIUtility.LookLikeControls(50, 40);
                                twData.loops = EditorGUILayout.IntField("Loops", twData.loops);
                                if (twData.loops < -1) twData.loops = -1;
                            });
                            EditorGUIUtility.LookLikeControls(86, 100);
                            twData.updateType = (Holoville.HOTween.UpdateType)EditorGUILayout.EnumPopup("Update Type", twData.updateType);
                            HOGUILayout.DisabledGroup(twData.loops == 0 || twData.loops == 1, () => {
                                twData.loopType = (Holoville.HOTween.LoopType)EditorGUILayout.EnumPopup("Loop Type", twData.loopType);
                            });
                            twData.easeType = (Holoville.HOTween.EaseType)EditorGUILayout.EnumPopup("Ease Type", twData.easeType);
                            HOGUILayout.HorizontalDivider(2);
                            EditorGUIUtility.LookLikeControls(86, 100);
                            twData.onCompleteActionType = (HOTweenManager.HOTweenData.OnCompleteActionType)EditorGUILayout.EnumPopup("OnComplete", twData.onCompleteActionType);
                            if (twData.onCompleteActionType == HOTweenManager.HOTweenData.OnCompleteActionType.PlayTweensById) {
                                twData.onCompletePlayId = EditorGUILayout.TextField("Id", twData.onCompletePlayId);
                            } else if (twData.onCompleteActionType == HOTweenManager.HOTweenData.OnCompleteActionType.SendMessage) {
                                HOGUILayout.ShadedGroup(twData.onCompleteTarget == null ? Color.red : GUI.backgroundColor, () => {
                                    twData.onCompleteTarget = EditorGUILayout.ObjectField("GameObject", twData.onCompleteTarget, typeof(GameObject), !EditorUtility.IsPersistent(src)) as GameObject;
                                });
                                HOGUILayout.ShadedGroup(twData.onCompleteMethodName == null || twData.onCompleteMethodName == "" ? Color.red : GUI.backgroundColor, () => {
                                    twData.onCompleteMethodName = EditorGUILayout.TextField("Method Name", twData.onCompleteMethodName);
                                });
                                twData.onCompleteParmType = (HOTweenManager.HOTweenData.ParameterType)EditorGUILayout.EnumPopup("Value type", twData.onCompleteParmType);
                                switch (twData.onCompleteParmType) {
                                    case HOTweenManager.HOTweenData.ParameterType.Color:
                                        twData.onCompleteParmColor = EditorGUILayout.ColorField("Value", twData.onCompleteParmColor);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Number:
                                        twData.onCompleteParmNumber = EditorGUILayout.FloatField("Value", twData.onCompleteParmNumber);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Object:
                                        twData.onCompleteParmObject = EditorGUILayout.ObjectField("Value", twData.onCompleteParmObject, typeof(UnityEngine.Object), !EditorUtility.IsPersistent(src));
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Quaternion:
                                        twData.onCompleteParmQuaternion = (Quaternion.Euler(EditorGUILayout.Vector3Field("Value", twData.onCompleteParmQuaternion.eulerAngles)));
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Rect:
                                        twData.onCompleteParmRect = EditorGUILayout.RectField("Value", twData.onCompleteParmRect);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.String:
                                        twData.onCompleteParmString = EditorGUILayout.TextField("Value", twData.onCompleteParmString);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Vector2:
                                        twData.onCompleteParmVector2 = EditorGUILayout.Vector2Field("Value", twData.onCompleteParmVector2);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Vector3:
                                        twData.onCompleteParmVector3 = EditorGUILayout.Vector3Field("Value", twData.onCompleteParmVector3);
                                        break;
                                    case HOTweenManager.HOTweenData.ParameterType.Vector4:
                                        twData.onCompleteParmVector4 = EditorGUILayout.Vector4Field("Value", twData.onCompleteParmVector4);
                                        break;
                                }
                            }
                            EditorGUIUtility.LookLikeControls(labelsWidth, fieldsWidth);
                            GUILayout.Space(HOTweenEditorGUI.ControlsVPadding);
                        });
                    }
                }
            });
        }
    }

    // ===================================================================================
    // METHODS ---------------------------------------------------------------------------

    /// <summary>
    /// Gathers all needed HOTween informations via reflection.
    /// </summary>
    static void ReflectHOTween()
    {
        pluginDatas = new List<PluginData>();
        validPropTypes = new List<Type>();

        // Find and all plugin classes and store valid value types.
        Assembly hoAssembly = Assembly.GetAssembly(typeof(HOTween));
        Type[] ts = hoAssembly.GetTypes();
        PluginData plugD;
        BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField;
        foreach (Type t in ts) {
            if (t.IsSubclassOf(typeof(ABSTweenPlugin)) && Array.IndexOf(invalidPlugins, t) == -1) {
                plugD = new PluginData(t);
                // Valid target types.
                try {
                    plugD.validTargetTypes = t.InvokeMember("validTargetTypes", flags, null, null, new object[] { }) as Type[];
                } catch (MissingFieldException) {
                    plugD.validTargetTypes = null;
                }
                // Valid property types.
                try {
                    plugD.validPropTypes = t.InvokeMember("validPropTypes", flags, null, null, new object[] { }) as Type[];
                    foreach (Type p in plugD.validPropTypes) {
                        if (validPropTypes.IndexOf(p) == -1) validPropTypes.Add(p);
                    }
                } catch (MissingFieldException) {
                    plugD.validPropTypes = null;
                }
                // Valid value types.
                try {
                    plugD.validValueTypes = t.InvokeMember("validValueTypes", flags, null, null, new object[] { }) as Type[];
                } catch (MissingFieldException) {
                    plugD.validValueTypes = null;
                }
                pluginDatas.Add(plugD);
            }
        }
    }

    /// <summary>
    /// Stores in dictionaries the valid plugins and plugins enums for the given target and property.
    /// </summary>
    static void StoreValidPluginsFor(Type p_targetType, HOTweenManager.HOPropData p_propData)
    {
        List<PluginData> li = new List<PluginData>();
        foreach (PluginData plugData in pluginDatas) {
            if ((plugData.validTargetTypes == null || Array.IndexOf(plugData.validTargetTypes, p_targetType) != -1)
                && (plugData.validPropTypes == null || Array.IndexOf(plugData.validPropTypes, p_propData.propType) != -1)
            ) {
                li.Add(plugData);
            }
        }
        li.Sort(SortPluginDatas);
        dcPropDataToValidPluginDatas[p_propData] = li;
        string[] plugNames = new string[li.Count];
        for (int i = 0; i < plugNames.Length; ++i) {
            plugNames[i] = li[i].name;
        }
        dcPropDataToValidPluginsEnum[p_propData] = plugNames;
    }

    // ===================================================================================
    // HELPERS ---------------------------------------------------------------------------

    /// <summary>
    /// Returns a reference to the Hierarchy panel.
    /// </summary>
    public static EditorWindow GetHierarchyPanel()
    {
        EditorWindow[] wins = Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[];
        foreach (EditorWindow win in wins) {
            if (win.GetType().ToString() == "UnityEditor.HierarchyWindow") return win;
        }
        return null;
    }

    /// <summary>
    /// Returns a list with the given target as first element,
    /// plus all its valid fields, properties, and components sorted alphabetically.
    /// </summary>
    static List<ObjectData> GetTargetAndSubTargets(object p_target, string p_targetName)
    {
        List<ObjectData> subs = new List<ObjectData>();
        Type t = p_target.GetType();
        object val;
        PropertyInfo[] pInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo p;
        for (int i = 0; i < pInfos.Length; ++i) {
            p = pInfos[i];
            try {
                val = p.GetValue(p_target, null);
                if (IsValidTarget(val, p_target)) subs.Add(new ObjectData(val, p.Name));
            } catch (Exception) { }
        }
        FieldInfo[] fInfos = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        FieldInfo f;
        for (int i = 0; i < fInfos.Length; ++i) {
            f = fInfos[i];
            try {
                val = f.GetValue(p_target);
                if (IsValidTarget(val, p_target)) subs.Add(new ObjectData(val, f.Name));
            } catch (Exception) { }
        }
        if (p_target is GameObject) {
            MonoBehaviour[] monos = (p_target as GameObject).GetComponentsInChildren<MonoBehaviour>();
            MonoBehaviour m;
            for (int i = 0; i < monos.Length; ++i) {
                m = monos[i];
                Type type = m.GetType();
                if (invalidTargetTypes.IndexOf(type) == -1) subs.Add(new ObjectData(m, m.GetType().ToString()));
            }
        }
        // Sort alphabetically.
        subs.Sort(SortSelectionDatas);
        // Add original target.
        subs.Insert(0, new ObjectData(p_target, p_targetName));
        return subs;
    }

    /// <summary>
    /// Returns a list with the valid properties of the given target, sorted alphabetically.
    /// </summary>
    static List<ObjectData> GetProperties(object p_target)
    {
        List<ObjectData> subs = new List<ObjectData>();
        Type t = p_target.GetType();
        object val;
        PropertyInfo[] pInfos = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo p;
        for (int i = 0; i < pInfos.Length; ++i) {
            p = pInfos[i];
            if (!p.CanWrite) continue;
            try {
                val = p.GetValue(p_target, null);
                if (IsValidProperty(val)) subs.Add(new ObjectData(val, p.Name));
            } catch (Exception) { }
        }
        FieldInfo[] fInfos = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        FieldInfo f;
        for (int i = 0; i < fInfos.Length; ++i) {
            f = fInfos[i];
            try {
                val = f.GetValue(p_target);
                if (IsValidProperty(val)) subs.Add(new ObjectData(val, f.Name));
            } catch (Exception) { }
        }
        // Sort alphabetically.
        subs.Sort(SortSelectionDatas);
        return subs;
    }

    static bool IsValidTarget(object p_target, object p_parentTarget)
    {
        if (!IsNullable(p_target) || p_target.Equals(null)) return false;
        if (p_target == p_parentTarget) return false;
        if (p_target is GameObject) return false;
        if (!(p_target is UnityEngine.Object)) return false;
        return true;
    }

    static bool IsValidProperty(object p_target)
    {
        if (p_target.Equals(null)) return false;
        return (validPropTypes.IndexOf(p_target.GetType()) != -1);
    }

    /// <summary>
    /// Returns a string with the representation of the current target,
    /// based on the current <see cref="objDatasCollection"/>.
    /// </summary>
    static string GetSelectionTargetPath()
    {
        string s = "";
        for (int i = 0; i < objDatasCollection.Count; ++i) {
            if (i > 0) s += ".";
            s += objDatasCollection[i][0].name;
        }
        return s;
    }

    static void SelectTweenTarget(ObjectData p_data)
    {
        currentTarget = p_data.obj as UnityEngine.Object;
        currentTargetPath = GetSelectionTargetPath();
        HOTweenManager.HOTweenData twData = new HOTweenManager.HOTweenData(++src.creationTime, currentTargetRoot, currentTarget, currentTargetPath);
        src.tweenDatas.Add(twData);
        panelMode = HOTweenPanelMode.Default;
    }

    /// <summary>
    /// Returns TRUE if the given object is nullable.
    /// </summary>
    static bool IsNullable(object p_obj)
    {
        if (p_obj == null) return true;
        Type t = p_obj.GetType();
        if (!t.IsValueType) return true; // Reference
        if (Nullable.GetUnderlyingType(t) != null) return true; // Nullable
        return false;
    }

    static int SortPluginDatas(PluginData p1, PluginData p2)
    {
        return String.Compare(p1.name, p2.name);
    }

    static int SortSelectionDatas(ObjectData o1, ObjectData o2)
    {
        return String.Compare(o1.name, o2.name);
    }

    static int SortTweenDatas(HOTweenManager.HOTweenData t1, HOTweenManager.HOTweenData t2)
    {
        if (t1.targetRoot == null && t2.targetRoot == null) return SortTweenDatasByCreationTime(t1, t2);
        if (t1.targetRoot == null) return -1;
        if (t2.targetRoot == null) return 1;
        int v = String.Compare(t1.targetRoot.name, t2.targetRoot.name);
        if (v != 0) return v;
        return SortTweenDatasByCreationTime(t1, t2);
    }

    static int SortTweenDatasByCreationTime(HOTweenManager.HOTweenData t1, HOTweenManager.HOTweenData t2)
    {
        if (t1.creationTime > t2.creationTime) return 1;
        if (t1.creationTime < t2.creationTime) return -1;
        return 0;
    }

    static int SortPropDatas(HOTweenManager.HOPropData p1, HOTweenManager.HOPropData p2)
    {
        return String.Compare(p1.propName, p2.propName);
    }

    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // ||| INTERNAL CLASSES ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private class ObjectData
    {
        public string name;
        public object obj;
        public Type type;

        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        public ObjectData(object p_obj, string p_name)
        {
            obj = p_obj;
            type = obj.GetType();
            name = p_name;
        }
    }

    private class PluginData
    {
        [SerializeField]
        private Type[] _validValueTypes;
        public string[] validValueTypesEnum;

        public string name;
        public Type type;
        public Type[] validTargetTypes;
        public Type[] validPropTypes;

        // GETS/SETS //////////////////////////////////////////////

        public Type[] validValueTypes
        {
            get { return _validValueTypes; }
            set
            {
                _validValueTypes = value;
                if (value == null) {
                    validValueTypesEnum = null;
                } else {
                    // Remove any valueType not accepted by HOTweenEditor.
                    List<Type> v = new List<Type>();
                    for (int i = 0; i < _validValueTypes.Length; ++i) {
                        if (Array.IndexOf(HOTweenEditorGUI.acceptedValueTypes, _validValueTypes[i]) != -1) v.Add(_validValueTypes[i]);
                    }
                    _validValueTypes = v.ToArray();
                    validValueTypesEnum = new string[_validValueTypes.Length];
                    for (int i = 0; i < validValueTypesEnum.Length; ++i) validValueTypesEnum[i] = _validValueTypes[i].Name;
                }
            }
        }

        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        public PluginData(Type p_pluginType)
        {
            type = p_pluginType;
            name = type.Name;
        }
    }
}
