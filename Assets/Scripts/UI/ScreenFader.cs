using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
 *  Controls a full screen overlay that fades in and out
 */

public class ScreenFader : MonoBehaviour {

    public Image Image;
    public CanvasGroup CanvasGroup;

    private Coroutine fadeInCoroutine, fadeOutCoroutine;

    public Coroutine FadeIn(UnityAction onComplete, float duration = 0.25f, Color color = default) {
        color.a = 1;
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        fadeInCoroutine = StartCoroutine(fadeInIEnum(onComplete, duration, color));
        return fadeInCoroutine;
    }

    public Coroutine FadeOut(UnityAction onComplete, float duration = 0.25f) {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        fadeOutCoroutine = StartCoroutine(fadeOutIEnum(onComplete, duration));
        return fadeOutCoroutine;
    }

    private IEnumerator fadeInIEnum(UnityAction onComplete, float duration, Color color) {
        CanvasGroup.alpha = 1;
        Image.color = color;
        while(CanvasGroup.alpha > 0) {
            CanvasGroup.alpha -= Time.deltaTime * (1f / duration);
            yield return null;
        }
        if(onComplete != null) onComplete();
    }

    private IEnumerator fadeOutIEnum(UnityAction onComplete, float duration) {
        CanvasGroup.alpha = 0;
        while (CanvasGroup.alpha < 1) {
            CanvasGroup.alpha += Time.deltaTime * (1f / duration);
            yield return null;
        }
        if(onComplete != null) onComplete();
    }

    public void SetValues(Color color, float alpha) {
        Image.color = color;
        CanvasGroup.alpha = alpha;
    }
}
