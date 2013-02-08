using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityManager : MonoBehaviour {
    [System.Serializable]
    public class FactoryData {
        public Transform template;

        public int startCapacity;
        public int maxCapacity;

        public Transform defaultParent;

        private List<Transform> available;

        private int allocateCounter = 0;

        private Transform poolHolder;

        private int nameCounter = 0;

        public void Init(Transform poolHolder) {
            this.poolHolder = poolHolder;

            nameCounter = 0;

            available = new List<Transform>(maxCapacity);
            Expand(startCapacity);
        }

        public void Expand(int num) {
            for(int i = 0; i < num; i++) {
                //PoolDataController
                Transform t = (Transform)Object.Instantiate(template);

                t.parent = poolHolder;
                t.localPosition = Vector3.zero;

                PoolDataController pdc = t.GetComponent<PoolDataController>();
                if(pdc == null) {
                    pdc = t.gameObject.AddComponent<PoolDataController>();
                }

                pdc.factoryKey = template.name;

                available.Add(t);
            }
        }

        public void Release(Transform t) {
            t.parent = poolHolder;
            t.localPosition = Vector3.zero;

            available.Add(t);
            allocateCounter--;
        }

        public T Allocate<T>(string name, Transform parent) where T : Component {
            if(available.Count == 0) {
                if(allocateCounter + 1 > maxCapacity) {
                    Debug.LogWarning(template.name + " is expanding beyond max capacity: " + maxCapacity);

                    Expand(maxCapacity);
                }
                else {
                    Expand(1);
                }
            }

            Transform t = available[available.Count - 1];
            T obj = t.GetComponent<T>();
            if(obj != null) {
                available.RemoveAt(available.Count - 1);

                t.GetComponent<PoolDataController>().claimed = false;

                t.name = string.IsNullOrEmpty(name) ? template.name + (nameCounter++) : name;
                t.parent = parent == null ? defaultParent : parent;
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;

                t.gameObject.SetActive(true);

                allocateCounter++;
            }

            return obj;
        }

        public void DeInit() {
            available.Clear();

            poolHolder = null;

            allocateCounter = 0;
        }
    }

    [SerializeField]
    FactoryData[] factory;

    [SerializeField]
    Transform poolHolder;

    [SerializeField]
    Transform _activatorHolder;

    private static EntityManager mInstance = null;

    private Dictionary<string, FactoryData> mFactory;

    public static EntityManager instance {
        get {
            return mInstance;
        }
    }

    public Transform activatorHolder {
        get { return _activatorHolder; }
    }

    //if toParent is null, then set parent to us or factory's default
    public T Spawn<T>(string type, string name, Transform toParent, string waypoint) where T : Component {
        T entityRet = null;

        FactoryData dat;
        if(mFactory.TryGetValue(type, out dat)) {
            entityRet = dat.Allocate<T>(name, toParent == null ? dat.defaultParent == null ? transform : null : toParent);

            if(entityRet != null) {
                if(WaypointManager.instance != null && !string.IsNullOrEmpty(waypoint)) {
                    Transform wp = WaypointManager.instance.GetWaypoint(waypoint);
                    if(wp != null) {
                        entityRet.transform.position = wp.position;
                    }
                }

                entityRet.gameObject.SendMessage("OnSpawned", null, SendMessageOptions.DontRequireReceiver);
            }
            else {
                Debug.LogWarning("Failed to allocate type: " + type + " for: " + name);
            }
        }
        else {
            Debug.LogWarning("No such type: " + type + " attempt to allocate: " + name);
        }

        return entityRet;
    }

    public void Release(GameObject entity) {
        PoolDataController pdc = entity.GetComponent<PoolDataController>();
        if(pdc != null) {
            FactoryData dat;
            if(mFactory.TryGetValue(pdc.factoryKey, out dat)) {
                pdc.claimed = true;
                dat.Release(entity.transform);
            }

            entity.SendMessage("OnDespawned", null, SendMessageOptions.DontRequireReceiver);
        }
        else { //not in the pool, just kill it
            //Object.Destroy(entity.gameObject);
            StartCoroutine(DestroyEntityDelay(entity.gameObject));
        }
    }

    IEnumerator DestroyEntityDelay(GameObject go) {
        yield return new WaitForFixedUpdate();

        Object.Destroy(go);

        yield break;
    }

    void OnDestroy() {
        mInstance = null;

        foreach(FactoryData dat in mFactory.Values) {
            dat.DeInit();
        }
    }

    void Awake() {
        mInstance = this;

        poolHolder.gameObject.SetActive(false);

        //generate cache and such
        mFactory = new Dictionary<string, FactoryData>(factory.Length);
        foreach(FactoryData factoryData in factory) {
            factoryData.Init(poolHolder);

            mFactory.Add(factoryData.template.name, factoryData);
        }
    }
}
