using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform imageRectTransform; 
    [SerializeField] private float scaleUpFactor = 1.5f;       
    [SerializeField] private float scaleDuration = 0.5f;       

    private Vector3 originalScale; 

    private void Start()
    {
        originalScale = imageRectTransform.localScale;
        
        AnimateImage();
    }

    private void AnimateImage()
    {
        imageRectTransform.DOScale(originalScale * scaleUpFactor, scaleDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                imageRectTransform.DOScale(originalScale, scaleDuration)
                    .SetEase(Ease.InQuad);
            });
    }
}