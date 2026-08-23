using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>아이템 한 개가 가진 효과, 수치, 조건을 조합해 정의하는 직렬화 데이터입니다.</summary>
[Serializable]
public class ItemEffectData
{
    [Tooltip("효과의 큰 분류입니다.")]
    public ItemEffectCategory Category;
    [Tooltip("수치를 더함, 곱함, 고정 등으로 적용하는 방식입니다.")]
    public ItemEffectOperation Operation = ItemEffectOperation.None;
    [Tooltip("이 효과를 검사할 게임 시점입니다.")]
    public ItemTrigger Trigger = ItemTrigger.None;
    [Tooltip("효과가 실제로 적용될 대상입니다. 조건 대상과 다르므로, 필요 없으면 None으로 둡니다.")]
    public ItemEffectTarget Target = ItemEffectTarget.None;
    [InspectorName("액티브 옵션")]
    [Tooltip("공통 카테고리만으로 표현하기 어려운 액티브 전용 동작입니다. 일반적인 직접 피해·회복·공격력 효과에는 None을 사용합니다.")]
    public ActiveEffectAction ActiveAction = ActiveEffectAction.None;

    [Header("세부 발동 조건")]
    [Tooltip("승리, 블랙잭, 버스트처럼 카드와 무관한 조건입니다. 필요 없으면 None입니다.")]
    public ItemConditionType Condition = ItemConditionType.None;
    [Tooltip("일반 조건을 판정할 대상입니다. Condition이 None이면 None으로 둡니다.")]
    public ItemConditionTarget ConditionTarget = ItemConditionTarget.None;
    [Tooltip("최소 드로우 수, 골드, 스테이지 번호 조건의 기준값입니다.")]
    public int ConditionValue;

    [Header("카드 보유 조건")]
    [Tooltip("특정 카드 보유 여부를 확인하는 방식입니다. 카드 조건이 필요 없으면 None입니다.")]
    public CardConditionMode CardConditionMode = CardConditionMode.None;
    [Tooltip("카드 조건을 확인할 패입니다. CardConditionMode가 None이면 None으로 둡니다.")]
    public CardConditionTarget CardConditionTarget = CardConditionTarget.None;
    [Tooltip("카드 조건에 사용할 숫자입니다. CardConditionMode가 SuitOnly 또는 None이면 무시합니다.")]
    public CardRank RequiredCardRank;
    [Tooltip("카드 조건에 사용할 무늬입니다. CardConditionMode가 RankOnly 또는 None이면 무시합니다.")]
    public CardSuit RequiredCardSuit;
    [Tooltip("조건에 필요한 카드 장수입니다. 0이면 카드 조건을 사용하지 않습니다.")]
    public int RequiredCardCount;

    [Header("효과 수치")]
    [Tooltip("체력, 피해, 카드 수 등 정수 수치입니다.")]
    public int IntValue;
    [Tooltip("공격력 계수, 확률, 체력 비율 등 소수 수치입니다.")]
    public float FloatValue;
    [Tooltip("Float 값의 적용 기준입니다. 패 장수·골드·체력 비율·점수처럼 동적 수치가 필요할 때 선택합니다.")]
    public ItemEffectValueSource ValueSource = ItemEffectValueSource.Fixed;
    [Tooltip("두 번째 수치가 필요한 복합 효과에 사용합니다.")]
    public float SecondaryFloatValue;
    [Header("지속 범위")]
    [Tooltip("효과가 적용될 범위입니다. 즉시 회복처럼 지속이 없으면 None입니다.")]
    public ItemEffectDurationScope DurationScope = ItemEffectDurationScope.None;
    [Tooltip("DurationScope 범위 안에서 유지할 라운드 또는 스테이지 수입니다. 기본값은 1입니다.")]
    public int Duration;

    [Header("카드 조작")]
    [Tooltip("카드 조작 분류에서 조작할 카드 영역입니다.")]
    public CardManipulationScope CardManipulationScope = CardManipulationScope.None;
    [Tooltip("초기 패 전체 교환, 무작위 카드 리롤, 패 교환 등 카드 조작 방식입니다.")]
    public CardManipulationMode CardManipulationMode = CardManipulationMode.None;
    [Tooltip("한 번 사용할 때 선택하거나 조작할 카드 수입니다.")]
    public int CardManipulationCount;
    [Tooltip("현재 라운드 또는 스테이지에서 사용할 수 있는 최대 횟수입니다.")]
    public int UsageLimit;
    [Tooltip("같은 제한 그룹의 카드 조작 효과 중 하나만 사용하게 합니다. 두 효과를 라운드당 카드 조작 하나로 설정하면 둘 중 하나만 사용할 수 있습니다.")]
    public CardManipulationExclusiveGroup ExclusiveGroup = CardManipulationExclusiveGroup.None;

    [Header("드로우 카드 보정")]
    [Tooltip("DrawCardModifier 분류에서만 사용합니다. 다음 드로우가 현재·다음 스테이지 중 어디에 적용될지 정합니다.")]
    public DrawModifierScope DrawScope = DrawModifierScope.None;
    [Tooltip("카드 보정을 적용할 대상입니다.")]
    public DrawModifierTarget DrawTarget = DrawModifierTarget.None;
    [Tooltip("강제 카드, 리롤, 마지막 카드 복제 등 보정 방식입니다.")]
    public DrawModifierMode DrawMode = DrawModifierMode.None;
    [Tooltip("SetSpecificCard일 때 강제할 카드 숫자입니다.")]
    public CardRank DrawCardRank;
    [Tooltip("SetSpecificCard일 때 강제할 카드 무늬입니다.")]
    public CardSuit DrawCardSuit;
}

/// <summary>한 라운드 또는 스테이지에서 사용한 카드 조작 제한 그룹을 기록합니다.</summary>
public sealed class CardManipulationUsageState
{
    private readonly System.Collections.Generic.HashSet<CardManipulationExclusiveGroup> _usedRoundGroups = new();
    private readonly System.Collections.Generic.HashSet<CardManipulationExclusiveGroup> _usedStageGroups = new();

    // 효과의 제한 그룹이 아직 사용 가능하면 기록하고 true를 반환합니다.
    public bool TryUse(ItemEffectData effect)
    {
        if (effect == null || effect.ExclusiveGroup == CardManipulationExclusiveGroup.None) return true;

        return effect.ExclusiveGroup switch
        {
            CardManipulationExclusiveGroup.OneActionPerRound => _usedRoundGroups.Add(effect.ExclusiveGroup),
            CardManipulationExclusiveGroup.OneActionPerStage => _usedStageGroups.Add(effect.ExclusiveGroup),
            _ => true
        };
    }

    // 새 블랙잭 라운드가 시작될 때 라운드 제한 사용 기록을 비웁니다.
    public void ResetRound()
    {
        _usedRoundGroups.Clear();
    }

    // 새 스테이지가 시작될 때 모든 제한 사용 기록을 비웁니다.
    public void ResetStage()
    {
        _usedRoundGroups.Clear();
        _usedStageGroups.Clear();
    }
}

/// <summary>아이템 획득과 전투 이벤트에 맞춰 아이템 효과를 캐릭터 능력치에 반영합니다.</summary>
public sealed class ItemEffectProcessor
{
    private float _nextRoundAttackBonus;
    private float _currentRoundAttackBonus;
    private readonly List<ActiveAttackRuntimeEffect> _activeAttackEffects = new();
    private readonly List<ItemEffectData> _activeStageBlackjackAttackEffects = new();
    private bool _hasAppliedStageBlackjackAttackThisRound;
    private readonly List<ActiveLifeStealRuntimeEffect> _activeLifeStealEffects = new();
    private readonly List<ActiveMatchEndHealRuntimeEffect> _activeMatchEndHealEffects = new();

    // 아이템 획득 직후 영구 공격력 패시브를 다시 계산합니다.
    public void ApplyOnItemAcquired(Player player, ItemInventory inventory, BlackjackHand playerHand, int playerGold)
    {
        RecalculateAttackMultiplier(player, inventory, playerHand, ItemTrigger.None, null, false, 0, playerGold);
        RecalculatePermanentMaxHp(player, inventory);
    }

    // 매 라운드 시작 전에 블랙잭 초기 패 강제 패시브의 확률을 합산해 한 번 판정합니다.
    public bool TryRollNextRoundBlackjack(ItemInventory inventory)
    {
        if (inventory == null) return false;

        float _totalChance = 0f;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsNextRoundBlackjackEffect(_effect)) continue;
                _totalChance += Mathf.Clamp01(_effect.FloatValue) * Math.Max(1, _item.StackCount);
            }
        }

        return UnityEngine.Random.value < Mathf.Clamp01(_totalChance);
    }

    // 보유 패시브를 기준으로 플레이어의 최종 영구 최대 체력을 계산합니다.
    public void RecalculatePermanentMaxHp(Player player, ItemInventory inventory)
    {
        if (player == null || inventory == null) return;

        int _maxHp = player.BaseMaxHp;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsPermanentMaxHpEffect(_effect)) continue;

                int _stackCount = Math.Max(1, _item.StackCount);
                switch (_effect.Operation)
                {
                    case ItemEffectOperation.Add:
                        _maxHp += _effect.IntValue * _stackCount;
                        break;
                    case ItemEffectOperation.Multiply:
                        _maxHp = Mathf.FloorToInt(_maxHp * Mathf.Pow(_effect.FloatValue, _stackCount));
                        break;
                    case ItemEffectOperation.Set:
                        _maxHp = _effect.IntValue;
                        break;
                }
            }
        }

        player.SetMaxHp(_maxHp);
    }

    // 매 라운드 시작 시 조건 없는 체력 회복 패시브를 적용합니다.
    public void HandleRoundStarted(Player player, ItemInventory inventory)
    {
        if (player == null || inventory == null) return;

        int _healAmount = 0;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsRoundStartHealEffect(_effect)) continue;
                _healAmount += _effect.IntValue * Math.Max(1, _item.StackCount);
            }
        }

        player.Heal(_healAmount);
    }

    // 조건 없는 영구 골드 드롭 패시브를 합산해 몬스터 처치 골드 배율을 반환합니다.
    public float CalculateMonsterGoldDropMultiplier(ItemInventory inventory)
    {
        if (inventory == null) return 1f;

        float _multiplier = 1f;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsPermanentGoldDropEffect(_effect)) continue;

                int _stackCount = Math.Max(1, _item.StackCount);
                switch (_effect.Operation)
                {
                    case ItemEffectOperation.Add:
                        _multiplier += _effect.FloatValue * _stackCount;
                        break;
                    case ItemEffectOperation.Multiply:
                        _multiplier *= Mathf.Pow(_effect.FloatValue, _stackCount);
                        break;
                    case ItemEffectOperation.Set:
                        _multiplier = _effect.FloatValue;
                        break;
                }
            }
        }

        return Math.Max(0f, _multiplier);
    }

    // 조건 없는 영구 점수 보정 패시브를 합산해 플레이어의 보정 가능 범위를 반환합니다.
    public int CalculatePlayerScoreAdjustmentRange(ItemInventory inventory)
    {
        if (inventory == null) return 0;

        int _adjustmentRange = 0;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsPermanentScoreAdjustmentEffect(_effect)) continue;
                _adjustmentRange += _effect.IntValue * Math.Max(1, _item.StackCount);
            }
        }

        return Math.Max(0, _adjustmentRange);
    }

    // 조건 없는 영구 상점 할인 패시브를 합산해 아이템 가격 할인율을 반환합니다.
    public float CalculateShopItemDiscountRate(ItemInventory inventory)
    {
        if (inventory == null) return 0f;

        float _discountRate = 0f;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsPermanentShopDiscountEffect(_effect)) continue;
                _discountRate += _effect.FloatValue * Math.Max(1, _item.StackCount);
            }
        }

        return Mathf.Clamp01(_discountRate);
    }

    // 액티브 아이템 사용 시 공격력 계수 또는 몬스터 최대 체력 비율을 기준으로 직접 피해를 계산합니다.
    public int CalculateActiveDirectDamage(Player player, Monster monster, ItemInstance itemInstance)
    {
        if (player == null || monster == null || itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return 0;

        int _damage = 0;
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveDirectDamageEffect(_effect)) continue;
            _damage += CalculateActiveDirectDamageEffect(player, monster, _effect);
        }

        return Math.Max(0, _damage);
    }

    // 즉시 피해 하나를 값 기준에 맞춰 정수 피해량으로 계산합니다.
    private int CalculateActiveDirectDamageEffect(Player player, Monster monster, ItemEffectData effect)
    {
        if (effect.ValueSource == ItemEffectValueSource.MonsterMaxHpPercent)
        {
            return Mathf.FloorToInt(monster.MaxHp * effect.FloatValue);
        }

        float _scale = effect.IntValue > 0 ? effect.IntValue : effect.FloatValue;
        return Mathf.FloorToInt(player.AttackMultiplier * _scale);
    }

    // 무승부 종료 시 플레이어 점수와 공격력 계수를 기준으로 추가 직접 피해를 계산합니다.
    public int CalculateDrawDirectDamage(Player player, ItemInventory inventory, MatchResult matchResult)
    {
        if (player == null || inventory == null || matchResult.Outcome != MatchOutcome.Draw) return 0;

        int _damage = 0;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsDrawDirectDamageEffect(_effect)) continue;
                float _damageScale = _effect.FloatValue * Math.Max(1, _item.StackCount);
                _damage += (int)MathF.Floor(matchResult.PlayerScore * player.AttackMultiplier * _damageScale);
            }
        }

        return Math.Max(0, _damage);
    }

    // 무승부 시 현재 플레이어 점수와 공격력 계수를 기준으로 발동하는 패시브 회복량을 계산합니다.
    public int CalculateDrawHealAmount(Player player, ItemInventory inventory, MatchResult matchResult)
    {
        if (player == null || inventory == null) return 0;

        int _healAmount = 0;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsDrawHealEffect(_effect)) continue;
                _healAmount += Mathf.FloorToInt(matchResult.PlayerScore * player.AttackMultiplier * _effect.FloatValue * Math.Max(1, _item.StackCount));
            }
        }

        return Math.Max(0, _healAmount);
    }

    // 몬스터 버스트 승리 시 플레이어를 블랙잭으로 간주하는 패시브가 있으면 결과를 블랙잭 승리로 변환합니다.
    public MatchResult ApplyMonsterBustBlackjackConversion(ItemInventory inventory, MatchResult matchResult, int targetScore)
    {
        if (inventory == null || matchResult.Outcome != MatchOutcome.PlayerWin) return matchResult;
        if (matchResult.DealerScore <= targetScore || matchResult.PlayerBlackjack) return matchResult;

        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsMonsterBustBlackjackEffect(_effect)) continue;
                return new MatchResult(matchResult.Outcome, matchResult.PlayerScore, matchResult.DealerScore, true, matchResult.DealerBlackjack);
            }
        }

        return matchResult;
    }

    // 플레이어가 몬스터에게 실제로 입힌 피해량을 기준으로 영구 체력 흡수 회복량을 계산합니다.
    public int CalculateDamageDealtLifeSteal(Player player, ItemInventory inventory, int dealtDamage)
    {
        if (player == null || inventory == null || dealtDamage <= 0) return 0;

        float _lifeStealRate = 0f;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsPermanentLifeStealEffect(_effect)) continue;
                _lifeStealRate += _effect.FloatValue * Math.Max(1, _item.StackCount);
            }
        }

        foreach (ActiveLifeStealRuntimeEffect _runtimeEffect in _activeLifeStealEffects)
        {
            _lifeStealRate += _runtimeEffect.Effect.FloatValue;
        }

        return Math.Max(0, Mathf.FloorToInt(dealtDamage * Math.Max(0f, _lifeStealRate)));
    }

    // 새 라운드를 시작하며 현재 라운드를 모두 소진한 액티브 공격력 효과를 제거합니다.
    public void BeginRound(Player player, ItemInventory inventory, BlackjackHand playerHand, int playerGold)
    {
        _hasAppliedStageBlackjackAttackThisRound = false;
        _currentRoundAttackBonus = _nextRoundAttackBonus;
        _nextRoundAttackBonus = 0f;
        AdvanceActiveAttackEffectDurations();
        AdvanceActiveLifeStealEffectDurations();
        AdvanceActiveMatchEndHealEffectDurations();
        RecalculateAttackMultiplier(player, inventory, playerHand, ItemTrigger.None, null, false, 0, playerGold);
    }

    // 사용 즉시 이번 라운드에 적용되는 공격력 계수 액티브 효과가 있는지 확인합니다.
    public bool HasThisRoundActiveAttackEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsThisRoundActiveAttackEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 다음 라운드 공격력 계수 효과를 예약하는 액티브가 있는지 확인합니다.
    public bool HasNextRoundActiveAttackEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsNextRoundActiveAttackEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 뒤 이번 스테이지의 블랙잭 승리 라운드에 공격력 계수를 더하는 액티브인지 확인합니다.
    public bool HasActiveStageBlackjackAttackEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveStageBlackjackAttackEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 다음 플레이어 드로우를 보정하는 액티브 효과가 있는지 확인합니다.
    public bool HasActiveDrawModifierEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active || match == null) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveDrawModifierEffect(_effect) && match.CanQueueActivePlayerDrawModifier(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 플레이어에게 보호막 한 개를 부여하는 액티브 효과가 있는지 확인합니다.
    public bool HasActiveBarrierEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveBarrierEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 플레이어 체력을 회복하는 액티브 효과가 있는지 확인합니다.
    public bool HasActiveHealEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveHealEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 플레이어에게 골드를 지급하는 액티브 효과가 있는지 확인합니다.
    public bool HasActiveGoldEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveGoldEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 이번 라운드 체력 흡수 효과를 등록하는 액티브가 있는지 확인합니다.
    public bool HasThisRoundActiveLifeStealEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsThisRoundActiveLifeStealEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 이번 라운드 종료 회복을 예약하는 액티브가 있는지 확인합니다.
    public bool HasThisRoundActiveMatchEndHealEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsThisRoundActiveMatchEndHealEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 최대 체력 비례 체력을 베팅하는 액티브 효과가 있는지 확인합니다.
    public bool HasActiveMaxHpBetEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveMaxHpBetEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 다음 라운드 몬스터 강제 드로우 스택을 추가할 수 있는지 확인합니다.
    public bool HasActiveNextRoundDealerForcedDrawEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active || match == null) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveNextRoundDealerForcedDrawEffect(_effect) && match.NextRoundDealerForcedDrawStacks < 2) return true;
        }

        return false;
    }

    // 사용 즉시 현재 매치를 무승부로 종료할 수 있는 액티브가 있는지 확인합니다.
    public bool HasActiveForceCurrentRoundDrawEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveForceCurrentRoundDrawEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 양측 패을 교환할 수 있는 액티브가 있는지 확인합니다.
    public bool HasActiveSwapHandsEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active || match == null || !match.CanSwapHands) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveSwapHandsEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 플레이어 패의 무작위 카드 한 장을 파괴하고 다시 뽑을 수 있는지 확인합니다.
    public bool HasActiveRerollRandomPlayerCardEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active || match == null || !match.CanRerollRandomPlayerCard) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveRerollRandomPlayerCardEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 다음 라운드 플레이어 강제 드로우를 예약하는 액티브가 있는지 확인합니다.
    public bool HasActiveNextRoundPlayerForcedDrawEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveNextRoundPlayerForcedDrawEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 이번 스테이지의 플레이어 버스트 한 번을 무시하는 액티브가 있는지 확인합니다.
    public bool HasActiveIgnoreStageBustEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveIgnoreStageBustEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 이번 스테이지의 버스트를 한 번 무시하고 버스트 점수 피해를 주는 액티브가 있는지 확인합니다.
    public bool HasActiveStageBustScoreDamageEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveStageBustScoreDamageEffect(_effect)) return true;
        }

        return false;
    }

    // 사용 즉시 현재 스테이지 보상을 포기하고 다음 스테이지로 이동하는 액티브인지 확인합니다.
    public bool HasActiveSkipStageWithoutRewardEffect(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return false;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveSkipStageWithoutRewardEffect(_effect)) return true;
        }

        return false;
    }

    // 인벤토리에 상점 상품 하나를 무료로 구매할 수 있는 액티브 이용권이 있는지 확인합니다.
    public bool TryGetFreeShopPurchaseTicket(ItemInventory inventory, out ItemInstance ticket)
    {
        ticket = null;
        if (inventory == null) return false;

        foreach (ItemInstance _item in inventory.ActiveItems)
        {
            if (_item?.Definition == null || _item.IsUsed) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsFreeShopPurchaseTicketEffect(_effect)) continue;
                ticket = _item;
                return true;
            }
        }

        return false;
    }

    // 사용한 액티브에 포함된 드로우 보정을 매치의 다음 플레이어 드로우 예약 큐에 등록합니다.
    public void ApplyActiveDrawModifierEffects(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || match == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveDrawModifierEffect(_effect)) match.TryQueueActivePlayerDrawModifier(_effect);
        }
    }

    // 사용한 액티브의 보호막 효과를 플레이어에게 적용합니다.
    public void ApplyActiveBarrierEffects(Player player, ItemInstance itemInstance)
    {
        if (player == null || itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveBarrierEffect(_effect)) continue;
            player.GrantBarrier();
            return;
        }
    }

    // 사용한 액티브의 즉시 체력 변화를 합산합니다. 양수는 회복, 음수는 체력 비용입니다.
    public int CalculateActiveHealAmount(Player player, ItemInstance itemInstance)
    {
        if (player == null || itemInstance?.Definition == null) return 0;

        int _healthChange = 0;
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveHealEffect(_effect)) continue;
            _healthChange += CalculateActiveHealEffectAmount(player, _effect);
        }

        return _healthChange;
    }

    // 사용한 액티브에 포함된 즉시 골드 획득량을 합산해 정수로 반환합니다.
    public int CalculateActiveGoldAmount(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null) return 0;

        int _goldAmount = 0;
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveGoldEffect(_effect)) continue;
            _goldAmount += _effect.IntValue != 0 ? _effect.IntValue : Mathf.FloorToInt(_effect.FloatValue);
        }

        return Math.Max(0, _goldAmount);
    }

    // 사용한 액티브의 최대 체력 비례 베팅 수치를 계산합니다. 현재 체력 부족으로 비용이 줄어도 약속한 베팅 수치는 유지합니다.
    public int CalculateActiveMaxHpBetAmount(Player player, ItemInstance itemInstance)
    {
        if (player == null || itemInstance?.Definition == null) return 0;

        int _betAmount = 0;
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveMaxHpBetEffect(_effect)) continue;
            float _value = _effect.ValueSource == ItemEffectValueSource.PlayerMaxHpPercent
                ? player.MaxHp * Mathf.Abs(_effect.FloatValue)
                : (_effect.IntValue != 0 ? Mathf.Abs(_effect.IntValue) : Mathf.Abs(_effect.FloatValue));
            _betAmount += Mathf.FloorToInt(_value);
        }

        return Math.Max(0, _betAmount);
    }

    // 사용한 액티브의 체력 흡수 효과를 런타임 상태에 등록합니다.
    public void ApplyThisRoundActiveLifeStealEffects(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsThisRoundActiveLifeStealEffect(_effect)) continue;
            _activeLifeStealEffects.Add(new ActiveLifeStealRuntimeEffect(_effect, Math.Max(1, _effect.Duration)));
        }
    }

    // 사용한 액티브의 매치 종료 회복 효과를 런타임 상태에 등록합니다.
    public void ApplyThisRoundActiveMatchEndHealEffects(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsThisRoundActiveMatchEndHealEffect(_effect)) continue;
            _activeMatchEndHealEffects.Add(new ActiveMatchEndHealRuntimeEffect(_effect, Math.Max(1, _effect.Duration)));
        }
    }

    // 사용한 액티브의 다음 라운드 몬스터 강제 드로우 스택을 등록합니다.
    public void ApplyActiveNextRoundDealerForcedDrawEffects(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || match == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveNextRoundDealerForcedDrawEffect(_effect)) continue;
            if (!match.TryAddNextRoundDealerForcedDrawStack()) return;
        }
    }

    // 사용한 액티브의 양측 패 교환을 매치에 적용합니다.
    public void ApplyActiveSwapHandsEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || match == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveSwapHandsEffect(_effect)) continue;
            match.TrySwapHands();
            return;
        }
    }

    // 사용한 액티브의 플레이어 무작위 카드 파괴 후 재드로우를 매치에 적용합니다.
    public void ApplyActiveRerollRandomPlayerCardEffect(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || match == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveRerollRandomPlayerCardEffect(_effect)) continue;
            match.TryRerollRandomPlayerCard();
            return;
        }
    }

    // 사용한 액티브의 다음 라운드 플레이어 강제 드로우를 매치에 예약합니다.
    public void ApplyActiveNextRoundPlayerForcedDrawEffects(ItemInstance itemInstance, MatchManager match)
    {
        if (itemInstance?.Definition == null || match == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveNextRoundPlayerForcedDrawEffect(_effect)) continue;
            match.AddNextRoundPlayerForcedDraws(2);
        }
    }

    // 예약된 매치 종료 회복 효과를 플레이어의 최종 점수와 공격력 계수로 계산합니다.
    public int CalculateActiveMatchEndHealAmount(Player player, int playerScore)
    {
        if (player == null) return 0;

        int _healAmount = 0;
        foreach (ActiveMatchEndHealRuntimeEffect _runtimeEffect in _activeMatchEndHealEffects)
        {
            ItemEffectData _effect = _runtimeEffect.Effect;
            float _scale = _effect.FloatValue != 0f ? _effect.FloatValue : (_effect.IntValue != 0 ? _effect.IntValue : 1f);
            _healAmount += Mathf.FloorToInt(playerScore * player.AttackMultiplier * _scale);
        }

        return Math.Max(0, _healAmount);
    }

    // 체력 배팅 후 승리 시 실행할 후속 효과인지 확인합니다.
    public bool IsHpBetVictoryFollowUpEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.HpBetVictory &&
               effect.ActiveAction == ActiveEffectAction.None &&
               effect.Category is ItemEffectCategory.Heal or ItemEffectCategory.DirectDamage;
    }

    // 체력 배팅 승리 보상에 포함된 최대 체력 비례 회복량을 계산합니다.
    public int CalculateHpBetVictoryHealAmount(Player player, IReadOnlyList<ItemEffectData> effects)
    {
        if (player == null || effects == null) return 0;

        int _healAmount = 0;
        foreach (ItemEffectData _effect in effects)
        {
            if (!IsHpBetVictoryFollowUpEffect(_effect) || _effect.Category != ItemEffectCategory.Heal) continue;
            if (_effect.Target is not (ItemEffectTarget.None or ItemEffectTarget.Player)) continue;
            _healAmount += _effect.ValueSource == ItemEffectValueSource.PlayerMaxHpPercent
                ? Mathf.FloorToInt(player.MaxHp * _effect.FloatValue)
                : (_effect.IntValue != 0 ? _effect.IntValue : Mathf.FloorToInt(_effect.FloatValue));
        }

        return Math.Max(0, _healAmount);
    }

    // 체력 배팅 승리 보상에 포함된 베팅값 비례 직접 피해를 계산합니다.
    public int CalculateHpBetVictoryDirectDamage(IReadOnlyList<ItemEffectData> effects, int hpBetAmount)
    {
        if (effects == null || hpBetAmount <= 0) return 0;

        int _damage = 0;
        foreach (ItemEffectData _effect in effects)
        {
            if (!IsHpBetVictoryFollowUpEffect(_effect) || _effect.Category != ItemEffectCategory.DirectDamage) continue;
            if (_effect.Target is not (ItemEffectTarget.None or ItemEffectTarget.Monster)) continue;
            if (_effect.ValueSource != ItemEffectValueSource.CurrentRoundHpBet) continue;

            float _multiplier = _effect.FloatValue == 0f ? 1f : _effect.FloatValue;
            _damage += Mathf.FloorToInt(hpBetAmount * _multiplier);
        }

        return Math.Max(0, _damage);
    }

    // 사용한 액티브의 공격력 계수 효과를 런타임 상태에 등록합니다.
    public void ApplyThisRoundActiveAttackEffects(ItemInstance itemInstance, BlackjackHand playerHand, int playerGold)
    {
        if (itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsThisRoundActiveAttackEffect(_effect)) continue;

            int _remainingRounds = Math.Max(1, _effect.Duration);
            _activeAttackEffects.Add(new ActiveAttackRuntimeEffect(_effect, _remainingRounds));
        }
    }

    // 사용한 액티브의 다음 라운드 공격력 계수 효과를 런타임 상태에 예약합니다.
    public void ApplyNextRoundActiveAttackEffects(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsNextRoundActiveAttackEffect(_effect)) continue;
            _activeAttackEffects.Add(new ActiveAttackRuntimeEffect(_effect, Math.Max(1, _effect.Duration), true));
        }
    }

    // 액티브 사용 후 이번 스테이지가 끝날 때까지 블랙잭 공격력 보너스를 보관합니다.
    public void ApplyActiveStageBlackjackAttackEffects(ItemInstance itemInstance)
    {
        if (itemInstance?.Definition == null) return;

        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (IsActiveStageBlackjackAttackEffect(_effect)) _activeStageBlackjackAttackEffects.Add(_effect);
        }
    }

    // 새 스테이지를 시작할 때 이전 스테이지의 블랙잭 공격력 보너스를 비웁니다.
    public void ClearActiveStageBlackjackAttackEffects()
    {
        _activeStageBlackjackAttackEffects.Clear();
    }

    // 매치 결과가 무승부일 때 다음 라운드 공격력 보너스 효과를 예약합니다.
    public void HandleMatchEnded(MatchResult matchResult, ItemInventory inventory)
    {
        if (matchResult.Outcome != MatchOutcome.Draw || inventory == null) return;

        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsNextRoundDrawAttackEffect(_effect)) continue;
                _nextRoundAttackBonus += _effect.FloatValue * Math.Max(1, _item.StackCount);
            }
        }
    }

    // 보유 패시브 중 초기 패 전체 교환 기능을 제공하는 첫 효과를 반환합니다.
    public ItemEffectData FindInitialHandSwapEffect(ItemInventory inventory)
    {
        if (inventory == null) return null;

        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (_effect == null || _effect.Category != ItemEffectCategory.CardManipulation) continue;
                if (_effect.CardManipulationScope != CardManipulationScope.InitialHand) continue;
                if (_effect.CardManipulationMode == CardManipulationMode.SwapInitialHand) return _effect;
            }
        }

        return null;
    }

    // 보유 패시브 중 버스트 시 마지막 카드를 삭제하는 효과를 반환합니다.
    public ItemEffectData FindBustCardRemovalEffect(ItemInventory inventory)
    {
        if (inventory == null) return null;

        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (_effect == null || _effect.Category != ItemEffectCategory.CardManipulation) continue;
                if (_effect.Trigger != ItemTrigger.AfterDraw || _effect.Condition != ItemConditionType.Bust) continue;
                if (_effect.CardManipulationScope != CardManipulationScope.LastDrawnCard) continue;
                if (_effect.CardManipulationMode == CardManipulationMode.RemoveLastCard) return _effect;
            }
        }

        return null;
    }

    // 플레이어가 실제 피해를 받은 뒤 확률형 보호막 패시브를 검사합니다.
    public void HandlePlayerDamageTaken(Player player, ItemInventory inventory)
    {
        if (player == null || inventory == null || player.HasBarrier) return;

        float _totalChance = 0f;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            foreach (ItemEffectData _effect in _item.Definition.Effects)
            {
                if (!IsDamageTakenBarrierEffect(_effect)) continue;
                _totalChance += Mathf.Clamp01(_effect.FloatValue) * Math.Max(1, _item.StackCount);
            }
        }

        if (UnityEngine.Random.value > Mathf.Clamp01(_totalChance)) return;
        player.GrantBarrier();
    }

    // 보유 패시브와 현재 패을 기준으로 플레이어의 최종 공격력 계수를 계산합니다.
    public void RecalculateAttackMultiplier(Player player, ItemInventory inventory, BlackjackHand playerHand, ItemTrigger trigger, MatchResult? matchResult = null, bool playerDoubleDownAttempted = false, int playerExtraDrawCount = 0, int playerGold = 0)
    {
        if (player == null || inventory == null) return;

        float _attackMultiplier = player.BaseAttackMultiplier + _currentRoundAttackBonus;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            ApplyAttackEffects(_item, playerHand, trigger, matchResult, playerDoubleDownAttempted, playerExtraDrawCount, playerGold, ref _attackMultiplier);
        }

        ApplyActiveAttackEffects(playerHand, playerGold, ref _attackMultiplier);
        ApplyActiveStageBlackjackAttackEffects(playerHand, playerGold, matchResult, ref _attackMultiplier);

        player.SetAttackMultiplier(_attackMultiplier);
    }

    // 등록된 액티브 공격력 효과를 기준으로 몬스터의 최종 공격력 계수를 다시 계산합니다.
    public void RecalculateMonsterAttackMultiplier(Monster monster)
    {
        if (monster == null) return;

        float _attackMultiplier = monster.BaseAttackMultiplier;
        ApplyActiveMonsterAttackEffects(ref _attackMultiplier);
        monster.SetAttackMultiplier(_attackMultiplier);
    }

    // 등록된 액티브 공격력 효과를 사용 순서대로 최종 공격력 계수에 반영합니다.
    private void ApplyActiveAttackEffects(BlackjackHand playerHand, int playerGold, ref float attackMultiplier)
    {
        foreach (ActiveAttackRuntimeEffect _runtimeEffect in _activeAttackEffects)
        {
            if (_runtimeEffect.Effect.Target is not (ItemEffectTarget.None or ItemEffectTarget.Player or ItemEffectTarget.Both)) continue;

            float _effectValue = GetAttackEffectValue(_runtimeEffect.Effect, playerHand, playerGold);
            switch (_runtimeEffect.Effect.Operation)
            {
                case ItemEffectOperation.Add:
                    attackMultiplier += _effectValue;
                    break;
                case ItemEffectOperation.Multiply:
                    attackMultiplier *= _effectValue;
                    break;
                case ItemEffectOperation.Set:
                    attackMultiplier = _effectValue;
                    break;
            }
        }
    }

    // 대상이 몬스터 또는 양측인 액티브 공격력 효과만 몬스터에게 적용합니다.
    private void ApplyActiveMonsterAttackEffects(ref float attackMultiplier)
    {
        foreach (ActiveAttackRuntimeEffect _runtimeEffect in _activeAttackEffects)
        {
            if (_runtimeEffect.Effect.Target is not (ItemEffectTarget.Monster or ItemEffectTarget.Both)) continue;

            switch (_runtimeEffect.Effect.Operation)
            {
                case ItemEffectOperation.Add:
                    attackMultiplier += _runtimeEffect.Effect.FloatValue;
                    break;
                case ItemEffectOperation.Multiply:
                    attackMultiplier *= _runtimeEffect.Effect.FloatValue;
                    break;
                case ItemEffectOperation.Set:
                    attackMultiplier = _runtimeEffect.Effect.FloatValue;
                    break;
            }
        }
    }

    // 이번 스테이지에 예약된 블랙잭 공격력 보너스를 블랙잭 승리 결과에만 적용합니다.
    private void ApplyActiveStageBlackjackAttackEffects(BlackjackHand playerHand, int playerGold, MatchResult? matchResult, ref float attackMultiplier)
    {
        if (_hasAppliedStageBlackjackAttackThisRound || !matchResult.HasValue || matchResult.Value.Outcome != MatchOutcome.PlayerWin || !matchResult.Value.PlayerBlackjack) return;

        foreach (ItemEffectData _effect in _activeStageBlackjackAttackEffects)
        {
            float _effectValue = GetAttackEffectValue(_effect, playerHand, playerGold);
            switch (_effect.Operation)
            {
                case ItemEffectOperation.Add:
                    attackMultiplier += _effectValue;
                    break;
                case ItemEffectOperation.Multiply:
                    attackMultiplier *= _effectValue;
                    break;
                case ItemEffectOperation.Set:
                    attackMultiplier = _effectValue;
                    break;
            }
        }

        _hasAppliedStageBlackjackAttackThisRound = true;
    }

    // 라운드가 끝난 효과는 제거하고 남은 효과는 다음 라운드 횟수를 하나 차감합니다.
    private void AdvanceActiveAttackEffectDurations()
    {
        for (int _index = _activeAttackEffects.Count - 1; _index >= 0; _index--)
        {
            ActiveAttackRuntimeEffect _runtimeEffect = _activeAttackEffects[_index];
            if (_runtimeEffect.StartsNextRound)
            {
                _runtimeEffect.StartsNextRound = false;
                continue;
            }
            if (_runtimeEffect.RemainingRounds <= 1)
            {
                _activeAttackEffects.RemoveAt(_index);
                continue;
            }

            _runtimeEffect.RemainingRounds--;
        }
    }

    // 라운드가 끝난 액티브 체력 흡수 효과는 제거하고 남은 효과는 다음 라운드 횟수를 하나 차감합니다.
    private void AdvanceActiveLifeStealEffectDurations()
    {
        for (int _index = _activeLifeStealEffects.Count - 1; _index >= 0; _index--)
        {
            ActiveLifeStealRuntimeEffect _runtimeEffect = _activeLifeStealEffects[_index];
            if (_runtimeEffect.RemainingRounds <= 1)
            {
                _activeLifeStealEffects.RemoveAt(_index);
                continue;
            }

            _runtimeEffect.RemainingRounds--;
        }
    }

    // 라운드가 끝난 매치 종료 회복 효과는 제거하고 남은 효과는 다음 라운드 횟수를 하나 차감합니다.
    private void AdvanceActiveMatchEndHealEffectDurations()
    {
        for (int _index = _activeMatchEndHealEffects.Count - 1; _index >= 0; _index--)
        {
            ActiveMatchEndHealRuntimeEffect _runtimeEffect = _activeMatchEndHealEffects[_index];
            if (_runtimeEffect.RemainingRounds <= 1)
            {
                _activeMatchEndHealEffects.RemoveAt(_index);
                continue;
            }

            _runtimeEffect.RemainingRounds--;
        }
    }

    // 한 패시브 아이템의 현재 시점에 적용 가능한 공격력 계수 효과를 계산값에 반영합니다.
    private void ApplyAttackEffects(ItemInstance item, BlackjackHand playerHand, ItemTrigger trigger, MatchResult? matchResult, bool playerDoubleDownAttempted, int playerExtraDrawCount, int playerGold, ref float attackMultiplier)
    {
        foreach (ItemEffectData _effect in item.Definition.Effects)
        {
            if (!CanApplyAttackEffect(_effect, playerHand, trigger, matchResult, playerDoubleDownAttempted, playerExtraDrawCount)) continue;

            int _stackCount = Math.Max(1, item.StackCount);
            float _effectValue = GetAttackEffectValue(_effect, playerHand, playerGold);
            switch (_effect.Operation)
            {
                case ItemEffectOperation.Add:
                    attackMultiplier += _effectValue * _stackCount;
                    break;
                case ItemEffectOperation.Multiply:
                    attackMultiplier *= Mathf.Pow(_effectValue, _stackCount);
                    break;
                case ItemEffectOperation.Set:
                    attackMultiplier = _effectValue;
                    break;
            }
        }
    }

    // 공격력 계수 효과가 고정 값인지 현재 플레이어 패 장수 비례 값인지 계산합니다.
    private float GetAttackEffectValue(ItemEffectData effect, BlackjackHand playerHand, int playerGold)
    {
        return effect.ValueSource switch
        {
            ItemEffectValueSource.PlayerHandCardCount => playerHand == null ? 0f : effect.FloatValue * playerHand.Cards.Count,
            ItemEffectValueSource.PlayerGold => CalculateGoldBasedAttackValue(effect, playerGold),
            _ => effect.FloatValue
        };
    }

    // 보유 골드를 Int 값 단위로 나눠 공격력 계수 증가량을 계산하고 두 번째 Float 값으로 상한을 제한합니다.
    private float CalculateGoldBasedAttackValue(ItemEffectData effect, int playerGold)
    {
        int _requiredGold = Math.Max(1, effect.IntValue);
        float _attackBonus = Math.Max(0, playerGold) / _requiredGold * effect.FloatValue;
        return effect.SecondaryFloatValue > 0f ? Mathf.Min(_attackBonus, effect.SecondaryFloatValue) : _attackBonus;
    }

    // 현재 이벤트 시점과 카드 보유 조건을 만족하는 공격력 계수 효과인지 확인합니다.
    private bool CanApplyAttackEffect(ItemEffectData effect, BlackjackHand playerHand, ItemTrigger trigger, MatchResult? matchResult, bool playerDoubleDownAttempted, int playerExtraDrawCount)
    {
        if (effect == null || effect.Category != ItemEffectCategory.AttackMultiplier) return false;
        if (!MeetsAttackCondition(effect, matchResult, playerDoubleDownAttempted, playerExtraDrawCount)) return false;

        bool _isAlwaysOn = effect.Trigger == ItemTrigger.None && effect.CardConditionMode == CardConditionMode.None;
        bool _isTriggeredNow = effect.Trigger == trigger;
        bool _isDrawEffectKeptUntilEnd = trigger == ItemTrigger.OnMatchEnd && effect.Trigger == ItemTrigger.AfterDraw;
        if (!_isAlwaysOn && !_isTriggeredNow && !_isDrawEffectKeptUntilEnd) return false;

        return MeetsPlayerCardCondition(effect, playerHand);
    }

    // 공격력 효과에 설정된 일반 조건이 현재 매치 결과를 만족하는지 확인합니다.
    private bool MeetsAttackCondition(ItemEffectData effect, MatchResult? matchResult, bool playerDoubleDownAttempted, int playerExtraDrawCount)
    {
        if (effect.Condition == ItemConditionType.None) return true;
        return effect.Condition switch
        {
            ItemConditionType.DrawCountAtLeast => playerExtraDrawCount >= effect.ConditionValue,
            ItemConditionType.Blackjack => matchResult.HasValue && matchResult.Value.Outcome == MatchOutcome.PlayerWin && matchResult.Value.PlayerBlackjack,
            ItemConditionType.DoubleDown => matchResult.HasValue && matchResult.Value.Outcome == MatchOutcome.PlayerWin && playerDoubleDownAttempted,
            _ => false
        };
    }

    // 플레이어 패에 설정한 숫자·무늬·필요 장수 조건이 충족됐는지 확인합니다.
    private bool MeetsPlayerCardCondition(ItemEffectData effect, BlackjackHand playerHand)
    {
        if (effect.CardConditionMode == CardConditionMode.None) return true;
        if (playerHand == null || effect.CardConditionTarget != CardConditionTarget.Player) return false;

        int _requiredCount = Math.Max(1, effect.RequiredCardCount);
        int _matchedCount = 0;
        foreach (Card _card in playerHand.Cards)
        {
            bool _matches = effect.CardConditionMode switch
            {
                CardConditionMode.RankOnly => _card.Rank == effect.RequiredCardRank,
                CardConditionMode.SuitOnly => _card.Suit == effect.RequiredCardSuit,
                CardConditionMode.ExactCard => _card.Rank == effect.RequiredCardRank && _card.Suit == effect.RequiredCardSuit,
                _ => false
            };
            if (_matches) _matchedCount++;
        }

        return _matchedCount >= _requiredCount;
    }

    // 무승부 후 다음 라운드에만 적용할 공격력 더하기 효과인지 확인합니다.
    private bool IsNextRoundDrawAttackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.AttackMultiplier &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.Draw &&
               effect.DurationScope == ItemEffectDurationScope.NextRound;
    }

    // 사용 즉시 발동해 이번 라운드 동안만 공격력 계수를 바꾸는 액티브인지 확인합니다.
    private bool IsThisRoundActiveAttackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.AttackMultiplier &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               effect.DurationScope == ItemEffectDurationScope.ThisRound;
    }

    // 사용 즉시 예약되어 다음 라운드부터 공격력 계수에 적용되는 액티브인지 확인합니다.
    private bool IsNextRoundActiveAttackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.AttackMultiplier &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               effect.DurationScope == ItemEffectDurationScope.NextRound;
    }

    // 사용 즉시 예약되어 이번 스테이지의 플레이어 블랙잭 승리에만 적용되는 공격력 효과인지 확인합니다.
    private bool IsActiveStageBlackjackAttackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.AttackMultiplier &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.Blackjack &&
               effect.DurationScope == ItemEffectDurationScope.ThisStage &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용 즉시 다음 플레이어 드로우를 예약하는 액티브 카드 보정 효과인지 확인합니다.
    private bool IsActiveDrawModifierEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DrawCardModifier &&
               effect.Operation == ItemEffectOperation.Force &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.DrawTarget == DrawModifierTarget.Player || effect.Target == ItemEffectTarget.Player);
    }

    // 다음 라운드 초기 패 뒤 플레이어가 지정한 장수만큼 강제로 드로우하는 액티브인지 확인합니다.
    private bool IsActiveNextRoundPlayerForcedDrawEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DrawCardModifier &&
               effect.ActiveAction == ActiveEffectAction.ForcePlayerNextRoundDraws &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 이번 스테이지에서 한 번 발생하는 플레이어 버스트 패배를 무시하는 액티브인지 확인합니다.
    private bool IsActiveIgnoreStageBustEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.StageControl &&
               effect.ActiveAction == ActiveEffectAction.IgnoreStageBust &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 이번 스테이지에서 한 번 발생하는 플레이어 버스트를 무시하고 버스트 점수 피해를 주는 액티브인지 확인합니다.
    private bool IsActiveStageBustScoreDamageEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.StageControl &&
               effect.ActiveAction == ActiveEffectAction.BurstScoreDamage &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 보스 스테이지를 제외한 현재 전투를 즉시 넘기는 전용 액티브인지 확인합니다.
    private bool IsActiveSkipStageWithoutRewardEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.StageControl &&
               effect.ActiveAction == ActiveEffectAction.SkipStageWithoutReward &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None;
    }

    // 사용 즉시 플레이어에게 한 번의 공격을 막는 보호막을 부여하는 액티브인지 확인합니다.
    private bool IsActiveBarrierEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Barrier &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용 즉시 플레이어의 체력을 회복하거나 비용으로 감소시키는 액티브인지 확인합니다.
    private bool IsActiveHealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Heal &&
               effect.ActiveAction == ActiveEffectAction.None &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용 즉시 플레이어에게 고정 골드를 지급하는 액티브인지 확인합니다.
    private bool IsActiveGoldEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Gold &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용 즉시 최대 체력 비례 체력을 비용으로 지불하고 이번 라운드 베팅 수치를 기록하는 액티브인지 확인합니다.
    private bool IsActiveMaxHpBetEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Heal &&
               effect.ActiveAction == ActiveEffectAction.MaxHpBet &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 다음 라운드 딜러 턴에서 기본 스탠드 판정 전에 카드 2장을 강제로 뽑게 하는 액티브인지 확인합니다.
    private bool IsActiveNextRoundDealerForcedDrawEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DrawCardModifier &&
               effect.ActiveAction == ActiveEffectAction.ForceDealerOpeningHand &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Monster);
    }

    // 사용 즉시 현재 매치를 무승부로 종료하는 액티브인지 확인합니다.
    private bool IsActiveForceCurrentRoundDrawEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.MatchOutcome &&
               effect.ActiveAction == ActiveEffectAction.ForceCurrentRoundDraw &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Both);
    }

    // 사용 즉시 플레이어와 몬스터의 패을 서로 교환하는 액티브인지 확인합니다.
    private bool IsActiveSwapHandsEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.CardManipulation &&
               effect.ActiveAction == ActiveEffectAction.SwapHands &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Both);
    }

    // 사용 즉시 플레이어 패의 무작위 카드 한 장을 파괴하고 새 카드로 교체하는 액티브인지 확인합니다.
    private bool IsActiveRerollRandomPlayerCardEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.CardManipulation &&
               effect.ActiveAction == ActiveEffectAction.RerollDeckCard &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용하지 않고 상점 구매 비용을 대신 지불하는 무료 구매권 액티브인지 확인합니다.
    private bool IsFreeShopPurchaseTicketEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Shop &&
               effect.ActiveAction == ActiveEffectAction.FreeShopPurchase &&
               effect.Condition == ItemConditionType.None &&
               effect.Trigger is ItemTrigger.None or ItemTrigger.OnUse &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 고정 수치 또는 최대 체력 비율을 사용해 액티브 체력 변화량을 내림 정수로 계산합니다.
    private int CalculateActiveHealEffectAmount(Player player, ItemEffectData effect)
    {
        if (effect.ValueSource == ItemEffectValueSource.PlayerMaxHpPercent)
        {
            return Mathf.FloorToInt(player.MaxHp * effect.FloatValue);
        }

        return effect.IntValue > 0 ? effect.IntValue : Mathf.FloorToInt(effect.FloatValue);
    }

    // 피격 뒤 확률적으로 보호막 하나를 부여하는 패시브 효과인지 확인합니다.
    private bool IsDamageTakenBarrierEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Barrier &&
               effect.Trigger == ItemTrigger.OnDamageTaken &&
               effect.Condition == ItemConditionType.None;
    }

    // 조건 없이 영구적으로 적용되는 몬스터 처치 골드 배율 효과인지 확인합니다.
    private bool IsPermanentGoldDropEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Gold &&
               effect.Trigger == ItemTrigger.None &&
               effect.Condition == ItemConditionType.None &&
               effect.CardConditionMode == CardConditionMode.None &&
               (effect.DurationScope == ItemEffectDurationScope.None || effect.DurationScope == ItemEffectDurationScope.Permanent);
    }

    // 조건 없이 영구적으로 적용되는 최대 체력 효과인지 확인합니다.
    private bool IsPermanentMaxHpEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.MaxHp &&
               effect.Trigger == ItemTrigger.None &&
               effect.Condition == ItemConditionType.None &&
               effect.CardConditionMode == CardConditionMode.None &&
               (effect.DurationScope == ItemEffectDurationScope.None || effect.DurationScope == ItemEffectDurationScope.Permanent);
    }

    // 조건 없이 영구적으로 적용되는 플레이어 점수 보정 효과인지 확인합니다.
    private bool IsPermanentScoreAdjustmentEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.ScoreAdjustment &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.None &&
               effect.Condition == ItemConditionType.None &&
               effect.CardConditionMode == CardConditionMode.None &&
               (effect.DurationScope == ItemEffectDurationScope.None || effect.DurationScope == ItemEffectDurationScope.Permanent);
    }

    // 조건 없이 영구적으로 상점 아이템 가격을 할인하는 효과인지 확인합니다.
    private bool IsPermanentShopDiscountEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Shop &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.None &&
               effect.Condition == ItemConditionType.None &&
               (effect.DurationScope == ItemEffectDurationScope.None || effect.DurationScope == ItemEffectDurationScope.Permanent);
    }

    // 다음 라운드 시작 시 확률적으로 플레이어 초기 패를 블랙잭으로 고정하는 효과인지 확인합니다.
    private bool IsNextRoundBlackjackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DrawCardModifier &&
               effect.Operation == ItemEffectOperation.Force &&
               effect.Trigger == ItemTrigger.RoundStart &&
               effect.Condition == ItemConditionType.None &&
               effect.DrawScope == DrawModifierScope.NextRoundOpeningHand &&
               effect.DrawTarget == DrawModifierTarget.Player &&
               effect.DrawMode == DrawModifierMode.GuaranteeBlackjack;
    }

    // 몬스터가 버스트로 패배했을 때 플레이어 승리를 블랙잭으로 취급하는 효과인지 확인합니다.
    private bool IsMonsterBustBlackjackEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.MatchOutcome &&
               effect.Operation == ItemEffectOperation.Force &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.MonsterBust;
    }

    // 사용 즉시 플레이어 공격력 계수에 수치를 곱해 피해를 주는 액티브 효과인지 확인합니다.
    private bool IsActiveDirectDamageEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DirectDamage &&
               effect.Operation == ItemEffectOperation.Multiply &&
               effect.Trigger == ItemTrigger.OnUse;
    }

    // 무승부 시 플레이어 점수와 공격력 계수에 비율을 곱해 피해를 주는 패시브인지 확인합니다.
    private bool IsDrawDirectDamageEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.DirectDamage &&
               effect.Operation == ItemEffectOperation.Multiply &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.Draw;
    }

    // 무승부 시 플레이어 점수와 공격력 계수에 비례해 회복하는 패시브인지 확인합니다.
    private bool IsDrawHealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Heal &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.Draw &&
               effect.ValueSource == ItemEffectValueSource.PlayerScoreWithAttackMultiplier &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 매 라운드 시작 시 조건 없이 적용되는 체력 회복 효과인지 확인합니다.
    private bool IsRoundStartHealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Heal &&
               effect.Trigger == ItemTrigger.RoundStart &&
               effect.Condition == ItemConditionType.None;
    }

    // 피해를 준 직후 실제 피해량의 일정 비율을 회복하는 영구 체력 흡수 효과인지 확인합니다.
    private bool IsPermanentLifeStealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.LifeSteal &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.OnDamageDealt &&
               effect.Condition == ItemConditionType.None &&
               (effect.DurationScope == ItemEffectDurationScope.None || effect.DurationScope == ItemEffectDurationScope.Permanent);
    }

    // 사용 즉시 일정 라운드 동안 플레이어가 입힌 피해를 흡수하는 액티브인지 확인합니다.
    private bool IsThisRoundActiveLifeStealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.LifeSteal &&
               effect.Operation == ItemEffectOperation.Add &&
               effect.Trigger == ItemTrigger.OnUse &&
               effect.Condition == ItemConditionType.None &&
               effect.DurationScope == ItemEffectDurationScope.ThisRound &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 사용 즉시 예약되어 매치 종료 시 플레이어 최종 점수와 공격력 계수만큼 회복하는 액티브인지 확인합니다.
    private bool IsThisRoundActiveMatchEndHealEffect(ItemEffectData effect)
    {
        return effect != null &&
               effect.Category == ItemEffectCategory.Heal &&
               effect.ActiveAction == ActiveEffectAction.None &&
               effect.Trigger == ItemTrigger.OnMatchEnd &&
               effect.Condition == ItemConditionType.None &&
               effect.ValueSource == ItemEffectValueSource.PlayerScore &&
               effect.DurationScope == ItemEffectDurationScope.ThisRound &&
               (effect.Target == ItemEffectTarget.None || effect.Target == ItemEffectTarget.Player);
    }

    // 여러 라운드 동안 유지할 액티브 공격력 계수 효과의 남은 라운드 정보를 보관합니다.
    private sealed class ActiveAttackRuntimeEffect
    {
        public ItemEffectData Effect { get; }
        public int RemainingRounds { get; set; }
        public bool StartsNextRound { get; set; }

        public ActiveAttackRuntimeEffect(ItemEffectData effect, int remainingRounds, bool startsNextRound = false)
        {
            Effect = effect;
            RemainingRounds = remainingRounds;
            StartsNextRound = startsNextRound;
        }
    }

    // 여러 라운드 동안 유지할 액티브 체력 흡수 효과의 남은 라운드 정보를 보관합니다.
    private sealed class ActiveLifeStealRuntimeEffect
    {
        public ItemEffectData Effect { get; }
        public int RemainingRounds { get; set; }

        public ActiveLifeStealRuntimeEffect(ItemEffectData effect, int remainingRounds)
        {
            Effect = effect;
            RemainingRounds = remainingRounds;
        }
    }

    // 여러 라운드 동안 유지할 매치 종료 회복 효과의 남은 라운드 정보를 보관합니다.
    private sealed class ActiveMatchEndHealRuntimeEffect
    {
        public ItemEffectData Effect { get; }
        public int RemainingRounds { get; set; }

        public ActiveMatchEndHealRuntimeEffect(ItemEffectData effect, int remainingRounds)
        {
            Effect = effect;
            RemainingRounds = remainingRounds;
        }
    }
}
