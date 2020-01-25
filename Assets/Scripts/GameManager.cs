using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [ReadOnly] public float timer;
    [ReadOnly] public int gems, minerals;
    public int maxGems, maxMinerals;

    [ReadOnly] public bool isPaused;

    protected override void Awake() {
        base.Awake();
    }

    void Update() {
        timer += Time.deltaTime;
    }

    public void AddGems(int value) {
        gems += value;
        gems = Mathf.Clamp(gems, 0, maxGems);
    }

    public void AddMinerals(int value) {
        minerals += value;
        minerals = Mathf.Clamp(minerals, 0, maxMinerals);
    }

    public bool IsAtMaxResource(ResourceType type) {
        if(type == ResourceType.Gem) {
            return gems >= maxGems;
        }
        if(type == ResourceType.Mineral) {
            return minerals >= maxMinerals;
        }    
        return false;
    }

    public void SetPaused(bool paused) {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        CameraController.instance.SetBlurEnabled(paused);
        HUD.instance.pausedOverlay.SetActive(paused);
    }

    public void TogglePaused() {
        SetPaused(!isPaused);
    }
}
