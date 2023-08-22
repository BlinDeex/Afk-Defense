using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienTurretUIRotate : MonoBehaviour
{
    [SerializeField] float _rotateSpeed;

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, _rotateSpeed * Time.deltaTime));
    }
}
