using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structure : WorldObject {

    [SerializeField] private int healthMax;
    [SerializeField] private int buildTime, buildCost;

    private int healthCurrent;
    private Healthbar healthbar;

    public int BuildTime { get { return buildTime; } }
    public int BuildCost { get { return buildCost; } }
    public int HealthMax { get { return healthMax; } }
    public int HealthCurrent { get { return healthCurrent; } }

    protected override void Awake() 
    {
        base.Awake();
        
        healthCurrent = healthMax;
        healthbar = HUD.instance.CreateHealthbar();
    }

    protected override void Update() 
    {
        base.Update();

        if(healthbar)
        {
            updateHealthbar();
        }
    }

    public override void OnLeftClick() 
    {
        base.OnLeftClick();
        
        PlayerController.instance.SelectObject(this);
    }

    public override void OnRightClick() 
    {
        base.OnRightClick();

        // Units with no health can't be attacked (e.g. base)
        if(healthMax > 0) 
        {
            PlayerController.instance.AttackObject(this);
        }
    }

    private void updateHealthbar() 
    {
        bool show = healthbar.isOnScreen() && (isHovered || isSelected) && isVisible && healthMax > 0;
        healthbar.gameObject.SetActive(show);
        healthbar.SetWorldPos(transform.position + Vector3.up * 1.5f);

        if(show) 
        {
            healthbar.SetPercentage((float) healthCurrent / healthMax);
        }
    }

    public int GetRepairCost() 
    {
        return (healthMax - healthCurrent) * 4;
    }

    public void Repair() 
    {
        healthCurrent = healthMax;
    }

    public void Damage(int damage) 
    {
        healthCurrent -= damage;
        if(healthCurrent <= 0) 
        {
            Destroy(gameObject);
        }
    }

    protected override void OnDestroy() 
    {
        base.OnDestroy();

        if(healthbar) 
        {
            Destroy(healthbar.gameObject);
        }
    }
}
