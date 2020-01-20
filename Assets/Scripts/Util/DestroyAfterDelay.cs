using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour {

    public float lifetime;

    void Awake() {
        Destroy(gameObject, lifetime);
    }
}
