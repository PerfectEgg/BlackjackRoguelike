using System.Collections.Generic;
using UnityEngine;

/// <summary>한 스테이지에 등장하는 몬스터의 고정 전투 데이터입니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Monster Definition")]
public class MonsterDefinition : ScriptableObject
{
    [Tooltip("1부터 10까지의 스테이지 번호입니다.")]
    [Range(1, 10)] public int StageNumber;
    [Tooltip("전투와 UI에 표시할 몬스터 이름입니다.")]
    public string MonsterName;
    [TextArea]
    [Tooltip("몬스터 아이콘에 마우스를 올렸을 때 표시할 기본 설명입니다.")]
    public string Description;
    [Header("몬스터 전투 스프라이트")]
    [Tooltip("평상시 표시할 몬스터 스프라이트입니다.")]
    public Sprite IdleSprite;
    [Tooltip("몬스터가 플레이어를 공격한 결과 연출에 표시할 스프라이트입니다.")]
    public Sprite AttackSprite;
    [Tooltip("몬스터가 플레이어 공격을 받은 결과 연출에 표시할 스프라이트입니다.")]
    public Sprite HitSprite;
    [Tooltip("몬스터의 최대 체력입니다.")]
    [Min(1)] public int MaxHp = 100;
    [Tooltip("몬스터가 승리할 때 점수에 곱할 공격력 계수입니다.")]
    [Min(0f)] public float AttackMultiplier = 1f;
    [Tooltip("몬스터 처치 시 플레이어에게 지급할 기본 골드입니다.")]
    [Min(0)] public int DropGold;
    [Header("몬스터 특성")]
    [Tooltip("몬스터 아이콘 호버 시 특성 설명을 함께 표시할 특성 에셋 목록입니다.")]
    public List<MonsterAbilityDefinition> Abilities = new();

    // 몬스터 호버 UI가 이름·설명·특성을 한 번에 표시할 때 사용할 텍스트를 만듭니다.
    public string GetTooltipText()
    {
        string _tooltipText = string.IsNullOrWhiteSpace(Description) ? MonsterName : $"{MonsterName}\n\n{Description}";
        if (Abilities == null || Abilities.Count == 0) return _tooltipText;

        _tooltipText += "\n\n[특성]";
        foreach (MonsterAbilityDefinition _ability in Abilities)
        {
            if (_ability == null) continue;
            _tooltipText += $"\n\n{_ability.GetTooltipText()}";
        }

        return _tooltipText;
    }
}
