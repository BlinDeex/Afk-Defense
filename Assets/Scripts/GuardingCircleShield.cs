using UnityEngine;

public class GuardingCircleShield : MonoBehaviour, INonEnemyObstacle
{
    [SerializeField] BaseEnemy _root;
    [SerializeField] ParticleSystem _hitEffect;
    [SerializeField] int _particlesCount = 30;
    [SerializeField] ParticleSystem _shieldBreakEffect;
    SpriteRenderer _sp;

    Color32 _startColor = new(0, 0, 255, 255);
    Color32 _endColor = new(255, 0, 0, 255);
    Color32 _currentColor;


    [SerializeField] float _maxShieldHealth;
    [SerializeField] float _currentShieldHealth;

    [SerializeField] float percentHealth;

    bool _shieldActive;

    private void Awake()
    {
        _sp = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BaseProjectile projectileBase = collision.GetComponent<BaseProjectile>();
        Vector2 hitPoint;
        Vector2 lastDir = projectileBase._lastVelocityDirNormalized;
        if (projectileBase._hasHitPoint)
        {
            hitPoint = projectileBase._lastHitPoint;
        }
        else
        {
            hitPoint = collision.transform.position;
        }

        float damageTaken = projectileBase._projectileDamage;
        projectileBase.NonEnemyHitReturnProjectile();
        Debug.Log("projectile hit shield");

        ParticleSystem borrowedEffect = DynamicObjectPooler.Instance.BorrowEffect(_hitEffect);
        borrowedEffect.transform.position = hitPoint;
        var shape = borrowedEffect.shape;
        var main = borrowedEffect.main;
        var color = new ParticleSystem.MinMaxGradient(_currentColor);
        main.startColor = color;
        float angle = Vector2.Angle(lastDir, borrowedEffect.transform.up);
        shape.rotation = new Vector3(0, angle, 0);
        borrowedEffect.Emit(_particlesCount);
        DynamicObjectPooler.Instance.ReturnBorrowedEffect(borrowedEffect);

        UpdateColorAndHealth(damageTaken);
    }

    void UpdateColorAndHealth(float damageTaken = 0)
    {
        _currentShieldHealth -= damageTaken;
        percentHealth = _currentShieldHealth / _maxShieldHealth;
        _sp.color = _currentColor = Color.Lerp(_endColor, _startColor, percentHealth);

        if (percentHealth <= 0)
        {
            BreakShield();
        }
    }

    public void BreakShield()
    {
        if (!_shieldActive) return;

        _shieldActive = false;
        DynamicObjectPooler.Instance.RequestInstantEffect(_shieldBreakEffect, transform.position, transform.rotation, 80);
        gameObject.SetActive(false);
    }

    public void PrepareShield(float health)
    {
        _shieldActive = true;
        _currentShieldHealth = _maxShieldHealth = health * 5f;
        percentHealth = _currentShieldHealth / _maxShieldHealth;
        UpdateColorAndHealth();
    }

    public void ProjectileHit()
    {

    }
}
