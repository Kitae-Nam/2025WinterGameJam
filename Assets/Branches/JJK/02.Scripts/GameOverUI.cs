using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform window;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            Show(123, "01:12");
    }

    public void Show(int score, string time)
    {
        panel.SetActive(true);
        Time.timeScale = 0f;

        scoreText.text = score.ToString();
        timeText.text = time;

        PlayEnterAnimation();
    }
    
    private void PlayEnterAnimation()
    {
        float targetY = window.anchoredPosition.y;

        window.anchoredPosition = new Vector2(
            window.anchoredPosition.x,
            targetY + 900f
        );

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        
        seq.Append(
            window.DOAnchorPosY(targetY, 0.6f)
                .SetEase(Ease.OutCubic)
        );
        
        seq.AppendCallback(PlayInfoFadeIn);
    }
    
    private void PlayInfoFadeIn()
    {
        scoreText.alpha = 0f;
        timeText.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            scoreText.DOFade(1f, 0.4f)
        );

        seq.AppendInterval(0.1f);

        seq.Append(
            timeText.DOFade(1f, 0.4f)
        );
    }
}
