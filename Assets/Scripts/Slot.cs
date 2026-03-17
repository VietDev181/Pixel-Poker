using UnityEngine;
using System.Collections.Generic;

public class Column : MonoBehaviour
{
    public List<Card> cards = new();

    public void Clear()
    {
        foreach (var c in cards)
            Destroy(c.gameObject);
        cards.Clear();
    }

    public bool CanAddCard(Card card)
    {
        if (cards.Count == 0)
            return card.data.rankValue == 13; // K

        Card top = cards[^1];
        return top.isFaceUp &&
               card.data.rankValue == top.data.rankValue - 1 &&
               card.data.color != top.data.color;
    }

    public void AddCard(Card card)
    {
        cards.Add(card);
        card.column = this;
        card.foundation = null;
        card.transform.SetParent(transform);
        UpdatePositions();
    }

    public void RemoveCard(Card card)
    {
        cards.Remove(card);
        if (cards.Count > 0)
            cards[^1].SetFace(true);
        
        UpdatePositions();
    }

    public void UpdatePositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = new Vector3(0, -i * 0.4f, 0);
            cards[i].GetComponent<SpriteRenderer>().sortingOrder = 100 + i;
        }
    }

    public List<Card> GetDraggableSequence(Card start)
    {
        int index = cards.IndexOf(start);
        if (index == -1) return null;

        List<Card> seq = new();
        for (int i = index; i < cards.Count - 1; i++)
        {
            Card a = cards[i];
            Card b = cards[i + 1];

            if (!b.isFaceUp) return null;
            if (b.data.rankValue != a.data.rankValue - 1) return null;
            if (b.data.color == a.data.color) return null;

            seq.Add(a);
        }

        seq.Add(cards[^1]);
        return seq;
    }
}
