using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class UserSlotData : UserData {
    public const int MaxNameLength = 16;
    public const string PrefixKey = "usd";

    private int mSlot = -1;
    private string mName;
    private Dictionary<string, int> mValueIs = null;

    public string slotName {
        get { return mName; }
        set {
            mName = !string.IsNullOrEmpty(value) ? value.Length > MaxNameLength ? value.Substring(0, MaxNameLength) : value : "";
        }
    }

    public void SetSlot(int slot, bool forceLoad) {
        if(mSlot != slot || forceLoad) {
            Save(); //save previous slot

            mSlot = slot;

            //integers
            string dat = PlayerPrefs.GetString(PrefixKey + mSlot + "i", null);
            if(dat != null) {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(dat));
                mValueIs = (Dictionary<string, int>)bf.Deserialize(ms);
            }
            else {
                mValueIs = new Dictionary<string, int>(0);
            }

            //name
            mName = PlayerPrefs.GetString(PrefixKey + mSlot + "name", "");

            SceneManager.RootBroadcastMessage("OnUserDataLoad", this, SendMessageOptions.DontRequireReceiver);
        }
    }

    public override void Save() {
        if(mSlot != -1) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, mValueIs);
            PlayerPrefs.SetString(PrefixKey + mSlot, Convert.ToBase64String(ms.GetBuffer()));

            PlayerPrefs.SetString(PrefixKey + mSlot + "name", mName);
        }
    }

    /// <summary>
    /// Make sure to set a new slot after this.
    /// </summary>
    public override void Delete() {
        if(mSlot != -1) {
            PlayerPrefs.DeleteKey(PrefixKey + mSlot);
            PlayerPrefs.DeleteKey(PrefixKey + mSlot + "name");
            mSlot = -1;
            mValueIs = null;
        }
    }

    /// <summary>
    /// This will get given name from current user data.  Make sure data has been loaded beforehand.
    /// </summary>
    public override int GetInt(string name, int defaultValue = 0) {
        int dat = defaultValue;
        mValueIs.TryGetValue(name, out dat);
        return dat;
    }

    /// <summary>
    /// This will set given name to current user data. Make sure data has been loaded beforehand.
    /// </summary>
    public override void SetInt(string name, int value) {
        mValueIs[name] = value;
    }
}
