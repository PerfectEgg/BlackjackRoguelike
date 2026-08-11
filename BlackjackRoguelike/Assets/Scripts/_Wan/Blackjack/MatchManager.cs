using System;

/// <summary>
/// 블랙잭 한 판의 카드 배분, 플레이어 선택, 딜러 턴, 승패 판정을 담당합니다.
/// 전투 피해 계산은 처리하지 않고 MatchEnded 이벤트로 결과만 전달합니다.
/// </summary>
public sealed class MatchManager
{
    private readonly Deck _deck;
    private readonly MatchRules _rules;
    private int _openingDrawIndex;

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
        _openingDrawIndex = 0;
        IsMatchActive = true;
        ChangeState(MatchState.OpeningDeal);
    }

    // 초기 카드 배분 또는 딜러 턴을 한 장씩 진행합니다.
    public void Advance()
    {
        if (!IsMatchActive) return;

        if (State == MatchState.OpeningDeal)
        {
            bool _drawForPlayer = _openingDrawIndex % 2 == 0;
            if (_drawForPlayer) DrawToPlayer();
            else DrawToDealer();

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
        if (PlayerHand.Score > _rules.TargetScore) EndMatch(MatchOutcome.DealerWin);
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
        if (PlayerHand.Score > _rules.TargetScore) EndMatch(MatchOutcome.DealerWin);
        else BeginDealerTurn();
    }

    // 딜러 턴을 시작하거나, 추가 드로우가 필요 없으면 즉시 결과를 확정합니다.
    private void BeginDealerTurn()
    {
        if (DealerHand.Score >= _rules.DealerStandScore) EndMatch(DetermineOutcome());
        else ChangeState(MatchState.DealerTurn);
    }

    // 플레이어에게 카드 한 장을 주고 현재 점수를 이벤트로 알립니다.
    private void DrawToPlayer()
    {
        Card _card = _deck.Draw();
        PlayerHand.Add(_card);
        CardDrawn?.Invoke(true, _card, PlayerHand.Score);
    }

    // 딜러에게 카드 한 장을 주고 현재 점수를 이벤트로 알립니다.
    private void DrawToDealer()
    {
        Card _card = _deck.Draw();
        DealerHand.Add(_card);
        CardDrawn?.Invoke(false, _card, DealerHand.Score);
    }

    // 버스트와 점수를 비교해 최종 승자를 계산합니다.
    private MatchOutcome DetermineOutcome()
    {
        bool _playerBust = PlayerHand.Score > _rules.TargetScore;
        bool _dealerBust = DealerHand.Score > _rules.TargetScore;
        if (_playerBust) return MatchOutcome.DealerWin;
        if (_dealerBust) return MatchOutcome.PlayerWin;
        if (PlayerHand.Score > DealerHand.Score) return MatchOutcome.PlayerWin;
        if (PlayerHand.Score < DealerHand.Score) return MatchOutcome.DealerWin;
        return MatchOutcome.Draw;
    }

    // 현재 상태를 갱신하고 상태 변경 이벤트를 호출합니다.
    private void ChangeState(MatchState nextState)
    {
        State = nextState;
        StateChanged?.Invoke(State);
    }

    // 매치를 닫고 전투 시스템이 사용할 결과를 발행합니다.
    private void EndMatch(MatchOutcome outcome)
    {
        IsMatchActive = false;
        ChangeState(MatchState.Finished);
        MatchEnded?.Invoke(new MatchResult(outcome, PlayerHand, DealerHand));
    }
}
