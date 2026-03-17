using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class FoundationSlot : MonoBehaviour
{
    public Suit suit;
    public List<Card> cards = new();

    [Header("VFX")]
    public GameObject addCardVFX;

    public static System.Action OnFoundationChanged;

    public bool CanAdd(Card card)
    {
        if (cards.Count == 0)
            return card.data.rankValue == 1;

        Card top = cards[^1];
        return card.data.suit == suit &&
               card.data.rankValue == top.data.rankValue + 1;
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
        card.foundation = this;
        card.column = null;

        card.transform.SetParent(transform);

        // 🔥 Đưa card về đúng vị trí + scale nhỏ trước
        card.transform.localPosition = Vector3.zero;
        card.transform.localScale = Vector3.one;

        var sr = card.GetComponent<SpriteRenderer>();
        sr.sortingOrder = cards.Count;
        
        if (addCardVFX != null)
        {
            GameObject vfx = Instantiate(
                addCardVFX,
                card.transform.position,
                Quaternion.identity
            );

            Destroy(vfx, 1.5f); // auto clean
        }
        // ===== EFFECT =====
        Sequence seq = DOTween.Sequence();

        seq.Append(card.transform
            .DOLocalMove(Vector3.zero, 0.2f)
            .SetEase(Ease.OutBack))
        .Join(card.transform
            .DOScale(1f, 0.2f)
            .SetEase(Ease.OutBack))
        .Append(card.transform
            .DOPunchScale(Vector3.one * 0.08f, 0.15f));

        // 🔊 SFX (nếu có)
        AudioManager.instance?.PlayFoundationPlaceSFX();

        OnFoundationChanged?.Invoke();
    }
}
