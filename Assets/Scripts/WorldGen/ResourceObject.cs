using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ResourceType { Gem, Mineral }

public class ResourceObject : WorldObject {

    public ResourceType type;
    public GameObject[] models; // First model is empty, last is full
    [ReadOnly] public float resourceAmount; // 0 to 1

    [ReadOnly] public float lastHarvestTime;
    [HideInInspector] public List<UnitHarvester> unitsHarvesting = new List<UnitHarvester>();

    private float shakeTick;
    private const float shakeInterval = 0.05f, shakeDist = 0.05f;
    private Vector3 shakeOffset;

    private Healthbar resourceBar;

    protected override void Awake() {
        base.Awake();
        
        resourceAmount = 1;

        lastHarvestTime = float.MaxValue;
        // HUD.instance.CreateResourceBar(this);
        resourceBar = HUD.instance.CreateResourceBar();
    }

    protected override void Update() {
        base.Update();

        if(resourceAmount < 1 && lastHarvestTime > 2) {
            updateResourceAmount(resourceAmount + Time.deltaTime * 0.05f);
            if(resourceAmount > 1) updateResourceAmount(1);
        }
        lastHarvestTime += Time.deltaTime;

        if(unitsHarvesting.Count > 0) {
            shakeTick += Time.deltaTime;
            if(shakeTick > shakeInterval) {
                shakeTick = 0;
                shakeOffset.x = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
                shakeOffset.y = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
                shakeOffset.z = -shakeDist / 2 + Random.Range(0, shakeDist) * unitsHarvesting.Count;
            }
        } else {
            shakeOffset = Vector3.zero;
        }
        foreach(GameObject model in models) {
            model.transform.localPosition = shakeOffset;
        }
        updateResourceBar();
    }

    public override void OnRightClick() {
        base.OnRightClick();

        PlayerController.instance.HarvestResource(this);
    }

    public void Harvest(float delta) {
        resourceAmount -= delta;
        if(resourceAmount < 0) resourceAmount = 0;

        updateResourceAmount(resourceAmount);

        lastHarvestTime = 0;
    }

    private void updateResourceAmount(float amount) {
        resourceAmount = amount;
        // Set model in array to active depending on the amount of resources it has left
        float a = resourceAmount * (models.Length - 1);
        for(int i = 0; i < models.Length; i++) {
            models[i].SetActive(a <= i && a > i - 1);
        }
    }

    private void updateResourceBar() {
        bool show = resourceBar.isOnScreen() && (IsHovered() || lastHarvestTime < 1.0f);
        resourceBar.SetWorldPos(transform.position + Vector3.up * 1.5f);
        resourceBar.gameObject.SetActive(show);
        resourceBar.SetPercentage(resourceAmount);
        resourceBar.offset = shakeOffset;
    }

    protected override void OnMouseEnter() {
        base.OnMouseEnter();
        // If the player has a harvester unit selected and it at max resource, show a tooltip
        UnitHarvester[] selectedUnits = PlayerController.instance.selectedObjects.Where(o => o is UnitHarvester).Select(o => (UnitHarvester) o).ToArray();
        if(GameManager.instance.IsAtMaxResource(type) && selectedUnits.Length > 0) {
            HUD.instance.ShowTooltip("At max capacity");
        }
    }

    protected override void OnMouseExit() {
        base.OnMouseExit();
        HUD.instance.HideTooltip();
    }

    void OnDestroy() {
        if(resourceBar) Destroy(resourceBar.gameObject);
    }
}
