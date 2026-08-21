using System;
using System.Collections.Generic;

/// <summary>
/// 블랙잭 한 판의 카드 배분, 플레이어 선택, 딜러 턴, 승패 판정을 담당합니다.
/// 전투 피해 계산은 처리하지 않고 MatchEnded 이벤트로 결과만 전달합니다.
/// </summary>
public sealed class MatchManager
{
    private readonly Deck _deck;
    private readonly MatchRules _rules;
    private readonly Queue<Card> _forcedOpeningPlayerCards = new();
    private readonly Queue<CardRank> _forcedPlayerDrawRanks = new();
    private readonly Queue<ItemEffectData> _activePlayerDrawModifiers = new();
    private int _openingDrawIndex;
    private int _playerScoreAdjustmentRange;
    private bool _forceNextPlayerOpeningBlackjack;

    // 플레이어의 현재 패입니다.
    public BlackjackHand PlayerHand { get; }
    // 몬스터(딜러)의 현재 패입니다.
    public BlackjackHand DealerHand { get; }
    // 매치가 아직 끝나지 않았는지 나타냅니다.
    public bool IsMatchActive { get; private set; }
    // 현재 카드 진행 단계입니다.
    public MatchState State { get; private set; }
    // 플레이어가 지금 더블 다운을 선택할 수 있는지 나타냅니다.
    public bool CanDoubleDown => IsMatchActive && State == MatchState.PlayerTurn && PlayerHand.Cards.Count == _rules.StartingHandSize;
    // 초기 패 전체 교환을 실행할 수 있는지 나타냅니다.
    public bool CanSwapInitialPlayerHand => IsMatchActive && State == MatchState.PlayerTurn && PlayerHand.Cards.Count == _rules.StartingHandSize;
    // 아이템 보정이 반영된 플레이어 판정 점수입니다.
    public int PlayerScore => GetAdjustedPlayerScore();
    // 몬스터의 판정 점수입니다.
    public int DealerScore => DealerHand.Score;
    // 현재 매치의 목표 점수입니다.
    public int TargetScore => _rules.TargetScore;
    // 초기 패를 제외하고 플레이어가 추가로 뽑은 카드 수입니다.
    public int PlayerExtraDrawCount => Math.Max(0, PlayerHand.Cards.Count - _rules.StartingHandSize);

    // 카드 한 장이 공개될 때 카드와 해당 패의 합계를 전달합니다.
    public event Action<bool, Card, int> CardDrawn;
    // 카드 진행 단계가 바뀔 때 발생합니다.
    public event Action<MatchState> StateChanged;
    // 매치가 끝나 승패가 확정될 때 발생합니다.
    public event Action<MatchResult> MatchEnded;

    // 기본 규칙과 새 덱으로 매치 매니저를 생성합니다.
    public MatchManager(MatchRules rules = null, Deck deck = null)
    {
        _rules = rules ?? new MatchRules();
        _deck = deck ?? new Deck();
        PlayerHand = new BlackjackHand(_rules.TargetScore);
        DealerHand = new BlackjackHand(_rules.TargetScore);
    }

    // 패를 비우고 초기 카드 배분 단계로 새 매치를 시작합니다.
    public void StartMatch()
    {
        PlayerHand.Clear();
        DealerHand.Clear();
        PrepareForcedOpeningCards();
        _openingDrawIndex = 0;
        IsMatchActive = true;
        ChangeState(MatchState.OpeningDeal);
    }

    // 영구 패시브가 제공하는 플레이어 점수 보정 범위를 설정합니다.
    public void SetPlayerScoreAdjustmentRange(int adjustmentRange)
    {
        _playerScoreAdjustmentRange = Math.Max(0, adjustmentRange);
    }

    // 다음 새 매치의 플레이어 초기 패를 스페이드 A와 스페이드 J 블랙잭으로 고정합니다.
    public void ForceNextPlayerOpeningBlackjack()
    {
        _forceNextPlayerOpeningBlackjack = true;
    }

    // 플레이어의 다음 세 드로우 카드를 모두 7로 고정합니다.
    public void ForceNextThreePlayerDrawsToSeven()
    {
        _forcedPlayerDrawRanks.Clear();
        for (int _index = 0; _index < 3; _index++) _forcedPlayerDrawRanks.Enqueue(CardRank.Seven);
    }

    // 액티브가 예약한 다음 플레이어 드로우 보정을 등록합니다.
    public bool TryQueueActivePlayerDrawModifier(ItemEffectData effect)
    {
        if (!CanQueueActivePlayerDrawModifier(effect)) return false;

        if (effect.DrawMode == DrawModifierMode.GuaranteeBlackjack)
        {
            ForceNextPlayerOpeningBlackjack();
            return true;
        }

        _activePlayerDrawModifiers.Enqueue(effect);
        return true;
    }

    // 현재 구현된 예약 드로우 보정인지 확인합니다.
    public bool CanQueueActivePlayerDrawModifier(ItemEffectData effect)
    {
        if (effect == null || effect.DrawTarget != DrawModifierTarget.Player) return false;
        if (effect.DrawScope is not (DrawModifierScope.NextDrawThisStage or DrawModifierScope.NextRoundOpeningHand)) return false;

        return effect.DrawMode switch
        {
            DrawModifierMode.SetSpecificCard => effect.DrawCardRank != CardRank.None,
            DrawModifierMode.SetFaceCard => true,
            DrawModifierMode.DuplicateLastCard => true,
            DrawModifierMode.GuaranteeBlackjack => true,
            _ => false
        };
    }

    // 현재 스테이지에서만 유효한 액티브 드로우 예약을 비웁니다.
    public void ClearActivePlayerDrawModifiers()
    {
        _activePlayerDrawModifiers.Clear();
    }

    // 초기 카드 배분 또는 딜러 턴을 한 장씩 진행합니다.
    public void Advance()
    {
        if (!IsMatchActive) return;

        if (State == MatchState.OpeningDeal)
        {
            bool _drawForPlayer = _openingDrawIndex % 2 == 0;
            if (_drawForPlayer) DrawToPlayer(true);
            else DrawToDealer(true);

            _openingDrawIndex++;
            if (_openingDrawIndex >= _rules.StartingHandSize * 2)
            {
                if (PlayerHand.IsBlackjack || DealerHand.IsBlackjack) EndMatch(DetermineOutcome());
                else ChangeState(MatchState.PlayerTurn);
            }
            return;
        }

        if (State == MatchState.DealerTurn)
        {
            if (DealerHand.Score < _rules.DealerStandScore) DrawToDealer();
            if (DealerHand.Score >= _rules.DealerStandScore || DealerHand.Score > _rules.TargetScore) EndMatch(DetermineOutcome());
        }
    }

    // 플레이어가 카드 한 장을 더 뽑습니다.
    public void PlayerHit()
    {
        if (!IsMatchActive || State != MatchState.PlayerTurn) return;
        DrawToPlayer();
        if (PlayerScore > _rules.TargetScore) EndMatch(MatchOutcome.DealerWin);
    }

    // 플레이어의 카드 뽑기를 끝내고 딜러 턴으로 넘깁니다.
    public void PlayerStand()
    {
        if (!IsMatchActive || State != MatchState.PlayerTurn) return;
        BeginDealerTurn();
    }

    // 초기 두 장에서 한 장만 더 뽑고, 자동으로 스탠드하는 더블 다운을 실행합니다.
    public void PlayerDoubleDown()
    {
        if (!CanDoubleDown) return;
        DrawToPlayer();
        if (PlayerScore > _rules.TargetScore) EndMatch(MatchOutcome.DealerWin);
        else BeginDealerTurn();
    }

    // 기존 초기 패을 덱에 넣지 않은 상태에서 새 두 장을 뽑고, 기존 패을 덱으로 되돌립니다.
    public bool TrySwapInitialPlayerHand()
    {
        if (!CanSwapInitialPlayerHand) return false;

        Card[] _previousCards = new Card[PlayerHand.Cards.Count];
        for (int _index = 0; _index < PlayerHand.Cards.Count; _index++) _previousCards[_index] = PlayerHand.Cards[_index];

        PlayerHand.Clear();
        for (int _index = 0; _index < _rules.StartingHandSize; _index++) DrawToPlayer(true);
        _deck.ReturnCards(_previousCards);

        if (PlayerHand.IsBlackjack) EndMatch(MatchOutcome.PlayerWin);
        return true;
    }

    // 플레이어 패의 마지막 카드를 삭제합니다. 버스트 구제 아이템에서 사용합니다.
    public bool TryRemoveLastPlayerCard(out Card removedCard)
    {
        if (!IsMatchActive)
        {
            removedCard = default;
            return false;
        }

        return PlayerHand.TryRemoveLastCard(out removedCard);
    }

    // 아이템 직접 피해 등으로 몬스터가 처치됐을 때 결과 피해 없이 현재 매치를 종료합니다.
    public void CancelMatch()
    {
        if (!IsMatchActive) return;
        IsMatchActive = false;
        ChangeState(MatchState.Finished);
    }

    // 딜러 턴을 시작하거나, 추가 드로우가 필요 없으면 즉시 결과를 확정합니다.
    private void BeginDealerTurn()
    {
        if (DealerHand.Score >= _rules.DealerStandScore) EndMatch(DetermineOutcome());
        else ChangeState(MatchState.DealerTurn);
    }

    // 플레이어에게 카드 한 장을 주고 현재 점수를 이벤트로 알립니다.
    private void DrawToPlayer(bool isOpeningCard = false)
    {
        Card _card = DrawForcedOrRandomPlayerCard(isOpeningCard);
        PlayerHand.Add(_card, isOpeningCard);
        CardDrawn?.Invoke(true, _card, PlayerHand.Score);
    }

    // 딜러에게 카드 한 장을 주고 현재 점수를 이벤트로 알립니다.
    private void DrawToDealer(bool isOpeningCard = false)
    {
        Card _card = _deck.Draw();
        DealerHand.Add(_card, isOpeningCard);
        CardDrawn?.Invoke(false, _card, DealerHand.Score);
    }

    // 버스트와 점수를 비교해 최종 승자를 계산합니다.
    private MatchOutcome DetermineOutcome()
    {
        bool _playerBust = PlayerScore > _rules.TargetScore;
        bool _dealerBust = DealerHand.Score > _rules.TargetScore;
        if (_playerBust) return MatchOutcome.DealerWin;
        if (_dealerBust) return MatchOutcome.PlayerWin;
        if (PlayerScore > DealerScore) return MatchOutcome.PlayerWin;
        if (PlayerScore < DealerScore) return MatchOutcome.DealerWin;
        return MatchOutcome.Draw;
    }

    // 현재 목표 점수를 향해 보정 범위만큼 플레이어 점수를 올리거나 내립니다.
    private int GetAdjustedPlayerScore()
    {
        int _rawScore = PlayerHand.Score;
        int _differenceToTarget = _rules.TargetScore - _rawScore;
        if (_differenceToTarget > 0) return _rawScore + Math.Min(_playerScoreAdjustmentRange, _differenceToTarget);
        if (_differenceToTarget < 0) return _rawScore - Math.Min(_playerScoreAdjustmentRange, -_differenceToTarget);
        return _rawScore;
    }

    // 현재 상태를 갱신하고 상태 변경 이벤트를 호출합니다.
    private void ChangeState(MatchState nextState)
    {
        State = nextState;
        StateChanged?.Invoke(State);
    }

    // 예약된 블랙잭 효과가 있으면 다음 초기 패에 스페이드 A와 스페이드 J를 준비합니다.
    private void PrepareForcedOpeningCards()
    {
        _forcedOpeningPlayerCards.Clear();
        if (!_forceNextPlayerOpeningBlackjack) return;

        _forcedOpeningPlayerCards.Enqueue(new Card(CardSuit.Spades, CardRank.Ace));
        _forcedOpeningPlayerCards.Enqueue(new Card(CardSuit.Spades, CardRank.Jack));
        _forceNextPlayerOpeningBlackjack = false;
    }

    // 테스트 예약 카드가 있으면 우선 사용하고, 없으면 일반 덱에서 카드를 뽑습니다.
    private Card DrawForcedOrRandomPlayerCard(bool isOpeningCard)
    {
        if (isOpeningCard && _forcedOpeningPlayerCards.Count > 0)
        {
            Card _forcedCard = _forcedOpeningPlayerCards.Dequeue();
            return _deck.DrawSpecificCard(_forcedCard.Suit, _forcedCard.Rank);
        }
        if (_activePlayerDrawModifiers.Count > 0) return DrawActiveModifierCard(_activePlayerDrawModifiers.Dequeue());
        if (_forcedPlayerDrawRanks.Count > 0) return _deck.DrawByRank(_forcedPlayerDrawRanks.Dequeue());
        return _deck.Draw();
    }

    // 예약된 액티브 드로우 보정에 맞는 카드 한 장을 덱에서 뽑습니다.
    private Card DrawActiveModifierCard(ItemEffectData effect)
    {
        return effect.DrawMode switch
        {
            DrawModifierMode.SetSpecificCard when effect.DrawCardSuit != CardSuit.None => _deck.DrawSpecificCard(effect.DrawCardSuit, effect.DrawCardRank),
            DrawModifierMode.SetSpecificCard => _deck.DrawByRank(effect.DrawCardRank),
            DrawModifierMode.SetFaceCard => _deck.DrawFaceCard(),
            DrawModifierMode.DuplicateLastCard when PlayerHand.Cards.Count > 0 => DrawSameAsLastPlayerCard(),
            _ => _deck.Draw()
        };
    }

    // 마지막으로 받은 플레이어 카드와 숫자·무늬가 동일한 카드를 덱에서 뽑습니다.
    private Card DrawSameAsLastPlayerCard()
    {
        Card _lastCard = PlayerHand.Cards[PlayerHand.Cards.Count - 1];
        return _deck.DrawSpecificCard(_lastCard.Suit, _lastCard.Rank);
    }

    // 매치를 닫고 전투 시스템이 사용할 결과를 발행합니다.
    private void EndMatch(MatchOutcome outcome)
    {
        IsMatchActive = false;
        ChangeState(MatchState.Finished);
        MatchEnded?.Invoke(new MatchResult(outcome, PlayerScore, DealerScore, PlayerHand.IsBlackjack, DealerHand.IsBlackjack));
    }
}
