using System;
using System.Collections.Generic;

/// <summary>현재 스테이지 몬스터의 능력과 라운드별 런타임 상태를 관리합니다.</summary>
public sealed class MonsterAbilityProcessor
{
    private readonly Random _random = new();
    private readonly List<MonsterAbilityRuntimeState> _abilityStates = new();
    private float _permanentAttackBonus;
    private float _roundAttackBonus;
    private float _roundAttackMultiplier = 1f;
    private float _baseAttackMultiplier;
    private int _hiddenDealerCardIndex = -1;

    // 이번 라운드에 선언된 첫 숫자를 외부 UI가 표시할 때 사용합니다.
    public CardRank DeclaredRank { get; private set; }
    // 이번 라운드에 숨겨진 몬스터 카드가 있는지 나타냅니다.
    public bool HasHiddenDealerCard => _hiddenDealerCardIndex >= 0;

    // 새 스테이지 몬스터의 능력 목록과 런타임 수치를 초기화합니다.
    public void BeginStage(Monster monster)
    {
        _abilityStates.Clear();
        _permanentAttackBonus = 0f;
        _roundAttackBonus = 0f;
        _roundAttackMultiplier = 1f;
        _baseAttackMultiplier = monster?.AttackMultiplier ?? 0f;
        _hiddenDealerCardIndex = -1;
        DeclaredRank = CardRank.None;

        if (monster?.Definition?.Abilities == null) return;
        foreach (MonsterAbilityDefinition _ability in monster.Definition.Abilities)
        {
            if (_ability != null) _abilityStates.Add(new MonsterAbilityRuntimeState(_ability));
        }
    }

    // 새 라운드마다 사용 제한을 초기화하고 카드 색상으로 얻은 공격력 보너스를 제거합니다.
    public void BeginRound(Monster monster, MatchManager match)
    {
        if (monster == null) return;

        _baseAttackMultiplier = monster.AttackMultiplier;
        _roundAttackBonus = 0f;
        _roundAttackMultiplier = 1f;
        _hiddenDealerCardIndex = -1;
        DeclaredRank = CardRank.None;
        int _dealerScoreAdjustmentRange = 0;
        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            _state.UsedThisRound = false;
            MonsterAbilityDefinition _ability = _state.Definition;
            switch (_ability.AbilityType)
            {
                case MonsterAbilityType.DeclareRankPermanentAttack:
                case MonsterAbilityType.DeclareRankHeal:
                    _state.DeclaredRank = GetRandomNumberRank();
                    if (DeclaredRank == CardRank.None) DeclaredRank = _state.DeclaredRank;
                    break;
                case MonsterAbilityType.DealerScoreAdjustment:
                    _dealerScoreAdjustmentRange = Math.Max(_dealerScoreAdjustmentRange, GetIntValueOrDefault(_ability, 1));
                    break;
                case MonsterAbilityType.ForceBlackJackAndBlackJackBonus:
                    QueueBlackJack(match);
                    _state.UsedThisRound = true;
                    break;
            }
        }

        match?.SetDealerScoreAdjustmentRange(_dealerScoreAdjustmentRange);
        match?.SetDealerCanDrawAboveStandOnce(CanRemoveLastDealerCardOnBust());
        SyncAttackMultiplier(monster);
    }

    // 아이템 등 다른 효과가 몬스터 계수를 다시 계산한 뒤, 기믹 보너스를 포함한 최종 계수를 복원합니다.
    public void ResyncAttackMultiplier(Monster monster)
    {
        if (monster == null) return;

        _baseAttackMultiplier = monster.AttackMultiplier;
        SyncAttackMultiplier(monster);
    }

    // 플레이어가 카드를 뽑은 직후 선언 숫자와 카드 색상 관련 기믹을 처리합니다.
    public void HandlePlayerCardDrawn(Card card, Monster monster)
    {
        if (monster == null) return;

        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            MonsterAbilityDefinition _ability = _state.Definition;
            switch (_ability.AbilityType)
            {
                case MonsterAbilityType.DeclareRankPermanentAttack when !_state.UsedThisRound && card.Rank == _state.DeclaredRank:
                    _permanentAttackBonus += card.BaseValue * GetFloatValueOrDefault(_ability, 0.05f);
                    _state.UsedThisRound = true;
                    SyncAttackMultiplier(monster);
                    break;
                case MonsterAbilityType.DeclareRankHeal when !_state.UsedThisRound && card.Rank == _state.DeclaredRank:
                    monster.Heal(card.BaseValue * GetIntValueOrDefault(_ability, 1));
                    _state.UsedThisRound = true;
                    break;
                case MonsterAbilityType.CardColorBonus:
                    ApplyCardColorBonus(card, monster, _ability);
                    break;
            }
        }
    }

    // 몬스터가 카드를 뽑은 직후 카드 숨김과 검은 J 공격력 배율을 처리합니다.
    public void HandleDealerCardDrawn(MatchManager match, Monster monster)
    {
        if (match == null || monster == null) return;

        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            if (_state.Definition.AbilityType == MonsterAbilityType.HideNextDealerCard && !_state.UsedThisRound)
            {
                _hiddenDealerCardIndex = match.DealerHand.Cards.Count - 1;
                _state.UsedThisRound = true;
            }
        }

        UpdateBlackJackAttackMultiplier(match.DealerHand, monster);
    }

    // 현재 라운드에 숨겨진 몬스터 카드의 UI 인덱스인지 확인합니다.
    public bool IsDealerCardHidden(int cardIndex)
    {
        return cardIndex == _hiddenDealerCardIndex;
    }

    // 숨겨지지 않은 딜러 카드의 기본 점수 합계를 반환해 UI가 "공개 점수 + ?"로 표시하게 합니다.
    public int GetVisibleDealerScore(BlackjackHand dealerHand)
    {
        if (dealerHand == null) return 0;

        int _visibleScore = 0;
        for (int _index = 0; _index < dealerHand.Cards.Count; _index++)
        {
            if (IsDealerCardHidden(_index)) continue;
            _visibleScore += dealerHand.Cards[_index].BaseValue;
        }

        return _visibleScore;
    }

    // 몬스터가 하트 조건을 만족하고 승리했을 때 적용할 처형 피해 배율을 반환합니다.
    public float GetHeartExecutionDamageMultiplier(MatchResult matchResult, BlackjackHand dealerHand)
    {
        if (matchResult.Outcome != MatchOutcome.DealerWin || dealerHand == null) return 1f;

        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            if (_state.Definition.AbilityType != MonsterAbilityType.HeartExecution) continue;

            int _requiredHeartCount = GetIntValueOrDefault(_state.Definition, 2);
            int _heartCount = 0;
            foreach (Card _card in dealerHand.Cards)
            {
                if (_card.Suit == CardSuit.Hearts) _heartCount++;
            }

            if (_heartCount >= _requiredHeartCount) return GetFloatValueOrDefault(_state.Definition, 1.5f);
        }

        return 1f;
    }

    // 몬스터 버스트 후 라운드당 한 번 마지막 카드를 삭제할 기믹이 있으면 처리합니다.
    public bool TryRemoveLastDealerCardOnBust(MatchManager match)
    {
        if (match == null || match.DealerScore <= match.TargetScore) return false;

        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            if (_state.UsedThisRound || _state.Definition.AbilityType != MonsterAbilityType.RemoveLastDealerCardOnBust) continue;
            if (!match.TryRemoveLastDealerCard(out Card _)) return false;

            _state.UsedThisRound = true;
            return true;
        }

        return false;
    }

    // 이번 라운드에 아직 사용하지 않은 몬스터 버스트 카드 제거 기회가 있는지 확인합니다.
    private bool CanRemoveLastDealerCardOnBust()
    {
        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            if (_state.Definition.AbilityType == MonsterAbilityType.RemoveLastDealerCardOnBust && !_state.UsedThisRound) return true;
        }

        return false;
    }

    // 카드 색상에 따라 회복 또는 이번 라운드 공격력 보너스를 적용합니다.
    private void ApplyCardColorBonus(Card card, Monster monster, MonsterAbilityDefinition ability)
    {
        bool _isRed = card.Suit is CardSuit.Hearts or CardSuit.Diamonds;
        if (_isRed)
        {
            monster.Heal(GetIntValueOrDefault(ability, 10));
            return;
        }

        _roundAttackBonus += GetFloatValueOrDefault(ability, 0.1f);
        SyncAttackMultiplier(monster);
    }

    // 영구·이번 라운드 보너스를 합산해 몬스터의 최종 공격력 계수를 갱신합니다.
    private void SyncAttackMultiplier(Monster monster)
    {
        if (monster == null) return;
        monster.SetAttackMultiplier((_baseAttackMultiplier + _permanentAttackBonus + _roundAttackBonus) * _roundAttackMultiplier);
    }

    // 검은 J 강제 능력이 있으면 다음 딜러 드로우를 클럽 또는 스페이드 J로 예약합니다.
    private void QueueBlackJack(MatchManager match)
    {
        if (match == null) return;
        CardSuit _suit = _random.Next(2) == 0 ? CardSuit.Clubs : CardSuit.Spades;
        match.QueueForcedDealerDraw(_suit, CardRank.Jack);
    }

    // 딜러 패에 검은 J가 있으면 이번 라운드의 최종 공격력 계수에 배율을 적용합니다.
    private void UpdateBlackJackAttackMultiplier(BlackjackHand dealerHand, Monster monster)
    {
        foreach (MonsterAbilityRuntimeState _state in _abilityStates)
        {
            if (_state.Definition.AbilityType != MonsterAbilityType.ForceBlackJackAndBlackJackBonus) continue;

            foreach (Card _card in dealerHand.Cards)
            {
                if (_card.Rank != CardRank.Jack || _card.Suit is not (CardSuit.Clubs or CardSuit.Spades)) continue;
                _roundAttackMultiplier = GetFloatValueOrDefault(_state.Definition, 1.1f);
                SyncAttackMultiplier(monster);
                return;
            }
        }
    }

    // 선언 숫자는 2부터 10 사이에서 무작위로 선택합니다.
    private CardRank GetRandomNumberRank()
    {
        return (CardRank)_random.Next((int)CardRank.Two, (int)CardRank.Ten + 1);
    }

    // 0을 기본값 미지정으로 취급해 능력별 기본 수치를 제공합니다.
    private int GetIntValueOrDefault(MonsterAbilityDefinition ability, int defaultValue)
    {
        return ability.Value == 0 ? defaultValue : ability.Value;
    }

    // 0을 기본값 미지정으로 취급해 능력별 기본 수치를 제공합니다.
    private float GetFloatValueOrDefault(MonsterAbilityDefinition ability, float defaultValue)
    {
        return Math.Abs(ability.FloatValue) < 0.0001f ? defaultValue : ability.FloatValue;
    }

    // 능력 하나의 선언 숫자와 이번 라운드 사용 여부를 보관합니다.
    private sealed class MonsterAbilityRuntimeState
    {
        public MonsterAbilityDefinition Definition { get; }
        public CardRank DeclaredRank { get; set; }
        public bool UsedThisRound { get; set; }

        public MonsterAbilityRuntimeState(MonsterAbilityDefinition definition)
        {
            Definition = definition;
        }
    }
}
