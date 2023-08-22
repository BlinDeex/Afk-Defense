using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquaTurretBufferCollider : MonoBehaviour
{
    [SerializeField] AquaTurretBuffer _parentTurret;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        _parentTurret.TurretCollided(collision.GetComponent<BaseTurret>());
    }
}
