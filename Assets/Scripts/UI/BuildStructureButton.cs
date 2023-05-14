﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildStructureButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public RawImage rawImage;
    public TextMeshProUGUI textCost, textTime;
    public RectTransform rectCost, rectTime;
    public Structure structurePrefab;

    private GameObject structureObject;

    void Awake()
    {
        structureObject = Instantiate(structurePrefab.Model);
        structureObject.transform.rotation = Quaternion.Euler(new Vector3(10, 135, -10));
        // structureObject.transform.localScale = Vector3.one * 1.5f;
        HUD.instance.objectUIRenderer.AddObject(structureObject, rawImage);

        textCost.text = structurePrefab.BuildCost.ToString();
        textTime.text = structurePrefab.BuildTime + "s";

        rectCost.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textCost.preferredWidth + 10);
        rectTime.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textTime.preferredWidth + 10);
    }

    void Update()
    {
        button.interactable = PlayerController.instance.minerals >= structurePrefab.BuildCost;
        GameUtil.SetImageAlpha(background, button.interactable ? 1 : 0.6f);
    }

    void OnDestroy()
    {
        HUD.instance.objectUIRenderer.RemoveObject(structureObject, true);
    }

    public void OnClick()
    {
        PlayerController.instance.Build(structurePrefab);
    }
}
