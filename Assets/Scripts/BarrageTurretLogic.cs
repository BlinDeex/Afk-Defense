using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrageTurretLogic : BaseTurret
{
    readonly List<(Transform, BarrageOrb, ParticleSystem)> _outerOrbs = new();
    [SerializeField] GameObject _orbsPrefab;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] int _startingOrbs = 4;
    [SerializeField] int _shootTicksCooldown = 40;
    [SerializeField] int _ticksBetweenShootSteps = 2;
    int _currentTicksCooldown;
    [SerializeField] float _damage;
    [SerializeField] float _projectileSpeed;
    bool _isShooting;
    Vector3 _offset = new(0, 0.5f, 0);

    private void Awake()
    {
        IncreaseOrbs(_startingOrbs);

        UpgradesDict.Add(0, Firerate);
        UpgradesDict.Add(1, Damage);
        UpgradesDict.Add(2, Orbs);
        UpgradesDict.Add(3, ProjectileSpeed);
    }

    private void FixedUpdate()
    {
        if (!_isShooting) _currentTicksCooldown--;

        if (_currentTicksCooldown <= 0)
        {
            _isShooting = true;
            _currentTicksCooldown = _shootTicksCooldown;
            StartCoroutine(ShootCycle());
        }
    }

    IEnumerator ShootCycle()
    {
        foreach ((Transform, BarrageOrb, ParticleSystem) outerOrb in _outerOrbs)
        {
            if (!TryShoot(outerOrb.Item1, outerOrb.Item3))
            {
                _isShooting = false;
                break;
            }

            for (int i = 0; i < _ticksBetweenShootSteps; i++) yield return new WaitForFixedUpdate();

            if (!_isShooting) break;

        }
        _isShooting = false;
    }

    bool TryShoot(Transform outerOrb, ParticleSystem shootPS)
    {
        UpdateTarget();

        if (HasTarget)
        {
            GameObject projectile = DynamicObjectPooler.Instance.RequestProjectile(_projectilePrefab);
            BarrageProjectile BP = projectile.GetComponent<BarrageProjectile>();
            Vector3 dir = (outerOrb.transform.position - CurrentTarget.transform.position).normalized;
            projectile.transform.position = outerOrb.position;
            BP.PrepareProjectile(CurrentTarget, _damage, _projectileSpeed, -dir);
            shootPS.Emit(30);
            projectile.SetActive(true);
            return true;
        }

        return false;
    }

    void IncreaseOrbs(int amount)
    {
        if (_isShooting) StopCoroutine(ShootCycle());

        for (int i = 0; i < amount; i++)
        {
            GameObject newOrb = Instantiate(_orbsPrefab, transform.position + _offset, Quaternion.identity);
            newOrb.transform.parent = transform;

            BarrageOrb orbClass = newOrb.GetComponent<BarrageOrb>();
            orbClass.Parent = transform;

            ParticleSystem ps = newOrb.GetComponent<ParticleSystem>();
            _outerOrbs.Add((newOrb.transform, orbClass, ps));


            double angleIncrement = 360d / _outerOrbs.Count;
            int multiplier = 1;
            foreach ((Transform, BarrageOrb, ParticleSystem) outer in _outerOrbs)
            {
                outer.Item2.UpdateAngle(angleIncrement * multiplier);
                multiplier++;
            }
        }
        _isShooting = false;
    }

    #region upgrades

    public float? Firerate(float? gainedStat)
    {
        if (gainedStat == null) return _shootTicksCooldown;

        _shootTicksCooldown += (int)gainedStat;

        return null;
    }

    public float? Damage(float? gainedStat)
    {
        if (gainedStat == null) return _damage;

        _damage += gainedStat.Value;

        return null;
    }

    public float? Orbs(float? gainedStat)
    {
        if (gainedStat == null) return _outerOrbs.Count;

        IncreaseOrbs((int)gainedStat);

        return null;
    }

    public float? ProjectileSpeed(float? gainedStat)
    {
        if (gainedStat == null) return _projectileSpeed;

        _projectileSpeed += gainedStat.Value;

        return null;
    }

    #endregion

    public override void PostUpgradeVisualRefresh()
    {

    }
}
