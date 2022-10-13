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
    
    private WorldObject target;
    private float fireTick;
    private bool isInRange;    
    private bool isMovingToTarget;

    protected override void Start() {
        base.Start();
        if(ownerId == 1) {
            EnemyController.instance.fighterUnits.Add(this);
        }
    }

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

            foreach(Unit unit in World.instance.units) {
                if(unit.ownerId != ownerId && Vector2Int.Distance(unit.tile.pos, tile.pos) <= range) {
                    Attack(unit);
                    break;
                }
            }
        }
    }

    public void Attack(WorldObject target) {
        if(this.target == target) return;
        this.target = target;
        float distToTarget = Vector2Int.Distance(target.tile.pos, tile.pos);
        fireTick = fireInterval - fireChargeupTime;
        if(distToTarget > range) {
            // Pathfinder will reject a path towards a structure tile, so temporarily set the tile type to ground to get a path
            // Bit of a hack, should be changed later
            if(target is Structure) {
                target.tile.type = TileType.Ground;
                MoveAndKeepTarget(target.tile);
                target.tile.type = TileType.Structure;
            } else {
                MoveAndKeepTarget(target.tile);
            }
            isMovingToTarget = true;
        }
    }

    public void ShootBullet(WorldObject target) {
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

    protected override void OnDestroy() {
        base.OnDestroy();
        if(ownerId == 1 && !GameManager.IsQuitting) {
            EnemyController.instance.fighterUnits.Remove(this);
        }
    }
}
