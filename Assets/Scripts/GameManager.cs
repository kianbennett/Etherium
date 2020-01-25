using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [ReadOnly] public float timer;
    [ReadOnly] public int gems, minerals;
    [ReadOnly] public int warehouses;
    public int MaxGems { get { return 500 + warehouses * 500; } }
    public int MaxMinerals { get { return 2000 + warehouses * 2000; } }

    [ReadOnly] public bool isPaused;

    protected override void Awake() {
        base.Awake();

        // TODO: Remove this before release
        gems = MaxGems;
        minerals = MaxMinerals;
    }

    void Update() {
        timer += Time.deltaTime;
    }

    public void AddGems(int value) {
        gems += value;
        gems = Mathf.Clamp(gems, 0, MaxGems);
    }

    public void AddMinerals(int value) {
        minerals += value;
        minerals = Mathf.Clamp(minerals, 0, MaxMinerals);
    }

    public bool IsAtMaxResource(ResourceType type) {
        if(type == ResourceType.Gem) {
            return gems >= MaxGems;
        }
        if(type == ResourceType.Mineral) {
            return minerals >= MaxMinerals;
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
