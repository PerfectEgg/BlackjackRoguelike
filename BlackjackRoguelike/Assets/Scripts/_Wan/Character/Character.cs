/// <summary>체력과 공격력 계수를 공유하는 플레이어·몬스터의 추상 기반 클래스입니다.</summary>
public abstract class Character
{
    // 공격력 계수가 바뀐 뒤 발생합니다.
    public event System.Action<Character> AttackMultiplierChanged;
    // 최대 체력이 바뀐 뒤 발생합니다.
    public event System.Action<Character> MaxHpChanged;
    // 현재 체력이 바뀐 뒤 발생합니다.
    public event System.Action<Character> CurrentHpChanged;
    // 캐릭터 이름입니다.
    public string Name { get; }
    // 최대 체력입니다.
    public int MaxHp { get; private set; }
    // 아이템 보정 전 캐릭터 고유 최대 체력입니다.
    public int BaseMaxHp { get; private set; }
    // 현재 체력입니다.
    public int CurrentHp { get; private set; }
    // 공격력 계수입니다.
    public float AttackMultiplier { get; protected set; }
    // 아이템 보정 전 캐릭터 고유 공격력 계수입니다.
    public float BaseAttackMultiplier { get; private set; }
    // 패배 여부입니다.
    public bool IsDefeated => CurrentHp <= 0f;

    // 기본 체력과 공격력 계수를 지정해 캐릭터를 생성합니다.
    protected Character(string name, int maxHp = 100, float attackMultiplier = 1f)
    {
        Name = name;
        MaxHp = maxHp;
        BaseMaxHp = maxHp;
        CurrentHp = maxHp;
        BaseAttackMultiplier = attackMultiplier;
        AttackMultiplier = attackMultiplier;
    }

    // 양수 피해만큼 체력을 감소시키며, 체력은 0 아래로 내려가지 않습니다.
    public virtual void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        int _nextHp = System.Math.Max(0, CurrentHp - damage);
        if (CurrentHp == _nextHp) return;

        CurrentHp = _nextHp;
        CurrentHpChanged?.Invoke(this);
    }

    // 아이템 사용 비용처럼 방어 효과를 거치지 않는 체력 감소를 적용하며 최소 체력 1을 보장합니다.
    public void LoseHp(int amount)
    {
        if (amount <= 0) return;
        int _nextHp = System.Math.Max(1, CurrentHp - amount);
        if (CurrentHp == _nextHp) return;

        CurrentHp = _nextHp;
        CurrentHpChanged?.Invoke(this);
    }

    // 최대 체력을 넘지 않도록 현재 체력을 회복합니다.
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int _nextHp = System.Math.Min(MaxHp, CurrentHp + amount);
        if (CurrentHp == _nextHp) return;

        CurrentHp = _nextHp;
        CurrentHpChanged?.Invoke(this);
    }

    // 아이템 효과를 반영한 최종 공격력 계수를 설정합니다.
    public void SetAttackMultiplier(float attackMultiplier)
    {
        float _nextMultiplier = System.Math.Max(0f, attackMultiplier);
        if (System.Math.Abs(AttackMultiplier - _nextMultiplier) < 0.0001f) return;

        AttackMultiplier = _nextMultiplier;
        AttackMultiplierChanged?.Invoke(this);
    }

    // 레벨업 등으로 아이템 보정 전 공격력 계수를 변경합니다.
    protected void SetBaseAttackMultiplier(float attackMultiplier)
    {
        BaseAttackMultiplier = System.Math.Max(0f, attackMultiplier);
    }

    // 레벨업 등으로 아이템 보정 전 최대 체력을 변경합니다.
    protected void SetBaseMaxHp(int maxHp)
    {
        BaseMaxHp = System.Math.Max(1, maxHp);
    }

    // 아이템 보정이 반영된 최종 최대 체력을 설정합니다.
    public void SetMaxHp(int maxHp)
    {
        int _nextMaxHp = System.Math.Max(1, maxHp);
        if (MaxHp == _nextMaxHp) return;

        int _increasedAmount = System.Math.Max(0, _nextMaxHp - MaxHp);
        MaxHp = _nextMaxHp;
        int _nextCurrentHp = _increasedAmount > 0
            ? System.Math.Min(MaxHp, CurrentHp + _increasedAmount)
            : System.Math.Min(CurrentHp, MaxHp);
        if (CurrentHp != _nextCurrentHp)
        {
            CurrentHp = _nextCurrentHp;
            CurrentHpChanged?.Invoke(this);
        }
        MaxHpChanged?.Invoke(this);
    }
}
