using System;
using System.Collections.Generic;

/// <summary>등급 확률과 중복 규칙에 따라 스테이지 아이템 보상을 생성합니다.</summary>
public class ItemDropManager
{
    private const int CommonWeight = 45;
    private const int RareWeight = 35;
    private const int LegendaryWeight = 20;

    private readonly Random _random;

    // 필요하면 시드를 지정해 재현 가능한 드롭 결과를 만들 수 있습니다.
    public ItemDropManager(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    // 패시브 후보 2개와 액티브 후보 3개를 생성합니다. 같은 보상 화면의 후보는 중복되지 않습니다.
    public StageItemDropResult GenerateStageDrops(ItemDatabase itemDatabase, ISet<ItemDefinition> ownedPassiveItems)
    {
        if (itemDatabase == null) return new StageItemDropResult(new List<ItemDefinition>(), new List<ItemDefinition>());

        List<ItemDefinition> _passiveCandidates = GenerateUniqueItems(itemDatabase, ItemType.Passive, 2, ownedPassiveItems);
        List<ItemDefinition> _activeCandidates = GenerateUniqueItems(itemDatabase, ItemType.Active, 3, null);
        return new StageItemDropResult(_passiveCandidates, _activeCandidates);
    }

    // 기존 제외 목록과 이미 뽑힌 후보를 모두 제외해 중복 없는 후보 목록을 만듭니다.
    // 동일한 등급 확률과 중복 제외 규칙으로 지정한 수만큼 아이템 후보를 생성합니다.
    public List<ItemDefinition> GenerateUniqueItems(ItemDatabase itemDatabase, ItemType itemType, int count, ISet<ItemDefinition> excludedItems)
    {
        HashSet<ItemDefinition> _excludedItems = excludedItems != null
            ? new HashSet<ItemDefinition>(excludedItems)
            : new HashSet<ItemDefinition>();
        List<ItemDefinition> _items = new();

        for (int _index = 0; _index < count; _index++)
        {
            ItemDefinition _item = RollItem(itemDatabase, itemType, _excludedItems);
            if (_item == null) break;

            _items.Add(_item);
            _excludedItems.Add(_item);
        }

        return _items;
    }

    // 확률로 등급을 선택한 뒤 해당 풀에서 아이템 하나를 뽑습니다.
    private ItemDefinition RollItem(ItemDatabase itemDatabase, ItemType itemType, ISet<ItemDefinition> excludedItems)
    {
        List<ItemRarity> _availableRarities = GetAvailableRarities(itemDatabase, itemType, excludedItems);
        if (_availableRarities.Count == 0) return null;

        ItemRarity _rolledRarity = RollRarity(_availableRarities);
        List<ItemDefinition> _candidates = GetCandidates(itemDatabase, itemType, _rolledRarity, excludedItems);
        return _candidates[_random.Next(_candidates.Count)];
    }

    // 후보가 하나 이상 존재하는 등급만 반환합니다.
    private List<ItemRarity> GetAvailableRarities(ItemDatabase itemDatabase, ItemType itemType, ISet<ItemDefinition> excludedItems)
    {
        List<ItemRarity> _availableRarities = new();
        foreach (ItemRarity _itemRarity in Enum.GetValues(typeof(ItemRarity)))
        {
            if (GetCandidates(itemDatabase, itemType, _itemRarity, excludedItems).Count > 0) _availableRarities.Add(_itemRarity);
        }
        return _availableRarities;
    }

    // 지정한 풀에서 제외 목록을 뺀 후보를 반환합니다.
    private List<ItemDefinition> GetCandidates(ItemDatabase itemDatabase, ItemType itemType, ItemRarity itemRarity, ISet<ItemDefinition> excludedItems)
    {
        List<ItemDefinition> _candidates = new();
        foreach (ItemDefinition _item in itemDatabase.GetItems(itemType, itemRarity))
        {
            if (_item != null && (excludedItems == null || !excludedItems.Contains(_item))) _candidates.Add(_item);
        }
        return _candidates;
    }

    // 사용 가능한 등급만 대상으로 일반 40%, 희귀 35%, 전설 25% 비율을 유지해 등급을 뽑습니다.
    private ItemRarity RollRarity(List<ItemRarity> availableRarities)
    {
        int _totalWeight = 0;
        foreach (ItemRarity _itemRarity in availableRarities) _totalWeight += GetRarityWeight(_itemRarity);

        int _roll = _random.Next(_totalWeight);
        int _accumulatedWeight = 0;
        foreach (ItemRarity _itemRarity in availableRarities)
        {
            _accumulatedWeight += GetRarityWeight(_itemRarity);
            if (_roll < _accumulatedWeight) return _itemRarity;
        }

        return availableRarities[availableRarities.Count - 1];
    }

    // 등급별 고정 가중치를 반환합니다.
    private int GetRarityWeight(ItemRarity itemRarity)
    {
        return itemRarity switch
        {
            ItemRarity.Common => CommonWeight,
            ItemRarity.Rare => RareWeight,
            ItemRarity.Legendary => LegendaryWeight,
            _ => 0
        };
    }
}
