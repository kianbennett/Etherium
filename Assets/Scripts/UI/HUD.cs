using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : Singleton<HUD> {

    public static bool hasSelectionBox => instance.selectionBox.gameObject.activeSelf;

    public UISelectionBox selectionBox;
    public ScreenFader screenFader;
    public Transform healthbarContainer;

    public Healthbar resourceBarPrefab, healthBarPrefab;

    private Dictionary<ResourceObject, Healthbar> resourceBars = new Dictionary<ResourceObject, Healthbar>();
    private Dictionary<Unit, Healthbar> healthbars = new Dictionary<Unit, Healthbar>();

    void Update() {
        updateHealthBars();
    }

    private void updateHealthBars() {
        foreach(ResourceObject resourceObject in resourceBars.Keys) {
            Vector3 pos = CameraController.instance.camera.WorldToScreenPoint(resourceObject.transform.position + Vector3.up * 1.5f);
            bool show = resourceBars[resourceObject].isOnScreen() && (resourceObject.IsHovered() || resourceObject.lastHarvestTime < 1.0f);
            resourceBars[resourceObject].SetPosition(pos);
            resourceBars[resourceObject].gameObject.SetActive(show);
            if(show) {
                resourceBars[resourceObject].SetPercentage(resourceObject.resourceAmount);
            }
        }
        foreach(Unit unit in healthbars.Keys) {
            Vector3 pos = CameraController.instance.camera.WorldToScreenPoint(unit.transform.position + Vector3.up * 1);
            bool show = healthbars[unit].isOnScreen() && unit.IsSelected();
            healthbars[unit].SetPosition(pos);
            healthbars[unit].gameObject.SetActive(show);
            if(show) {
                healthbars[unit].SetPercentage(unit.healthCurrent / unit.healthCurrent);
            }
        }
    }

    public void CreateResourceBar(ResourceObject resourceObject) {
        Healthbar bar = Instantiate(resourceBarPrefab, Vector3.zero, Quaternion.identity, healthbarContainer);
        resourceBars.Add(resourceObject, bar);
    }

    public void CreateHealthbar(Unit unit) {
        Healthbar bar = Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity, healthbarContainer);
        bar.rect.sizeDelta = new Vector2(unit.healthMax * 8, bar.rect.sizeDelta.y);
        healthbars.Add(unit, bar);
    }

    public Healthbar GetResourceBar(ResourceObject resourceObject) {
        return resourceBars[resourceObject];
    }

    public Healthbar GetHealthbar(Unit unit) {
        return healthbars[unit];
    }

    public void clear() {
        foreach(Healthbar bar in resourceBars.Values) Destroy(bar.gameObject);
        foreach(Healthbar bar in healthbars.Values) Destroy(bar.gameObject);
        resourceBars.Clear();
        healthbars.Clear();
    }
}
