using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/*
*   A transparent box that can is displayed when the player clicks and drags across the canvas
*/

[ExecuteInEditMode]
public class UISelectionBox : MonoBehaviour {

    public RectTransform Rect;
    public Image[] Lines;
    public Image Fill;

    public Color LineColourEmpty, FillColourEmpty;
    public Color LineColourSelected, FillColourSelected;

    private Color lineColour, fillColour; 
    private bool isEmpty;

    void Update () {
        //SetEmpty(PlayerController.Instance.SelectedCharacters.Count == 0);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void SetEmpty(bool empty) {
        if (empty == isEmpty) return;

        isEmpty = empty;
        setLineColour(empty ? LineColourEmpty : LineColourSelected);
        setFillColour(empty ? FillColourEmpty : FillColourSelected);
    }

    public void SetPos(Vector2 pos) {
        transform.position = pos;
    }

    public void SetSize(Vector2 size) {
        size = MathUtil.MakePositive(size);
        Rect.sizeDelta = size;
    }

    private void setLineColour(Color colour) {
        lineColour = colour;
        foreach (Image line in Lines) {
            line.color = lineColour;
        }
    }

    private void setFillColour(Color colour) {
        fillColour = colour;
        Fill.color = fillColour;
    }
}
