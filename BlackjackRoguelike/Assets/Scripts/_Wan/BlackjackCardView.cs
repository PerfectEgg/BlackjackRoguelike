using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>Inspector에서 만든 카드 UI 템플릿에 카드 숫자와 무늬를 표시합니다.</summary>
public sealed class BlackjackCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _cardText;
    [SerializeField] private Image _cardImage;
    [Tooltip("다이아몬드, 하트, 클럽, 스페이드 순서이며 각 무늬 안에서는 A부터 K 순서로 넣습니다.")]
    [SerializeField] private Sprite[] _frontCardSprites = new Sprite[52];
    [Tooltip("딜러 카드가 공개되기 전 표시할 카드 뒷면입니다.")]
    [SerializeField] private Sprite _cardBackSprite;

    // 카드 데이터를 텍스트 표기로 설정합니다.
    public void SetCard(Card card)
    {
        SetFrontSprite(card);
        if (_cardText == null) _cardText = GetComponentInChildren<TMP_Text>();
        if (_cardText == null) return;

        _cardText.gameObject.SetActive(_cardImage == null || _cardImage.sprite == null);
        _cardText.text = $"{GetRankText(card.Rank)}\n{GetSuitText(card.Suit)}";
        _cardText.color = card.Suit == CardSuit.Diamonds || card.Suit == CardSuit.Hearts
            ? new Color(0.72f, 0.12f, 0.16f)
            : new Color(0.08f, 0.1f, 0.14f);
    }

    // 카드 값이 공개되기 전까지 카드 뒷면처럼 물음표로 표시합니다.
    public void SetHidden()
    {
        if (_cardImage == null) _cardImage = GetComponent<Image>();
        if (_cardImage != null)
        {
            _cardImage.sprite = _cardBackSprite;
            _cardImage.enabled = _cardBackSprite != null;
        }
        if (_cardText == null) _cardText = GetComponentInChildren<TMP_Text>();
        if (_cardText == null) return;

        _cardText.gameObject.SetActive(_cardBackSprite == null);
        _cardText.text = "?\n?";
        _cardText.color = new Color(0.42f, 0.42f, 0.42f);
    }

    // 카드 무늬와 숫자에 대응하는 앞면 스프라이트를 적용합니다.
    private void SetFrontSprite(Card card)
    {
        if (_cardImage == null) _cardImage = GetComponent<Image>();
        if (_cardImage == null) return;

        int _spriteIndex = GetFrontSpriteIndex(card);
        if (_spriteIndex < 0 || _spriteIndex >= _frontCardSprites.Length || _frontCardSprites[_spriteIndex] == null)
        {
            _cardImage.enabled = false;
            return;
        }

        _cardImage.sprite = _frontCardSprites[_spriteIndex];
        _cardImage.enabled = true;
    }

    // 스프라이트 배열의 다이아몬드→하트→클럽→스페이드, A→K 배치 순서를 카드 인덱스로 변환합니다.
    private int GetFrontSpriteIndex(Card card)
    {
        int _suitIndex = card.Suit switch
        {
            CardSuit.Diamonds => 0,
            CardSuit.Hearts => 1,
            CardSuit.Clubs => 2,
            CardSuit.Spades => 3,
            _ => -1
        };
        int _rankIndex = (int)card.Rank - 1;
        if (_suitIndex < 0 || _rankIndex < 0 || _rankIndex >= 13) return -1;
        return _suitIndex * 13 + _rankIndex;
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
