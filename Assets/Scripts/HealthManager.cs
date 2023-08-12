using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    [SerializeField] ParticleSystem _lineBreakEffect;
    [SerializeField] int _lineBreakParticleCount;
    [SerializeField] bool _enableDeath;

    [SerializeField] TextMeshProUGUI _healthText;
    [SerializeField] LineRenderer finishLine;
    [SerializeField] Gradient _lineColorGradient;
    [SerializeField] float _lineColorLerpSpeed;
    [SerializeField] float _colorMultiplier;
    [SerializeField] float _fullHpLineWidth;
    [SerializeField] float _noHpLineWidth;

    [SerializeField] float _startingHealth;
    [SerializeField] float _maxHealth;
    [field: SerializeField] public float CurrentHealth { get; private set; }

    [Header("Debug")]
    [SerializeField] float _lineLerperCurrentValue;
    [SerializeField] float _lineColorTarget;


    bool _dead;

    private void Awake()
    {
        Instance = this;
        CurrentHealth = _startingHealth;
        _lineLerperCurrentValue = 1;
        UpdateLine();
        UpdateHealthText();
        
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        UpdateHealthText();
        UpdateLine();
        if (!_dead && CurrentHealth <= 0 && _enableDeath) NoHPLeft();
    }

    private void Update()
    {
        if(_lineLerperCurrentValue > _lineColorTarget)
        {
            _lineLerperCurrentValue -= Time.deltaTime * _lineColorLerpSpeed;
            UpdateLine();
        }
    }

    void UpdateLine()
    {
        _lineColorTarget = 1 / _maxHealth * CurrentHealth;
        Color targetColor = _lineColorGradient.Evaluate(1 - _lineLerperCurrentValue);
        finishLine.material.SetColor("_Color", targetColor * _colorMultiplier);
        float lineWidthLerper = Mathf.Lerp(_noHpLineWidth, _fullHpLineWidth, _lineLerperCurrentValue);
        finishLine.widthMultiplier = lineWidthLerper;
    }

    void NoHPLeft()
    {
        finishLine.gameObject.SetActive(false);
        _lineBreakEffect.Emit(_lineBreakParticleCount);
        _dead = true;
    }

    void UpdateHealthText()
    {
        int currentHealth = Mathf.CeilToInt(CurrentHealth);
        _healthText.text = currentHealth.ToString();
    }



}
