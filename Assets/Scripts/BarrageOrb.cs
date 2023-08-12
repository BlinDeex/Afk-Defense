using UnityEngine;

public class BarrageOrb : MonoBehaviour
{
    [field: SerializeField] public Transform Parent { get; set; }

    private void Update()
    {
        transform.RotateAround(Parent.position, new Vector3(0, 0, 0.5f), -90 * Time.deltaTime);
    }

    public void UpdateAngle(double targetAngle)
    {
        double offset = 360d - transform.rotation.eulerAngles.z;
        transform.RotateAround(Parent.position, new Vector3(0, 0, 0.5f), (float)(targetAngle + offset));
    }
}
