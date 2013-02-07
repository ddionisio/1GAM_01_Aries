using UnityEngine;
using System.Collections;

public class PlayerStat : StatBase {
    public FlockType flockGroup = FlockType.PlayerOneUnits;

    public float maxResource;
    public float minResource; //regen if current resource is below this number

    public float resourcePerSecond = 1.0f;

    private float mResource = 0.0f;

    public float curResource {
        get { return Main.instance != null ? mResource : 0.0f; }

        set {
            if(Main.instance != null) {
                float resource = mResource;

                if(resource != value) {
                    mResource = value;
                    if(mResource < 0)
                        mResource = 0;
                    else if(mResource > maxResource)
                        mResource = maxResource;

                    if(mResource != resource)
                        StatChanged(true);
                }
            }
        }
    }

    public float curResourceScale {
        get {
            if(Main.instance != null) {
                return mResource / maxResource;
            }

            return 0.0f;
        }
    }

    public void InitResource() {
        if(curResource < minResource) {
            curResource = minResource;
        }
    }

    public override void Refresh() {

        base.Refresh();
    }

    public override void ResetStats() {
        base.ResetStats();
    }

    // Update is called once per frame
    void Update() {
        if(curResource < minResource) {
            curResource += resourcePerSecond * Time.deltaTime;
        }
    }
}
