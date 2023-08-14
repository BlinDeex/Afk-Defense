using System.Collections;
using UnityEngine;

public class ContinueGameShockwave : MonoBehaviour
{
    [SerializeField] float _travelSpeed;
    [SerializeField] float _travelDistance;
    bool _colliderEnabled;
    [SerializeField] Collider2D _collider;

    [SerializeField] float _delayForReset;
    [SerializeField] float currentLerper;
    [SerializeField] Vector3 _startPos;

    [SerializeField] float _damageDealt;
    [SerializeField] EasingsList _easingType;
    private void OnEnable()
    {
        _colliderEnabled = true;
        currentLerper = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_colliderEnabled) return;

        if(collision.TryGetComponent(out BaseEnemy enemy))
        {
            enemy.TakeDamage(_damageDealt);
        }
    }

    private void Update()
    {
        if(currentLerper < 1)
        {
            currentLerper += Time.deltaTime * _travelSpeed;
            float currentLerperEased = Easings.RunEasingType(_easingType, currentLerper);
            transform.localPosition = Vector3.Lerp(_startPos, _startPos + new Vector3(0, _travelDistance, 0), currentLerperEased);

        }
        else
        {
            _colliderEnabled = false;
            StartCoroutine(DelayDisable());
        }
    }

    private void OnDisable()
    {
        gameObject.transform.localPosition = _startPos;
    }

    IEnumerator DelayDisable()
    {
        yield return new WaitForSeconds(_delayForReset);

        gameObject.SetActive(false);
    }
}
