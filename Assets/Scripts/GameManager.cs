using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : Singleton<GameManager> {

    public enum GameState { InGame, Victory, Defeat }

    [ReadOnly] public float timer;
    [ReadOnly] public bool isPaused, isSpedUp;
    [ReadOnly] public GameState gameState;

    public static bool quitting;

    protected override void Awake() {
        base.Awake();

        HUD.instance.screenFader.FadeIn(null, 0.5f);
        quitting = false;
    }

    void Update() {
        timer += Time.deltaTime;

        if(gameState == GameState.InGame) {
            // If the player has no more harvester units and no gems to create more then they lose
            UnitHarvester[] harvesters = PlayerController.instance.ownedUnits.Where(o => o is UnitHarvester).Select(o => (UnitHarvester) o).ToArray();
            if (harvesters.Length == 0 && PlayerController.instance.gems < World.instance.unitBuilderPrefab.buildCost) {
                Defeat();
            }
        }

        if(Input.GetKeyDown(KeyCode.K)) Victory();
        if(Input.GetKeyDown(KeyCode.L)) Defeat();
    }

    public void SetPaused(bool paused) {
        isPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        CameraController.instance.SetBlurEnabled(paused);
        HUD.instance.pausedOverlay.SetActive(paused);
        if(isSpedUp) SetSpedUp(false);
    }

    public void TogglePaused() {
        SetPaused(!isPaused);
    }

    public void SetSpedUp(bool spedUp) {
        isSpedUp = spedUp;
        Time.timeScale = spedUp ? 8 : 1;
        if(isPaused) SetPaused(false);
    }

    public void ToggleSpedUp() {
        SetSpedUp(!isSpedUp);
    }

    public int GetResourceAmount(int playerId, ResourceType type) {
        if(playerId == 0) {
            if(type == ResourceType.Gem) return PlayerController.instance.gems;
            if(type == ResourceType.Mineral) return PlayerController.instance.minerals;
        }
        if(playerId == 1) {
            if(type == ResourceType.Gem) return EnemyController.instance.gems;
            if(type == ResourceType.Mineral) return EnemyController.instance.minerals;
        }
        return 0;
    }

    public void Victory() {
        gameState = GameState.Victory;
        Time.timeScale = 0;
        int minutes = (int) (timer / 60);
        int seconds = (int) timer % 60;
        HUD.instance.textFinalTime.text = "Final Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        HUD.instance.victoryScreen.SetActive(true);
    }
    
    public void Defeat() {
        gameState = GameState.Defeat;
        HUD.instance.defeatScreen.SetActive(true);
        Time.timeScale = 0;
    }

    void OnApplicationQuit() {
        quitting = true;
    }
}
