using UnityEngine;
using System.Collections.Generic;

/// <summary>아이템의 고정 데이터와 효과 설정을 보관하는 ScriptableObject입니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Tooltip("UI에 표시할 아이템 이름입니다.")]
    public string ItemName;
    [Tooltip("UI에 표시할 아이콘 스프라이트입니다.")]
    public Sprite Icon;
    [TextArea(2, 5)]
    [Tooltip("아이콘 위에 커서를 올렸을 때 표시할 아이템 설명입니다.")]
    public string Description;

    [Tooltip("패시브 또는 액티브 분류입니다.")]
    public ItemType ItemType;
    [Tooltip("일반, 희귀, 전설 등급입니다.")]
    public ItemRarity Rarity;

    [Tooltip("한 아이템에 연결할 효과 목록입니다. 예: 체력 회복과 이번 라운드 공격력 0 고정은 효과를 2개 추가합니다.")]
    public List<ItemEffectData> Effects = new();
}
