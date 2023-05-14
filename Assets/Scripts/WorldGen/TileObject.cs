using UnityEngine;

public class TileObject : MonoBehaviour
{
    public TileData tileData;

    public Material groundMaterial, groundMaterialDark;
    public Renderer groundRenderer;

    void Awake()
    {
        SetDark(World.instance.fogOfWar.FogOfWarEnabled);
    }

    public virtual void SetDark(bool dark)
    {
        groundRenderer.material = dark ? groundMaterialDark : groundMaterial;
    }
}
