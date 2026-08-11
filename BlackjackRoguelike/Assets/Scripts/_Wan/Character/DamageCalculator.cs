/// <summary>블랙잭 점수와 조건부 보너스를 실제 전투 피해로 변환합니다.</summary>
public static class DamageCalculator
{
    // 승리 점수에 공격력 계수와 블랙잭·더블 다운 보너스를 합산해 피해를 계산합니다.
    public static DamageResult Calculate(MatchResult matchResult, Player player, Monster monster, bool playerDoubleDownAttempted)
    {
        if (matchResult.Outcome == MatchOutcome.Draw) return default;

        bool _playerWon = matchResult.Outcome == MatchOutcome.PlayerWin;
        Character _attacker = _playerWon ? player : monster;
        Character _defender = _playerWon ? monster : player;
        int _winningScore = _playerWon ? matchResult.PlayerScore : matchResult.DealerScore;
        bool _winningBlackjack = _playerWon ? matchResult.PlayerBlackjack : matchResult.DealerBlackjack;

        float _appliedMultiplier = _attacker.AttackMultiplier;
        if (_winningBlackjack) _appliedMultiplier += 0.5f;
        if (playerDoubleDownAttempted) _appliedMultiplier += 0.5f;

        // 계수 적용 결과의 소수점은 버리고 정수 피해로 변환합니다.
        int _damage = (int)System.MathF.Floor(_winningScore * _appliedMultiplier);
        return new DamageResult(_attacker, _defender, _damage, _appliedMultiplier);
    }
}
