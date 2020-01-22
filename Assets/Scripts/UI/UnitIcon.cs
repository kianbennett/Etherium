using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitIcon : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

    public Healthbar healthbar;
    public Image border;
    public RawImage rawImage;
    
    private Unit unit;
    private GameObject unitObject;
    private bool isHovered;

    void Update() {
        if(unit != null) {
            healthbar.SetPercentage(unit.healthCurrent / unit.healthMax);
            bool borderActive = unit.IsSelected() || isHovered;
            border.gameObject.SetActive(borderActive);
            if(borderActive) {
                border.color = unit.IsSelected() ? unit.outlineColourSelected : unit.outlineColourHovered;
            }
        }
    }

    public void SetUnit(Unit unit) {
        this.unit = unit;

        if(unitObject != null) {
            HUD.instance.objectUIRenderer.RemoveObject(unitObject, true);                
        }

        unitObject = Instantiate(unit.model.gameObject);
        unitObject.transform.rotation = Quaternion.Euler(new Vector3(10, 135, -10));
        unitObject.transform.localScale = Vector3.one * 1.5f;
        HUD.instance.objectUIRenderer.AddObject(unitObject, rawImage);
    }

    void OnDestroy() {
        HUD.instance.objectUIRenderer.RemoveObject(unitObject, true);
    }

    public void OnPointerDown(PointerEventData eventData) {
        PlayerController.instance.DeselectAll();
        PlayerController.instance.SelectObject(unit);
        CameraController.instance.SetAbsolutePosition(unit.transform.position);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
    }
}
