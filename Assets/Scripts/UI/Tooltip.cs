using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour {

    public TextMeshProUGUI textContent;
    public RectTransform rect;
    public float maxWidth;
    [ReadOnly] public bool showTooltip;
    // [ReadOnly] public Vector2 tooltipPos;

    void LateUpdate() {
        SetPos(Input.mousePosition);
    }

    public void SetPos(Vector2 pos) {
        float x = 0, y = 0;
        if(pos.x < Screen.width - rect.sizeDelta.x) {
            x = pos.x + rect.sizeDelta.x / 2;
        } else {
            x = pos.x - rect.sizeDelta.x / 2;
        }
        if(pos.y < Screen.height - rect.sizeDelta.y) {
            y = pos.y + rect.sizeDelta.y / 2;
        } else {
            y = pos.y - rect.sizeDelta.y / 2;
        }
        transform.position = new Vector2(x, y);
    }

    public void SetContent(string text) {
        // Reset rect size to preferredSize gets set properly (and not based on the previous size)
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxWidth);
        
        textContent.text = text;
        float rectWidth = Mathf.Clamp(textContent.preferredWidth, 0, maxWidth);
        float rectHeight = textContent.preferredHeight;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectWidth + 16);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectHeight + 16);
    }

    public void SetContent(string header, string body) {
        string s = header;
        if(body != "") s += "\n<color=#dddddddd><size=20>" + body;
        SetContent(s);
    }
}
