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

using System.Collections.Generic;
using UnityEngine;

public class ABSHOTweenEditorElement : MonoBehaviour
{
    public List<HOTweenManager.HOTweenData> tweenDatas = new List<HOTweenManager.HOTweenData>();
    public float globalDelay = 0;
    public float globalTimeScale = 1;
    public int creationTime = 0;

    protected bool destroyed;

    // ===================================================================================
    // METHODS ---------------------------------------------------------------------------

    virtual protected void DoDestroy()
    {
        if (destroyed) return;

        destroyed = true;
        tweenDatas = null;
    }

    /// <summary>
    /// Returns the total number of empty tweens.
    /// </summary>
    public int TotEmptyTweens()
    {
        if (tweenDatas == null) return 0;

        int tot = 0;
        for (int i = 0; i < tweenDatas.Count; ++i) {
            if (tweenDatas[i].propDatas.Count == 0) {
                ++tot;
            } else {
                foreach (HOTweenManager.HOPropData propData in tweenDatas[i].propDatas) {
                    if (propData.isActive) goto Continue;
                }
                ++tot;
            Continue:
                continue;
            }
        }

        return tot;
    }

    /// <summary>
    /// Returns the total number of ready tweens.
    /// </summary>
    public int TotReadyTweens()
    {
        if (tweenDatas == null) return 0;

        int tot = 0;
        for (int i = 0; i < tweenDatas.Count; ++i) {
            if (tweenDatas[i].propDatas.Count > 0) {
                foreach (HOTweenManager.HOPropData propData in tweenDatas[i].propDatas) {
                    if (propData.isActive) {
                        ++tot;
                        break;
                    }
                }
            }
        }

        return tot;
    }
}
