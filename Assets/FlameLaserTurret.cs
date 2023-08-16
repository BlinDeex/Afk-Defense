using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameLaserTurret : BaseTurret
{
    [Header("Stats")]
    [SerializeField] float _minDamage;
    [SerializeField] float _maxDamage;
    
    [SerializeField] int _flameHitEffectMinParticles;
    [SerializeField] int _flameHitEffectMaxParticles;
    [SerializeField] float _flameRampUpSpeed;
    [SerializeField] float _laserBreakParticleFlatCount;
    [SerializeField] float _laserBreakParticleCountDistanceMultiplier;
    [SerializeField] float _laserBreakParticleMinSize;
    [SerializeField] float _laserBreakParticleMaxSize;
    [SerializeField] float _maximumFlameLineWidth;
    [SerializeField] float _flameHitEffectMaxSize;

    [Header("Setup")]
    [SerializeField] ParticleSystem _flameHitEffect;
    [SerializeField] SpriteRenderer _flameLaserTurretSprite;
    [SerializeField] Color _flameLaserStartColor;
    [SerializeField] Color _flameLaserMaxColor;
    [SerializeField] ParticleSystem _laserBreakEffect;
    [SerializeField] LineRenderer _flameLineRenderer;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] EasingsList _easingType;
    [SerializeField] int _ticksBeforeRetargeting;
    int _currentRetargetingCooldown;

    int _targetLayerMask;
    float _flameRampUpValue;
    float _rampUpLerper;
    private Vector2 _raycastHitPoint;
    private Vector2 _lastNormalHit;

    private void Awake()
    {
        _targetLayerMask = LayerMask.GetMask("Enemy", "NonEnemyObstacle");
        _flameLineRenderer.SetPositions(new Vector3[] {transform.position, transform.position});
    }

    private void FixedUpdate()
    {
        if (_currentRetargetingCooldown > 0)
        {
            _currentRetargetingCooldown--;
            return;
        }

        UpdateTarget();

        if (TargetChanged)
        {
            LostTarget();
            return;
        }

        if (!HasTarget) return;

        if (_flameRampUpValue < 1)
        {
            _flameRampUpValue += Time.fixedDeltaTime * _flameRampUpSpeed;
            _rampUpLerper = Easings.RunEasingType(_easingType, _flameRampUpValue);
        }

        Vector3 dirToEnemy = CurrentTarget.transform.position - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, dirToEnemy,
            Vector3.Distance(transform.position, CurrentTarget.transform.position), _targetLayerMask);

        if(hit == true)
        {
            _raycastHitPoint = hit.point;
            _lastNormalHit = hit.normal;
        }
        

        UpdateVisuals();
        SpawnProjectile();
    }

    void UpdateVisuals()
    {
        _flameLaserTurretSprite.color = Color.Lerp(_flameLaserStartColor, _flameLaserMaxColor, _rampUpLerper);
        Vector3 targetPos = _raycastHitPoint;
        _flameLineRenderer.SetPosition(1, targetPos);
        _flameLineRenderer.widthMultiplier = _rampUpLerper * _maximumFlameLineWidth;
        _flameHitEffect.transform.position = targetPos;
        var main = _flameHitEffect.main;
        main.startSize = _rampUpLerper * _flameHitEffectMaxSize;
        int particlesCount = (int)Mathf.Lerp(_flameHitEffectMinParticles, _flameHitEffectMaxParticles, _rampUpLerper);
        _flameHitEffect.Emit(particlesCount);
    }

    void SpawnProjectile()
    {
        GameObject projectile = DynamicObjectPooler.Instance.RequestProjectile(_projectilePrefab);
        FlameLaserTurretProjectile FLTP = projectile.GetComponent<FlameLaserTurretProjectile>();
        float currentDamage = Mathf.Lerp(_minDamage, _maxDamage, _rampUpLerper);
        FLTP.PrepareProjectile(currentDamage, _lastNormalHit);
        FLTP.transform.position = _raycastHitPoint;
        FLTP.gameObject.SetActive(true);
    }

    void LostTarget()
    {
        HasTarget = false;
        
        _currentRetargetingCooldown = _ticksBeforeRetargeting;

        _flameLineRenderer.widthMultiplier = 0;
        _flameLineRenderer.SetPosition(0, transform.position);
        _flameLaserTurretSprite.color = _flameLaserStartColor;
        ParticleSystem lineBreak = DynamicObjectPooler.Instance.BorrowEffect(_laserBreakEffect);
        Vector3 _flameLinePos0 = _flameLineRenderer.GetPosition(0);
        Vector3 _flameLinePos1 = _flameLineRenderer.GetPosition(1);
        Vector3 lineCenter = (_flameLinePos0 + _flameLinePos1) / 2;
        float _flameLineDistance = Vector3.Distance(_flameLinePos0, _flameLinePos1);

        var shape = lineBreak.shape;
        var main = lineBreak.main;
        
        float particleSize = Mathf.Lerp(_laserBreakParticleMinSize, _laserBreakParticleMaxSize, _rampUpLerper);
        main.startSize = particleSize;
        shape.radius = _flameLineDistance / 2f;
        lineBreak.transform.position = lineCenter;
        lineBreak.transform.right = (_flameLinePos0 - _flameLinePos1).normalized;
        int particlesToEmit = (int)((_flameLineDistance * _laserBreakParticleCountDistanceMultiplier) + _laserBreakParticleFlatCount);
        lineBreak.Emit(particlesToEmit);
        DynamicObjectPooler.Instance.ReturnBorrowedEffect(lineBreak);

        _flameRampUpValue = 0;
        _rampUpLerper = 0;
    }

    public override void ApplyUpgrade(int index)
    {

    }

    public override void PostUpgradeVisualRefresh()
    {

    }
}
