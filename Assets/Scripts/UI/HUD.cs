using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class HUD : Singleton<HUD> {

    public static bool hasSelectionBox => instance.selectionBox.gameObject.activeSelf;

    public UISelectionBox selectionBox;
    public ObjectUIRenderer objectUIRenderer;
    public ScreenFader screenFader;
    public Transform healthbarContainer, unitIconContainer;
    
    public TextMeshProUGUI textGemsValue, textMineralsValue;
    public TextMeshProUGUI textTimer;

    public Healthbar resourceBarPrefab, healthBarPrefab, progressBarPrefab;
    public UnitIcon unitIconPrefab;

    private Dictionary<Unit, UnitIcon> unitIcons = new Dictionary<Unit, UnitIcon>();

    void Update() {
        textGemsValue.text = GameManager.instance.gems.ToString();
        textMineralsValue.text = GameManager.instance.minerals.ToString();

        int minutes = (int) (GameManager.instance.timer / 60);
        int seconds = (int) GameManager.instance.timer % 60;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        updateUnitIcons();
    }

    private void updateUnitIcons() {
        int i = 0;
        int columnHeight = 4;
        foreach(Unit unit in unitIcons.Keys) {
            if(!unit) {
                Destroy(unitIcons[unit].gameObject);
                continue;
            }
            float x = 50 + (i / columnHeight) * 90;
            float y = Screen.height - 180 - (i % columnHeight) * 100;
            unitIcons[unit].transform.position = new Vector2(x, y);
            i++;
        }
        unitIcons = unitIcons.Where(o => o.Key != null).ToDictionary(o => o.Key, o => o.Value);
    }

    public Healthbar CreateResourceBar() {
        return Instantiate(resourceBarPrefab, Vector3.zero, Quaternion.identity, healthbarContainer);
    }

    public Healthbar CreateHealthbar() {
        return Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity, healthbarContainer);
    }

    public Healthbar CreateProgressBar() {
        return Instantiate(progressBarPrefab, Vector3.zero, Quaternion.identity, healthbarContainer);
    }

    public void CreateUnitIcon(Unit unit) {
        UnitIcon unitIcon = Instantiate(unitIconPrefab, Vector3.zero, Quaternion.identity, unitIconContainer);
        unitIcon.SetUnit(unit);
        unitIcons.Add(unit, unitIcon);
    }
}
