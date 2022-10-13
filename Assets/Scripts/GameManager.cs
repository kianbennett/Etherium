using UnityEngine;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { InGame, Victory, Defeat }

    [ReadOnly] public float Timer;
    [ReadOnly] public bool IsPaused, IsSpedUp;
    [ReadOnly] public GameState State;

    public static bool IsQuitting;

    protected override void Awake()
    {
        base.Awake();

        HUD.instance.screenFader.FadeIn(null, 0.5f);
        IsQuitting = false;
    }

    void Update()
    {
        Timer += Time.deltaTime;

        if (State == GameState.InGame)
        {
            // If the player has no more harvester units and no gems to create more then they lose
            UnitHarvester[] harvesters = PlayerController.instance.ownedUnits.Where(o => o is UnitHarvester).Select(o => (UnitHarvester)o).ToArray();
            if (harvesters.Length == 0 && PlayerController.instance.gems < World.instance.unitBuilderPrefab.buildCost)
            {
                Defeat();
            }
        }

        if (Input.GetKeyDown(KeyCode.K)) Victory();
        if (Input.GetKeyDown(KeyCode.L)) Defeat();
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0 : 1;
        CameraController.instance.SetBlurEnabled(paused);
        HUD.instance.pausedOverlay.SetActive(paused);
        if (IsSpedUp) SetSpedUp(false);
    }

    public void TogglePaused()
    {
        SetPaused(!IsPaused);
    }

    public void SetSpedUp(bool spedUp)
    {
        IsSpedUp = spedUp;
        Time.timeScale = spedUp ? 8 : 1;
        if (IsPaused) SetPaused(false);
    }

    public void ToggleSpedUp()
    {
        SetSpedUp(!IsSpedUp);
    }

    public int GetResourceAmount(int playerId, ResourceType type)
    {
        if (playerId == 0)
        {
            if (type == ResourceType.Gem) return PlayerController.instance.gems;
            if (type == ResourceType.Mineral) return PlayerController.instance.minerals;
        }
        if (playerId == 1)
        {
            if (type == ResourceType.Gem) return EnemyController.instance.gems;
            if (type == ResourceType.Mineral) return EnemyController.instance.minerals;
        }
        return 0;
    }

    public void Victory()
    {
        State = GameState.Victory;
        Time.timeScale = 0;
        int minutes = (int)(Timer / 60);
        int seconds = (int)Timer % 60;
        HUD.instance.textFinalTime.text = "Final Time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        HUD.instance.victoryScreen.SetActive(true);
    }

    public void Defeat()
    {
        State = GameState.Defeat;
        HUD.instance.defeatScreen.SetActive(true);
        Time.timeScale = 0;
    }

    void OnApplicationQuit()
    {
        IsQuitting = true;
    }
}
