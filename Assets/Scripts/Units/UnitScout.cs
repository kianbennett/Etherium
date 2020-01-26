using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScout : Unit {

    protected override void Start() {
        base.Start();
        if(ownerId == 1) {
            EnemyController.instance.scoutUnits.Add(this);
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if(ownerId == 1 && !GameManager.quitting) {
            EnemyController.instance.scoutUnits.Remove(this);
        }
    }
}
