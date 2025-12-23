using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonZoom : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private float zoomScale = 1.05f;
    private Vector3 _origin;

    private void Awake()
    {
        _origin = transform.localScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform
            .DOScale(_origin, 0.1f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform
            .DOScale(zoomScale, 0.12f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
}
