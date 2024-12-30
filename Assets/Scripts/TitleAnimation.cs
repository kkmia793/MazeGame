using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private Text text1; // 最初の文字
    [SerializeField] private Text text2; // 次の文字

    [SerializeField] private float fadeDuration = 0.5f; // フェードインの時間
    [SerializeField] private float delayBetween = 0.3f; // 次の文字までの遅延時間

    private void Start()
    {
        // アルファ値を 0 に設定（非表示にする）
        SetAlpha(text1, 0f);
        SetAlpha(text2, 0f);
        
        AnimateText();
    }

    private void SetAlpha(Text text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private void AnimateText()
    {
        // 最初の文字をフェードイン
        text1.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 次の文字をフェードイン
                text2.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad)
                    .SetDelay(delayBetween);
            });
    }
}