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
    [Tooltip("몬스터를 표시할 스프라이트입니다.")]
    public Sprite Icon;
    [Tooltip("몬스터의 최대 체력입니다.")]
    [Min(1)] public int MaxHp = 100;
    [Tooltip("몬스터가 승리할 때 점수에 곱할 공격력 계수입니다.")]
    [Min(0f)] public float AttackMultiplier = 1f;
    [Tooltip("몬스터 처치 시 플레이어에게 지급할 기본 골드입니다.")]
    [Min(0)] public int DropGold;
    [Tooltip("몬스터가 보유할 능력 에셋 목록입니다. 실제 효과 처리는 추후 연결합니다.")]
    public List<MonsterAbilityDefinition> Abilities = new();
}
