/// <summary>블랙잭에서 딜러 역할을 맡는 적 캐릭터입니다.</summary>
public sealed class Monster : Character
{
    // 이 몬스터를 생성한 원본 데이터입니다.
    public MonsterDefinition Definition { get; }
    // 몬스터 처치 시 지급할 기본 골드입니다.
    public int DropGold { get; }

    // 테스트용 기본 몬스터를 생성합니다.
    public Monster(string name = "몬스터") : base(name, 100, 1f)
    {
    }

    // MonsterDefinition 데이터로 전투용 몬스터를 생성합니다.
    public Monster(MonsterDefinition definition) : base(definition.MonsterName, definition.MaxHp, definition.AttackMultiplier)
    {
        Definition = definition;
        DropGold = definition.DropGold;
    }
}
