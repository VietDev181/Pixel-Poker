using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Collections;

public class CardsController : MonoBehaviour
{
    public static CardsController instance;

    [Header("Prefabs")]
    public Card cardPrefab;
    public Sprite[] cardSprites; // 52
    public Sprite hiddenSprite;

    [Header("Tableau")]
    public Column[] columns;

    [Header("Foundation")]
    public FoundationSlot[] foundations;

    [Header("Stock")]
    public StockClick stockClick;

    [Header("Draw Mode")]
    public bool drawThree = false;

    [Header("Stock / Waste")]
    public Transform stockSpawn;
    public WastePile wastePile;
    bool hasWon = false;

    [Header("Possible Moves UI")]
    public TextMeshProUGUI possibleMovesText;

    List<CardData> deck = new();

    public float dealDuration = 0.35f;
    public float dealDelay = 0.08f;
    public float flyDuration = 0.35f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitGame();
    }

    void InitGame()
    {
        foreach (var c in columns) c.Clear();
        foreach (var f in foundations) f.cards.Clear();
        wastePile.Clear();

        CreateDeck();
        ShuffleDeck();
        DealInitialCards();
        UpdateUI();
    }

    void OnEnable()
    {
        FoundationSlot.OnFoundationChanged += UpdateUI;
        FoundationSlot.OnFoundationChanged += RefreshPossibleMoves;
    }

    void OnDisable()
    {
        FoundationSlot.OnFoundationChanged -= UpdateUI;
        FoundationSlot.OnFoundationChanged -= RefreshPossibleMoves;
    }

    void CreateDeck()
    {
        deck.Clear();
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            for (int i = 0; i < 13; i++)
            {
                deck.Add(new CardData
                {
                    suit = suit,
                    rank = ranks[i],
                    rankValue = i + 1,
                    color = (suit == Suit.Hearts || suit == Suit.Diamonds)
                        ? CardColor.Red : CardColor.Black,
                    sprite = cardSprites[(int)suit * 13 + i]
                });
            }
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int r = Random.Range(i, deck.Count);
            (deck[i], deck[r]) = (deck[r], deck[i]);
        }
    }

    public void SetDrawMode(bool isDrawThree)
    {
        drawThree = isDrawThree;
    }

    void DealInitialCards()
    {
        AudioManager.instance?.PlayDealCardSFX();
        StartCoroutine(DealCoroutine());
    }

    IEnumerator DealCoroutine()
    {
        for (int c = 0; c < 7; c++)
        {
            for (int i = 0; i <= c; i++)
            {
                var data = deck[0];
                deck.RemoveAt(0);

                // 1️⃣ SPAWN TẠI STOCK
                Card card = Instantiate(cardPrefab);
                card.Init(data, hiddenSprite);
                card.SetFace(false);
                card.transform.position = stockClick.transform.position;

                bool faceUp = (i == c);
                int columnIndex = c;
                int cardIndex = i;

                // 2️⃣ BAY TỚI COLUMN
                yield return new WaitForSeconds(dealDelay);

                Sequence seq = DOTween.Sequence();

                seq.Append(card.transform.DOMove(
                    columns[columnIndex].transform.position
                    + new Vector3(0, -cardIndex * 0.4f, 0),
                    dealDuration
                ).SetEase(Ease.OutCubic));

                seq.OnComplete(() =>
                {
                    // 3️⃣ ADD SAU KHI BAY XONG
                    columns[columnIndex].AddCard(card);

                    // 4️⃣ CHỈ LÁ CUỐI ĐƯỢC NGỬA
                    card.SetFace(faceUp);
                });
            }
        }

        yield return new WaitForSeconds(dealDuration + 0.1f);
        RefreshPossibleMoves();
    }

    IEnumerator DealCardsCoroutine(int count, Transform stockClick)
    {
        if (deck.Count == 0)
            yield break;

        int realCount = Mathf.Min(count, deck.Count);

        for (int i = 0; i < realCount; i++)
        {
            var data = deck[0];
            deck.RemoveAt(0);

            AudioManager.instance?.PlayDrawCardSFX();

            Card card = Instantiate(cardPrefab, stockClick.position, Quaternion.identity);
            card.Init(data, hiddenSprite);
            card.SetFace(false);
            card.transform.localScale = Vector3.one * 0.85f;

            Vector3 targetPos = wastePile.transform.position + new Vector3(0.25f * i, 0, 0);

            Sequence seq = DOTween.Sequence();

            seq.Append(card.transform
                .DOMove(targetPos, dealDuration)
                .SetEase(Ease.OutCubic));

            seq.Join(card.transform
                .DOScale(1f, dealDuration)
                .SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                card.SetFace(true);
                wastePile.AddCard(card);
                stockClick.GetComponent<StockClick>()?.UpdateVisual();
                
                RefreshPossibleMoves();
            });

            yield return new WaitForSeconds(0.12f);
        }
    }

    public void DealCards(int count, Transform stockClick)
    {
        if (deck.Count == 0)
        {
            var returned = wastePile.CollectRemainingCards(stockClick);
            if (returned.Count == 0) return;

            foreach (var c in returned)
                deck.Add(c);

            stockClick.GetComponent<StockClick>()?.UpdateVisual();
            RefreshPossibleMoves();
            return;
        }

        StartCoroutine(DealCardsCoroutine(count, stockClick));
    }

    void UpdateUI()
    {
        if (hasWon) return;

        if (foundations.All(f => f.cards.Count == 13))
        {
            hasWon = true;
            StartCoroutine(FoundationFlyOutEffect());
        }
    }

    public bool HasStockCards()
    {
        return deck.Count > 0;
    }

    bool AllCardsAreRevealed()
    {
        foreach (var col in columns)
            foreach (var card in col.cards)
                if (!card.isFaceUp) return false;

        return true;
    }

    IEnumerator AutoComplete()
    {
        // Tắt cả 2 event trong lúc auto-complete
        FoundationSlot.OnFoundationChanged -= UpdateUI;
        FoundationSlot.OnFoundationChanged -= RefreshPossibleMoves;

        while (true)
        {
            bool moved = false;
            Card cardToMove = null;
            FoundationSlot targetFoundation = null;

            // Waste trước
            if (wastePile.cards.Count > 0)
            {
                Card topWaste = wastePile.cards[^1];
                foreach (var f in foundations)
                {
                    if (f.CanAdd(topWaste))
                    {
                        wastePile.cards.Remove(topWaste);
                        wastePile.ForceUpdate();
                        cardToMove = topWaste;
                        targetFoundation = f;
                        moved = true;
                        break;
                    }
                }
            }

            // Tableau → foundation
            if (!moved)
            {
                Card bestCard = null;
                FoundationSlot bestFoundation = null;
                int lowestRank = int.MaxValue;

                foreach (var col in columns)
                {
                    if (col.cards.Count == 0) continue;
                    Card top = col.cards[^1];

                    foreach (var f in foundations)
                    {
                        if (f.CanAdd(top) && top.data.rankValue < lowestRank)
                        {
                            lowestRank = top.data.rankValue;
                            bestCard = top;
                            bestFoundation = f;
                        }
                    }
                }

                if (bestCard != null)
                {
                    bestCard.column.RemoveCard(bestCard);
                    cardToMove = bestCard;
                    targetFoundation = bestFoundation;
                    moved = true;
                }
            }

            if (!moved) break;

            // Bay vào Foundation — KHÔNG gọi AddCard (tránh conflict animation)
            cardToMove.transform.SetParent(null);
            cardToMove.GetComponent<Collider2D>().enabled = false;
            cardToMove.GetComponent<SpriteRenderer>().sortingOrder = targetFoundation.cards.Count;

            bool done = false;

            cardToMove.transform
                .DOMove(targetFoundation.transform.position, flyDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    // Thêm thủ công vào list, không dùng AddCard để tránh animation conflict
                    targetFoundation.cards.Add(cardToMove);
                    cardToMove.foundation = targetFoundation;
                    cardToMove.transform.SetParent(targetFoundation.transform);
                    cardToMove.transform.localPosition = Vector3.zero;
                    AudioManager.instance?.PlayFoundationPlaceSFX();
                    done = true;
                });

            yield return new WaitUntil(() => done);
            yield return new WaitForSeconds(0.05f);
        }

        // Tất cả vào xong → trigger win
        hasWon = true;
        StartCoroutine(FoundationFlyOutEffect());
    }

    public int CountPossibleMoves()
    {
        int count = 0;

        // ── 1. RÚT BÀI TỪ STOCK / ĐẢO LẠI WASTE ──────────────
        if (deck.Count > 0 || wastePile.cards.Count > 0)
            count++;

        // ── 2. LÁ TRÊN CÙNG CỦA WASTE ─────────────────────────
        if (wastePile.cards.Count > 0)
        {
            Card topWaste = wastePile.cards[^1];
            foreach (var f in foundations)
                if (f.CanAdd(topWaste)) count++;
            foreach (var col in columns)
                if (col.CanAddCard(topWaste)) count++;
        }

        // ── 3. LÁ NGỬA TRÊN TABLEAU ───────────────────────────
        foreach (var col in columns)
        {
            for (int i = 0; i < col.cards.Count; i++)
            {
                Card card = col.cards[i];
                if (!card.isFaceUp) continue;

                // Lá trên cùng → kiểm tra foundation
                if (i == col.cards.Count - 1)
                    foreach (var f in foundations)
                        if (f.CanAdd(card)) count++;

                // Kiểm tra các column khác
                foreach (var targetCol in columns)
                {
                    if (targetCol == col) continue;
                    if (targetCol.CanAddCard(card)) count++;
                }
            }
        }

        return count;
    }

    public void RefreshPossibleMoves()
    {
        if (hasWon) return;

        if (AllCardsAreRevealed() && wastePile.cards.Count == 0 && deck.Count == 0)
        {
            StartCoroutine(AutoComplete());
            return;
        }

        int moves = CountPossibleMoves();

        if (possibleMovesText != null)
        {
            possibleMovesText.text = moves > 0
                ? $"Moves: {moves}"
                : "No moves left!";

            // Đổi màu đỏ khi sắp hết
            possibleMovesText.color = moves == 0
                ? UnityEngine.Color.red
                : moves <= 3
                    ? new UnityEngine.Color(1f, 0.6f, 0f)   // cam
                    : UnityEngine.Color.white;
        }

        if (moves == 0)
            GameManager.instance?.GameOver();
    }

    IEnumerator FoundationFlyOutEffect()
    {
        float popTime = 0.18f;
        float flyTime = 1.1f;
        float delayBetweenCards = 0.03f;

        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        Vector3 center = Vector3.zero;

        foreach (var foundation in foundations)
        {
            foreach (var card in foundation.cards)
            {
                card.transform.SetParent(null);

                SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
                sr.sortingOrder += 500;

                Vector3 startPos = card.transform.position;

                // 👇 bật XUỐNG trước
                Vector3 popPos = startPos + Vector3.down * Random.Range(0.8f, 1.4f);

                // 🎯 điểm giữa gần trung tâm màn hình
                Vector3 midPos = Vector3.Lerp(popPos, center, 0.6f)
                                + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0.5f, 1.5f), 0);

                // 🚀 điểm cuối ra khỏi màn hình
                Vector3 endPos = center +
                    new Vector3(
                        Random.Range(-screenWidth, screenWidth),
                        -screenHeight - 2f,
                        0
                    );

                Sequence seq = DOTween.Sequence();

                // 1️⃣ bật xuống
                seq.Append(card.transform
                    .DOScale(1.25f, popTime)
                    .SetEase(Ease.OutBack));

                seq.Join(card.transform
                    .DOMove(popPos, popTime)
                    .SetEase(Ease.OutBack));

                // 2️⃣ bay cong về trung tâm
                seq.Append(
                    card.transform.DOPath(
                        new Vector3[] { popPos, midPos, endPos },
                        flyTime,
                        PathType.CatmullRom
                    ).SetEase(Ease.OutCubic)
                );

                // xoáy + tan
                seq.Join(card.transform
                    .DORotate(
                        new Vector3(0, 0, Random.Range(900f, 1400f)),
                        flyTime,
                        RotateMode.FastBeyond360
                    ));

                seq.Join(card.transform
                    .DOScale(0.4f, flyTime)
                    .SetEase(Ease.InQuad));

                seq.Join(sr
                    .DOFade(0f, flyTime * 0.7f)
                    .SetDelay(flyTime * 0.3f));

                yield return new WaitForSeconds(delayBetweenCards);
            }
        }

        yield return new WaitForSeconds(flyTime + 0.4f);
        GameManager.instance.WinGame();
    }
}
