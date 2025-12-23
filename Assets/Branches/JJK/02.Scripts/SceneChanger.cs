using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class SceneChanger : MonoSingleton<SceneChanger>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime = 0.4f;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        Camera.main.orthographicSize = 5f;
    }
    
    public void ChangeScene(int index)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(
            Camera.main
                .DOOrthoSize(4.6f, fadeTime)
                .SetEase(Ease.OutCubic)
        );

        seq.Join(
            fadeImage.DOFade(1f, fadeTime)
        );

        seq.OnComplete(() =>
        {
            SceneManager.LoadScene(index);
            FadeOut();
        });
    }

    private void FadeOut()
    {
        fadeImage
            .DOFade(0f, fadeTime)
            .SetDelay(0.2f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                fadeImage.gameObject.SetActive(false);
            });
    }
}
