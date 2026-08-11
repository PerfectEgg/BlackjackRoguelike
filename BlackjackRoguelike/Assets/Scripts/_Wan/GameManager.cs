using System;

/// <summary>블랙잭 매치 결과를 플레이어·몬스터의 체력 피해로 연결하는 전투 조정자입니다.</summary>
public sealed class GameManager
{
    private bool _playerDoubleDownAttempted;

    // 현재 전투에 참여하는 플레이어입니다.
    public Player Player { get; }
    // 현재 전투에 참여하는 몬스터입니다.
    public Monster Monster { get; }
    // 카드 진행을 담당하는 블랙잭 매치입니다.
    public MatchManager Match { get; }

    // 매치 결과로 피해가 체력에 적용된 직후 발생합니다.
    public event Action<DamageResult> DamageApplied;

    // 기본 플레이어·몬스터와 새 매치 매니저로 전투를 생성합니다.
    public GameManager(Player player = null, Monster monster = null, MatchManager match = null)
    {
        Player = player ?? new Player();
        Monster = monster ?? new Monster();
        Match = match ?? new MatchManager();
        Match.MatchEnded += ApplyMatchDamage;
    }

    // 양쪽 모두 살아 있을 때 새 블랙잭 매치를 초기화합니다.
    public void StartMatch()
    {
        if (Player.IsDefeated || Monster.IsDefeated) return;
        _playerDoubleDownAttempted = false;
        Match.StartMatch();
    }

    // 초기 배분 또는 딜러의 카드 공개를 한 단계 진행합니다.
    public void AdvanceMatch() => Match.Advance();
    // 플레이어 히트 입력을 매치에 전달합니다.
    public void PlayerHit() => Match.PlayerHit();
    // 플레이어 스탠드 입력을 매치에 전달합니다.
    public void PlayerStand() => Match.PlayerStand();
    
    // 플레이어 더블 다운 입력을 매치에 전달합니다.
    public void PlayerDoubleDown()
    {
        if (!Match.CanDoubleDown) return;
        _playerDoubleDownAttempted = true;
        Match.PlayerDoubleDown();
    }

    // 무승부를 제외한 매치 결과를 피해로 계산해 패배 캐릭터에게 적용합니다.
    private void ApplyMatchDamage(MatchResult matchResult)
    {
        if (matchResult.Outcome == MatchOutcome.Draw) return;
        DamageResult _damageResult = DamageCalculator.Calculate(matchResult, Player, Monster, _playerDoubleDownAttempted);
        _damageResult.Defender.TakeDamage(_damageResult.Damage);
        DamageApplied?.Invoke(_damageResult);
    }
}
