using UnityEngine;

public class ShieldCircle : MonoBehaviour
{
    [SerializeField] Transform _parent;
    [SerializeField] float _rotationSpeed = 50f;

    private void Update()
    {
        transform.RotateAround(_parent.position, new Vector3(0, 0, 1f), _rotationSpeed * Time.deltaTime);
    }
}
