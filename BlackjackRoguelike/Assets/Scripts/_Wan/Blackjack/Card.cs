/// <summary>무늬와 등급으로 이루어진 변경 불가능한 카드 한 장입니다.</summary>
public readonly struct Card
{
    // 카드 무늬입니다.
    public CardSuit Suit { get; }
    // 카드 등급입니다.
    public CardRank Rank { get; }

    // 에이스를 1로 본 기본 점수입니다.
    public int BaseValue => Rank switch
    {
        CardRank.Ace => 1,
        CardRank.Jack or CardRank.Queen or CardRank.King => 10,
        _ => (int)Rank
    };

    // 지정한 무늬와 등급으로 카드를 생성합니다.
    public Card(CardSuit suit, CardRank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    // Console에 표시할 카드 이름을 반환합니다.
    public override string ToString() => $"{Suit} {Rank}";
}
