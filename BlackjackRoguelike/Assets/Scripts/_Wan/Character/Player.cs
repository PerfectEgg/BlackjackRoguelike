/// <summary>레벨을 가지며 플레이어 입력으로 조작되는 캐릭터입니다.</summary>
public sealed class Player : Character
{
    // 보호막 보유 여부가 바뀐 뒤 발생합니다.
    public event System.Action<bool> BarrierChanged;
    // 현재 레벨입니다.
    public int Level { get; private set; }
    // 다음 한 번의 피해를 막을 보호막 보유 여부입니다.
    public bool HasBarrier { get; private set; }

    // 체력 100, 공격력 계수 1, 레벨 1의 플레이어를 생성합니다.
    public Player(string name = "플레이어") : base(name, 100, 1f)
    {
        Level = 1;
    }

    // 전투 스테이지를 통과해 레벨과 기본 능력치를 올립니다.
    public void LevelUp()
    {
        Level++;
        SetBaseMaxHp(BaseMaxHp + 20);
        SetBaseAttackMultiplier(BaseAttackMultiplier + 0.1f);
    }

    // 보호막을 하나 부여합니다. 이미 보호막이 있어도 중첩하지 않습니다.
    public void GrantBarrier()
    {
        if (HasBarrier) return;
        HasBarrier = true;
        BarrierChanged?.Invoke(HasBarrier);
    }

    // 현재 보유한 보호막을 제거합니다.
    public void RemoveBarrier()
    {
        if (!HasBarrier) return;
        HasBarrier = false;
        BarrierChanged?.Invoke(HasBarrier);
    }

    // 보호막이 있으면 피해를 막고 보호막만 제거합니다.
    public override void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (HasBarrier)
        {
            RemoveBarrier();
            return;
        }

        base.TakeDamage(damage);
    }
}
