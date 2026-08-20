using UnityEngine;

/// <summary>몬스터가 가진 방해 또는 강화 능력의 데이터 정의입니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Monster Ability Definition")]
public class MonsterAbilityDefinition : ScriptableObject
{
    [Tooltip("UI에 표시할 능력 이름입니다.")]
    public string AbilityName;
    [TextArea]
    [Tooltip("UI 툴팁에 표시할 능력 설명입니다.")]
    public string Description;
    [Tooltip("능력을 확인하거나 적용할 시점입니다.")]
    public ItemTrigger Trigger;
    [Tooltip("능력의 정수 수치입니다.")]
    public int Value;
    [Tooltip("능력의 소수 수치입니다.")]
    public float FloatValue;
}
