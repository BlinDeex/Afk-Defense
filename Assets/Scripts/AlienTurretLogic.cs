using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienTurretLogic : BaseTurret
{
    [Serializable]
    public class Beam
    {
        public Transform BeamOrigin;
        public LineRenderer BeamLineRenderer;
        public ParticleSystem HitFastShatter;
        public ParticleSystem HitSlowShatter;
        public ParticleSystem HitCenterEffect;
        public Vector2 RaycastHitPoint;
        public Vector2 RaycastHitNormal;
    }

    [SerializeField] EasingsList _easingType;

    [SerializeField] Beam[] _beams;
    [SerializeField] float _damage;
    [SerializeField] Transform _beamOriginsRoot;

    [SerializeField] float _maxRotationSpeed;
    [SerializeField] float _rotationRampUpSpeed;
    [SerializeField] float _rotationCooldownSpeed;
    [SerializeField] int _hitFastShatterParticleCount;
    [SerializeField] int _hitSlowShatterParticleCount;
    [SerializeField] int _hitCenterEffectParticleCount;
    [SerializeField] float _hitFastShatterParticleSize;
    [SerializeField] float _hitSlowShatterParticleSize;
    [SerializeField] float _hitCenterEffectParticleSize;
    [SerializeField] float _laserBreakParticleFlatCount;
    [SerializeField] float _laserBreakParticleCountDistanceMultiplier;

    [SerializeField] float _percentDamageIncrease;
    [SerializeField] int _effectLengthInTicks;
    [SerializeField] int _alienatedEffectEmissionRate;

    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] ParticleSystem _laserBreakEffect;

    [SerializeField] GameObject _innerCircle;
    Material _mat;

    float _rampUpValue;
    float _rampUpLerper;

    int _targetLayerMask;

    private void Awake()
    {
        BaseEarlyAwake();
        _targetLayerMask = LayerMask.GetMask("Enemy", "NonEnemyObstacle");
        _mat = _innerCircle.GetComponent<SpriteRenderer>().material;
        foreach(Beam beam in _beams)
        {
            beam.BeamLineRenderer.SetPositions(new Vector3[] { beam.BeamOrigin.position, beam.BeamOrigin.position });
        }
    }

    private void Update()
    {
        _beamOriginsRoot.Rotate(new Vector3(0, 0, Time.deltaTime * Mathf.Lerp(0, _maxRotationSpeed, _rampUpLerper)));
    }

    private void FixedUpdate()
    {
        BaseEarlyFixedUpdate();
        UpdateTarget();

        if (TargetChanged || JustLostTarget)
        {
            
            LostTarget();
            return;
        }

        if (!HasTarget)
        {
            if (_rampUpValue > 0) _rampUpValue -= Time.fixedDeltaTime * _rotationRampUpSpeed;
            _rampUpLerper = Easings.RunEasingType(_easingType, _rampUpValue);
            _mat.SetFloat("_Lerp", _rampUpLerper);
            return;
        }
        else if(_rampUpValue < 1)
        {
            _rampUpValue += Time.fixedDeltaTime * _rotationCooldownSpeed;
            _rampUpLerper = Easings.RunEasingType(_easingType, _rampUpValue);
            _mat.SetFloat("_Lerp", _rampUpLerper);
        }

        if (_rampUpValue < 1) return;

        for(int i = 0; i < _beams.Length; i++)
        {
            TargetingForEachBeam(_beams[i]);
        }
        BaseLateFixedUpdate();
    }

    private void TargetingForEachBeam(Beam beam)
    {
        Vector3 dirToEnemy = CurrentTarget.transform.position - transform.position;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, dirToEnemy,
            Vector3.Distance(transform.position, CurrentTarget.transform.position), _targetLayerMask);

        if (hit == true)
        {
            beam.RaycastHitNormal = hit.normal;
            beam.RaycastHitPoint = hit.point;
        }

        UpdateVisuals(beam);
        SpawnProjectile(beam);
    }

    private void LostTarget()
    {
        HasTarget = false;
        if (_rampUpValue < 1) return;

        foreach (Beam beam in _beams)
        {
            
            ParticleSystem lineBreak = DynamicObjectPooler.Instance.BorrowEffect(_laserBreakEffect);
            Vector3 linePos0 = beam.BeamLineRenderer.GetPosition(0);
            Vector3 linePos1 = beam.BeamLineRenderer.GetPosition(1);
            Vector3 lineCenter = (linePos0 + linePos1) / 2;
            float _lineDistance = Vector3.Distance(linePos0, linePos1);

            var shape = lineBreak.shape;

            shape.radius = _lineDistance / 2f;
            lineBreak.transform.position = lineCenter;
            lineBreak.transform.right = (linePos0 - linePos1).normalized;
            int particlesToEmit = (int)((_lineDistance * _laserBreakParticleCountDistanceMultiplier) + _laserBreakParticleFlatCount);
            lineBreak.Emit(particlesToEmit);
            DynamicObjectPooler.Instance.ReturnBorrowedEffect(lineBreak);
            beam.BeamLineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        }
    }

    private void UpdateVisuals(Beam beam)
    {
        Vector3 targetPos = beam.RaycastHitPoint;
        
        beam.BeamLineRenderer.SetPositions(new Vector3[] {beam.BeamOrigin.position, targetPos});
        beam.HitFastShatter.transform.position = targetPos;
        beam.HitSlowShatter.transform.position = targetPos;
        beam.HitCenterEffect.transform.position = targetPos;

        var HFSMain = beam.HitFastShatter.main;
        var HSSMain = beam.HitSlowShatter.main;
        var HCEMain = beam.HitCenterEffect.main;

        HFSMain.startSize = _hitFastShatterParticleSize;
        HSSMain.startSize = _hitSlowShatterParticleSize;
        HCEMain.startSize = _hitCenterEffectParticleSize;

        beam.HitFastShatter.Emit(_hitFastShatterParticleCount);
        beam.HitSlowShatter.Emit(_hitSlowShatterParticleCount);
        beam.HitCenterEffect.Emit(_hitCenterEffectParticleCount);


    }

    private void SpawnProjectile(Beam beam)
    {
        GameObject projectile = DynamicObjectPooler.Instance.RequestProjectile(_projectilePrefab);
        AlienProjectile AP = projectile.GetComponent<AlienProjectile>();
        AP.PrepareProjectile(_damage, beam.RaycastHitNormal, _percentDamageIncrease, _effectLengthInTicks,
            _alienatedEffectEmissionRate);
        
        AP.transform.position = beam.RaycastHitPoint;
        AP.gameObject.SetActive(true);
    }


    public override void PostUpgradeVisualRefresh()
    {

    }
}
