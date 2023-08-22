using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AquaTurretBuffer : BaseTurret
{

    [SerializeField] private int _ticksBetweenPulses;
    [SerializeField] private int _currentTicksCooldown;
    [SerializeField] private float _pulseSpeed;
    [SerializeField] private float _pulseRange;
    [SerializeField] private float _rangeMultiplierStrength;
    [SerializeField] private int _effectLengthInTicks;

    [Header("Setup")]
    [SerializeField] SpriteRenderer _pulseSpriteRenderer;
    [SerializeField] Transform _pulseSpriteT;
    [SerializeField] CircleCollider2D _col;

    [SerializeField] EasingsList _easingType;

    readonly float _initialScaleXY = 0.42f;
    bool _pulseActive;
    float _pulseLerpValue;

    private const float SPRITE_TO_COLLIDER_RATIO = 10f;

    public override void PostUpgradeVisualRefresh()
    {

    }

    private void Awake()
    {
        BaseEarlyAwake();
    }

    private void FixedUpdate()
    {
        BaseEarlyFixedUpdate();

        if (!_pulseActive)
        {
            _currentTicksCooldown--;
            if (_currentTicksCooldown <= 0) StartCoroutine(Pulse());
        }

        BaseLateFixedUpdate();
    }

    IEnumerator Pulse()
    {
        _pulseActive = true;
        while(_pulseLerpValue < 1)
        {
            _pulseLerpValue += Time.fixedDeltaTime * _pulseSpeed;
            float lerper = Easings.RunEasingType(_easingType, _pulseLerpValue);
            float newXY = _initialScaleXY * lerper * _pulseRange;
            _pulseSpriteT.transform.localScale = new Vector3(newXY, newXY, 1);
            float newAlpha = 1 - lerper;
            Color currentColor = _pulseSpriteRenderer.color;
            currentColor.a = newAlpha;
            _pulseSpriteRenderer.color = currentColor;
            _col.radius = newXY * SPRITE_TO_COLLIDER_RATIO;
            yield return new WaitForFixedUpdate();
        }

        _col.radius = _initialScaleXY * SPRITE_TO_COLLIDER_RATIO;
        _pulseLerpValue = 0;
        _pulseSpriteT.localScale = new Vector3(_initialScaleXY, _initialScaleXY, 1f);
        _pulseActive = false;
        _currentTicksCooldown = _ticksBetweenPulses;
    }

    public void TurretCollided(BaseTurret turret)
    {
        if(turret.RangeBuffSensitive)
        turret.ApplyEffect(TurretEffect.AquaPulseBuffer, _effectLengthInTicks, _rangeMultiplierStrength);
    }
}
