using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    [SerializeField] GameObject _shockwaveGO;
    [SerializeField] int _lineBreakParticleCount;
    [SerializeField] int _lineContinueShockwaveCount;
    [SerializeField] bool _enableDeath;
    [SerializeField] GameObject _gameLostPanel;
    [SerializeField] float _initialSlowdownStrength;

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
    [SerializeField] ParticleSystem _lineBreakEffect;


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
        Color targetColor = _lineColorGradient.Evaluate(_lineLerperCurrentValue);
        finishLine.material.SetColor("_Color", targetColor * _colorMultiplier);
        float lineWidthLerper = Mathf.Lerp(_noHpLineWidth, _fullHpLineWidth, _lineLerperCurrentValue);
        finishLine.widthMultiplier = lineWidthLerper;
    }

    void NoHPLeft()
    {
        finishLine.gameObject.SetActive(false);
        UIManager.Instance.ChangeGearButtonState(false);
        _lineBreakEffect.Emit(_lineBreakParticleCount);
        _dead = true;
        Time.timeScale = _initialSlowdownStrength;
        StartCoroutine(DelayDeathScreen());
    }

    IEnumerator DelayDeathScreen()
    {
        yield return new WaitForSecondsRealtime(3f);
        Debug.Log("coroutine");
        _gameLostPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ContinueGame()
    {
        _gameLostPanel.SetActive(false);
        _shockwaveGO.SetActive(true);
        UIManager.Instance.ChangeGearButtonState(true);
        Time.timeScale = 1;
        CurrentHealth = _maxHealth;
        _lineLerperCurrentValue = 1;
        UpdateHealthText();
        UpdateLine();
        finishLine.gameObject.SetActive(true);
        _dead = false;

    }

    public void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1;
    }

    void UpdateHealthText()
    {
        int currentHealth = Mathf.CeilToInt(CurrentHealth);
        if(currentHealth > 0)
        {
            _healthText.text = currentHealth.ToString();
        }
        else
        {
            _healthText.text = "0";
        }
    }
}
