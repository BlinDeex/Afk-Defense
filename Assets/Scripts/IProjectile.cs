using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IProjectile
{
    public abstract void NonEnemyHitReturnProjectile();

    public abstract Vector2 ReturnRaycastHitNormal();

    public abstract Vector2 ReturnRaycastHitPoint();

    public abstract float ReturnProjectileDamage();

    public abstract Vector2 ReturnLastProjectileDirection();

    public abstract bool HasHitPointValue();
}
