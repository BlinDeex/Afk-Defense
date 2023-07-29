using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCircle : MonoBehaviour
{
    Transform _parent;
    [SerializeField] float _rotationSpeed = 50f;
    private void Awake()
    {
        _parent = gameObject.GetComponentInParent<Transform>();
    }

    private void Update()
    {
        transform.RotateAround(_parent.position, new Vector3(0, 0, 1f), _rotationSpeed * Time.deltaTime);
    }
}
