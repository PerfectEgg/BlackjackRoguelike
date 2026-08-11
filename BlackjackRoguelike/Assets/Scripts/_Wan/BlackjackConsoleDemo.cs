using UnityEngine;

/// <summary>Unity Console에서 블랙잭과 전투 피해를 빠르게 검증하는 임시 입력 컴포넌트입니다.</summary>
public sealed class BlackjackConsoleDemo : MonoBehaviour
{
    private GameManager _game;

    // 게임 시작과 함께 체력 100의 플레이어·몬스터 전투를 준비합니다.
    private void Start()
    {
        _game = new GameManager();
        _game.Match.CardDrawn += OnCardDrawn;
        _game.Match.StateChanged += OnStateChanged;
        _game.Match.MatchEnded += OnMatchEnded;
        _game.DamageApplied += OnDamageApplied;
        StartNewMatch();
    }

    // 키보드 입력을 블랙잭 선택과 다음 카드 공개 동작으로 연결합니다.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) _game.AdvanceMatch();
        if (Input.GetKeyDown(KeyCode.H)) _game.PlayerHit();
        if (Input.GetKeyDown(KeyCode.S)) _game.PlayerStand();
        if (Input.GetKeyDown(KeyCode.D)) _game.PlayerDoubleDown();
        if (Input.GetKeyDown(KeyCode.R)) StartNewMatch();
    }

    // 동일한 두 캐릭터의 체력을 유지한 채 다음 블랙잭 매치를 시작합니다.
    private void StartNewMatch()
    {
        _game.StartMatch();
        Debug.Log("[블랙잭] Space: 다음 카드 / H: 히트 / S: 스탠드 / D: 더블 다운 / R: 새 매치");
    }

    // 카드가 뽑힐 때 카드 이름과 해당 패의 현재 합계를 출력합니다.
    private void OnCardDrawn(bool isPlayer, Card card, int score)
    {
        string _owner = isPlayer ? "플레이어" : "몬스터";
        Debug.Log($"[블랙잭] {_owner}: {card}  | 현재 점수: {score}");
    }

    // 진행 단계에 따라 현재 필요한 입력을 Console에 안내합니다.
    private void OnStateChanged(MatchState state)
    {
        string _guide = state switch
        {
            MatchState.OpeningDeal => "시작 카드 공개: Space를 누르세요.",
            MatchState.PlayerTurn => "플레이어 턴: H / S / D 중 하나를 누르세요.",
            MatchState.DealerTurn => "몬스터 턴: Space를 눌러 다음 카드를 공개하세요.",
            _ => "매치 종료: R을 눌러 다음 매치를 시작하세요."
        };
        Debug.Log($"[블랙잭] {_guide}");
    }

    // 매치 종료 시 최종 패과 점수를 출력합니다.
    private void OnMatchEnded(MatchResult result)
    {
        Debug.Log($"[블랙잭] 결과: {result.Outcome} | 플레이어 {result.PlayerScore} ({_game.Match.PlayerHand.ToDisplayString()}) | 몬스터 {result.DealerScore} ({_game.Match.DealerHand.ToDisplayString()})");
    }

    // 전투 피해와 양측의 남은 체력을 출력합니다.
    private void OnDamageApplied(DamageResult result)
    {
        Debug.Log($"[전투] {result.Attacker.Name} → {result.Defender.Name}: {result.Damage} 피해 (적용 계수 {result.AppliedMultiplier:0.##}) | 플레이어 HP {_game.Player.CurrentHp} / 몬스터 HP {_game.Monster.CurrentHp}");
    }

    // 이벤트 구독을 해제합니다.
    private void OnDestroy()
    {
        if (_game == null) return;
        _game.Match.CardDrawn -= OnCardDrawn;
        _game.Match.StateChanged -= OnStateChanged;
        _game.Match.MatchEnded -= OnMatchEnded;
        _game.DamageApplied -= OnDamageApplied;
    }
}
