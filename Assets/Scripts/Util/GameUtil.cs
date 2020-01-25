using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUtil {

    public static void SetMaterialAlpha(Material material, float a) {
        Color colour = material.color;
        colour.a = a;
        material.color = colour;
    }

    public static void SetImageAlpha(Image image, float a) {
        Color colour = image.color;
        colour.a = a;
        image.color = colour;
    }
}
