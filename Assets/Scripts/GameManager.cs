using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

    [ReadOnly] public float timer;
    public int gems, minerals;

    void Update() {
        timer += Time.deltaTime;
    }
}
