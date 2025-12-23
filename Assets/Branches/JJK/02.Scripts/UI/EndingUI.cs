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
            _targetY - 1100f
        );

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        
        seq.Append(
            credit.DOAnchorPosY(_targetY + 1100, 20f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                    SceneChanger.Instance.ChangeScene(0))
        );
    }
}
