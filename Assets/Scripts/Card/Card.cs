using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class Card : MonoBehaviour
{
    public CardData data;
    public Column column;
    public FoundationSlot foundation;
    public bool isFaceUp;

    SpriteRenderer sr;
    Camera cam;

    bool dragging;
    Vector3 offset;
    List<Card> draggingCards;
    List<int> originalOrders;

    public void Init(CardData d, Sprite hidden)
    {
        data = d;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = hidden;
        cam = Camera.main;
    }

    public void SetFace(bool faceUp)
    {
        isFaceUp = faceUp;
        sr.sprite = faceUp ? data.sprite : sr.sprite;
    }

    void Update()
    {
        if (!isFaceUp) return;

        if (Input.GetMouseButtonDown(0))
            TryPick();

        if (dragging && Input.GetMouseButton(0))
            Drag();

        if (dragging && Input.GetMouseButtonUp(0))
            Drop();
    }

    void TryPick()
    {
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mouse, Vector2.zero);

        Card picked = null;
        int highestOrder = int.MinValue;

        foreach (var hit in hits)
        {
            if (!hit.collider.TryGetComponent(out Card c)) continue;
            if (!c.isFaceUp) continue;

            int order = c.GetComponent<SpriteRenderer>().sortingOrder;
            if (order > highestOrder)
            {
                highestOrder = order;
                picked = c;
            }
        }

        // ❗ Chỉ cho phép card được pick
        if (picked != this) return;

        // ❌ Click trúng phần bị che → bỏ
        if (!IsPointInVisibleArea(mouse)) return;

        // ❌ Không kéo card từ foundation
        if (foundation != null) return;

        // ===== XÁC ĐỊNH CHUỖI KÉO =====
        if (column != null)
        {
            draggingCards = column.GetDraggableSequence(this);
            if (draggingCards == null) return;
        }
        else if (GetComponentInParent<WastePile>() != null)
        {
            draggingCards = new List<Card> { this };
        }
        else return;

        dragging = true;

        originalOrders = new List<int>();
        int dragBaseOrder = 1000;

        // 🔥 Đưa card lên trên
        for (int i = 0; i < draggingCards.Count; i++)
        {
            var r = draggingCards[i].GetComponent<SpriteRenderer>();
            originalOrders.Add(r.sortingOrder);
            r.sortingOrder = dragBaseOrder + i;
        }

        offset = transform.position - (Vector3)mouse;

        // ❌ TẮT collider khi drag
        foreach (var c in draggingCards)
            c.GetComponent<Collider2D>().enabled = false;
    }

    bool IsPointInVisibleArea(Vector2 worldPoint)
    {
        // Không nằm trong column → full click
        if (column == null) return true;

        int index = column.cards.IndexOf(this);
        if (index == -1) return false;

        // ✅ Card trên cùng → click full
        if (index == column.cards.Count - 1)
            return true;

        // ===== Card bị che → chỉ click phần đầu =====
        Vector2 local = transform.InverseTransformPoint(worldPoint);
        Bounds b = sr.sprite.bounds;

        float visiblePercent = 0.175f;
        float visibleHeight = b.size.y * visiblePercent;
        float visibleMinY = b.max.y - visibleHeight;

        return local.y >= visibleMinY;
    }

    void OnDrawGizmos()
{
    if (sr == null)
        sr = GetComponent<SpriteRenderer>();

    if (column == null || sr == null || sr.sprite == null)
        return;

    int index = column.cards.IndexOf(this);
    if (index == -1) return;

    Bounds b = sr.sprite.bounds;

    // Card trên cùng → vẽ full xanh
    if (index == column.cards.Count - 1)
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(b.center, b.size);
        return;
    }

    // ===== Card bị che → chỉ vẽ phần đầu =====
    float visiblePercent = 0.175f;
    float visibleHeight = b.size.y * visiblePercent;

    Vector3 center = new Vector3(
        b.center.x,
        b.max.y - visibleHeight * 0.5f,
        0
    );

    Vector3 size = new Vector3(
        b.size.x,
        visibleHeight,
        0.01f
    );

    Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawCube(center, size);
}

    void Drag()
    {
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 basePos = (Vector3)mouse + offset;

        for (int i = 0; i < draggingCards.Count; i++)
            draggingCards[i].transform.position = basePos + Vector3.down * i * 0.4f;
    }

    void Drop()
    {
        
        // ❌ Kéo nhiều lá thì không bao giờ được drop vào foundation
        bool canDropToFoundation = draggingCards.Count == 1;

        dragging = false;
        Card topCard = draggingCards[^1];   // dùng cho FOUNDATION
        Card bottomCard = draggingCards[0]; // dùng cho COLUMN

        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mouse);

        Column targetColumn = null;
        FoundationSlot targetFoundation = null;

        float minDist = float.MaxValue;

        if (draggingCards.Count > 1)
        {
            targetFoundation = null;
        }
        
        foreach (var h in hits)
        {
            // ===== FOUNDATION =====
            if (canDropToFoundation && h.TryGetComponent(out FoundationSlot f))
            {
                if (f.suit != topCard.data.suit) continue;

                float d = Vector2.Distance(mouse, f.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    targetFoundation = f;
                }
            }

            // ===== COLUMN =====
            if (h.TryGetComponent(out Column c))
            {
                targetColumn = c;
            }

            // ===== DROP LÊN CARD → COLUMN CỦA CARD =====
            if (h.TryGetComponent(out Card hitCard))
            {
                if (hitCard.column != null)
                    targetColumn = hitCard.column;
            }
        }

        bool moved = false;

        // ====== DROP TO FOUNDATION ======
        if (targetFoundation && draggingCards.Count == 1 &&
            targetFoundation.CanAdd(topCard))
        {
            RemoveFromOrigin(topCard);
            targetFoundation.AddCard(topCard);
            moved = true;
        }
        // ====== DROP TO COLUMN ======
        else if (targetColumn && targetColumn.CanAddCard(bottomCard))
        {
            foreach (var c in draggingCards)
                RemoveFromOrigin(c);

            foreach (var c in draggingCards)
                targetColumn.AddCard(c);

            moved = true;
        }

        if (moved)
        {
            CardsController.instance?.RefreshPossibleMoves();
        }

        // ====== FAIL → RETURN ======
        if (!moved)
        {
            AudioManager.instance?.PlayWrongMoveSFX();

            for (int i = 0; i < draggingCards.Count; i++)
            {
                draggingCards[i].GetComponent<SpriteRenderer>().sortingOrder =
                    originalOrders[i];
            }

            if (column != null)
            {
                column.UpdatePositions();
            }
            else
            {
                var waste = GetComponentInParent<WastePile>();
                if (waste != null)
                {
                    transform.SetParent(waste.transform);
                    waste.ForceUpdate();
                }
            }
        }

        foreach (var c in draggingCards)
            c.GetComponent<Collider2D>().enabled = true;

        draggingCards = null;
    }

    void RemoveFromOrigin(Card card)
    {
        if (card.column != null)
        {
            card.column.RemoveCard(card);
            card.column = null;
        }
        else
        {
            var waste = card.GetComponentInParent<WastePile>();
            if (waste != null)
            {
                waste.cards.Remove(card);
                waste.ForceUpdate(); // 🔥 BẮT BUỘC
            }
        }
    }
}
