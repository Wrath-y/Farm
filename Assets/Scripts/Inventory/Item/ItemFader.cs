using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeIn()
    {
        spriteRenderer.DOColor(new Color(1, 1, 1, 1), Settings.fadeDuration);
    }

    public void FadeOut()
    {
        spriteRenderer.DOColor(new Color(1, 1, 1, Settings.targetAlpha), Settings.fadeDuration);
    }
}
