using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransparencyFromInteractable : MonoBehaviour {

    public Image imageToChange;
    public Selectable selectable;
    public float alpha;

    void Update() {
        GameUtil.SetImageAlpha(imageToChange, selectable.interactable ? 1 : alpha);
    }
}
