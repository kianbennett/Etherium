using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : WorldObject {

    public GameObject model;
    
    [ReadOnly] public int healthCurrent;
    public int healthMax;

    private Healthbar healthbar;

    protected override void Awake() {
        base.Awake();
        
        healthCurrent = healthMax;

        healthbar = HUD.instance.CreateHealthbar();
    }

    protected override void Update() {
        base.Update();
        updateHealthbar();
    }

    private void updateHealthbar() {
        bool show = healthbar.isOnScreen() && (IsHovered() || IsSelected());
        healthbar.SetWorldPos(transform.position + Vector3.up * 1.5f);
        healthbar.gameObject.SetActive(show);
        healthbar.SetPercentage((float) healthCurrent / healthMax);
    }

    void OnDestroy() {
        Destroy(healthbar.gameObject);
    }
}
