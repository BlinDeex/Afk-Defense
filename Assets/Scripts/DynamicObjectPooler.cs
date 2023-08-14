using System.Collections.Generic;
using UnityEngine;

public class DynamicObjectPooler : MonoBehaviour
{
    public Dictionary<string, Queue<GameObject>> EnemyPools = new();
    public Dictionary<string, ParticleSystem> InstantEffectPools = new();
    public Dictionary<string, Queue<ParticleSystem>> EffectPools = new();
    public Dictionary<string, Queue<GameObject>> ProjectilePools = new();
    public Dictionary<string, Queue<GameObject>> CurrencyEffectPools = new();

    class DelayedPool
    {
        public int CurrentCooldown;
        public ParticleSystem Effect;

        public DelayedPool(int CurrentCooldown, ParticleSystem Effect)
        {
            this.CurrentCooldown = CurrentCooldown;
            this.Effect = Effect;
        }
    }

    public static DynamicObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject RequestEnemy(GameObject requestedObject)
    {
        string name = requestedObject.name;
        if (EnemyPools.ContainsKey(name))
        {
            if (EnemyPools[name].TryDequeue(out var gameObject)) return gameObject;

            return Instantiate(requestedObject);
            
        }

        EnemyPools.Add(name, new Queue<GameObject>());

        return Instantiate(requestedObject);
    }

    public void ReturnEnemy(GameObject gameObject)
    {
        string actualName = gameObject.name[..^7];
        EnemyPools[actualName].Enqueue(gameObject);
    }

    public GameObject RequestProjectile(GameObject requestedObject)
    {
        string name = requestedObject.name;
        if (ProjectilePools.ContainsKey(name))
        {
            if (ProjectilePools[name].TryDequeue(out var gameObject)) return gameObject;

            return Instantiate(requestedObject);
        }

        ProjectilePools.Add(name, new Queue<GameObject>());
        return Instantiate(requestedObject);
    }

    public ParticleSystem BorrowEffect(ParticleSystem requestedEffect)
    {
        string name = requestedEffect.name;
        if (EffectPools.ContainsKey(name))
        {
            if (EffectPools[name].TryDequeue(out var effect)) return effect;

            return Instantiate(requestedEffect);
        }
        EffectPools.Add(name, new Queue<ParticleSystem>());
        return Instantiate(requestedEffect);
    }

    public void ReturnBorrowedEffect(ParticleSystem effect)
    {
        string actualName = effect.gameObject.name[..^7];
        EffectPools[actualName].Enqueue(effect);
    }

    public void ReturnProjectile(GameObject gameObject)
    {
        
        string actualName = gameObject.name[..^7];
        ProjectilePools[actualName].Enqueue(gameObject);
    }

    public void RequestInstantEffect(ParticleSystem effect, Vector3 position, Quaternion rotation, int particlesCount)
    {
        if (InstantEffectPools.TryGetValue(effect.name, out ParticleSystem ps))
        {
            ps.transform.position = position;
            ps.transform.rotation = rotation;
            ps.Emit(particlesCount);
            return;
        }

        ParticleSystem newEffect = Instantiate(effect);
        InstantEffectPools.Add(effect.name, newEffect);
        newEffect.transform.SetPositionAndRotation(position, rotation);
        newEffect.Emit(particlesCount);
    }

    public void RequestInstantEffect(ParticleSystem effect, Vector3 position, int particlesCount)
    {
        if (InstantEffectPools.TryGetValue(effect.name, out ParticleSystem ps))
        {
            ps.transform.position = position;
            ps.Emit(particlesCount);
            return;
        }

        ParticleSystem newEffect = Instantiate(effect);
        InstantEffectPools.Add(effect.name, newEffect);
        newEffect.transform.position = position;
        newEffect.Emit(particlesCount);
    }

    public GameObject RequestCurrencyEffect(GameObject currencyEffect)
    {
        string name = currencyEffect.name;
        if (CurrencyEffectPools.ContainsKey(name))
        {
            if (CurrencyEffectPools[name].TryDequeue(out var effect)) return effect;

            return Instantiate(currencyEffect);
        }

        CurrencyEffectPools.Add(name, new Queue<GameObject>());
        return Instantiate(currencyEffect);
    }

    public void ReturnCurrencyEffect(GameObject gameObject)
    {
        string actualName = gameObject.name[..^7];
        CurrencyEffectPools[actualName].Enqueue(gameObject);
    }
}
