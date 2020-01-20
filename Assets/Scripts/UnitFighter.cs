using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFighter : Unit {

    public Bullet bulletPrefab;
    
    private Unit target;
    private List<Bullet> bullets = new List<Bullet>();

    public void ShootBullet(Unit target) {
        
    }
}
