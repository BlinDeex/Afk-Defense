using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : BaseEnemy
{
    private void FixedUpdate()
    {
        base.BaseFixedUpdate();
    }

    private void Awake()
    {
        BaseAwake();
    }
}
