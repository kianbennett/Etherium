using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFighter : Unit {

    public Bullet bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed, fireInterval;
    public float fireChargeupTime; // The time taken from clicking on the enemy to the first shot being fired
    public int damage;
    public float range;
    
    private Unit target;
    private float fireTick;
    private bool isInRange;    
    private bool isMovingToTarget;

    protected override void Update() {
        base.Update();

        if(target != null) {
            float distToTarget = Vector2Int.Distance(target.tile.pos, tile.pos);
            isInRange = distToTarget <= range;

            if(isMovingToTarget && isInRange) {
                isMovingToTarget = false;
                movement.StopMoving();
            }

            if(!isMovingToTarget) {
                if(isInRange) {
                    fireTick += Time.deltaTime;
                    if(fireTick > fireInterval) {
                        fireTick = 0;
                        ShootBullet(target);
                    }
                    movement.LookAtTile(target.tile);
                } else {
                    MoveAndKeepTarget(target.tile);
                    isMovingToTarget = true;
                }
            }
        } else {
            fireTick = 0;
        }
    }

    public void Attack(Unit target) {
        if(this.target == target) return;
        this.target = target;
        float distToTarget = Vector2Int.Distance(target.tile.pos, tile.pos);
        fireTick = fireInterval - fireChargeupTime;
        if(distToTarget > range) {
            MoveAndKeepTarget(target.tile);
            isMovingToTarget = true;
        }
    }

    public void ShootBullet(Unit target) {
        Bullet bullet = Instantiate(bulletPrefab.gameObject, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.Init(target, bulletSpeed, damage);
    }

    public void MoveAndKeepTarget(TileData tile) {
        base.MoveToPoint(tile);
    }

    public override void MoveToPoint(TileData tile) {
        base.MoveToPoint(tile);
        target = null;
    }
}
