using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : WorldObject {

    public GameObject model;
    
    [ReadOnly] public int healthCurrent;
    public int healthMax;
    public int buildTime, buildCost;

    private Healthbar healthbar;

    protected override void Awake() {
        base.Awake();
        
        healthCurrent = healthMax;

        healthbar = HUD.instance.CreateHealthbar();
    }

    protected override void Update() {
        base.Update();
        if(healthbar) updateHealthbar();
    }

    public override void OnLeftClick() {
        base.OnLeftClick();
        
        PlayerController.instance.SelectObject(this);
    }

    public override void OnRightClick() {
        base.OnRightClick();

        // Units with no health can't be attacked (e.g. base)
        if(healthMax > 0) {
            PlayerController.instance.AttackObject(this);
        }
    }

    private void updateHealthbar() {
        bool show = healthbar.isOnScreen() && (IsHovered() || IsSelected()) && healthMax > 0;
        healthbar.gameObject.SetActive(show);
        if(show) {
            healthbar.SetWorldPos(transform.position + Vector3.up * 1.5f);
            healthbar.SetPercentage((float) healthCurrent / healthMax);
        }
    }

    public int GetRepairCost() {
        return (healthMax - healthCurrent) * 4;
    }

    public void Damage(int damage) {
        healthCurrent -= damage;
        if(healthCurrent <= 0) {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy() {
        if(healthbar) Destroy(healthbar.gameObject);
    }
}
