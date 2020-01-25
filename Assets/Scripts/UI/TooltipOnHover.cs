using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public string header, body;

    public void OnPointerEnter(PointerEventData eventData) {
        if(header == "") {
            HUD.instance.ShowTooltip(body);
        } else {
            HUD.instance.ShowTooltip(header, body);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        HUD.instance.HideTooltip();
    }
}
