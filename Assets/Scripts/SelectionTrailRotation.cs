using UnityEngine;

public class SelectionTrailRotation : MonoBehaviour
{
    [field: SerializeField] public Transform Parent { get; private set; }
    [SerializeField] float distance = 0.5f;
    [SerializeField] float speed = -90;
    [SerializeField] double startingAngle;
    public bool IsActive { get; set; } = true;

    private void Start()
    {
        UpdateAngle(startingAngle);
    }

    private void Update()
    {
        if (!IsActive) return;

        transform.RotateAround(Parent.position, new Vector3(0, 0, distance), speed * Time.deltaTime);
    }

    public void PrepareEffect(float distance, float speed, double startingAngle, Transform parent)
    {
        this.distance = distance;
        this.speed = speed;
        this.startingAngle = startingAngle;
        Parent = parent;
    }

    public void UpdateAngle(double targetAngle)
    {
        double offset = 360d - transform.rotation.eulerAngles.z;
        transform.RotateAround(Parent.position, new Vector3(0, 0, 0.5f), (float)(targetAngle + offset));
    }
}
