using System;

/// <summary>블랙잭 매치 결과를 플레이어·몬스터의 체력 피해로 연결하는 전투 조정자입니다.</summary>
public sealed class GameManager
{
    private bool _playerDoubleDownAttempted;
    private bool _isCurrentMonsterDefeated;
    private MonsterDatabase _monsterDatabase;
    private ItemDatabase _itemDatabase;
    private readonly ItemDropManager _itemDropManager = new();
    private readonly CardManipulationUsageState _cardManipulationUsageState = new();
    private readonly ItemEffectProcessor _itemEffectProcessor = new();
    private ItemEffectData _initialHandSwapEffect;
    private ItemEffectData _bustCardRemovalEffect;
    private bool _hasUsedInitialHandSwap;
    private bool _hasUsedBustCardRemoval;

    // 현재 전투에 참여하는 플레이어입니다.
    public Player Player { get; }
    // 현재 전투에 참여하는 몬스터입니다.
    public Monster Monster { get; private set; }
    // 카드 진행을 담당하는 블랙잭 매치입니다.
    public MatchManager Match { get; }
    // 현재 진행 중인 스테이지 번호입니다. 1부터 10까지 사용합니다.
    public int CurrentStage { get; private set; }
    // 플레이어가 보유한 골드입니다.
    public int Gold { get; private set; }
    // 몬스터 처치 보상에만 적용되는 골드 획득 배율입니다.
    public float MonsterGoldDropMultiplier { get; private set; } = 1f;
    // 현재 런에서 사용하는 10스테이지 몬스터 데이터베이스입니다.
    public MonsterDatabase StageDatabase => _monsterDatabase;
    // 현재 런에서 사용할 아이템 데이터베이스입니다.
    public ItemDatabase ItemDatabase => _itemDatabase;
    // 획득 확정된 아이템을 보관하는 플레이어 인벤토리입니다.
    public ItemInventory ItemInventory { get; } = new();
    // 처치 후 다음 스테이지로 진행할 수 있는지 나타냅니다.
    public bool CanProceedToNextStage { get; private set; }
    // 상점 종료를 기다리고 있는지 나타냅니다.
    public bool IsWaitingForShop { get; private set; }
    // 10스테이지까지 모두 완료했는지 나타냅니다.
    public bool IsRunCompleted { get; private set; }
    // 초기 패 전체 교환 패시브를 보유했는지 나타냅니다.
    public bool HasInitialHandSwapPassive => _initialHandSwapEffect != null;
    // 현재 라운드에서 초기 패 전체 교환을 실행할 수 있는지 나타냅니다.
    public bool CanSwapInitialHand => HasInitialHandSwapPassive && !_hasUsedInitialHandSwap && Match.CanSwapInitialPlayerHand;
    // 영구 패시브로 합산된 플레이어 점수 보정 범위입니다.
    public int PlayerScoreAdjustmentRange { get; private set; }

    // 매치 결과로 피해가 체력에 적용된 직후 발생합니다.
    public event Action<DamageResult> DamageApplied;
    // 골드가 바뀐 뒤 현재 골드를 전달합니다.
    public event Action<int> GoldChanged;
    // 몬스터 처치 시 처치한 몬스터를 전달합니다.
    public event Action<Monster> MonsterDefeated;
    // 5 또는 10스테이지 진입 전에 상점을 열어야 할 때 다음 스테이지 번호를 전달합니다.
    public event Action<int> ShopVisitRequested;
    // 새 스테이지 전투가 준비되었을 때 스테이지 번호와 몬스터를 전달합니다.
    public event Action<int, Monster> StageStarted;
    // 모든 스테이지를 완료했을 때 발생합니다.
    public event Action RunCompleted;
    // 스테이지 처치 보상 아이템이 생성되었을 때 발생합니다. UI에서 선택 화면을 열 때 사용합니다.
    public event Action<StageItemDropResult> StageItemDropsGenerated;
    // 플레이어 공격력 계수나 보호막 상태가 바뀐 뒤 발생합니다.
    public event Action PlayerStateChanged;
    // 아이템을 인벤토리에 추가한 뒤 획득한 정의를 전달합니다.
    public event Action<ItemDefinition> ItemAdded;

    // 기본 플레이어·몬스터와 새 매치 매니저로 전투를 생성합니다.
    public GameManager(Player player = null, Monster monster = null, MatchManager match = null)
    {
        Player = player ?? new Player();
        Monster = monster ?? new Monster();
        Match = match ?? new MatchManager();
        Match.MatchEnded += ApplyMatchDamage;
        Match.CardDrawn += RecalculateAttackMultiplierAfterDraw;
        Match.CardDrawn += HandleBustCardRemovalAfterDraw;
        Player.AttackMultiplierChanged += OnPlayerAttackMultiplierChanged;
        Player.MaxHpChanged += OnPlayerMaxHpChanged;
        Player.CurrentHpChanged += OnPlayerCurrentHpChanged;
        Player.BarrierChanged += OnPlayerBarrierChanged;
    }

    // 지정한 몬스터 데이터로 새 스테이지 전투를 준비합니다.
    public void StartStage(MonsterDefinition monsterDefinition)
    {
        if (monsterDefinition == null) return;

        int _previousStage = CurrentStage;
        if (_previousStage > 0 && monsterDefinition.StageNumber > _previousStage) RecoverOnNextCombatStage();

        Monster = new Monster(monsterDefinition);
        CurrentStage = monsterDefinition.StageNumber;
        _isCurrentMonsterDefeated = false;
        CanProceedToNextStage = false;
        IsWaitingForShop = false;
        _cardManipulationUsageState.ResetStage();
        StartMatch();
        StageStarted?.Invoke(CurrentStage, Monster);
    }

    // 데이터베이스에서 지정한 스테이지의 몬스터를 찾아 전투를 시작합니다.
    public bool StartStage(MonsterDatabase monsterDatabase, int stageNumber)
    {
        if (monsterDatabase == null) return false;
        MonsterDefinition _monsterDefinition = monsterDatabase.GetMonster(stageNumber);
        if (_monsterDefinition == null) return false;

        _monsterDatabase = monsterDatabase;
        StartStage(_monsterDefinition);
        return true;
    }

    // 데이터베이스를 등록하고 1스테이지부터 새 런을 시작합니다.
    public bool StartRun(MonsterDatabase monsterDatabase)
    {
        if (monsterDatabase == null || monsterDatabase.StageMonsters.Length != MonsterDatabase.StageCount) return false;

        _monsterDatabase = monsterDatabase;
        CurrentStage = 0;
        IsRunCompleted = false;
        IsWaitingForShop = false;
        CanProceedToNextStage = true;
        return ProceedToNextStage();
    }

    // 아이템 데이터베이스를 등록해 이후 몬스터 처치 때 스테이지 보상을 생성합니다.
    public void ConfigureItemDrops(ItemDatabase itemDatabase)
    {
        _itemDatabase = itemDatabase;
        _itemDatabase?.RebuildItemPools();
    }

    // 보상 UI에서 선택한 아이템을 실제 인벤토리에 추가합니다. 보유 패시브 중복은 자동으로 거절됩니다.
    public bool TryAddItemToInventory(ItemDefinition itemDefinition)
    {
        if (!ItemInventory.TryAdd(itemDefinition)) return false;
        _itemEffectProcessor.ApplyOnItemAcquired(Player, ItemInventory);
        MonsterGoldDropMultiplier = _itemEffectProcessor.CalculateMonsterGoldDropMultiplier(ItemInventory);
        PlayerScoreAdjustmentRange = _itemEffectProcessor.CalculatePlayerScoreAdjustmentRange(ItemInventory);
        Match.SetPlayerScoreAdjustmentRange(PlayerScoreAdjustmentRange);
        CachePassiveAbilities();
        ItemAdded?.Invoke(itemDefinition);
        return true;
    }

    // 액티브 아이템의 사용 즉시 직접 피해 효과를 몬스터에게 적용합니다.
    public bool TryUseActiveItem(ItemInstance itemInstance)
    {
        if (itemInstance == null || itemInstance.Definition == null) return false;

        int _damage = _itemEffectProcessor.CalculateActiveDirectDamage(Player, itemInstance);
        if (_damage <= 0 || !ItemInventory.TryUseActive(itemInstance)) return false;

        ApplyDirectDamageToMonster(_damage);
        return true;
    }

    // 처치 결과 확인 또는 상점 종료 후 다음 스테이지를 시작합니다.
    public bool ProceedToNextStage()
    {
        if (_monsterDatabase == null || !CanProceedToNextStage || IsWaitingForShop || IsRunCompleted) return false;

        int _nextStage = CurrentStage + 1;
        MonsterDefinition _nextMonster = _monsterDatabase.GetMonster(_nextStage);
        if (_nextMonster == null) return false;

        StartStage(_nextMonster);
        return true;
    }

    // 상점 UI가 닫힌 뒤 호출합니다. 다음 스테이지 진행을 다시 허용합니다.
    public bool CompleteShopVisit()
    {
        if (!IsWaitingForShop) return false;
        IsWaitingForShop = false;
        return true;
    }

    // 상점 UI가 다음 스테이지 전에 열려야 하는지 확인합니다.
    public bool IsShopBeforeStage(int stageNumber) => stageNumber == 5 || stageNumber == 10;

    // 패시브 효과로 몬스터 골드 드롭 배율을 증가시킵니다. 0.25를 전달하면 +25%입니다.
    public void AddMonsterGoldDropMultiplier(float amount)
    {
        if (amount <= 0f) return;
        MonsterGoldDropMultiplier += amount;
    }

    // 상점 구매 등에 사용할 골드를 차감합니다. 골드가 부족하면 false를 반환합니다.
    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || Gold < amount) return false;
        Gold -= amount;
        GoldChanged?.Invoke(Gold);
        return true;
    }

    // 양쪽 모두 살아 있을 때 새 블랙잭 매치를 초기화합니다.
    public void StartMatch()
    {
        if (Player.IsDefeated || Monster.IsDefeated) return;
        _playerDoubleDownAttempted = false;
        _hasUsedInitialHandSwap = false;
        _hasUsedBustCardRemoval = false;
        _cardManipulationUsageState.ResetRound();
        Match.StartMatch();
        _itemEffectProcessor.BeginRound(Player, ItemInventory, Match.PlayerHand);
        _itemEffectProcessor.HandleRoundStarted(Player, ItemInventory);
    }

    // 카드 교환·리롤 효과를 사용하기 전에 제한 그룹이 남아 있는지 확인하고 사용 처리합니다.
    public bool TryUseCardManipulation(ItemEffectData effect)
    {
        return _cardManipulationUsageState.TryUse(effect);
    }

    // 보유한 초기 패 교환 패시브를 사용해 플레이어의 시작 카드 두 장을 새로 뽑습니다.
    public bool TrySwapInitialHand()
    {
        if (!CanSwapInitialHand || !TryUseCardManipulation(_initialHandSwapEffect)) return false;
        if (!Match.TrySwapInitialPlayerHand()) return false;

        _hasUsedInitialHandSwap = true;
        return true;
    }

    // 전투 중인 양쪽이 살아 있을 때 현재 상대와 새 블랙잭 매치를 시작합니다.
    public bool RestartCurrentMatch()
    {
        if (Player.IsDefeated || Monster.IsDefeated) return false;
        StartMatch();
        return true;
    }

    // 다음 새 매치에서 플레이어 초기 패 블랙잭을 강제합니다.
    public void ForceNextPlayerOpeningBlackjack()
    {
        Match.ForceNextPlayerOpeningBlackjack();
    }

    // 플레이어의 다음 세 드로우 카드를 7로 고정합니다.
    public void ForceNextThreePlayerDrawsToSeven()
    {
        Match.ForceNextThreePlayerDrawsToSeven();
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
        _itemEffectProcessor.HandleMatchEnded(matchResult, ItemInventory);
        if (matchResult.Outcome == MatchOutcome.Draw)
        {
            int _drawDamage = _itemEffectProcessor.CalculateDrawDirectDamage(Player, ItemInventory, matchResult);
            ApplyDirectDamageToMonster(_drawDamage);
            return;
        }
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.OnMatchEnd, matchResult, _playerDoubleDownAttempted, Match.PlayerExtraDrawCount);
        DamageResult _damageResult = DamageCalculator.Calculate(matchResult, Player, Monster, _playerDoubleDownAttempted);
        bool _wasPlayerProtected = _damageResult.Defender == Player && Player.HasBarrier;
        int _actualDamageToMonster = _damageResult.Defender == Monster ? Math.Min(_damageResult.Damage, Monster.CurrentHp) : 0;
        _damageResult.Defender.TakeDamage(_damageResult.Damage);
        if (_damageResult.Defender == Player && !_wasPlayerProtected && !Player.IsDefeated)
        {
            _itemEffectProcessor.HandlePlayerDamageTaken(Player, ItemInventory);
        }
        else if (_damageResult.Defender == Monster)
        {
            ApplyLifeSteal(_actualDamageToMonster);
        }
        DamageApplied?.Invoke(_damageResult);

        if (_damageResult.Defender != Monster || !Monster.IsDefeated || _isCurrentMonsterDefeated) return;
        ClearCurrentMonster();
    }

    // 플레이어가 아이템 효과로 몬스터에게 직접 피해를 주고, 처치 시 스테이지 진행 상태를 갱신합니다.
    private void ApplyDirectDamageToMonster(int damage)
    {
        if (damage <= 0 || Monster.IsDefeated || _isCurrentMonsterDefeated) return;

        int _actualDamage = Math.Min(damage, Monster.CurrentHp);
        Monster.TakeDamage(_actualDamage);
        ApplyLifeSteal(_actualDamage);
        DamageApplied?.Invoke(new DamageResult(Player, Monster, damage, Player.AttackMultiplier));
        if (!Monster.IsDefeated) return;

        Match.CancelMatch();
        ClearCurrentMonster();
    }

    // 몬스터에게 실제로 가한 피해를 기준으로 보유 체력 흡수 패시브의 회복을 적용합니다.
    private void ApplyLifeSteal(int dealtDamage)
    {
        int _healAmount = _itemEffectProcessor.CalculateDamageDealtLifeSteal(Player, ItemInventory, dealtDamage);
        Player.Heal(_healAmount);
    }

    // 카드가 공개된 뒤 현재 패의 조건부 공격력 효과를 다시 계산합니다.
    private void RecalculateAttackMultiplierAfterDraw(bool isPlayer, Card card, int score)
    {
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.AfterDraw, null, false, Match.PlayerExtraDrawCount);
    }

    // 드로우 후 버스트가 발생하면 보유 패시브로 마지막 카드를 한 번 삭제합니다.
    private void HandleBustCardRemovalAfterDraw(bool isPlayer, Card card, int score)
    {
        if (!isPlayer || _bustCardRemovalEffect == null || _hasUsedBustCardRemoval) return;
        if (Match.PlayerScore <= Match.TargetScore) return;
        int _bustScore = Match.PlayerScore;
        if (!Match.TryRemoveLastPlayerCard(out Card _removedCard)) return;

        _hasUsedBustCardRemoval = true;
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.AfterDraw, null, false, Match.PlayerExtraDrawCount);
        UnityEngine.Debug.Log($"[버스트 카드 제거] 버스트 {_bustScore}/{Match.TargetScore} 감지 → {_removedCard} 삭제 → 판정 점수 {Match.PlayerScore}/{Match.TargetScore}");
        if (Match.PlayerScore > Match.TargetScore)
        {
            UnityEngine.Debug.LogWarning($"[버스트 카드 제거] 카드 삭제 후에도 버스트 상태입니다: {Match.PlayerScore}/{Match.TargetScore}");
        }
    }

    // 인벤토리 변경 시에만 패시브 능력의 사용 가능 여부를 캐시합니다.
    private void CachePassiveAbilities()
    {
        _initialHandSwapEffect = _itemEffectProcessor.FindInitialHandSwapEffect(ItemInventory);
        _bustCardRemovalEffect = _itemEffectProcessor.FindBustCardRemovalEffect(ItemInventory);
    }

    // 플레이어 공격력 계수가 변경됐음을 UI 등 외부 시스템에 알립니다.
    private void OnPlayerAttackMultiplierChanged(Character character)
    {
        PlayerStateChanged?.Invoke();
    }

    // 플레이어 최대 체력이 변경됐음을 UI 등 외부 시스템에 알립니다.
    private void OnPlayerMaxHpChanged(Character character)
    {
        PlayerStateChanged?.Invoke();
    }

    // 플레이어 현재 체력이 변경됐음을 UI 등 외부 시스템에 알립니다.
    private void OnPlayerCurrentHpChanged(Character character)
    {
        PlayerStateChanged?.Invoke();
    }

    // 플레이어 보호막 상태가 변경됐음을 UI 등 외부 시스템에 알립니다.
    private void OnPlayerBarrierChanged(bool hasBarrier)
    {
        PlayerStateChanged?.Invoke();
    }

    // 몬스터 처치 보상을 지급하고 필요하면 다음 스테이지 전 상점 방문을 요청합니다.
    private void ClearCurrentMonster()
    {
        _isCurrentMonsterDefeated = true;
        LevelUpAfterMonsterDefeat();
        AwardMonsterGold(Monster.DropGold);
        MonsterDefeated?.Invoke(Monster);
        GenerateStageItemDrops();

        int _nextStage = CurrentStage + 1;
        if (_monsterDatabase == null) return;

        if (_nextStage > MonsterDatabase.StageCount)
        {
            IsRunCompleted = true;
            RunCompleted?.Invoke();
            return;
        }

        CanProceedToNextStage = true;
        if (!IsShopBeforeStage(_nextStage)) return;

        IsWaitingForShop = true;
        ShopVisitRequested?.Invoke(_nextStage);
    }

    // 몬스터 처치 직후 레벨업 보상과 영구 아이템 보정을 다시 적용합니다.
    private void LevelUpAfterMonsterDefeat()
    {
        Player.LevelUp();
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.None);
        _itemEffectProcessor.RecalculatePermanentMaxHp(Player, ItemInventory);
    }

    // 다음 전투 스테이지에 진입할 때 현재 최대 체력의 25%만큼 회복합니다.
    private void RecoverOnNextCombatStage()
    {
        int _recoveryAmount = (int)MathF.Floor(Player.MaxHp * 0.25f);
        Player.Heal(_recoveryAmount);
    }

    // 몬스터 처치 골드에 드롭 배율을 적용한 뒤 지급합니다.
    private void AwardMonsterGold(int baseGold)
    {
        int _adjustedGold = (int)MathF.Floor(baseGold * MonsterGoldDropMultiplier);
        GainGold(_adjustedGold);
    }

    // 아이템 데이터베이스가 등록되어 있을 때 패시브 후보 2개와 액티브 후보 3개를 생성합니다.
    private void GenerateStageItemDrops()
    {
        if (_itemDatabase == null) return;
        StageItemDropResult _itemDrops = _itemDropManager.GenerateStageDrops(_itemDatabase, ItemInventory.GetOwnedPassiveDefinitions());
        StageItemDropsGenerated?.Invoke(_itemDrops);
    }

    // 액티브 아이템, 상점 환불 등에 사용할 골드를 즉시 지급합니다. 드롭 배율은 적용하지 않습니다.
    public void GainGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        GoldChanged?.Invoke(Gold);
    }
}
