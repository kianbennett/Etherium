using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public GameObject explosionParticlePrefab;

    private Unit target;
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

    public void Init(Unit target, float speed, int damage) {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    void OnTriggerEnter(Collider other) {
        Unit unit = other.gameObject.GetComponent<Unit>();
        if(unit != null && unit == target) {
            unit.Damage(damage);
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
