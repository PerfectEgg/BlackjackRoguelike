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

    // 아이템 획득 직후 영구 공격력 패시브를 다시 계산합니다.
    public void ApplyOnItemAcquired(Player player, ItemInventory inventory)
    {
        RecalculateAttackMultiplier(player, inventory, null, ItemTrigger.None);
        RecalculatePermanentMaxHp(player, inventory);
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

    // 액티브 아이템 사용 시 공격력 계수에 수치를 곱한 직접 피해를 계산합니다.
    public int CalculateActiveDirectDamage(Player player, ItemInstance itemInstance)
    {
        if (player == null || itemInstance?.Definition == null || itemInstance.Definition.ItemType != ItemType.Active) return 0;

        int _damage = 0;
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (!IsActiveDirectDamageEffect(_effect)) continue;
            float _scale = _effect.IntValue > 0 ? _effect.IntValue : _effect.FloatValue;
            _damage += (int)MathF.Floor(player.AttackMultiplier * _scale);
        }

        return Math.Max(0, _damage);
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

        return Math.Max(0, Mathf.FloorToInt(dealtDamage * Math.Max(0f, _lifeStealRate)));
    }

    // 새 라운드에 예약된 공격력 보너스를 적용하고 이전 라운드 보너스는 제거합니다.
    public void BeginRound(Player player, ItemInventory inventory, BlackjackHand playerHand)
    {
        _currentRoundAttackBonus = _nextRoundAttackBonus;
        _nextRoundAttackBonus = 0f;
        RecalculateAttackMultiplier(player, inventory, playerHand, ItemTrigger.None);
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
    public void RecalculateAttackMultiplier(Player player, ItemInventory inventory, BlackjackHand playerHand, ItemTrigger trigger, MatchResult? matchResult = null, bool playerDoubleDownAttempted = false, int playerExtraDrawCount = 0)
    {
        if (player == null || inventory == null) return;

        float _attackMultiplier = player.BaseAttackMultiplier + _currentRoundAttackBonus;
        foreach (ItemInstance _item in inventory.PassiveItems)
        {
            if (_item.Definition == null) continue;
            ApplyAttackEffects(_item, playerHand, trigger, matchResult, playerDoubleDownAttempted, playerExtraDrawCount, ref _attackMultiplier);
        }

        player.SetAttackMultiplier(_attackMultiplier);
    }

    // 한 패시브 아이템의 현재 시점에 적용 가능한 공격력 계수 효과를 계산값에 반영합니다.
    private void ApplyAttackEffects(ItemInstance item, BlackjackHand playerHand, ItemTrigger trigger, MatchResult? matchResult, bool playerDoubleDownAttempted, int playerExtraDrawCount, ref float attackMultiplier)
    {
        foreach (ItemEffectData _effect in item.Definition.Effects)
        {
            if (!CanApplyAttackEffect(_effect, playerHand, trigger, matchResult, playerDoubleDownAttempted, playerExtraDrawCount)) continue;

            int _stackCount = Math.Max(1, item.StackCount);
            switch (_effect.Operation)
            {
                case ItemEffectOperation.Add:
                    attackMultiplier += _effect.FloatValue * _stackCount;
                    break;
                case ItemEffectOperation.Multiply:
                    attackMultiplier *= Mathf.Pow(_effect.FloatValue, _stackCount);
                    break;
                case ItemEffectOperation.Set:
                    attackMultiplier = _effect.FloatValue;
                    break;
            }
        }
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
}
