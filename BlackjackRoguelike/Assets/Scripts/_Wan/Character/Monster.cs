/// <summary>블랙잭에서 딜러 역할을 맡는 적 캐릭터입니다.</summary>
public sealed class Monster : Character
{
    // 체력 100, 공격력 계수 1의 기본 몬스터를 생성합니다.
    public Monster(string name = "몬스터") : base(name, 100, 1f)
    {
    }
}
