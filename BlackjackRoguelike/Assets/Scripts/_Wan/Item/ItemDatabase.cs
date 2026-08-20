using System.Collections.Generic;
using UnityEngine;

/// <summary>모든 아이템 정의를 타입과 등급별 드롭 풀로 분류하는 데이터베이스입니다.</summary>
[CreateAssetMenu(menuName = "Blackjack/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Tooltip("생성한 모든 ItemDefinition 에셋을 등록합니다. 타입과 등급별 풀은 자동으로 분류됩니다.")]
    public List<ItemDefinition> AllItems = new();

    private readonly Dictionary<ItemType, Dictionary<ItemRarity, List<ItemDefinition>>> _itemPools = new();

    // 데이터베이스가 로드될 때 타입·등급별 드롭 풀을 구성합니다.
    private void OnEnable()
    {
        RebuildItemPools();
    }

    // Inspector에서 아이템 목록을 수정했을 때 드롭 풀을 다시 구성합니다.
    private void OnValidate()
    {
        RebuildItemPools();
    }

    // 등록된 모든 아이템을 타입과 등급별 리스트에 다시 분류합니다.
    public void RebuildItemPools()
    {
        _itemPools.Clear();

        foreach (ItemType _itemType in System.Enum.GetValues(typeof(ItemType)))
        {
            Dictionary<ItemRarity, List<ItemDefinition>> _rarityPools = new();
            foreach (ItemRarity _itemRarity in System.Enum.GetValues(typeof(ItemRarity)))
            {
                _rarityPools.Add(_itemRarity, new List<ItemDefinition>());
            }
            _itemPools.Add(_itemType, _rarityPools);
        }

        foreach (ItemDefinition _item in AllItems)
        {
            if (_item == null || _itemPools[_item.ItemType][_item.Rarity].Contains(_item)) continue;
            _itemPools[_item.ItemType][_item.Rarity].Add(_item);
        }
    }

    // 지정한 타입과 등급에 속하는 드롭 후보 목록을 반환합니다.
    public IReadOnlyList<ItemDefinition> GetItems(ItemType itemType, ItemRarity itemRarity)
    {
        if (_itemPools.Count == 0) RebuildItemPools();
        return _itemPools[itemType][itemRarity];
    }
}
