using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public Transform cameraTarget;
    public ScreenFader screenFader;

    void Awake() {
        screenFader.FadeIn(null, 0.25f);
    }

    void Update() {
        cameraTarget.Rotate(Vector3.up * Time.deltaTime * -15);
    }

    public void Play() {
        screenFader.FadeOut(delegate {
            SceneManager.LoadScene("GameScene");
        }, 0.5f);
        screenFader.CanvasGroup.blocksRaycasts = true;
    }

    public void Quit() {
        Application.Quit();
    }
}
