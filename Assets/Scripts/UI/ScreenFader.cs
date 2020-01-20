using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  Controls a full screen overlay that fades in and out
 */

public class ScreenFader : MonoBehaviour {

    public Image Image;
    public CanvasGroup CanvasGroup;

    private Coroutine fadeInCoroutine, fadeOutCoroutine;

    public Coroutine FadeIn(float duration = 0.25f, Color color = default) {
        color.a = 1;
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        fadeInCoroutine = StartCoroutine(fadeInIEnum(duration, color));
        return fadeInCoroutine;
    }

    public Coroutine FadeOut(float duration = 0.25f) {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        fadeOutCoroutine = StartCoroutine(fadeOutIEnum(duration));
        return fadeOutCoroutine;
    }

    private IEnumerator fadeInIEnum(float duration, Color color) {
        CanvasGroup.alpha = 1;
        Image.color = color;
        while(CanvasGroup.alpha > 0) {
            CanvasGroup.alpha -= Time.deltaTime * (1f / duration);
            yield return null;
        }
    }

    private IEnumerator fadeOutIEnum(float duration) {
        CanvasGroup.alpha = 0;
        while (CanvasGroup.alpha < 1) {
            CanvasGroup.alpha += Time.deltaTime * (1f / duration);
            yield return null;
        }
    }

    public void SetValues(Color color, float alpha) {
        Image.color = color;
        CanvasGroup.alpha = alpha;
    }
}
