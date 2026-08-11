/// <summary>한 번의 블랙잭 결과로 적용된 피해 정보를 담습니다.</summary>
public readonly struct DamageResult
{
    // 피해를 준 캐릭터입니다.
    public Character Attacker { get; }
    // 피해를 받은 캐릭터입니다.
    public Character Defender { get; }
    // 최종 피해량입니다.
    public int Damage { get; }
    // 보너스가 반영된 공격력 계수입니다.
    public float AppliedMultiplier { get; }

    // 전투에 적용된 결과를 생성합니다.
    public DamageResult(Character attacker, Character defender, int damage, float appliedMultiplier)
    {
        Attacker = attacker;
        Defender = defender;
        Damage = damage;
        AppliedMultiplier = appliedMultiplier;
    }
}
