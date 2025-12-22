using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform window;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    private float targetY;

    private void Awake()
    {
        targetY = window.anchoredPosition.y;
    }

    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
            Show();
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0;
        PlayEnterAnimation();
    }

    private void PlayEnterAnimation()
    {
        // 시작 위치 (위)
        window.anchoredPosition = new Vector2(
            window.anchoredPosition.x,
            targetY + 900f
        );

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            window.DOAnchorPosY(targetY - 150f, 0.45f)
                .SetEase(Ease.InCubic)
        );

        seq.Append(
            window.DOAnchorPosY(targetY, 0.3f)
                .SetEase(Ease.OutBack)
        );

        seq.Join(
            window.DOScale(1.05f, 0.25f).From(0.95f)
        );

        seq.OnComplete(() =>
        {
            PlayScoreAnimation(123);
        });
    }
    
    private void PlayScoreAnimation(int finalScore)
    {
        int current = 0;
        scoreText.text = "0";

        DOTween.To(
                () => current,
                x =>
                {
                    current = x;
                    scoreText.text = current.ToString("N0");
                },
                finalScore,
                0.6f
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                scoreText.rectTransform
                    .DOPunchScale(Vector3.one * 0.15f, 0.25f)
                    .SetUpdate(true);
                
                PlayTimeFadeIn();
            });
    }
    
    private void PlayTimeFadeIn()
    {
        timeText.alpha = 0f;

        timeText
            .DOFade(1f, 0.4f)
            .SetDelay(0.25f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                timeText.rectTransform
                    .DOPunchScale(Vector3.one * 0.1f, 0.25f)
                    .SetUpdate(true);
            });
    }
}
