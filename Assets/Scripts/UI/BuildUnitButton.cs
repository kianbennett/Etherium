using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUnitButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public RawImage rawImage;
    public TextMeshProUGUI textCost, textTime;
    public RectTransform rectCost, rectTime;
    public Unit unitPrefab;

    private GameObject unitObject;

    void Awake()
    {
        unitObject = Instantiate(unitPrefab.Model.gameObject);
        unitObject.transform.rotation = Quaternion.Euler(new Vector3(10, 135, -10));
        unitObject.transform.localScale = Vector3.one * 1.5f;
        HUD.instance.objectUIRenderer.AddObject(unitObject, rawImage);

        textCost.text = unitPrefab.BuildCost.ToString();
        textTime.text = unitPrefab.BuildTime + "s";

        rectCost.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textCost.preferredWidth + 10);
        rectTime.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textTime.preferredWidth + 10);
    }

    void Update()
    {
        button.interactable = PlayerController.instance.gems >= unitPrefab.BuildCost;
        GameUtil.SetImageAlpha(background, button.interactable ? 1 : 0.6f);
    }

    void OnDestroy()
    {
        HUD.instance.objectUIRenderer.RemoveObject(unitObject, true);
    }

    public void OnClick()
    {
        PlayerController.instance.BaseStructure.AddUnitToQueue(unitPrefab);
    }
}
