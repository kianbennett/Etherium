using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureWarehouse : Structure {

    protected override void Awake() {
        base.Awake();
    }

    protected override void Start() {
        if(ownerId == 0) {
            PlayerController.instance.warehouses++;
        } else {
            EnemyController.instance.warehouses++;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if(!GameManager.IsQuitting) {
            if(ownerId == 0) {
                PlayerController.instance.warehouses--;
                // Use these to clamp the values to the new max
                PlayerController.instance.AddGems(0);
                PlayerController.instance.AddMinerals(0);
            } else {
                EnemyController.instance.AddGems(0);
                EnemyController.instance.AddMinerals(0);
            }
        }
    }
}
