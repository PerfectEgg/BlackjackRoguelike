using UnityEngine;
using TMPro;

/// <summary>Inspector에서 만든 카드 UI 템플릿에 카드 숫자와 무늬를 표시합니다.</summary>
public sealed class BlackjackCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _cardText;

    // 카드 데이터를 텍스트 표기로 설정합니다.
    public void SetCard(Card card)
    {
        if (_cardText == null) _cardText = GetComponentInChildren<TMP_Text>();
        if (_cardText == null) return;

        _cardText.text = $"{GetRankText(card.Rank)}\n{GetSuitText(card.Suit)}";
        _cardText.color = card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Hearts
            ? new Color(0.72f, 0.12f, 0.16f)
            : new Color(0.08f, 0.1f, 0.14f);
    }

    // 카드 값이 공개되기 전까지 카드 뒷면처럼 물음표로 표시합니다.
    public void SetHidden()
    {
        if (_cardText == null) _cardText = GetComponentInChildren<TMP_Text>();
        if (_cardText == null) return;

        _cardText.text = "?\n?";
        _cardText.color = new Color(0.42f, 0.42f, 0.42f);
    }

    // 카드 등급을 짧은 표기로 변환합니다.
    private string GetRankText(CardRank rank)
    {
        return rank switch
        {
            CardRank.Ace => "A",
            CardRank.Jack => "J",
            CardRank.Queen => "Q",
            CardRank.King => "K",
            _ => ((int)rank).ToString()
        };
    }

    // 카드 무늬를 문자로 변환합니다.
    private string GetSuitText(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Clubs => "♣",
            CardSuit.Diamonds => "♦",
            CardSuit.Hearts => "♥",
            _ => "♠"
        };
    }
}
