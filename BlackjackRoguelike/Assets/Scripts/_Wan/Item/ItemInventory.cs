using System.Collections.Generic;

/// <summary>플레이어가 획득한 패시브와 액티브 아이템을 구분해 보관합니다.</summary>
public class ItemInventory
{
    private readonly List<ItemInstance> _passiveItems = new();
    private readonly List<ItemInstance> _activeItems = new();

    // 보유한 패시브 아이템입니다.
    public IReadOnlyList<ItemInstance> PassiveItems => _passiveItems;
    // 보유한 액티브 아이템입니다.
    public IReadOnlyList<ItemInstance> ActiveItems => _activeItems;

    // 패시브 중복을 막고 액티브는 중첩을 허용하며 아이템을 인벤토리에 추가합니다.
    public bool TryAdd(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null) return false;

        if (itemDefinition.ItemType == ItemType.Passive)
        {
            foreach (ItemInstance _item in _passiveItems)
            {
                if (_item.Definition == itemDefinition) return false;
            }

            _passiveItems.Add(new ItemInstance { Definition = itemDefinition, StackCount = 1 });
            return true;
        }

        _activeItems.Add(new ItemInstance { Definition = itemDefinition, StackCount = 1 });
        return true;
    }

    // 드롭 중복 방지에 사용할 현재 보유 패시브 정의 목록을 반환합니다.
    public HashSet<ItemDefinition> GetOwnedPassiveDefinitions()
    {
        HashSet<ItemDefinition> _ownedItems = new();
        foreach (ItemInstance _item in _passiveItems)
        {
            if (_item.Definition != null) _ownedItems.Add(_item.Definition);
        }
        return _ownedItems;
    }

    // 보유한 액티브 아이템을 사용한 뒤 인벤토리에서 제거합니다.
    public bool TryUseActive(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.IsUsed) return false;

        for (int _index = 0; _index < _activeItems.Count; _index++)
        {
            if (_activeItems[_index] != itemInstance) continue;
            itemInstance.IsUsed = true;
            _activeItems.RemoveAt(_index);
            return true;
        }

        return false;
    }
}
