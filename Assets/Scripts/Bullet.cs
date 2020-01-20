using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    private Unit target;
    private float speed;

    void Awake() {
        // Incase it misses the target for some reason
        Destroy(gameObject, 5);
    }

    void Update() {
        transform.LookAt(target.transform);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void Init(Unit target, float speed) {
        this.target = target;
        this.speed = speed;
    }

    void OnTriggerEnter(Collider other) {
        
    }
}
