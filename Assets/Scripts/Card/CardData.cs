using UnityEngine;

public enum Suit { Hearts, Diamonds, Clubs, Spades }
public enum CardColor { Red, Black }

[System.Serializable]
public class CardData
{
    public Suit suit;
    public string rank;
    public int rankValue; // A=1 ... K=13
    public CardColor color;
    public Sprite sprite;
}
