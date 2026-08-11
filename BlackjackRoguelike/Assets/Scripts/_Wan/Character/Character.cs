/// <summary>체력과 공격력 계수를 공유하는 플레이어·몬스터의 추상 기반 클래스입니다.</summary>
public abstract class Character
{
    // 캐릭터 이름입니다.
    public string Name { get; }
    // 최대 체력입니다.
    public int MaxHp { get; }
    // 현재 체력입니다.
    public int CurrentHp { get; private set; }
    // 공격력 계수입니다.
    public float AttackMultiplier { get; protected set; }
    // 패배 여부입니다.
    public bool IsDefeated => CurrentHp <= 0f;

    // 기본 체력과 공격력 계수를 지정해 캐릭터를 생성합니다.
    protected Character(string name, int maxHp = 100, float attackMultiplier = 1f)
    {
        Name = name;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        AttackMultiplier = attackMultiplier;
    }

    // 양수 피해만큼 체력을 감소시키며, 체력은 0 아래로 내려가지 않습니다.
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        CurrentHp = System.Math.Max(0, CurrentHp - damage);
    }
}
