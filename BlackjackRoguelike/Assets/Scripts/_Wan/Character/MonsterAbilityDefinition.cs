using UnityEngine;

/// <summary>몬스터가 가진 방해 또는 강화 능력의 데이터 정의입니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Monster Ability Definition")]
public class MonsterAbilityDefinition : ScriptableObject
{
    [Header("표시 정보")]
    [TextArea]
    [Tooltip("UI 툴팁에 표시할 능력 설명입니다.")]
    public string Description;
    [Header("능력 동작")]
    [Tooltip("몬스터 기믹의 구체적인 동작입니다.")]
    public MonsterAbilityType AbilityType;
    [Tooltip("능력의 정수 수치입니다.")]
    public int Value;
    [Tooltip("능력의 소수 수치입니다.")]
    public float FloatValue;

    // 아이콘 호버 UI가 특성 설명을 표시할 때 사용합니다.
    public string GetTooltipText()
    {
        return Description;
    }
}

// 몬스터가 스테이지 전투에서 사용할 수 있는 고유 기믹 종류입니다.
public enum MonsterAbilityType
{
    None = 0,
    HideNextDealerCard = 1,
    SameSuitPairRoundAttack = 9,
    RemoveLastDealerCardOnBust = 2,
    DeclareRankPermanentAttack = 3,
    DeclareRankHeal = 4,
    CardColorBonus = 5,
    DealerScoreAdjustment = 6,
    HeartExecution = 7,
    ForceBlackJackAndBlackJackBonus = 8
}
