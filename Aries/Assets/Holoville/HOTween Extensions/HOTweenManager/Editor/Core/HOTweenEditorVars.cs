// Author: Daniele Giardini
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// Created: 2012/10/03 21:36

using Holoville.HOEditorGUIFramework;
using UnityEngine;

public class HOTweenEditorVars
{
    public static Color mainToolbarColor { get { return HOGUIStyle.IsProSkin ? _MainToolbarColorPro : _MainToolbarColorFree; } }
    public static Color tweenToolbarColor { get { return HOGUIStyle.IsProSkin ? _TweenToolbarColorPro : _TweenToolbarColorFree; } }
    public static Color toggleBgColor { get { return HOGUIStyle.IsProSkin ? _ToggleBgColorPro : _ToggleBgColorFree; } }
    public static Color toggleFontColor { get { return HOGUIStyle.IsProSkin ? _ToggleFontColorPro : _ToggleFontColorFree; } }
    public static Color globalsBgColor { get { return HOGUIStyle.IsProSkin ? _GlobalsBgColorPro : _GlobalsBgColorFree; } }
    public static Color warningBgColor { get { return HOGUIStyle.IsProSkin ? _WarningBgColorPro : _WarningBgColorFree; } }
    public static Color errorBgColor { get { return HOGUIStyle.IsProSkin ? _ErrorBgColorPro : _ErrorBgColorFree; } }

    public static readonly Color strongButtonBgColor = new Color(0.1322678f, 0.6091087f, 0.9328358f, 1f);
    public static readonly Color strongButtonFontColor = new Color(1f, 1f, 1f, 1f);

    static readonly Color _MainToolbarColorFree = new Color(0.8208956f, 0.8208956f, 0.8208956f, 1f);
    static readonly Color _MainToolbarColorPro = new Color(0.5522388f, 0.5522388f, 0.5522388f, 1f);
    static readonly Color _TweenToolbarColorFree = new Color(0.9336155f, 0.974279f, 0.9850746f, 1f);
    static readonly Color _TweenToolbarColorPro = new Color(0.858209f, 0.975507f, 1f, 1f);
    static readonly Color _GlobalsBgColorFree = new Color(0.7f, 0.7f, 0.7f, 1f);
    static readonly Color _GlobalsBgColorPro = new Color(0.16f, 0.16f, 0.16f, 1f);
    static readonly Color _ToggleBgColorFree = new Color(0.7085704f, 0.9477612f, 0.2899866f, 1f);
    static readonly Color _ToggleBgColorPro = new Color(0f, 1f, 0.2027972f, 1f);
    static readonly Color _ToggleFontColorFree = new Color(1f, 1f, 1f, 1f);
    static readonly Color _ToggleFontColorPro = new Color(0.7367707f, 1f, 0.2761194f, 1f);
    static readonly Color _WarningBgColorFree = new Color(1f, 0.967853f, 0.6716418f, 1f);
    static readonly Color _WarningBgColorPro = new Color(1f, 0.851482f, 0.5149254f, 1f);
    static readonly Color _ErrorBgColorFree = new Color(1f, 0.5970149f, 0.5970149f, 1f);
    static readonly Color _ErrorBgColorPro = new Color(1f, 0.4552239f, 0.4552239f, 1f);
}
