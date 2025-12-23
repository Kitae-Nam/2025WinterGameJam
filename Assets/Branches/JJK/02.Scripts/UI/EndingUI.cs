using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private RectTransform credit;
    private float _targetY;

    private void Awake()
    {
        _targetY = credit.anchoredPosition.y;
    }

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
            Show();
    }

    public void Show()
    {
        credit.gameObject.SetActive(true);
        PlayEnterAnimation();
    }
    
    private void PlayEnterAnimation()
    {
        credit.anchoredPosition = new Vector2(
            credit.anchoredPosition.x,
            _targetY - 900f
        );

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        
        seq.Append(
            credit.DOAnchorPosY(_targetY, 10f)
                .SetEase(Ease.Linear)
        );
    }
}
