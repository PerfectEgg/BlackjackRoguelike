using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>상점에 진열되는 아이템 한 칸과 현재 구매 상태를 담습니다.</summary>
public sealed class ShopOffer
{
    // 진열된 아이템 정의입니다.
    public ItemDefinition Item { get; }
    // 할인까지 반영된 최종 구매 가격입니다.
    public int Price { get; }
    // 이번 진열에서 이미 구매됐는지 나타냅니다.
    public bool IsSold { get; private set; }

    // 아이템과 최종 가격으로 진열 정보를 생성합니다.
    public ShopOffer(ItemDefinition item, int price)
    {
        Item = item;
        Price = Math.Max(0, price);
    }

    // 해당 진열 아이템을 구매 완료 상태로 변경합니다.
    public void MarkSold()
    {
        IsSold = true;
    }
}

/// <summary>스테이지별 상점 진열, 가격, 새로 고침을 전투와 분리해 관리합니다.</summary>
public sealed class ShopManager
{
    private const int PassiveOfferCount = 1;
    private const int ActiveOfferCount = 2;
    private const int StagePriceIncrement = 25;

    private readonly ItemDropManager _itemDropManager = new();
    private readonly List<ShopOffer> _offers = new();
    private ItemDatabase _itemDatabase;
    private int _stageNumber;
    private int _refreshCount;
    private float _itemDiscountRate;

    // 현재 상점에 진열된 아이템 목록입니다. 앞쪽은 패시브, 뒤쪽은 액티브입니다.
    public IReadOnlyList<ShopOffer> Offers => _offers;
    // 상점이 열려 있는지 나타냅니다.
    public bool IsOpen => _itemDatabase != null;
    // 현재 상점이 기준으로 삼는 다음 전투 스테이지 번호입니다.
    public int StageNumber => _stageNumber;
    // 이미 사용한 새로 고침 횟수입니다.
    public int RefreshCount => _refreshCount;
    // 첫 새로 고침은 스테이지 x 10, 이후에는 그 배수로 선형 증가합니다.
    public int CurrentRefreshCost => Math.Max(0, _stageNumber * 10 * (_refreshCount + 1));

    // 지정한 스테이지의 상점을 열고 패시브 1개, 액티브 2개를 진열합니다.
    public void Open(ItemDatabase itemDatabase, int stageNumber, ISet<ItemDefinition> ownedPassiveItems, float itemDiscountRate)
    {
        _itemDatabase = itemDatabase;
        _stageNumber = Math.Max(1, stageNumber);
        _refreshCount = 0;
        _itemDiscountRate = Mathf.Clamp01(itemDiscountRate);
        RebuildOffers(ownedPassiveItems);
    }

    // 상점을 닫고 이전 진열 정보를 비웁니다.
    public void Close()
    {
        _itemDatabase = null;
        _offers.Clear();
        _stageNumber = 0;
        _refreshCount = 0;
        _itemDiscountRate = 0f;
    }

    // 새로 고침 비용 지불이 끝난 뒤 새 진열을 생성합니다. 구매한 칸도 모두 다시 채워집니다.
    public bool Refresh(ISet<ItemDefinition> ownedPassiveItems, float itemDiscountRate)
    {
        if (!IsOpen) return false;

        _refreshCount++;
        _itemDiscountRate = Mathf.Clamp01(itemDiscountRate);
        RebuildOffers(ownedPassiveItems);
        return true;
    }

    // 구매 가능한 진열 칸을 반환합니다.
    public bool TryGetPurchasableOffer(int index, out ShopOffer offer)
    {
        offer = null;
        if (index < 0 || index >= _offers.Count) return false;

        ShopOffer _offer = _offers[index];
        if (_offer == null || _offer.Item == null || _offer.IsSold) return false;
        offer = _offer;
        return true;
    }

    // 구매 완료된 진열 칸을 판매 완료 상태로 변경합니다.
    public void MarkOfferPurchased(ShopOffer offer)
    {
        if (offer == null || !_offers.Contains(offer)) return;
        offer.MarkSold();
    }

    // 아이템 데이터베이스의 등급별 확률 풀을 재사용해 상점 진열을 구성합니다.
    private void RebuildOffers(ISet<ItemDefinition> ownedPassiveItems)
    {
        _offers.Clear();
        if (_itemDatabase == null) return;

        List<ItemDefinition> _passiveItems = _itemDropManager.GenerateUniqueItems(_itemDatabase, ItemType.Passive, PassiveOfferCount, ownedPassiveItems);
        List<ItemDefinition> _activeItems = _itemDropManager.GenerateUniqueItems(_itemDatabase, ItemType.Active, ActiveOfferCount, null);
        AddOffers(_passiveItems);
        AddOffers(_activeItems);
    }

    // 뽑힌 아이템을 등급·스테이지·할인이 반영된 진열 정보로 변환합니다.
    private void AddOffers(IReadOnlyList<ItemDefinition> items)
    {
        foreach (ItemDefinition _item in items)
        {
            if (_item == null) continue;
            _offers.Add(new ShopOffer(_item, CalculateItemPrice(_item)));
        }
    }

    // 타입·등급 기본 가격에 스테이지 추가 가격을 더하고 보유 할인율을 적용합니다.
    private int CalculateItemPrice(ItemDefinition item)
    {
        int _basePrice = GetBasePrice(item.ItemType, item.Rarity);
        int _undiscountedPrice = _basePrice + _stageNumber * StagePriceIncrement;
        return Mathf.FloorToInt(_undiscountedPrice * (1f - _itemDiscountRate));
    }

    // 패시브와 액티브의 등급별 기본 가격을 반환합니다.
    private int GetBasePrice(ItemType itemType, ItemRarity rarity)
    {
        return itemType switch
        {
            ItemType.Passive => rarity switch
            {
                ItemRarity.Common => 150,
                ItemRarity.Rare => 200,
                ItemRarity.Legendary => 250,
                _ => 0
            },
            ItemType.Active => rarity switch
            {
                ItemRarity.Common => 50,
                ItemRarity.Rare => 100,
                ItemRarity.Legendary => 150,
                _ => 0
            },
            _ => 0
        };
    }
}
