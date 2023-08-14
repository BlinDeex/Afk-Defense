using TMPro;
using UnityEngine;

public class CoinGainEffect : MonoBehaviour
{
    float _lerper;
    [SerializeField] TextMeshPro _text;
    [SerializeField] float _lerpSpeed = 1;
    Color _invisibleColor;
    Color _initialColor;

    private void Awake()
    {
        _invisibleColor = _text.color;
        _invisibleColor.a = 0;
        _initialColor = _text.color;
    }

    private void Update()
    {
        _lerper += Time.deltaTime * _lerpSpeed;
        _text.color = Color.Lerp(_initialColor, _invisibleColor, _lerper);
        if(_lerper >= 1)
        {
            gameObject.SetActive(false);
            DynamicObjectPooler.Instance.ReturnCurrencyEffect(gameObject);
        }
    }

    public void PrepareCurrencyEffect(int amount)
    {
        string amountString = $"+{amount}";
        _text.text = amountString;
        _lerper = 0;
    }
}
