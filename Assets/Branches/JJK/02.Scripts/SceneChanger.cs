using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class SceneChanger : MonoSingleton<SceneChanger>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime = 0.4f;
    
    public void ChangeScene(int index)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 0);

        fadeImage
            .DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
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
