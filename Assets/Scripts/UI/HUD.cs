using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class HUD : Singleton<HUD> {

    public static bool hasSelectionBox => instance.selectionBox.gameObject.activeSelf;

    public UISelectionBox selectionBox;
    public ObjectUIRenderer objectUIRenderer;
    public ScreenFader screenFader;
    public Transform healthbarContainer, unitIconContainer;
    public GameObject pausedOverlay;
    
    public TextMeshProUGUI textGemsValue, textMineralsValue;
    public TextMeshProUGUI textGemsMaxValue, textMineralsMaxValue;
    public TextMeshProUGUI textTimer;

    public GameObject unitActionButtons;
    public GameObject unitBuildButtons;
    public GameObject structureActionButtons;
    public GameObject structureBuildButtons;

    public Healthbar resourceBarPrefab, healthBarPrefab, progressBarPrefab;
    public UnitIcon unitIconPrefab;

    private Dictionary<Unit, UnitIcon> unitIcons = new Dictionary<Unit, UnitIcon>();

    void Update() {
        textGemsValue.text = GameManager.instance.gems.ToString();
        textMineralsValue.text = GameManager.instance.minerals.ToString();
        textGemsMaxValue.text = "MAX: " + GameManager.instance.maxGems.ToString();
        textMineralsMaxValue.text = "MAX: " + GameManager.instance.maxMinerals.ToString();

        int minutes = (int) (GameManager.instance.timer / 60);
        int seconds = (int) GameManager.instance.timer % 60;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        Unit[] selectedUnits = PlayerController.instance.selectedObjects.Where(o => o is Unit).Select(o => (Unit) o).ToArray();
        UnitBuilder[] selectedBuilderUnits = selectedUnits.Where(o => o is UnitBuilder).Select(o => (UnitBuilder) o).ToArray();
        Structure[] selectedStructures = PlayerController.instance.selectedObjects.Where(o => o is Structure).Select(o => (Structure) o).ToArray();
        StructureBase[] selectedBaseStructures = PlayerController.instance.selectedObjects.Where(o => o is StructureBase).Select(o => (StructureBase) o).ToArray();

        unitActionButtons.SetActive(selectedUnits.Length > 0);
        structureBuildButtons.SetActive(selectedBuilderUnits.Length > 0);
        structureActionButtons.SetActive(selectedStructures.Length > 0);
        unitBuildButtons.SetActive(selectedBaseStructures.Length > 0);

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
            float x = 40 + (i / columnHeight) * 70;
            float y = Screen.height - 180 - (i % columnHeight) * 80;
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

    public bool IsMouseOverHUD() {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
