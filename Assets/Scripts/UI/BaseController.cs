using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController<T> : Singleton<T> where T : Singleton<T> {

    [ReadOnly] public int gems, minerals;
    [ReadOnly] public int warehouses;
    public int MaxGems { get { return 500 + warehouses * 500; } }
    public int MaxMinerals { get { return 2000 + warehouses * 2000; } }

    public void AddGems(int value) {
        gems += value;
        gems = Mathf.Clamp(gems, 0, MaxGems);
    }

    public void AddMinerals(int value) {
        minerals += value;
        minerals = Mathf.Clamp(minerals, 0, MaxMinerals);
    }

    public bool IsAtMaxResource(ResourceType type) {
        if(type == ResourceType.Gem) {
            return gems >= MaxGems;
        }
        if(type == ResourceType.Mineral) {
            return minerals >= MaxMinerals;
        }    
        return false;
    }
}
