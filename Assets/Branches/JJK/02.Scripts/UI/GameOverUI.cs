using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform window;
    [SerializeField] private TextMeshProUGUI timeText;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        
        if (Keyboard.current.rKey.wasPressedThisFrame)
            Show();
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        
        timeText.text = TimeSpan.FromSeconds(_timer).ToString(@"mm\:ss\.ff");

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
        seq.AppendCallback(PlayGameOverZoom);
    }
    
    private void PlayInfoFadeIn()
    {
        timeText.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.AppendInterval(0.1f);

        seq.Append(
            timeText.DOFade(1f, 0.4f)
        );
    }
    
    private void PlayGameOverZoom()
    {
        Camera.main
            .DOOrthoSize(5.5f, 1.5f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
}
