// 
// HOTweenManager.cs
//  
// Author: Daniele Giardini
// 
// Copyright (c) 2012 Daniele Giardini - Holoville - http://www.holoville.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, merge, publish, distribute, sublicense, and/or sell
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

using UnityEngine;
using System;
using System.Collections.Generic;
using Holoville.HOTween;

public class HOTweenManager : ABSHOTweenEditorElement
{
    // ===================================================================================
    // UNITY METHODS ---------------------------------------------------------------------

    private void Awake()
    {
        HOTween.Init(true, true, true);
        foreach (HOTweenData twData in tweenDatas) CreateTween(twData, globalDelay, globalTimeScale);

        DoDestroy();
    }

    // ===================================================================================
    // METHODS ---------------------------------------------------------------------------

    override protected void DoDestroy()
    {
        if (destroyed) return;

        base.DoDestroy();
        if (this.gameObject != null) Destroy(this.gameObject);
    }

    /// <summary>
    /// Creates a tween based on the given tweenData and returns it,
    /// or returns <code>null</code> if the tween was empty.
    /// </summary>
    /// <param name="p_twData">
    /// A <see cref="HOTweenData"/>
    /// </param>
    public static Holoville.HOTween.Tweener CreateTween(HOTweenData p_twData, float p_globalDelay, float p_globalTimeScale)
    {
        if (p_twData.propDatas.Count == 0 || !p_twData.isActive) return null;

        TweenParms parms = new TweenParms()
            .Delay(p_twData.delay + p_globalDelay)
            .Id(p_twData.id)
            .Loops(p_twData.loops, p_twData.loopType)
            .UpdateType(p_twData.updateType)
            .Ease(p_twData.easeType)
            .TimeScale(p_twData.timeScale * p_globalTimeScale)
            .AutoKill(p_twData.autoKill)
            .Pause(p_twData.paused);

        // Eventual onComplete
        if (p_twData.onCompleteActionType != HOTweenData.OnCompleteActionType.None) {
            switch (p_twData.onCompleteActionType) {
                case HOTweenData.OnCompleteActionType.PlayAll:
                    parms.OnComplete(() => HOTween.Play());
                    break;
                case HOTweenData.OnCompleteActionType.PlayTweensById:
                    parms.OnComplete(() => HOTween.Play(p_twData.onCompletePlayId));
                    break;
                case HOTweenData.OnCompleteActionType.SendMessage:
                    if (p_twData.onCompleteTarget == null || p_twData.onCompleteMethodName == "") break;
                    object onCompleteParm = null;
                    switch (p_twData.onCompleteParmType) {
                        case HOTweenData.ParameterType.Color:
                            onCompleteParm = p_twData.onCompleteParmColor;
                            break;
                        case HOTweenData.ParameterType.Number:
                            onCompleteParm = p_twData.onCompleteParmNumber;
                            break;
                        case HOTweenData.ParameterType.Object:
                            onCompleteParm = p_twData.onCompleteParmObject;
                            break;
                        case HOTweenData.ParameterType.Quaternion:
                            onCompleteParm = p_twData.onCompleteParmQuaternion;
                            break;
                        case HOTweenData.ParameterType.Rect:
                            onCompleteParm = p_twData.onCompleteParmRect;
                            break;
                        case HOTweenData.ParameterType.String:
                            onCompleteParm = p_twData.onCompleteParmString;
                            break;
                        case HOTweenData.ParameterType.Vector2:
                            onCompleteParm = p_twData.onCompleteParmVector2;
                            break;
                        case HOTweenData.ParameterType.Vector3:
                            onCompleteParm = p_twData.onCompleteParmVector3;
                            break;
                        case HOTweenData.ParameterType.Vector4:
                            onCompleteParm = p_twData.onCompleteParmVector4;
                            break;
                    }
                    parms.OnComplete(p_twData.onCompleteTarget, p_twData.onCompleteMethodName, onCompleteParm);
                    break;
            }
        }

        foreach (HOPropData propData in p_twData.propDatas) {
            if (propData.isActive) {
                parms.Prop(propData.propName, Activator.CreateInstance(propData.pluginType, propData.endVal, propData.isRelative));
            }
        }
        if (!parms.hasProps) return null;

        if (p_twData.tweenFrom) return HOTween.From(p_twData.target, p_twData.duration, parms);
        return HOTween.To(p_twData.target, p_twData.duration, parms);
    }

    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // ||| INTERNAL CLASS ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    [System.Serializable]
    public class HOTweenData
    {
        public enum OnCompleteActionType
        {
            None,
            PlayAll,
            PlayTweensById,
            SendMessage
        }

        /// <summary>
        /// Used to show the correct inspector for onCompleteParms
        /// </summary>
        public enum ParameterType
        {
            None,
            Color,
            Number,
            Object,
            Quaternion,
            Rect,
            String,
            Vector2,
            Vector3,
            Vector4
        }

        public string _targetType;
        public UnityEngine.Object target;
        public GameObject targetRoot;
        public string _targetPath;
        public string targetName;
        public bool foldout = true; // If FALSE is collapsed inside HOTweenEditor
        public bool isActive = true;
        public int creationTime = 0; // Time of creation (used for final sorting)

        public List<HOPropData> propDatas;

        public float duration = 1;
        public bool tweenFrom = false;
        public bool paused = false;
        public bool autoKill = true;
        public Holoville.HOTween.UpdateType updateType = HOTween.defUpdateType;
        public float delay = 0;
        public string id = "";
        public Holoville.HOTween.LoopType loopType = HOTween.defLoopType;
        public int loops = 1;
        public float timeScale = HOTween.defTimeScale;
        public Holoville.HOTween.EaseType easeType = HOTween.defEaseType;
        public OnCompleteActionType onCompleteActionType = OnCompleteActionType.None;
        public string onCompletePlayId = "";
        public GameObject onCompleteTarget;
        public string onCompleteMethodName = "";
        public ParameterType onCompleteParmType = ParameterType.None; // Stored only to show correct inspector
        public Color onCompleteParmColor = new Color(0,0,0,1);
        public float onCompleteParmNumber;
        public UnityEngine.Object onCompleteParmObject;
        public Quaternion onCompleteParmQuaternion;
        public Rect onCompleteParmRect;
        public String onCompleteParmString;
        public Vector2 onCompleteParmVector2;
        public Vector3 onCompleteParmVector3;
        public Vector4 onCompleteParmVector4;

        public Type targetType {
            get { return Type.GetType(_targetType); }
        }

        // GETS/SETS //////////////////////////////////////////////

        public string targetPath {
            get {
                _targetPath = (targetRoot == null ? _targetPath : targetRoot.name + (targetRoot == target ? "" : "." + _targetPath.Substring(_targetPath.IndexOf(".") + 1)));
                return _targetPath;
            }
        }

        public string partialTargetPath {
            get { return _targetPath.Substring(_targetPath.IndexOf(".") + 1); }
        }


        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        public HOTweenData(int p_creationTime, GameObject p_targetRoot, UnityEngine.Object p_target, string p_targetPath)
        {
            targetRoot = p_targetRoot;
            target = p_target;
            _targetPath = p_targetPath;
            creationTime = p_creationTime;

            Type t = p_target.GetType();
            _targetType = t.FullName + ", " + t.Assembly.GetName().Name;
            targetName = targetPath.Substring(targetPath.LastIndexOf(".") + 1);
            propDatas = new List<HOPropData>();
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        public HOTweenData Clone(int p_creationTime)
        {
            HOTweenData clone = new HOTweenData(p_creationTime, targetRoot, target, _targetPath);
            clone.duration = duration;
            clone.tweenFrom = tweenFrom;
            clone.paused = paused;
            clone.updateType = updateType;
            clone.delay = delay;
            clone.id = id;
            clone.loopType = loopType;
            clone.loops = loops;
            clone.timeScale = timeScale;
            clone.easeType = easeType;
            clone.onCompleteActionType = onCompleteActionType;
            clone.onCompletePlayId = onCompletePlayId;
            clone.onCompleteTarget = onCompleteTarget;
            clone.onCompleteMethodName = onCompleteMethodName;
            clone.onCompleteParmType = onCompleteParmType;
            clone.onCompleteParmColor = new Color(onCompleteParmColor.r, onCompleteParmColor.g, onCompleteParmColor.b, onCompleteParmColor.a);
            clone.onCompleteParmNumber = onCompleteParmNumber;
            clone.onCompleteParmObject = onCompleteParmObject;
            clone.onCompleteParmQuaternion = new Quaternion(onCompleteParmQuaternion.x, onCompleteParmQuaternion.y, onCompleteParmQuaternion.z, onCompleteParmQuaternion.w);
            clone.onCompleteParmRect = new Rect(onCompleteParmRect);
            clone.onCompleteParmString = onCompleteParmString;
            clone.onCompleteParmVector2 = new Vector2(onCompleteParmVector2.x, onCompleteParmVector2.y);
            clone.onCompleteParmVector3 = new Vector3(onCompleteParmVector3.x, onCompleteParmVector3.y, onCompleteParmVector3.z);
            clone.onCompleteParmVector4 = new Vector4(onCompleteParmVector4.x, onCompleteParmVector4.y, onCompleteParmVector4.z, onCompleteParmVector4.w);
            foreach (HOPropData propData in propDatas) {
                clone.propDatas.Add(propData.Clone());
            }

            return clone;
        }
    }

    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // ||| INTERNAL CLASS ||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||
    // +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    [System.Serializable]
    public class HOPropData
    {
        [SerializeField]
        private string _propType;
        [SerializeField]
        private string _pluginType = "";
        [SerializeField]
        private string _valueType = "";

        public string propName;
        public string shortPropType;
        public bool isRelative = false;
        public bool isActive = true;

        // END VALS x TYPE ////////////////////////////////////////

        public Vector2 endValVector2 = Vector2.zero;
        public Vector3 endValVector3 = Vector3.zero;
        public Vector4 endValVector4 = Vector4.zero;
        public float endValFloat = 0;
        public int endValInt = 0;
        public string endValString = "";
        public Color endValColor = Color.white;

        // GETS/SETS //////////////////////////////////////////////

        public Type pluginType
        {
            set { _pluginType = (value == null ? "" : value.FullName + ", " + value.Assembly.GetName().Name); }
            get { return (_pluginType == "" ? null : Type.GetType(_pluginType)); }
        }

        public Type valueType
        {
            set { _valueType = (value == null ? "" : value.FullName + ", " + value.Assembly.GetName().Name); }
            get { return (_valueType == "" ? null : Type.GetType(_valueType)); }
        }

        public Type propType
        {
            get { return Type.GetType(_propType); }
        }

        public object endVal
        {
            get
            {
                Type t = valueType;
                if (t == typeof(Vector2)) return endValVector2;
                if (t == typeof(Vector3)) return endValVector3;
                if (t == typeof(Vector4)) return endValVector4;
                if (t == typeof(String)) return endValString;
                if (t == typeof(Color)) return endValColor;
                if (t == typeof(Int32)) return endValInt;
                return endValFloat;
            }
            set
            {
                Type t = valueType;
                if (t == typeof(Vector2))
                    endValVector2 = (Vector2)value;
                else if (t == typeof(Vector3))
                    endValVector3 = (Vector3)value;
                else if (t == typeof(Vector4))
                    endValVector4 = (Vector4)value;
                else if (t == typeof(String))
                    endValString = (String)value;
                else if (t == typeof(Color))
                    endValColor = (Color)value;
                else if (t == typeof(Int32))
                    endValInt = Convert.ToInt32(value);
                else
                    endValFloat = Convert.ToSingle(value);
            }
        }

        // ***********************************************************************************
        // CONSTRUCTOR
        // ***********************************************************************************

        public HOPropData(string p_propName, Type p_propType)
        {
            propName = p_propName;

            _propType = p_propType.FullName + ", " + p_propType.Assembly.GetName().Name;
            shortPropType = p_propType.Name;
        }

        // ===================================================================================
        // METHODS ---------------------------------------------------------------------------

        public HOPropData Clone()
        {
            HOPropData clone = new HOPropData(propName, propType);
            clone._pluginType = _pluginType;
            clone._valueType = _valueType;
            clone.isRelative = isRelative;
            clone.isActive = isActive;
            clone.endValVector2 = new Vector2(endValVector2.x, endValVector2.y);
            clone.endValVector3 = new Vector3(endValVector3.x, endValVector3.y, endValVector3.z);
            clone.endValVector4 = new Vector4(endValVector4.x, endValVector4.y, endValVector4.z, endValVector4.w);
            clone.endValFloat = endValFloat;
            clone.endValInt = endValInt;
            clone.endValString = endValString;
            clone.endValColor = new Color(endValColor.r, endValColor.g, endValColor.b, endValColor.a);

            return clone;
        }
    }
}
