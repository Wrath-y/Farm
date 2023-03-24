using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ItemFader : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeIn()
    {
        _spriteRenderer.DOColor(new Color(1, 1, 1, 1), Settings.ItemFadeDuration);
    }

    public void FadeOut()
    {
        _spriteRenderer.DOColor(new Color(1, 1, 1, Settings.TargetAlpha), Settings.ItemFadeDuration);
    }
}
