using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StructureDefenceTower : Structure
{
    public Bullet bulletPrefab;
    public Transform bulletSpawnPoint;
    public MeshRenderer targetIndicatorMesh;
    public LineRenderer targetLine;
    public Material matNoTarget, matTarget;

    public float bulletSpeed, fireInterval;
    public int damage;
    public float range;

    private Unit target;
    private float fireTick;

    protected override void Update()
    {
        base.Update();

        // Gets all units within range, ordered by distance (closest first)
        Unit[] unitsInRange = World.instance.units.Where(o => Vector3.Distance(o.transform.position, transform.position) <= range && o.OwnerId != OwnerId)
            .OrderBy(o => Vector3.Distance(o.transform.position, transform.position)).ToArray();

        targetLine.gameObject.SetActive(unitsInRange.Length > 0);

        if (unitsInRange.Length > 0)
        {
            target = unitsInRange[0];
            targetIndicatorMesh.material = matTarget;

            fireTick += Time.deltaTime;
            if (fireTick >= fireInterval)
            {
                ShootBullet(target);
                fireTick = 0;
            }

            targetLine.SetPositions(new Vector3[] { transform.position + Vector3.up * 1.2f, target.transform.position + Vector3.up * 0.5f });
        }
        else
        {
            target = null;
            targetIndicatorMesh.material = matNoTarget;

            fireTick = 0;
        }
    }

    public void ShootBullet(Unit target)
    {
        Bullet bullet = Instantiate(bulletPrefab.gameObject, bulletSpawnPoint.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.Init(target, bulletSpeed, damage);
    }
}
