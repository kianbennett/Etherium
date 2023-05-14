using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Transform barContainer;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector3 offset;

    public void SetPercentage(float value)
    {
        barContainer.localScale = new Vector3(value, 1, 1);
    }

    public void SetWorldPos(Vector3 worldPos)
    {
        Vector3 pos = CameraController.instance.camera.WorldToScreenPoint(worldPos);
        transform.position = pos + offset * 70; // offset needs to be scaled up to screen coordinate range
    }

    public void SetOffset(Vector3 offset)
    {
        this.offset = offset;
    }

    public bool isOnScreen()
    {
        return transform.position.x > -rect.sizeDelta.x / 2 && transform.position.x < Screen.width + rect.sizeDelta.x / 2 &&
            transform.position.y > -rect.sizeDelta.y / 2 && transform.position.y < Screen.height + rect.sizeDelta.y / 2;
    }
}