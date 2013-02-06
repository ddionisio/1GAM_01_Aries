using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLocalize : MonoBehaviour {
    [System.Serializable]
    public class TableDataPlatform {
        public GamePlatform.Type platform;
        public TextAsset file;
    }

    [System.Serializable]
    public class TableData {
        public GameLanguage language;
        public TextAsset file;
        public TableDataPlatform[] platforms; //these overwrite certain keys in the string table
    }

    public class Entry {
        public string key;
        public string text;
    }

    public TableData[] tables; //table info for each language

    private static Dictionary<string, string> mTable;
    private static bool mLoaded = false;

    /// <summary>
    /// Only call this after Load.
    /// </summary>
    public static string GetText(string key) {
        string ret = "";

        if(mTable != null) {
            if(!mTable.TryGetValue(key, out ret)) {
                Debug.LogWarning("String table key not found: " + key);
            }
        }
        else {
            Debug.LogWarning("Attempting to access string table when not initialized! Key: " + key);
        }

        return ret;
    }

    /// <summary>
    /// Make sure to call this during Main's initialization based on user settings for language.
    /// </summary>
    public void Load(GameLanguage language) {
        int langInd = (int)language;

        TableData dat = tables[langInd];

        fastJSON.JSON.Instance.Parameters.UseExtensions = false;
        List<Entry> tableEntries = fastJSON.JSON.Instance.ToObject<List<Entry>>(dat.file.text);

        mTable = new Dictionary<string, string>(tableEntries.Count);

        foreach(Entry entry in tableEntries) {
            mTable.Add(entry.key, entry.text);
        }

        //append platform specific entries
        TableDataPlatform platform = null;
        foreach(TableDataPlatform platformDat in dat.platforms) {
            if(platformDat.platform == GamePlatform.current) {
                platform = platformDat;
                break;
            }
        }

        if(platform != null) {
            List<Entry> platformEntries = fastJSON.JSON.Instance.ToObject<List<Entry>>(dat.file.text);

            foreach(Entry platformEntry in platformEntries) {
                if(mTable.ContainsKey(platformEntry.key)) {
                    mTable[platformEntry.key] = platformEntry.text;
                }
            }
        }

        //already loaded before? then let everyone know it has changed
        if(mLoaded) {
            SceneManager.RootBroadcastMessage("OnLocalize", null, SendMessageOptions.DontRequireReceiver);
        }
        else {
            mLoaded = true;
        }
    }

    void OnDestroy() {
        mTable = null;
        mLoaded = false;
    }
}
