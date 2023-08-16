using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    public Vector2 _lastNormalHit { get; protected set; }
    public Vector3 _lastVelocityDirNormalized { get; protected set; }
    public Vector3 _lastHitPoint { get; protected set; }

    public float _projectileDamage { get; protected set; }
    public bool _hasHitPoint { get; protected set; }

    public abstract void NonEnemyHitReturnProjectile();

    public int PROJECTILE_HITMASK { get; protected set; }

    public void BaseAwake()
    {
        PROJECTILE_HITMASK = LayerMask.GetMask("Enemy", "NonEnemyObstacle");
    }
}
