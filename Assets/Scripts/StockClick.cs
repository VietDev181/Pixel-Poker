using UnityEngine;

public class StockClick : MonoBehaviour
{
    public CardsController controller;

    [Header("Sprites")]
    public Sprite stockBackSprite;
    public Sprite stockEmptySprite;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        int drawCount = controller.drawThree ? 3 : 1;
        controller.DealCards(drawCount, transform);
    }

    public void UpdateVisual()
    {
        sr.sprite = controller.HasStockCards()
            ? stockBackSprite
            : stockEmptySprite;
    }
}
