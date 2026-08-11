/// <summary>레벨을 가지며 플레이어 입력으로 조작되는 캐릭터입니다.</summary>
public sealed class Player : Character
{
    // 현재 레벨입니다.
    public int Level { get; private set; }

    // 체력 100, 공격력 계수 1, 레벨 1의 플레이어를 생성합니다.
    public Player(string name = "플레이어") : base(name, 100, 1f)
    {
        Level = 1;
    }
}
