using UnityEngine;

public class GuardingCircleEnemy : BaseEnemy
{
    [SerializeField] GuardingCircleShield[] _circleShields;
    [SerializeField] float _rotationSpeed;
    [SerializeField] Transform _centerSpriteT;

    private void FixedUpdate()
    {
        base.BaseFixedUpdate();
    }

    private void Update()
    {
        _centerSpriteT.RotateAround(_centerSpriteT.position, new Vector3(0, 0, 1f), _rotationSpeed * Time.deltaTime);
    }

    public override void PrepareEnemy(float health, float movingPower, int uid)
    {
        base.PrepareEnemy(health, movingPower, uid);
        
        foreach(GuardingCircleShield circleShield in _circleShields)
        {
            circleShield.PrepareShield(health);
            circleShield.gameObject.SetActive(true);
        }
    }

    public override void ReturnEnemy()
    {
        foreach (GuardingCircleShield circleShield in _circleShields)
        {
            circleShield.DeactivateShield();
            circleShield.gameObject.SetActive(false);
        }

        base.ReturnEnemy();
    }
}
