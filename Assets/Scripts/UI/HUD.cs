using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD : Singleton<HUD> {

    public static bool hasSelectionBox => instance.selectionBox.gameObject.activeSelf;

    public UISelectionBox selectionBox;
    public ObjectUIRenderer objectUIRenderer;
    public ScreenFader screenFader;
    public Transform healthbarContainer, unitIconContainer;
    
    public TextMeshProUGUI textGemsValue, textMineralsValue;
    public TextMeshProUGUI textTimer;

    public Healthbar resourceBarPrefab, healthBarPrefab;
    public UnitIcon unitIconPrefab;

    private Dictionary<ResourceObject, Healthbar> resourceBars = new Dictionary<ResourceObject, Healthbar>();
    private Dictionary<Unit, Healthbar> healthbars = new Dictionary<Unit, Healthbar>();

    private Dictionary<Unit, UnitIcon> unitIcons = new Dictionary<Unit, UnitIcon>();

    private List<ResourceObject> deadResources = new List<ResourceObject>();
    private List<Unit> deadUnits = new List<Unit>();

    void Update() {
        textGemsValue.text = GameManager.instance.gems.ToString();
        textMineralsValue.text = GameManager.instance.minerals.ToString();

        int minutes = (int) (GameManager.instance.timer / 60);
        int seconds = (int) GameManager.instance.timer % 60;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        updateHealthBars();
        updateUnitIcons();

        foreach(ResourceObject resource in deadResources) resourceBars.Remove(resource);
        foreach(Unit unit in deadUnits) {
            healthbars.Remove(unit);
            unitIcons.Remove(unit);
        }
        deadResources.Clear();
        deadUnits.Clear();
    }

    private void updateHealthBars() {
        foreach(ResourceObject resourceObject in resourceBars.Keys) {
            if(!resourceObject) {
                deadResources.Add(resourceObject);
                Destroy(resourceBars[resourceObject].gameObject);
                continue;
            }
            Vector3 pos = CameraController.instance.camera.WorldToScreenPoint(resourceObject.transform.position + Vector3.up * 1.5f);
            bool show = resourceBars[resourceObject].isOnScreen() && (resourceObject.IsHovered() || resourceObject.lastHarvestTime < 1.0f);
            resourceBars[resourceObject].SetPosition(pos);
            resourceBars[resourceObject].gameObject.SetActive(show);
            if(show) {
                resourceBars[resourceObject].SetPercentage(resourceObject.resourceAmount);
            }
        }
        foreach(Unit unit in healthbars.Keys) {
            if(!unit) {
                deadUnits.Add(unit);
                Destroy(healthbars[unit].gameObject);
                continue;
            }
            Vector3 pos = CameraController.instance.camera.WorldToScreenPoint(unit.transform.position + Vector3.up * 1);
            bool show = healthbars[unit].isOnScreen() && (unit.IsSelected() || unit.IsHovered());
            healthbars[unit].SetPosition(pos);
            healthbars[unit].gameObject.SetActive(show);
            if(show) {
                healthbars[unit].SetPercentage((float) unit.healthCurrent / unit.healthMax);
            }
        }
    }

    private void updateUnitIcons() {
        int i = 0;
        int columnHeight = 4;
        foreach(Unit unit in unitIcons.Keys) {
            if(unit == null) {
                Destroy(unitIcons[unit].gameObject);
                continue;
            }
            float x = 50 + (i / columnHeight) * 90;
            float y = Screen.height - 180 - (i % columnHeight) * 100;
            unitIcons[unit].transform.position = new Vector2(x, y);
            i++;
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

    public void CreateUnitIcon(Unit unit) {
        UnitIcon unitIcon = Instantiate(unitIconPrefab, Vector3.zero, Quaternion.identity, unitIconContainer);
        unitIcon.SetUnit(unit);
        unitIcons.Add(unit, unitIcon);
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
