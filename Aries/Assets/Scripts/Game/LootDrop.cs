using UnityEngine;
using System.Collections;

public class LootDrop : MonoBehaviour {
    public class CriteriaPlayerHealth : Criteria {
        protected override int DoCompare(Object param, object val) {
            //get player with lowest health
            float hp = float.MaxValue;
            for(int i = 0, numPlayer = Player.playerCount; i < numPlayer; i++) {
                Player player = Player.GetPlayer(i);
                if(player.stats.curHP < hp)
                    hp = player.stats.curHP;
            }

            float ret = hp - (float)val;
            return ret == 0.0f ? 0 : ret < 0.0f ? -1 : 1;
        }
    }

    public class CriteriaPlayerResource : Criteria {
        protected override int DoCompare(Object param, object val) {
            //get player with lowest resource
            float res = float.MaxValue;
            for(int i = 0, numPlayer = Player.playerCount; i < numPlayer; i++) {
                Player player = Player.GetPlayer(i);
                if(player.stats.curResource < res)
                    res = player.stats.curResource;
            }

            float ret = res - (float)val;
            return ret == 0.0f ? 0 : ret < 0.0f ? -1 : 1;
        }
    }

    public class Data {
        public string id;
        public string drop; //what to spawn
        public bool exclusive = false; // if true, don't process the rest
        public float percent;
        public Criteria[] criterias;
    }

    private static LootDrop mInstance = null;

    public static LootDrop instance { get { return mInstance; } }

    /// <summary>
    /// Drop a loot. Make sure EntityManager has the item available.
    /// </summary>
    public void Drop(string id) {
        //TODO: advance feature: look-up in server database and download the asset bundle!

    }

    void Awake() {

    }

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }


}
