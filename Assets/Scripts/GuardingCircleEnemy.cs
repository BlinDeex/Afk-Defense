using UnityEngine;

public class GuardingCircleEnemy : BaseEnemy
{
    [SerializeField] GuardingCircleShield[] _circleShields;
    [SerializeField] float _rotationSpeed;
    [SerializeField] Transform _centerSpriteT;
    [SerializeField] ParticleSystem _killEffect;
    [SerializeField] int _killParticlesCount;
    Transform _t;

    private void Awake()
    {
        BaseAwake();

        _t = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        base.BaseFixedUpdate();
    }

    private void Update()
    {
        _centerSpriteT.RotateAround(_centerSpriteT.position, new Vector3(0, 0, 1f), _rotationSpeed * Time.deltaTime);
    }

    public override void PrepareEnemy(float health, float movingPower, int uid, float damage, int bounty)
    {
        base.PrepareEnemy(health, movingPower, uid, damage, bounty);
        
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
            circleShield.BreakShield();
            circleShield.gameObject.SetActive(false);
        }

        DynamicObjectPooler.Instance.RequestInstantEffect(_killEffect, _t.position, _killParticlesCount);

        base.ReturnEnemy();
    }
}
