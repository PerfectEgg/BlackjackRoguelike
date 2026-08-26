using System;
using System.Collections.Generic;

/// <summary>블랙잭 매치 결과를 플레이어·몬스터의 체력 피해로 연결하는 전투 조정자입니다.</summary>
public sealed class GameManager
{
    private const int FirstBossStage = 9;
    private bool _playerDoubleDownAttempted;
    private bool _isCurrentMonsterDefeated;
    private MonsterDatabase _monsterDatabase;
    private ItemDatabase _itemDatabase;
    private readonly ItemDropManager _itemDropManager = new();
    private readonly ShopManager _shopManager = new();
    private readonly HashSet<int> _shopVisitStages = new() { 5, 10 };
    private readonly CardManipulationUsageState _cardManipulationUsageState = new();
    private readonly ItemEffectProcessor _itemEffectProcessor = new();
    private readonly MonsterAbilityProcessor _monsterAbilityProcessor = new();
    private ItemEffectData _initialHandSwapEffect;
    private ItemEffectData _bustCardRemovalEffect;
    private bool _hasUsedInitialHandSwap;
    private bool _hasUsedBustCardRemoval;
    private int _currentRoundHpBetAmount;
    private bool _hasStageBustIgnore;
    private bool _hasStageBustScoreDamage;
    private readonly System.Collections.Generic.List<ItemEffectData> _currentRoundHpBetVictoryEffects = new();

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
    public int LastMonsterGoldReward { get; private set; }
    // 몬스터 처치 보상에만 적용되는 골드 획득 배율입니다.
    public float MonsterGoldDropMultiplier { get; private set; } = 1f;
    // 현재 런에서 사용하는 10스테이지 몬스터 데이터베이스입니다.
    public MonsterDatabase StageDatabase => _monsterDatabase;
    // 현재 런에서 사용할 아이템 데이터베이스입니다.
    public ItemDatabase ItemDatabase => _itemDatabase;
    // 현재 스테이지 몬스터 능력의 런타임 상태입니다.
    public MonsterAbilityProcessor MonsterAbilities => _monsterAbilityProcessor;
    // 획득 확정된 아이템을 보관하는 플레이어 인벤토리입니다.
    public ItemInventory ItemInventory { get; } = new();
    // 현재 상점의 진열과 새로 고침 정보를 관리합니다.
    public ShopManager Shop => _shopManager;
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
    // 액티브 아이템을 사용할 수 있는 플레이어 행동 턴인지 나타냅니다.
    public bool CanUseActiveItems => Match.IsMatchActive && Match.State == MatchState.PlayerTurn;
    // 영구 패시브로 합산된 플레이어 점수 보정 범위입니다.
    public int PlayerScoreAdjustmentRange { get; private set; }
    // 상점 상품 하나를 무료로 구매할 수 있는 액티브 이용권 보유 여부입니다.
    public bool HasFreeShopPurchaseTicket => _itemEffectProcessor.TryGetFreeShopPurchaseTicket(ItemInventory, out _);
    // 이번 라운드에 액티브로 약속한 최대 체력 베팅 수치입니다.
    public int CurrentRoundHpBetAmount => _currentRoundHpBetAmount;

    // 매치 결과로 피해가 체력에 적용된 직후 발생합니다.
    public event Action<DamageResult> DamageApplied;
    // 플레이어 보호막이 피해를 막은 직후 발생합니다.
    public event Action PlayerBarrierBlocked;
    // 골드가 바뀐 뒤 현재 골드를 전달합니다.
    public event Action<int> GoldChanged;
    // 몬스터 처치 시 처치한 몬스터를 전달합니다.
    public event Action<Monster> MonsterDefeated;
    // 지정된 상점 스테이지 진입 전에 상점을 열어야 할 때 다음 스테이지 번호를 전달합니다.
    public event Action<int> ShopVisitRequested;
    // 상점 진열이 처음 생성되거나 새로 고침된 뒤 발생합니다.
    public event Action<ShopManager> ShopStockChanged;
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

        _itemEffectProcessor.ClearActiveStageBlackjackAttackEffects();
        int _previousStage = CurrentStage;
        if (_previousStage > 0 && monsterDefinition.StageNumber > _previousStage) RecoverOnNextCombatStage();

        Monster = new Monster(monsterDefinition);
        _monsterAbilityProcessor.BeginStage(Monster);
        CurrentStage = monsterDefinition.StageNumber;
        _isCurrentMonsterDefeated = false;
        _hasStageBustIgnore = false;
        _hasStageBustScoreDamage = false;
        CanProceedToNextStage = false;
        IsWaitingForShop = false;
        Match.ClearActivePlayerDrawModifiers();
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

    // 상점을 열 스테이지 번호 목록을 설정합니다. 빈 목록을 전달하면 런 동안 상점을 열지 않습니다.
    public void ConfigureShopVisitStages(IEnumerable<int> stages)
    {
        _shopVisitStages.Clear();
        if (stages == null) return;

        foreach (int _stageNumber in stages)
        {
            if (_stageNumber is >= 1 and <= MonsterDatabase.StageCount) _shopVisitStages.Add(_stageNumber);
        }
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
        _itemEffectProcessor.ApplyOnItemAcquired(Player, ItemInventory, Match.PlayerHand, Gold);
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
        if (itemInstance == null || itemInstance.Definition == null || !CanUseActiveItems) return false;

        bool _hasThisRoundAttackEffect = _itemEffectProcessor.HasThisRoundActiveAttackEffect(itemInstance);
        bool _hasNextRoundAttackEffect = _itemEffectProcessor.HasNextRoundActiveAttackEffect(itemInstance);
        bool _hasActiveStageBlackjackAttackEffect = _itemEffectProcessor.HasActiveStageBlackjackAttackEffect(itemInstance);
        bool _hasActiveDrawModifierEffect = _itemEffectProcessor.HasActiveDrawModifierEffect(itemInstance, Match);
        bool _hasActiveBarrierEffect = _itemEffectProcessor.HasActiveBarrierEffect(itemInstance);
        bool _hasActiveHealEffect = _itemEffectProcessor.HasActiveHealEffect(itemInstance);
        bool _hasActiveGoldEffect = _itemEffectProcessor.HasActiveGoldEffect(itemInstance);
        bool _hasThisRoundActiveLifeStealEffect = _itemEffectProcessor.HasThisRoundActiveLifeStealEffect(itemInstance);
        bool _hasThisRoundActiveMatchEndHealEffect = Match.IsMatchActive && _itemEffectProcessor.HasThisRoundActiveMatchEndHealEffect(itemInstance);
        bool _hasActiveMaxHpBetEffect = Match.IsMatchActive && _currentRoundHpBetAmount == 0 && _itemEffectProcessor.HasActiveMaxHpBetEffect(itemInstance);
        bool _hasActiveNextRoundDealerForcedDrawEffect = _itemEffectProcessor.HasActiveNextRoundDealerForcedDrawEffect(itemInstance, Match);
        bool _hasActiveForceCurrentRoundDrawEffect = Match.IsMatchActive && _itemEffectProcessor.HasActiveForceCurrentRoundDrawEffect(itemInstance);
        bool _hasActiveSwapHandsEffect = _itemEffectProcessor.HasActiveSwapHandsEffect(itemInstance, Match);
        bool _hasActiveRerollRandomPlayerCardEffect = _itemEffectProcessor.HasActiveRerollRandomPlayerCardEffect(itemInstance, Match);
        bool _hasActiveNextRoundPlayerForcedDrawEffect = _itemEffectProcessor.HasActiveNextRoundPlayerForcedDrawEffect(itemInstance);
        bool _hasActiveIgnoreStageBustEffect = !_hasStageBustIgnore && !_hasStageBustScoreDamage && _itemEffectProcessor.HasActiveIgnoreStageBustEffect(itemInstance);
        bool _hasActiveStageBustScoreDamageEffect = !_hasStageBustIgnore && !_hasStageBustScoreDamage && _itemEffectProcessor.HasActiveStageBustScoreDamageEffect(itemInstance);
        bool _hasActiveSkipStageWithoutRewardEffect = CanSkipCurrentStageWithoutReward && _itemEffectProcessor.HasActiveSkipStageWithoutRewardEffect(itemInstance);
        int _damage = _itemEffectProcessor.CalculateActiveDirectDamage(Player, Monster, itemInstance);
        if (!_hasThisRoundAttackEffect && !_hasNextRoundAttackEffect && !_hasActiveStageBlackjackAttackEffect && !_hasActiveDrawModifierEffect && !_hasActiveBarrierEffect && !_hasActiveHealEffect && !_hasActiveGoldEffect && !_hasThisRoundActiveLifeStealEffect && !_hasThisRoundActiveMatchEndHealEffect && !_hasActiveMaxHpBetEffect && !_hasActiveNextRoundDealerForcedDrawEffect && !_hasActiveForceCurrentRoundDrawEffect && !_hasActiveSwapHandsEffect && !_hasActiveRerollRandomPlayerCardEffect && !_hasActiveNextRoundPlayerForcedDrawEffect && !_hasActiveIgnoreStageBustEffect && !_hasActiveStageBustScoreDamageEffect && !_hasActiveSkipStageWithoutRewardEffect && _damage <= 0) return false;
        if (!ItemInventory.TryUseActive(itemInstance)) return false;

        if (_hasThisRoundAttackEffect)
        {
            _itemEffectProcessor.ApplyThisRoundActiveAttackEffects(itemInstance, Match.PlayerHand, Gold);
            _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.None, null, false, Match.PlayerExtraDrawCount, Gold);
            _itemEffectProcessor.RecalculateMonsterAttackMultiplier(Monster);
            _monsterAbilityProcessor.ResyncAttackMultiplier(Monster);
            _damage = _itemEffectProcessor.CalculateActiveDirectDamage(Player, Monster, itemInstance);
        }
        if (_hasNextRoundAttackEffect) _itemEffectProcessor.ApplyNextRoundActiveAttackEffects(itemInstance);
        if (_hasActiveStageBlackjackAttackEffect) _itemEffectProcessor.ApplyActiveStageBlackjackAttackEffects(itemInstance);
        if (_hasActiveDrawModifierEffect) _itemEffectProcessor.ApplyActiveDrawModifierEffects(itemInstance, Match);
        if (_hasActiveBarrierEffect) _itemEffectProcessor.ApplyActiveBarrierEffects(Player, itemInstance);
        if (_hasActiveHealEffect)
        {
            int _healthChange = _itemEffectProcessor.CalculateActiveHealAmount(Player, itemInstance);
            if (_healthChange > 0) Player.Heal(_healthChange);
            else if (_healthChange < 0) Player.LoseHp(-_healthChange);
        }
        if (_hasActiveGoldEffect) GainGold(_itemEffectProcessor.CalculateActiveGoldAmount(itemInstance));
        if (_hasThisRoundActiveLifeStealEffect) _itemEffectProcessor.ApplyThisRoundActiveLifeStealEffects(itemInstance);
        if (_hasThisRoundActiveMatchEndHealEffect) _itemEffectProcessor.ApplyThisRoundActiveMatchEndHealEffects(itemInstance);
        if (_hasActiveNextRoundDealerForcedDrawEffect) _itemEffectProcessor.ApplyActiveNextRoundDealerForcedDrawEffects(itemInstance, Match);
        if (_hasActiveSwapHandsEffect) _itemEffectProcessor.ApplyActiveSwapHandsEffect(itemInstance, Match);
        if (_hasActiveRerollRandomPlayerCardEffect) _itemEffectProcessor.ApplyActiveRerollRandomPlayerCardEffect(itemInstance, Match);
        if (_hasActiveNextRoundPlayerForcedDrawEffect) _itemEffectProcessor.ApplyActiveNextRoundPlayerForcedDrawEffects(itemInstance, Match);
        if (_hasActiveIgnoreStageBustEffect) _hasStageBustIgnore = true;
        if (_hasActiveStageBustScoreDamageEffect) _hasStageBustScoreDamage = true;
        if (_hasActiveSkipStageWithoutRewardEffect)
        {
            SkipCurrentStageWithoutReward();
            return true;
        }
        if (_hasActiveMaxHpBetEffect)
        {
            _currentRoundHpBetAmount = _itemEffectProcessor.CalculateActiveMaxHpBetAmount(Player, itemInstance);
            RegisterHpBetVictoryEffects(itemInstance);
            Player.LoseHp(_currentRoundHpBetAmount);
        }
        if (_hasActiveForceCurrentRoundDrawEffect) Match.ForceCurrentRoundDraw();

        if (_damage > 0) ApplyDirectDamageToMonster(_damage);
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
        _shopManager.Close();
        return true;
    }

    // 현재 상점의 새로 고침 비용을 지불하고 패시브 1개·액티브 2개를 새로 진열합니다.
    public bool TryRefreshShop()
    {
        if (!IsWaitingForShop || !_shopManager.IsOpen) return false;
        int _refreshCost = _shopManager.CurrentRefreshCost;
        if (!TrySpendGold(_refreshCost)) return false;
        if (!_shopManager.Refresh(ItemInventory.GetOwnedPassiveDefinitions(), _itemEffectProcessor.CalculateShopItemDiscountRate(ItemInventory))) return false;

        ShopStockChanged?.Invoke(_shopManager);
        return true;
    }

    // 현재 상점 진열 아이템을 구매하고, 패시브 중복 규칙은 인벤토리에 그대로 위임합니다.
    public bool TryBuyShopOffer(int offerIndex)
    {
        if (!IsWaitingForShop || !_shopManager.TryGetPurchasableOffer(offerIndex, out ShopOffer _offer)) return false;
        if (!CanAddItemToInventory(_offer.Item)) return false;

        bool _useTicket = _itemEffectProcessor.TryGetFreeShopPurchaseTicket(ItemInventory, out ItemInstance _ticket);
        if (!_useTicket && !TrySpendGold(_offer.Price)) return false;
        if (!TryAddItemToInventory(_offer.Item)) return false;
        if (_useTicket) ItemInventory.TryUseActive(_ticket);

        _shopManager.MarkOfferPurchased(_offer);
        ShopStockChanged?.Invoke(_shopManager);
        return true;
    }

    // 지정한 스테이지 진입 전에 상점 UI가 열려야 하는지 확인합니다.
    public bool IsShopBeforeStage(int stageNumber) => _shopVisitStages.Contains(stageNumber);

    // 9·10스테이지 보스전을 제외한 전투 스테이지를 보상 없이 넘길 수 있는지 확인합니다.
    public bool CanSkipCurrentStageWithoutReward => _monsterDatabase != null && CurrentStage is >= 1 and < FirstBossStage && !IsWaitingForShop && !IsRunCompleted;

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
        RecalculateGoldBasedAttackMultiplier();
        GoldChanged?.Invoke(Gold);
        return true;
    }

    // 양쪽 모두 살아 있을 때 새 블랙잭 매치를 초기화합니다.
    public void StartMatch()
    {
        if (Player.IsDefeated || Monster.IsDefeated) return;
        _playerDoubleDownAttempted = false;
        _currentRoundHpBetAmount = 0;
        _currentRoundHpBetVictoryEffects.Clear();
        _hasUsedInitialHandSwap = false;
        _hasUsedBustCardRemoval = false;
        _cardManipulationUsageState.ResetRound();
        if (_itemEffectProcessor.TryRollNextRoundBlackjack(ItemInventory)) Match.ForceNextPlayerOpeningBlackjack();
        Match.StartMatch();
        _itemEffectProcessor.BeginRound(Player, ItemInventory, Match.PlayerHand, Gold);
        _itemEffectProcessor.RecalculateMonsterAttackMultiplier(Monster);
        _monsterAbilityProcessor.BeginRound(Monster, Match);
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

    // 결과 연출이 끝난 완료 매치의 패 데이터를 UI 전환 전에 비웁니다.
    public void ClearFinishedMatchHands()
    {
        if (Match.IsMatchActive) return;
        Match.ClearHands();
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
        if (TryIgnorePlayerBust(matchResult)) return;
        matchResult = _itemEffectProcessor.ApplyMonsterBustBlackjackConversion(ItemInventory, matchResult, Match.TargetScore);
        _itemEffectProcessor.HandleMatchEnded(matchResult, ItemInventory);
        if (matchResult.Outcome == MatchOutcome.Draw)
        {
            int _drawDamage = _itemEffectProcessor.CalculateDrawDirectDamage(Player, ItemInventory, matchResult);
            int _drawHealAmount = _itemEffectProcessor.CalculateDrawHealAmount(Player, ItemInventory, matchResult);
            ApplyDirectDamageToMonster(_drawDamage);
            if (_drawHealAmount > 0) Player.Heal(_drawHealAmount);
            ApplyActiveMatchEndHealEffects();
            return;
        }
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.OnMatchEnd, matchResult, _playerDoubleDownAttempted, Match.PlayerExtraDrawCount, Gold);
        DamageResult _damageResult = DamageCalculator.Calculate(matchResult, Player, Monster, _playerDoubleDownAttempted);
        float _heartExecutionMultiplier = _monsterAbilityProcessor.GetHeartExecutionDamageMultiplier(matchResult, Match.DealerHand);
        if (_heartExecutionMultiplier > 1f)
        {
            int _executionDamage = (int)MathF.Floor(_damageResult.Damage * _heartExecutionMultiplier);
            _damageResult = new DamageResult(_damageResult.Attacker, _damageResult.Defender, _executionDamage, _damageResult.AppliedMultiplier * _heartExecutionMultiplier);
        }
        bool _wasPlayerProtected = _damageResult.Defender == Player && Player.HasBarrier;
        int _actualDamageToMonster = _damageResult.Defender == Monster ? Math.Min(_damageResult.Damage, Monster.CurrentHp) : 0;
        _damageResult.Defender.TakeDamage(_damageResult.Damage);
        if (_wasPlayerProtected) PlayerBarrierBlocked?.Invoke();
        if (_damageResult.Defender == Player && !_wasPlayerProtected && !Player.IsDefeated)
        {
            _itemEffectProcessor.HandlePlayerDamageTaken(Player, ItemInventory);
        }
        else if (_damageResult.Defender == Monster)
        {
            ApplyLifeSteal(_actualDamageToMonster);
        }
        if (!_wasPlayerProtected) DamageApplied?.Invoke(_damageResult);

        ApplyHpBetVictoryEffects(matchResult);
        ApplyActiveMatchEndHealEffects();

        if (_damageResult.Defender != Monster || !Monster.IsDefeated || _isCurrentMonsterDefeated) return;
        ClearCurrentMonster();
    }

    // 이번 스테이지에 예약된 버스트 무시가 있으면 체력 피해 없이 같은 몬스터와 새 라운드를 즉시 시작합니다.
    private bool TryIgnorePlayerBust(MatchResult matchResult)
    {
        if ((!_hasStageBustIgnore && !_hasStageBustScoreDamage) || matchResult.Outcome != MatchOutcome.DealerWin || matchResult.PlayerScore <= Match.TargetScore) return false;

        bool _dealBustScoreDamage = _hasStageBustScoreDamage;
        _hasStageBustIgnore = false;
        _hasStageBustScoreDamage = false;
        if (_dealBustScoreDamage)
        {
            int _damage = (int)MathF.Floor(matchResult.PlayerScore * Player.AttackMultiplier);
            ApplyDirectDamageToMonster(_damage);
        }
        if (!Monster.IsDefeated) StartMatch();
        return true;
    }

    // 사용한 체력 배팅 액티브에 포함된 승리 후속 효과만 이번 라운드 동안 보관합니다.
    private void RegisterHpBetVictoryEffects(ItemInstance itemInstance)
    {
        _currentRoundHpBetVictoryEffects.Clear();
        foreach (ItemEffectData _effect in itemInstance.Definition.Effects)
        {
            if (_itemEffectProcessor.IsHpBetVictoryFollowUpEffect(_effect)) _currentRoundHpBetVictoryEffects.Add(_effect);
        }
    }

    // 체력 배팅을 사용한 라운드에서 플레이어가 승리하면 베팅값 피해와 회복 보상을 적용합니다.
    private void ApplyHpBetVictoryEffects(MatchResult matchResult)
    {
        if (matchResult.Outcome != MatchOutcome.PlayerWin || _currentRoundHpBetAmount <= 0) return;

        int _damage = _itemEffectProcessor.CalculateHpBetVictoryDirectDamage(_currentRoundHpBetVictoryEffects, _currentRoundHpBetAmount);
        int _healAmount = _itemEffectProcessor.CalculateHpBetVictoryHealAmount(Player, _currentRoundHpBetVictoryEffects);
        if (_damage > 0) ApplyDirectDamageToMonster(_damage);
        if (_healAmount > 0) Player.Heal(_healAmount);
    }

    // 사용한 매치 종료 회복 액티브가 있으면 플레이어 최종 점수와 현재 공격력 계수만큼 회복합니다.
    private void ApplyActiveMatchEndHealEffects()
    {
        int _healAmount = _itemEffectProcessor.CalculateActiveMatchEndHealAmount(Player, Match.PlayerScore);
        if (_healAmount > 0) Player.Heal(_healAmount);
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
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.AfterDraw, null, false, Match.PlayerExtraDrawCount, Gold);
        if (isPlayer) _monsterAbilityProcessor.HandlePlayerCardDrawn(card, Monster);
        else _monsterAbilityProcessor.HandleDealerCardDrawn(Match, Monster);
    }

    // 드로우 후 버스트가 발생하면 보유 패시브로 마지막 카드를 한 번 삭제합니다.
    private void HandleBustCardRemovalAfterDraw(bool isPlayer, Card card, int score)
    {
        if (!isPlayer)
        {
            if (_monsterAbilityProcessor.TryRemoveLastDealerCardOnBust(Match))
            {
                UnityEngine.Debug.Log($"[몬스터 능력] 몬스터 버스트 감지 → 마지막 카드 삭제 → 판정 점수 {Match.DealerScore}/{Match.TargetScore}");
            }
            return;
        }
        if (_bustCardRemovalEffect == null || _hasUsedBustCardRemoval) return;
        if (Match.PlayerScore <= Match.TargetScore) return;
        int _bustScore = Match.PlayerScore;
        if (!Match.TryRemoveLastPlayerCard(out Card _removedCard)) return;

        _hasUsedBustCardRemoval = true;
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.AfterDraw, null, false, Match.PlayerExtraDrawCount, Gold);
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
        OpenShop(_nextStage);
        ShopVisitRequested?.Invoke(_nextStage);
    }

    // 현재 몬스터의 처치 보상·골드·레벨업을 모두 생략하고 다음 전투 스테이지로 이동합니다.
    private void SkipCurrentStageWithoutReward()
    {
        if (!CanSkipCurrentStageWithoutReward) return;

        int _nextStage = CurrentStage + 1;
        MonsterDefinition _nextMonster = _monsterDatabase.GetMonster(_nextStage);
        if (_nextMonster == null) return;

        Match.CancelMatch();
        CanProceedToNextStage = true;

        if (IsShopBeforeStage(_nextStage) && _itemDatabase != null)
        {
            IsWaitingForShop = true;
            OpenShop(_nextStage);
            ShopVisitRequested?.Invoke(_nextStage);
            return;
        }

        StartStage(_nextMonster);
    }

    // 다음 전투 스테이지 기준 가격으로 상점을 열고 현재 진열 정보를 외부 UI에 전달합니다.
    private void OpenShop(int stageNumber)
    {
        if (_itemDatabase == null) return;
        _shopManager.Open(_itemDatabase, stageNumber, ItemInventory.GetOwnedPassiveDefinitions(), _itemEffectProcessor.CalculateShopItemDiscountRate(ItemInventory));
        ShopStockChanged?.Invoke(_shopManager);
    }

    // 실제 추가 전에 패시브 중복으로 거절될 아이템인지 확인합니다.
    private bool CanAddItemToInventory(ItemDefinition itemDefinition)
    {
        if (itemDefinition == null) return false;
        if (itemDefinition.ItemType != ItemType.Passive) return true;
        return !ItemInventory.GetOwnedPassiveDefinitions().Contains(itemDefinition);
    }

    // 몬스터 처치 직후 레벨업 보상과 영구 아이템 보정을 다시 적용합니다.
    private void LevelUpAfterMonsterDefeat()
    {
        Player.LevelUp();
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.None, null, false, Match.PlayerExtraDrawCount, Gold);
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
        LastMonsterGoldReward = Math.Max(0, _adjustedGold);
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
        RecalculateGoldBasedAttackMultiplier();
        GoldChanged?.Invoke(Gold);
    }

    // 골드가 바뀐 직후 골드 비례 영구 공격력 계수 패시브를 다시 계산합니다.
    private void RecalculateGoldBasedAttackMultiplier()
    {
        _itemEffectProcessor.RecalculateAttackMultiplier(Player, ItemInventory, Match.PlayerHand, ItemTrigger.None, null, false, Match.PlayerExtraDrawCount, Gold);
    }
}
