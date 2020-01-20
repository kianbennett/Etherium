using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil {

    public static void SetMaterialAlpha(Material material, float a) {
        Color colour = material.color;
        colour.a = a;
        material.color = colour;
    }
}
