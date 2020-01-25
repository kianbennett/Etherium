using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public GameObject explosionParticlePrefab;

    private WorldObject target;
    private float speed;
    private int damage;

    void Awake() {
        // Incase it misses the target for some reason
        Destroy(gameObject, 5);
    }

    void Update() {
        if(target != null) transform.forward = (target.transform.position + Vector3.up * 0.5f) - transform.position;
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void Init(WorldObject target, float speed, int damage) {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    void OnTriggerEnter(Collider other) {
        WorldObject worldObject = other.gameObject.GetComponent<WorldObject>();
        if(worldObject != null && worldObject == target) {
            if(worldObject is Unit) ((Unit) worldObject).Damage(damage);
            if(worldObject is Structure) ((Structure) worldObject).Damage(damage);
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
