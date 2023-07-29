using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class BarrageOrb : MonoBehaviour
{
    public Transform Parent { get; set; }

    public BarrageOrb gg;
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
