using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameLaserTurretProjectile : BaseProjectile
{
    bool _spawnedThisFrame;

    public override void NonEnemyHitReturnProjectile()
    {
        ReturnProjectile();
    }

    public void PrepareProjectile(float damage, Vector3 lastDir)
    {
        _projectileDamage = damage;
        _lastVelocityDirNormalized = lastDir;
        _spawnedThisFrame = true;
    }

    private void FixedUpdate()
    {
        if (_spawnedThisFrame)
        {
            _spawnedThisFrame = false;
        }
        else
        {
            ReturnProjectile();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out BaseEnemy enemy))
        {
            enemy.TakeDamage(_projectileDamage);
            ReturnProjectile();
            return;
        }
    }

    private void Awake()
    {
        BaseAwake();
    }

    void ReturnProjectile()
    {
        DynamicObjectPooler.Instance.ReturnProjectile(gameObject);
        gameObject.SetActive(false);
    }
}
