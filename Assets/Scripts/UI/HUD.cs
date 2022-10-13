using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class HUD : Singleton<HUD> {

    public static bool hasSelectionBox => instance.selectionBox.gameObject.activeSelf;

    public UISelectionBox selectionBox;
    public ObjectUIRenderer objectUIRenderer;
    public ScreenFader screenFader;
    public Transform healthbarContainer, unitIconContainer;
    public GameObject pausedOverlay, victoryScreen, defeatScreen;
    public TextMeshProUGUI textFinalTime;
    public Tooltip tooltip;
    
    public TextMeshProUGUI textGemsValue, textMineralsValue;
    public TextMeshProUGUI textGemsMaxValue, textMineralsMaxValue;
    public TextMeshProUGUI textTimer;

    public GameObject unitActionButtons;
    public GameObject unitBuildButtons;
    public GameObject structureActionButtons;
    public GameObject structureBuildButtons;

    public Button unitRepairButton, structureRepairButton, stopUnitsButtons;
    public TextMeshProUGUI textRepairUnitsCost, textRepairStructuresCost;
    public RectTransform rectRepairUnitCost, rectRepairStructureCost;

    public Healthbar resourceBarPrefab, healthBarPrefab, progressBarPrefab;
    public UnitIcon unitIconPrefab;

    private Dictionary<Unit, UnitIcon> unitIcons = new Dictionary<Unit, UnitIcon>();

    void Update() {
        textGemsValue.text = PlayerController.instance.gems.ToString();
        textMineralsValue.text = PlayerController.instance.minerals.ToString();
        textGemsMaxValue.text = "MAX: " + PlayerController.instance.MaxGems.ToString();
        textMineralsMaxValue.text = "MAX: " + PlayerController.instance.MaxMinerals.ToString();

        int minutes = (int) (GameManager.instance.Timer / 60);
        int seconds = (int) GameManager.instance.Timer % 60;
        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        Unit[] selectedUnits = PlayerController.instance.selectedObjects.Where(o => o is Unit).Select(o => (Unit) o).ToArray();
        UnitBuilder[] selectedBuilderUnits = selectedUnits.Where(o => o is UnitBuilder).Select(o => (UnitBuilder) o).ToArray();
        Structure[] selectedStructures = PlayerController.instance.selectedObjects.Where(o => o is Structure).Select(o => (Structure) o).ToArray();
        StructureBase[] selectedBaseStructures = PlayerController.instance.selectedObjects.Where(o => o is StructureBase).Select(o => (StructureBase) o).ToArray();

        unitActionButtons.SetActive(selectedUnits.Length > 0);
        structureBuildButtons.SetActive(selectedBuilderUnits.Length > 0);
        structureActionButtons.SetActive(selectedStructures.Length > 0);
        unitBuildButtons.SetActive(selectedBaseStructures.Length > 0);

        int unitRepairCost = 0;
        foreach(Unit unit in selectedUnits) {
            unitRepairCost += unit.GetRepairCost();
        }
        unitRepairButton.interactable = unitRepairCost > 0 && PlayerController.instance.gems >= unitRepairCost;
        textRepairUnitsCost.text = unitRepairCost.ToString();
        rectRepairUnitCost.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textRepairUnitsCost.preferredWidth + 10);

        int structureRepairCost = 0;
        foreach(Structure structure in selectedStructures) {
            structureRepairCost += structure.GetRepairCost();
        }
        structureRepairButton.interactable = structureRepairCost > 0 && PlayerController.instance.minerals >= structureRepairCost;
        textRepairStructuresCost.text = structureRepairCost.ToString();
        rectRepairStructureCost.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textRepairStructuresCost.preferredWidth + 10);

        stopUnitsButtons.interactable = selectedUnits.Where(o => o.movement.pathNodes != null).Count() > 0;

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

    public void ShowTooltip(string text) {
        tooltip.gameObject.SetActive(true);
        tooltip.SetContent(text);
    }

    public void ShowTooltip(string header, string body) {
        tooltip.gameObject.SetActive(true);
        tooltip.SetContent(header, body);
    }

    public void HideTooltip() {
        tooltip.gameObject.SetActive(false);
    }

    public void Resume() {
        GameManager.instance.SetPaused(false);
    }

    public void Restart() {
        GameManager.IsQuitting = true;
        screenFader.FadeOut(delegate {
            SceneManager.LoadScene("GameScene");
        }, 0.5f);
        Time.timeScale = 1;
        screenFader.CanvasGroup.blocksRaycasts = true;
    }

    public void Quit() {
        GameManager.IsQuitting = true;
        screenFader.FadeOut(delegate {
            SceneManager.LoadScene("MainMenu");
        }, 0.5f);
        Time.timeScale = 1;
        screenFader.CanvasGroup.blocksRaycasts = true;
    }
}
