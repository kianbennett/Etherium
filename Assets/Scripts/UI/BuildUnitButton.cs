using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuildUnitButton : MonoBehaviour {
    
    public Button button;
    public Image background;
    public RawImage rawImage;
    public TextMeshProUGUI textCost, textTime;
    public RectTransform rectCost, rectTime;
    public Unit unitPrefab;

    private GameObject unitObject;

    void Awake() {
        unitObject = Instantiate(unitPrefab.model.gameObject);
        unitObject.transform.rotation = Quaternion.Euler(new Vector3(10, 135, -10));
        unitObject.transform.localScale = Vector3.one * 1.5f;
        HUD.instance.objectUIRenderer.AddObject(unitObject, rawImage);

        textCost.text = unitPrefab.buildCost.ToString();
        textTime.text = unitPrefab.buildTime + "s";

        rectCost.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textCost.preferredWidth + 10);
        rectTime.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textTime.preferredWidth + 10);
    }

    void Update() {
        button.interactable = PlayerController.instance.gems >= unitPrefab.buildCost;
        GameUtil.SetImageAlpha(background, button.interactable ? 1 : 0.6f);
    }

    void OnDestroy() {
        HUD.instance.objectUIRenderer.RemoveObject(unitObject, true);
    }

    public void OnClick() {
        PlayerController.instance.playerBase.AddUnitToQueue(unitPrefab);
    }

    // public void OnPointerEnter(PointerEventData eventData) {
    //     HUD.instance.ShowTooltip(eventData);
    // }

    // public void OnPointerExit(PointerEventData eventData) {
    // }
}
