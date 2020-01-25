using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureWarehouse : Structure {

    protected override void Awake() {
        base.Awake();
        GameManager.instance.warehouses++;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        GameManager.instance.warehouses--;
        // Use these to clamp the values to the new max
        GameManager.instance.AddGems(0);
        GameManager.instance.AddMinerals(0);
    }
}
