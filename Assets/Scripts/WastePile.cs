using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class WastePile : MonoBehaviour
{
    public List<Card> cards = new();

    [Header("Config")]
    public int maxVisible = 3;          // Draw 1 / Draw 3
    public float offsetX = 0.4f;
    public float tweenDuration = 0.25f;
    public float returnDuration = 0.25f;

    const int BASE_SORT = 1000;

    // =========================
    // CLEAR
    // =========================
    public void Clear()
    {
        foreach (var c in cards)
            Destroy(c.gameObject);
        cards.Clear();
    }

    // =========================
    // ADD CARD FROM STOCK
    // =========================
    public void AddCard(Card card)
    {
        cards.Add(card);

        card.column = null;
        card.foundation = null;
        card.transform.SetParent(transform);

        UpdatePositions();
    }

    // =========================
    // UPDATE POSITIONS (CORE)
    // =========================
    void UpdatePositions()
    {
        if (cards.Count == 0) return;

        int groupStart = Mathf.Max(0, cards.Count - maxVisible);

        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            card.transform.DOKill();

            Collider2D col = card.GetComponent<Collider2D>();
            SpriteRenderer sr = card.GetComponent<SpriteRenderer>();

            // 🔥 SORTING LUÔN THEO INDEX THẬT
            sr.sortingOrder = BASE_SORT + i;

            if (i >= groupStart)
            {
                // ====== VISIBLE GROUP ======
                int visibleIndex = i - groupStart;

                card.gameObject.SetActive(true);
                card.SetFace(true);

                Vector3 targetPos = new Vector3(visibleIndex * offsetX, 0, 0);

                card.transform
                    .DOLocalMove(targetPos, tweenDuration)
                    .SetEase(Ease.OutQuad);

                card.transform
                    .DOScale(Vector3.one, tweenDuration * 0.8f);

                // 🔥 CHỈ LÁ TRÊN CÙNG KÉO ĐƯỢC
                col.enabled = (i == cards.Count - 1);
            }
            else
            {
                // ====== HIDDEN BEHIND ======
                card.transform
                    .DOLocalMove(Vector3.zero, tweenDuration * 0.8f)
                    .SetEase(Ease.InQuad);

                card.transform
                    .DOScale(Vector3.one * 0.95f, tweenDuration * 0.8f);

                col.enabled = false;
            }
        }
    }

    // =========================
    // RETURN ALL TO STOCK
    // =========================
    public List<CardData> CollectRemainingCards(Transform stockSpawn)
    {
        List<CardData> result = new();

        foreach (var card in cards)
        {
            result.Add(card.data);

            card.transform.DOMove(stockSpawn.position, returnDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(card.gameObject);
                });
        }

        cards.Clear();
        return result;
    }

    // =========================
    // FORCE REFRESH (WHEN DROP FAIL)
    // =========================
    public void ForceUpdate()
    {
        UpdatePositions();
    }
}
