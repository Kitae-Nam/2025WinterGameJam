using System;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using Sequence = DG.Tweening.Sequence;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform window;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    private float _targetY;
    private float _timer;

    private void Awake()
    {
        _targetY = window.anchoredPosition.y;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        
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
        window.anchoredPosition = new Vector2(
            window.anchoredPosition.x,
            _targetY + 900f
        );

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            window.DOAnchorPosY(_targetY - 150f, 0.45f)
                .SetEase(Ease.InCubic)
        );

        seq.Append(
            window.DOAnchorPosY(_targetY, 0.3f)
                .SetEase(Ease.OutBack)
        );

        seq.Join(
            window.DOScale(1.05f, 0.25f).From(0.95f)
        );

        seq.OnComplete(() =>
        {
            PlayScoreAnimation(BuildBridge.Instance.RemainingBudget);
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
        timeText.text = TimeSpan.FromSeconds(_timer).ToString(@"mm\:ss\.ff");

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
