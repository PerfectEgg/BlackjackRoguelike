using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Inspector에 연결한 UI 항목으로 블랙잭 전투, 인벤토리, 보상을 표시하는 시스템입니다.</summary>
public sealed class BlackjackSystem : MonoBehaviour
{
    [Header("게임 데이터")]
    [SerializeField] private MonsterDatabase _monsterDatabase;
    [SerializeField] private ItemDatabase _itemDatabase;
    [Tooltip("해당 스테이지 진입 전에 상점을 엽니다. 디버그용으로 2를 넣으면 1스테이지 처치 뒤 상점이 열립니다.")]
    [SerializeField] private int[] _shopVisitStages = { 5, 10 };

    [Header("공용 아이템 UI 프리팹")]
    [SerializeField] private ItemIconView _itemIconPrefab;
    [SerializeField] private ItemDescriptionView _itemDescriptionPrefab;
    [SerializeField] private Transform _itemDescriptionRoot;

    [Header("전투 정보 UI")]
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _dealerNameText;
    [SerializeField] private Image _dealerSpriteImage;
    [SerializeField] private TMP_Text _dealerHpText;
    [SerializeField] private Image _dealerHpBarFillImage;
    [SerializeField] private TMP_Text _dealerAttackMultiplierText;
    [SerializeField] private TMP_Text _dealerScoreText;
    [SerializeField] private TMP_Text _dealerScoreBonusText;
    [SerializeField] private TMP_Text _playerHpText;
    [SerializeField] private Image _playerHpBarFillImage;
    [SerializeField] private TMP_Text _playerAttackMultiplierText;
    [SerializeField] private GameObject _playerBarrierIndicator;
    [Range(0f, 1f)] [SerializeField] private float _disabledUiAlpha = 0.35f;
    [SerializeField] private TMP_Text _playerScoreText;
    [SerializeField] private TMP_Text _playerScoreBonusText;
    [SerializeField] private TMP_Text _hintText;

    [Header("자동 진행 타이밍")]
    [Min(0f)] [SerializeField] private float _openingCardInterval = 0.2f;
    [Min(0f)] [SerializeField] private float _automaticDrawInterval = 0.4f;
    [Min(0f)] [SerializeField] private float _resultDisplayDuration = 2.5f;
    [Min(0f)] [SerializeField] private float _hitMotionDuration = 0.25f;
    [Min(0f)] [SerializeField] private float _hitMotionDistance = 16f;
    [Min(0f)] [SerializeField] private float _cardClearDuration = 1f;
    [Min(0f)] [SerializeField] private float _nextRoundDelay = 1f;
    [Min(0f)] [SerializeField] private float _shopPanelFadeDuration = 0.35f;

    [Header("결과 피격 모션 UI")]
    [SerializeField] private RectTransform _dealerHitMotionTarget;
    [SerializeField] private RectTransform _playerHitMotionTarget;

    [Header("카드 UI")]
    [SerializeField] private RectTransform _dealerCardContainer;
    [SerializeField] private RectTransform _playerCardContainer;
    [Header("딜러 카드 UI")]
    [SerializeField] private BlackjackCardView _dealerCardTemplate;
    [SerializeField] private float _dealerCardWidth = 110f;
    [SerializeField] private float _dealerMaximumCardSpacing = 120f;
    [Header("플레이어 카드 UI")]
    [SerializeField] private BlackjackCardView _playerCardTemplate;
    [SerializeField] private float _playerCardWidth = 170f;
    [SerializeField] private float _playerMaximumCardSpacing = 105f;

    [Header("전투 버튼")]
    [SerializeField] private Button _hitButton;
    [SerializeField] private Button _standButton;
    [SerializeField] private Button _doubleDownButton;
    [SerializeField] private Button _handSwapButton;
    [SerializeField] private Button _inventoryButton;

    [Header("인벤토리 UI")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Button _passiveInventoryTabButton;
    [SerializeField] private Button _activeInventoryTabButton;
    [SerializeField] private GameObject _passiveInventoryContent;
    [SerializeField] private GameObject _activeInventoryContent;
    [SerializeField] private TMP_Text _passiveInventoryText;
    [SerializeField] private TMP_Text _activeInventoryText;
    [SerializeField] private Button _inventoryCloseButton;

    [Header("보상 UI")]
    [SerializeField] private GameObject _rewardPanel;
    [Tooltip("패시브 보상 아이콘을 넣을 슬롯입니다. 후보 순서대로 연결합니다.")]
    [SerializeField] private Transform[] _passiveRewardSlots;
    [Tooltip("액티브 보상 아이콘을 넣을 슬롯입니다. 후보 순서대로 연결합니다.")]
    [SerializeField] private Transform[] _activeRewardSlots;
    [Tooltip("보상 슬롯을 아직 연결하지 않았을 때만 사용하는 이전 방식의 직접 생성 컨테이너입니다.")]
    [SerializeField] private GameObject _passiveRewardContent;
    [Tooltip("보상 슬롯을 아직 연결하지 않았을 때만 사용하는 이전 방식의 직접 생성 컨테이너입니다.")]
    [SerializeField] private GameObject _activeRewardContent;
    [SerializeField] private Button _rewardConfirmButton;

    [Header("상점 UI")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private TMP_Text _shopTitleText;
    [SerializeField] private Sprite _shopkeeperSprite;
    [Tooltip("상점 패시브 상품 1개를 표시할 슬롯입니다.")]
    [SerializeField] private Transform _passiveShopSlot;
    [Tooltip("상점 액티브 상품 2개를 표시할 슬롯입니다. 진열 순서대로 연결합니다.")]
    [SerializeField] private Transform[] _activeShopSlots;
    [Tooltip("상점 슬롯을 아직 연결하지 않았을 때만 사용하는 이전 방식의 직접 생성 컨테이너입니다.")]
    [SerializeField] private GameObject _shopOfferContent;
    [SerializeField] private Button _shopRefreshButton;
    [SerializeField] private TMP_Text _shopRefreshCostText;
    [SerializeField] private Button _shopExitButton;

    private readonly List<BlackjackCardView> _dealerCardViews = new();
    private readonly List<BlackjackCardView> _playerCardViews = new();
    private readonly List<ItemIconView> _passiveInventoryViews = new();
    private readonly List<ItemIconView> _activeInventoryViews = new();
    private readonly List<ItemIconView> _passiveRewardViews = new();
    private readonly List<ItemIconView> _activeRewardViews = new();
    private readonly List<ItemIconView> _shopOfferViews = new();

    private GameManager _game;
    private StageItemDropResult _pendingReward;
    private readonly HashSet<int> _selectedPassiveRewardIndexes = new();
    private readonly HashSet<int> _selectedActiveRewardIndexes = new();
    private ItemType _selectedInventoryType = ItemType.Passive;
    private ItemDescriptionView _itemDescriptionView;
    private Coroutine _openingDealCoroutine;
    private Coroutine _dealerTurnCoroutine;
    private Coroutine _resultCoroutine;
    private Coroutine _hitMotionCoroutine;
    private Coroutine _panelTransitionCoroutine;
    private Coroutine _shopFadeCoroutine;
    private StageItemDropResult _queuedReward;
    private bool _isShopVisitQueued;
    private bool _isShowingMatchResult;
    private string _matchResultText;

    // 게임 매니저와 UI 버튼을 준비하고, 데이터베이스가 있으면 1스테이지 런을 시작합니다.
    private void Start()
    {
        _game = new GameManager();
        SubscribeGameEvents();
        BindUiButtons();

        HideCardTemplate(_dealerCardTemplate);
        HideCardTemplate(_playerCardTemplate);
        if (_inventoryPanel != null) _inventoryPanel.SetActive(false);
        CreateItemDescriptionView();
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        if (_shopPanel != null) _shopPanel.SetActive(false);
        if (_itemDatabase != null) _game.ConfigureItemDrops(_itemDatabase);
        _game.ConfigureShopVisitStages(_shopVisitStages);
        if (_monsterDatabase != null) _game.StartRun(_monsterDatabase);
        else _game.StartMatch();

        RefreshBattleUi();
    }

    // 자동 진행과 충돌하지 않도록 다음 패 블랙잭 디버그 키만 유지합니다.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) _game.ForceNextPlayerOpeningBlackjack();
    }

    // GameManager와 MatchManager 이벤트를 UI 갱신 함수에 연결합니다.
    private void SubscribeGameEvents()
    {
        _game.Match.CardDrawn += OnCardDrawn;
        _game.Match.StateChanged += OnMatchStateChanged;
        _game.Match.MatchEnded += OnMatchEnded;
        _game.DamageApplied += OnDamageApplied;
        _game.GoldChanged += OnGoldChanged;
        _game.StageStarted += OnStageStarted;
        _game.StageItemDropsGenerated += QueueRewardPanel;
        _game.ShopVisitRequested += OnShopVisitRequested;
        _game.ShopStockChanged += OnShopStockChanged;
        _game.PlayerStateChanged += RefreshStatusUi;
        _game.ItemAdded += OnItemAdded;
    }

    // Inspector에서 연결한 버튼에 게임 동작을 등록합니다.
    private void BindUiButtons()
    {
        if (_hitButton != null) _hitButton.onClick.AddListener(_game.PlayerHit);
        if (_standButton != null) _standButton.onClick.AddListener(_game.PlayerStand);
        if (_doubleDownButton != null) _doubleDownButton.onClick.AddListener(_game.PlayerDoubleDown);
        if (_handSwapButton != null) _handSwapButton.onClick.AddListener(() => _game.TrySwapInitialHand());
        if (_inventoryButton != null) _inventoryButton.onClick.AddListener(ToggleInventory);
        if (_inventoryCloseButton != null) _inventoryCloseButton.onClick.AddListener(ToggleInventory);
        if (_passiveInventoryTabButton != null) _passiveInventoryTabButton.onClick.AddListener(() => ShowInventoryTab(ItemType.Passive));
        if (_activeInventoryTabButton != null) _activeInventoryTabButton.onClick.AddListener(() => ShowInventoryTab(ItemType.Active));
        if (_rewardConfirmButton != null) _rewardConfirmButton.onClick.AddListener(ConfirmRewardSelection);
        if (_shopRefreshButton != null) _shopRefreshButton.onClick.AddListener(RefreshShop);
        if (_shopExitButton != null) _shopExitButton.onClick.AddListener(ExitShop);

    }

    // 카드가 공개될 때 카드 영역과 점수를 갱신합니다.
    private void OnCardDrawn(bool isPlayer, Card card, int score)
    {
        RefreshBattleUi();
    }

    // 입력 가능 상태가 바뀔 때 버튼과 안내 문구를 갱신합니다.
    private void OnMatchStateChanged(MatchState state)
    {
        RefreshBattleUi();
        if (_inventoryPanel != null && _inventoryPanel.activeSelf) RefreshInventoryUi();
        if (state == MatchState.OpeningDeal || state == MatchState.PlayerForcedDraw) StartOpeningDealSequence();
        if (state == MatchState.DealerTurn) StartDealerTurnSequence();
    }

    // 매치가 끝난 뒤 전투 정보를 갱신합니다.
    private void OnMatchEnded(MatchResult result)
    {
        RefreshBattleUi();
        if (!_game.Match.IsMatchActive) StartMatchResultSequence(result);
    }

    // 피해가 적용된 뒤 체력을 갱신합니다.
    private void OnDamageApplied(DamageResult result)
    {
        RefreshBattleUi();
    }

    // 골드가 바뀐 뒤 상단 정보를 갱신합니다.
    private void OnGoldChanged(int gold)
    {
        RefreshStatusUi();
    }

    // 새 스테이지 시작 시 적 이름과 패를 갱신합니다.
    private void OnStageStarted(int stageNumber, Monster monster)
    {
        SetMonsterIdleSprite();
        RefreshBattleUi();
        StartOpeningDealSequence();
    }

    // 아이템 획득으로 인벤토리·공격력·카드 조작 버튼 상태를 갱신합니다.
    private void OnItemAdded(ItemDefinition itemDefinition)
    {
        RefreshBattleUi();
    }

    // 처치 보상 선택이 끝난 뒤 상점을 표시할 수 있도록 요청만 보관합니다.
    private void OnShopVisitRequested(int nextStageNumber)
    {
        _isShopVisitQueued = true;
    }

    // 처치 직후에는 결과·카드 제거 연출을 우선 처리하기 위해 보상 표시를 예약합니다.
    private void QueueRewardPanel(StageItemDropResult itemDrops)
    {
        _queuedReward = itemDrops;
        if (_game == null || _game.Match.IsMatchActive || _resultCoroutine != null) return;

        MatchResult _directDefeatResult = new(MatchOutcome.PlayerWin, _game.Match.PlayerScore, _game.Match.DealerScore, _game.Match.PlayerHand.IsBlackjack, _game.Match.DealerHand.IsBlackjack);
        StartMatchResultSequence(_directDefeatResult);
    }

    // 초기 패와 강제 드로우 카드를 설정한 간격마다 한 장씩 공개합니다.
    private void StartOpeningDealSequence()
    {
        if (_game == null || !_game.Match.IsMatchActive) return;
        if (_openingDealCoroutine != null) StopCoroutine(_openingDealCoroutine);
        _openingDealCoroutine = StartCoroutine(RunOpeningDealSequence());
    }

    // 딜러 턴에서만 자동으로 다음 카드를 공개합니다.
    private void StartDealerTurnSequence()
    {
        if (_game == null || !_game.Match.IsMatchActive) return;
        if (_dealerTurnCoroutine != null) StopCoroutine(_dealerTurnCoroutine);
        _dealerTurnCoroutine = StartCoroutine(RunDealerTurnSequence());
    }

    // 초기 배분과 플레이어 강제 드로우를 상태가 끝날 때까지 자동 진행합니다.
    private IEnumerator RunOpeningDealSequence()
    {
        while (_game != null && _game.Match.IsMatchActive && _game.Match.State is MatchState.OpeningDeal or MatchState.PlayerForcedDraw)
        {
            float _delay = _game.Match.State == MatchState.OpeningDeal ? _openingCardInterval : _automaticDrawInterval;
            yield return new WaitForSeconds(_delay);
            if (_game.Match.IsMatchActive && _game.Match.State is MatchState.OpeningDeal or MatchState.PlayerForcedDraw) _game.AdvanceMatch();
        }
        _openingDealCoroutine = null;
    }

    // 딜러가 스탠드 또는 버스트할 때까지 일정 간격으로 자동 드로우합니다.
    private IEnumerator RunDealerTurnSequence()
    {
        while (_game != null && _game.Match.IsMatchActive && _game.Match.State == MatchState.DealerTurn)
        {
            yield return new WaitForSeconds(_automaticDrawInterval);
            if (_game.Match.IsMatchActive && _game.Match.State == MatchState.DealerTurn) _game.AdvanceMatch();
        }
        _dealerTurnCoroutine = null;
    }

    // 결과 표시, 카드 제거, 다음 라운드 또는 보상 화면 전환을 순서대로 처리합니다.
    private void StartMatchResultSequence(MatchResult result)
    {
        if (_resultCoroutine != null) StopCoroutine(_resultCoroutine);
        if (_hitMotionCoroutine != null) StopCoroutine(_hitMotionCoroutine);
        _resultCoroutine = StartCoroutine(RunMatchResultSequence(result));
    }

    // 결과를 잠시 보여준 뒤 양쪽 카드를 사라지게 하고 후속 화면으로 이동합니다.
    private IEnumerator RunMatchResultSequence(MatchResult result)
    {
        _isShowingMatchResult = true;
        _matchResultText = GetMatchResultText(result);
        SetMonsterResultSprite(result);
        _hitMotionCoroutine = StartCoroutine(PlayHitMotion(result));
        RefreshBattleUi();
        if (result.Outcome == MatchOutcome.PlayerWin && _game.Monster.IsDefeated)
        {
            yield return StartCoroutine(FadeOutDefeatedMonsterSprite());
        }
        else
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            SetMonsterIdleSprite();
        }
        yield return StartCoroutine(FadeAndClearBattleCards());
        yield return new WaitForSeconds(_nextRoundDelay);

        _isShowingMatchResult = false;
        _resultCoroutine = null;
        if (_queuedReward != null)
        {
            StageItemDropResult _reward = _queuedReward;
            _queuedReward = null;
            ShowRewardPanel(_reward);
            yield break;
        }
        if (_isShopVisitQueued || _game.IsWaitingForShop)
        {
            ShowShopPanel();
            yield break;
        }
        StartAutomaticNextMatch();
    }

    // 승패에 따라 피해를 받은 진영의 UI를 짧게 좌우로 튕겨 공격 적중감을 표시합니다.
    private IEnumerator PlayHitMotion(MatchResult result)
    {
        RectTransform _target = result.Outcome switch
        {
            MatchOutcome.PlayerWin => _dealerHitMotionTarget,
            MatchOutcome.DealerWin => _playerHitMotionTarget,
            _ => null
        };
        if (_target == null || _hitMotionDuration <= 0f)
        {
            _hitMotionCoroutine = null;
            yield break;
        }

        Vector2 _origin = _target.anchoredPosition;
        float _direction = result.Outcome == MatchOutcome.PlayerWin ? 1f : -1f;
        float _elapsed = 0f;
        while (_elapsed < _hitMotionDuration)
        {
            _elapsed += Time.deltaTime;
            float _progress = Mathf.Clamp01(_elapsed / _hitMotionDuration);
            float _offset = Mathf.Sin(_progress * Mathf.PI) * _hitMotionDistance * _direction;
            _target.anchoredPosition = _origin + Vector2.right * _offset;
            yield return null;
        }
        _target.anchoredPosition = _origin;
        _hitMotionCoroutine = null;
    }

    // 처치된 몬스터의 피격 스프라이트를 결과 표시 시간 동안 페이드 아웃한 뒤 비웁니다.
    private IEnumerator FadeOutDefeatedMonsterSprite()
    {
        if (_dealerSpriteImage == null)
        {
            yield return new WaitForSeconds(_resultDisplayDuration);
            yield break;
        }

        CanvasGroup _canvasGroup = _dealerSpriteImage.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = _dealerSpriteImage.gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;
        float _elapsed = 0f;
        float _duration = Mathf.Max(0.01f, _resultDisplayDuration);
        while (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            _canvasGroup.alpha = 1f - Mathf.Clamp01(_elapsed / _duration);
            yield return null;
        }

        _dealerSpriteImage.sprite = null;
        _dealerSpriteImage.enabled = false;
        _canvasGroup.alpha = 1f;
    }

    // 카드 UI를 지정한 시간 동안 투명하게 만든 뒤 제거합니다.
    private IEnumerator FadeAndClearBattleCards()
    {
        List<CanvasGroup> _canvasGroups = new();
        AddCardCanvasGroups(_dealerCardViews, _canvasGroups);
        AddCardCanvasGroups(_playerCardViews, _canvasGroups);
        float _elapsed = 0f;
        float _duration = Mathf.Max(0.01f, _cardClearDuration);
        while (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            float _alpha = 1f - Mathf.Clamp01(_elapsed / _duration);
            foreach (CanvasGroup _canvasGroup in _canvasGroups)
            {
                if (_canvasGroup != null) _canvasGroup.alpha = _alpha;
            }
            yield return null;
        }
        ClearBattleCardsImmediately();
    }

    // 상점·보상 전환 전에 이전 전투의 카드와 점수 표기를 즉시 비웁니다.
    private void ClearBattleCardsImmediately()
    {
        _game?.ClearFinishedMatchHands();
        ClearCardViews(_dealerCardViews);
        ClearCardViews(_playerCardViews);
        if (_dealerScoreText != null) _dealerScoreText.text = string.Empty;
        if (_dealerScoreBonusText != null) _dealerScoreBonusText.text = string.Empty;
        if (_playerScoreText != null) _playerScoreText.text = string.Empty;
        if (_playerScoreBonusText != null) _playerScoreBonusText.text = string.Empty;
    }

    // 카드 뷰에 페이드용 CanvasGroup을 준비합니다.
    private void AddCardCanvasGroups(IReadOnlyList<BlackjackCardView> cardViews, List<CanvasGroup> canvasGroups)
    {
        foreach (BlackjackCardView _cardView in cardViews)
        {
            if (_cardView == null) continue;
            CanvasGroup _canvasGroup = _cardView.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = _cardView.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 1f;
            canvasGroups.Add(_canvasGroup);
        }
    }

    // 보상·상점 대기 상태가 아니라면 다음 스테이지 또는 같은 몬스터의 새 라운드를 시작합니다.
    private void StartAutomaticNextMatch()
    {
        if (_game == null || _game.IsWaitingForShop || _game.Match.IsMatchActive) return;
        if (_game.CanProceedToNextStage && _game.ProceedToNextStage()) return;
        _game.RestartCurrentMatch();
    }

    // 승패 결과를 결과 연출 중 안내 문구로 표시합니다.
    private string GetMatchResultText(MatchResult result)
    {
        return result.Outcome switch
        {
            MatchOutcome.PlayerWin => "VICTORY! 공격 적중",
            MatchOutcome.DealerWin => "DEFEAT! 공격을 받았습니다",
            _ => "DRAW"
        };
    }

    // 결과 연출 중 몬스터의 승패에 맞춰 공격 또는 피격 스프라이트를 표시합니다.
    private void SetMonsterResultSprite(MatchResult result)
    {
        if (_game?.Monster?.Definition == null || _dealerSpriteImage == null) return;
        MonsterDefinition _definition = _game.Monster.Definition;
        Sprite _sprite = result.Outcome switch
        {
            MatchOutcome.PlayerWin => _definition.HitSprite,
            MatchOutcome.DealerWin => _definition.AttackSprite,
            _ => _definition.IdleSprite
        };
        SetMonsterSprite(_sprite ?? _definition.IdleSprite);
    }

    // 전투 결과 연출이 끝나거나 새 스테이지가 시작되면 기본 스프라이트로 되돌립니다.
    private void SetMonsterIdleSprite()
    {
        if (_game?.Monster?.Definition == null) return;
        MonsterDefinition _definition = _game.Monster.Definition;
        SetMonsterSprite(_definition.IdleSprite);
    }

    // 중앙 몬스터 UI 이미지에 지정한 스프라이트를 반영합니다.
    private void SetMonsterSprite(Sprite sprite)
    {
        if (_dealerSpriteImage == null) return;
        CanvasGroup _canvasGroup = _dealerSpriteImage.GetComponent<CanvasGroup>();
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        _dealerSpriteImage.sprite = sprite;
        _dealerSpriteImage.enabled = sprite != null;
    }

    // 상점 최초 진열·구매·새로 고침 뒤 표시된 상품과 가격을 갱신합니다.
    private void OnShopStockChanged(ShopManager shop)
    {
        RefreshShopUi();
    }

    // 처치 보상 후보를 Inspector에 연결한 보상 패널에 표시합니다.
    private void ShowRewardPanel(StageItemDropResult itemDrops)
    {
        _pendingReward = itemDrops;
        _selectedPassiveRewardIndexes.Clear();
        _selectedActiveRewardIndexes.Clear();
        RebuildRewardItems(ItemType.Passive, itemDrops.PassiveCandidates, _passiveRewardSlots, _passiveRewardContent, _passiveRewardViews);
        RebuildRewardItems(ItemType.Active, itemDrops.ActiveCandidates, _activeRewardSlots, _activeRewardContent, _activeRewardViews);
        UpdateRewardSelectionUi(ItemType.Passive);
        UpdateRewardSelectionUi(ItemType.Active);
        UpdateRewardConfirmButton();
        if (_rewardPanel != null) _rewardPanel.SetActive(true);
    }

    // 보상 후보를 지정한 슬롯 안에 생성하고 각 아이콘에 선택 동작을 연결합니다.
    private void RebuildRewardItems(ItemType itemType, IReadOnlyList<ItemDefinition> candidates, Transform[] rewardSlots, GameObject fallbackContent, List<ItemIconView> views)
    {
        ClearItemIconViews(views);
        if (candidates == null) return;

        for (int _index = 0; _index < candidates.Count; _index++)
        {
            int _candidateIndex = _index;
            Transform _parent = GetRewardItemParent(_index, rewardSlots, fallbackContent);
            ItemIconView _view = CreateItemIcon(candidates[_index], _parent, false, () => TrySelectReward(itemType, _candidateIndex));
            if (_view != null) views.Add(_view);
        }
    }

    // 후보 순서와 같은 슬롯을 우선 사용하고, 슬롯이 없을 때만 기존 컨테이너를 사용합니다.
    private Transform GetRewardItemParent(int index, Transform[] rewardSlots, GameObject fallbackContent)
    {
        if (rewardSlots != null && rewardSlots.Length > 0)
        {
            return index >= 0 && index < rewardSlots.Length ? rewardSlots[index] : null;
        }
        return fallbackContent != null ? fallbackContent.transform : null;
    }

    // 보상 후보 선택을 토글합니다. 선택한 아이콘을 다시 누르면 선택이 해제됩니다.
    private void TrySelectReward(ItemType itemType, int index)
    {
        if (_pendingReward == null) return;
        IReadOnlyList<ItemDefinition> _candidates = itemType == ItemType.Passive
            ? _pendingReward.PassiveCandidates
            : _pendingReward.ActiveCandidates;
        if (index >= _candidates.Count) return;
        HashSet<int> _selectedIndexes = itemType == ItemType.Passive ? _selectedPassiveRewardIndexes : _selectedActiveRewardIndexes;
        int _maximumPickCount = itemType == ItemType.Passive ? _pendingReward.PassivePickCount : _pendingReward.ActivePickCount;
        if (_selectedIndexes.Contains(index)) _selectedIndexes.Remove(index);
        else if (_selectedIndexes.Count < _maximumPickCount) _selectedIndexes.Add(index);

        UpdateRewardSelectionUi(itemType);
        UpdateRewardConfirmButton();
    }

    // 선택된 후보는 강조하고, 선택 한도에 도달하면 선택되지 않은 후보만 비활성화합니다.
    private void UpdateRewardSelectionUi(ItemType itemType)
    {
        HashSet<int> _selectedIndexes = itemType == ItemType.Passive ? _selectedPassiveRewardIndexes : _selectedActiveRewardIndexes;
        int _maximumPickCount = itemType == ItemType.Passive ? _pendingReward.PassivePickCount : _pendingReward.ActivePickCount;
        List<ItemIconView> _views = itemType == ItemType.Passive ? _passiveRewardViews : _activeRewardViews;
        for (int _index = 0; _index < _views.Count; _index++)
        {
            bool _isSelected = _selectedIndexes.Contains(_index);
            _views[_index].SetSelected(_isSelected);
            _views[_index].SetInteractable(_isSelected || _selectedIndexes.Count < _maximumPickCount);
        }
    }

    // 패시브와 액티브의 필수 선택 수를 모두 채웠는지 반환합니다.
    private bool IsRewardSelectionComplete()
    {
        return _pendingReward != null &&
               _selectedPassiveRewardIndexes.Count == _pendingReward.PassivePickCount &&
               _selectedActiveRewardIndexes.Count == _pendingReward.ActivePickCount;
    }

    // 현재 보상 선택 상태에 맞춰 아이템 받기 버튼을 활성화합니다.
    private void UpdateRewardConfirmButton()
    {
        if (_rewardConfirmButton != null) _rewardConfirmButton.interactable = IsRewardSelectionComplete();
    }

    // 필수 선택을 모두 마친 경우에만 선택한 보상을 인벤토리에 지급합니다.
    private void ConfirmRewardSelection()
    {
        if (!IsRewardSelectionComplete()) return;

        foreach (int _index in _selectedPassiveRewardIndexes) _game.TryAddItemToInventory(_pendingReward.PassiveCandidates[_index]);
        foreach (int _index in _selectedActiveRewardIndexes) _game.TryAddItemToInventory(_pendingReward.ActiveCandidates[_index]);
        CloseRewardPanel();
    }

    // 보상 창을 닫고 다음 상점 또는 다음 라운드를 예약합니다.
    private void CloseRewardPanel()
    {
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        _pendingReward = null;
        RefreshInventoryUi();
        StartPanelTransitionSequence();
    }

    // 현재 상점의 진열 정보를 표시하고 상점 패널을 엽니다.
    private void ShowShopPanel()
    {
        if (_shopPanel == null || !_game.IsWaitingForShop) return;
        _isShopVisitQueued = false;
        ClearBattleCardsImmediately();
        SetShopkeeperSprite();
        _shopPanel.SetActive(true);
        RefreshShopUi();
        StartShopFade(true);
    }

    // 상품 이름·희귀도·가격과 새로 고침 가격을 상점 UI에 표시합니다.
    private void RefreshShopUi()
    {
        if (_game == null || !_game.Shop.IsOpen) return;
        ShopManager _shop = _game.Shop;
        if (_shopTitleText != null) _shopTitleText.text = $"SHOP | STAGE {_shop.StageNumber}";

        ClearItemIconViews(_shopOfferViews);
        int _activeOfferIndex = 0;
        for (int _index = 0; _index < _shop.Offers.Count; _index++)
        {
            int _offerIndex = _index;
            ShopOffer _offer = _shop.Offers[_index];
            int _currentActiveOfferIndex = _offer.Item.ItemType == ItemType.Active ? _activeOfferIndex++ : -1;
            Transform _parent = GetShopOfferParent(_offer.Item.ItemType, _currentActiveOfferIndex);
            string _footerText = _offer.IsSold ? "SOLD OUT" : (_game.HasFreeShopPurchaseTicket ? $"{_offer.Price} GOLD | TICKET" : $"{_offer.Price} GOLD");
            ItemIconView _view = CreateItemIcon(_offer.Item, _parent, false, () => TryBuyShopOffer(_offerIndex), _footerText);
            if (_view == null) continue;
            _view.SetInteractable(!_offer.IsSold && (_game.Gold >= _offer.Price || _game.HasFreeShopPurchaseTicket));
            _view.SetPurchased(_offer.IsSold);
            _shopOfferViews.Add(_view);
        }

        if (_shopRefreshCostText != null) _shopRefreshCostText.text = _shop.CurrentRefreshCost.ToString();
        if (_shopRefreshButton != null) _shopRefreshButton.interactable = _game.Gold >= _shop.CurrentRefreshCost;
    }

    // 패시브는 전용 한 칸, 액티브는 진열 순서의 전용 슬롯을 우선 사용합니다.
    private Transform GetShopOfferParent(ItemType itemType, int activeOfferIndex)
    {
        if (itemType == ItemType.Passive && _passiveShopSlot != null) return _passiveShopSlot;
        if (itemType == ItemType.Active && _activeShopSlots != null && _activeShopSlots.Length > 0)
        {
            return activeOfferIndex >= 0 && activeOfferIndex < _activeShopSlots.Length ? _activeShopSlots[activeOfferIndex] : null;
        }
        return _shopOfferContent != null ? _shopOfferContent.transform : null;
    }

    // 모든 상점 방문에서 공통 상점 주인 스프라이트를 표시합니다.
    private void SetShopkeeperSprite()
    {
        SetMonsterSprite(_shopkeeperSprite);
    }

    // 지정한 상점 진열 아이템을 구매하고 버튼 표기를 갱신합니다.
    private void TryBuyShopOffer(int index)
    {
        if (!_game.TryBuyShopOffer(index)) return;
        RefreshInventoryUi();
        RefreshShopUi();
    }

    // 새로 고침 비용을 지불하고 상점의 모든 진열 칸을 다시 구성합니다.
    private void RefreshShop()
    {
        if (_game.TryRefreshShop()) RefreshShopUi();
    }

    // 상점 이용을 종료해 다음 스테이지 진행을 허용합니다.
    private void ExitShop()
    {
        if (!_game.CompleteShopVisit()) return;
        StartShopFade(false);
    }

    // 상점 패널 전체를 투명도 변화로 열거나 닫습니다.
    private void StartShopFade(bool fadeIn)
    {
        if (_shopFadeCoroutine != null) StopCoroutine(_shopFadeCoroutine);
        _shopFadeCoroutine = StartCoroutine(FadeShopPanel(fadeIn));
    }

    // 페이드 아웃이 끝난 뒤에만 패널을 닫고 다음 스테이지 진행을 예약합니다.
    private IEnumerator FadeShopPanel(bool fadeIn)
    {
        if (_shopPanel == null) yield break;
        CanvasGroup _canvasGroup = _shopPanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = _shopPanel.AddComponent<CanvasGroup>();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        float _startAlpha = fadeIn ? 0f : _canvasGroup.alpha;
        float _endAlpha = fadeIn ? 1f : 0f;
        _canvasGroup.alpha = _startAlpha;
        float _elapsed = 0f;
        float _duration = Mathf.Max(0.01f, _shopPanelFadeDuration);
        while (_elapsed < _duration)
        {
            _elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(_startAlpha, _endAlpha, Mathf.Clamp01(_elapsed / _duration));
            yield return null;
        }
        _canvasGroup.alpha = _endAlpha;
        _shopFadeCoroutine = null;

        if (fadeIn)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            yield break;
        }

        _shopPanel.SetActive(false);
        RefreshBattleUi();
        StartPanelTransitionSequence();
    }

    // 보상 수령 또는 상점 퇴장 후 지정 시간만큼 기다렸다 다음 흐름을 시작합니다.
    private void StartPanelTransitionSequence()
    {
        if (_panelTransitionCoroutine != null) StopCoroutine(_panelTransitionCoroutine);
        _panelTransitionCoroutine = StartCoroutine(RunPanelTransitionSequence());
    }

    // 패널 전환 직후 전투가 바로 시작되지 않도록 짧은 여유 시간을 둡니다.
    private IEnumerator RunPanelTransitionSequence()
    {
        yield return new WaitForSeconds(_nextRoundDelay);
        _panelTransitionCoroutine = null;
        if (_game.IsWaitingForShop)
        {
            ShowShopPanel();
            yield break;
        }
        StartAutomaticNextMatch();
    }

    // 체력, 점수, 카드, 버튼 상태와 안내 문구를 갱신합니다.
    private void RefreshBattleUi()
    {
        if (_game == null) return;
        RefreshStatusUi();
        bool _hideDealerCards = _game.MonsterAbilities.HasHiddenDealerCard && _game.Match.State is not (MatchState.DealerTurn or MatchState.Finished);
        RebuildCards(_game.Match.DealerHand, _dealerCardContainer, _dealerCardViews, _dealerCardTemplate, _dealerCardWidth, _dealerMaximumCardSpacing, true, _hideDealerCards);
        RebuildCards(_game.Match.PlayerHand, _playerCardContainer, _playerCardViews, _playerCardTemplate, _playerCardWidth, _playerMaximumCardSpacing);
        if (_dealerScoreText != null)
        {
            _dealerScoreText.text = _hideDealerCards
                ? $"{_game.MonsterAbilities.GetVisibleDealerScore(_game.Match.DealerHand)} + ?"
                : GetScoreText(_game.Match.DealerHand);
        }
        if (_dealerScoreBonusText != null) _dealerScoreBonusText.text = _hideDealerCards ? string.Empty : GetScoreBonusText(_game.Match.DealerHand.Score, _game.Match.DealerScore);
        if (_playerScoreText != null) _playerScoreText.text = GetScoreText(_game.Match.PlayerHand);
        if (_playerScoreBonusText != null) _playerScoreBonusText.text = GetScoreBonusText(_game.Match.PlayerHand.Score, _game.Match.PlayerScore);

        bool _isPlayerTurn = _game.Match.IsMatchActive && _game.Match.State == MatchState.PlayerTurn;
        if (_hitButton != null) _hitButton.interactable = _isPlayerTurn;
        if (_standButton != null) _standButton.interactable = _isPlayerTurn;
        if (_doubleDownButton != null) _doubleDownButton.interactable = _game.Match.CanDoubleDown;
        if (_handSwapButton != null)
        {
            _handSwapButton.interactable = _game.CanSwapInitialHand;
            SetDisabledAppearance(_handSwapButton.gameObject, _game.CanSwapInitialHand);
        }
        if (_hintText != null) _hintText.text = GetHintText();
    }

    // 이름, 체력, 골드와 인벤토리 텍스트를 갱신합니다.
    private void RefreshStatusUi()
    {
        if (_game == null) return;
        if (_stageText != null) _stageText.text = $"스테이지 {_game.CurrentStage}";
        if (_goldText != null) _goldText.text = $"{_game.Gold}";
        if (_dealerNameText != null) _dealerNameText.text = _game.Monster.Name.ToUpperInvariant();
        if (_dealerHpText != null) _dealerHpText.text = $"{_game.Monster.CurrentHp} / {_game.Monster.MaxHp}";
        UpdateHpBar(_dealerHpBarFillImage, _game.Monster.CurrentHp, _game.Monster.MaxHp);
        if (_dealerAttackMultiplierText != null) _dealerAttackMultiplierText.text = $"x{_game.Monster.AttackMultiplier:0.##}";
        if (_playerHpText != null) _playerHpText.text = $"{_game.Player.CurrentHp} / {_game.Player.MaxHp}";
        UpdateHpBar(_playerHpBarFillImage, _game.Player.CurrentHp, _game.Player.MaxHp);
        if (_playerAttackMultiplierText != null) _playerAttackMultiplierText.text = $"x{_game.Player.AttackMultiplier:0.##}";
        SetDisabledAppearance(_playerBarrierIndicator, _game.Player.HasBarrier);
        RefreshInventoryUi();
    }

    // 현재 체력 비율을 UI Image의 Fill Amount로 반영합니다.
    private void UpdateHpBar(Image hpBarFillImage, int currentHp, int maxHp)
    {
        if (hpBarFillImage == null) return;
        hpBarFillImage.fillAmount = maxHp <= 0 ? 0f : Mathf.Clamp01((float)currentHp / maxHp);
    }

    // UI를 숨기지 않고, 비활성 상태에는 반투명·비클릭 상태로 표시합니다.
    private void SetDisabledAppearance(GameObject targetObject, bool isEnabled)
    {
        if (targetObject == null) return;
        if (!targetObject.activeSelf) targetObject.SetActive(true);

        CanvasGroup _canvasGroup = targetObject.GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = targetObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = isEnabled ? 1f : _disabledUiAlpha;
        _canvasGroup.interactable = isEnabled;
        _canvasGroup.blocksRaycasts = isEnabled;
    }

    // 패시브와 액티브 보유 목록을 각각의 텍스트 UI에 표시합니다.
    private void RefreshInventoryUi()
    {
        if (_game == null) return;
        if (_passiveInventoryText != null) _passiveInventoryText.text = BuildInventoryText("PASSIVES", _game.ItemInventory.PassiveItems);
        if (_activeInventoryText != null) _activeInventoryText.text = BuildInventoryText("ACTIVES", _game.ItemInventory.ActiveItems);
        if (_inventoryPanel != null && !_inventoryPanel.activeSelf) return;
        RebuildInventoryItems(_game.ItemInventory.PassiveItems, _passiveInventoryContent, _passiveInventoryViews);
        RebuildInventoryItems(_game.ItemInventory.ActiveItems, _activeInventoryContent, _activeInventoryViews);
    }

    // 지정한 타입의 인벤토리 탭만 보여 주고 탭 버튼의 선택 상태를 갱신합니다.
    private void ShowInventoryTab(ItemType itemType)
    {
        _selectedInventoryType = itemType;
        if (_passiveInventoryContent != null) _passiveInventoryContent.SetActive(itemType == ItemType.Passive);
        if (_activeInventoryContent != null) _activeInventoryContent.SetActive(itemType == ItemType.Active);
        if (_passiveInventoryTabButton != null) _passiveInventoryTabButton.interactable = itemType != ItemType.Passive;
        if (_activeInventoryTabButton != null) _activeInventoryTabButton.interactable = itemType != ItemType.Active;
        _itemDescriptionView?.Hide();
    }

    // 각 탭의 아이템 아이콘을 현재 인벤토리 목록 기준으로 새로 생성합니다.
    private void RebuildInventoryItems(IReadOnlyList<ItemInstance> items, GameObject content, List<ItemIconView> views)
    {
        if (content == null || _itemIconPrefab == null) return;
        ClearInventoryItemViews(views);

        foreach (ItemInstance _item in items)
        {
            UnityEngine.Events.UnityAction _clickAction = _item.Definition.ItemType == ItemType.Active && !_item.IsUsed
                ? () => TryUseActiveInventoryItem(_item)
                : null;
            ItemIconView _view = CreateItemIcon(_item.Definition, content.transform, _item.IsUsed, _clickAction);
            if (_view == null) continue;
            if (_item.Definition.ItemType == ItemType.Active) _view.SetInteractable(_game != null && _game.CanUseActiveItems && !_item.IsUsed);
            views.Add(_view);
        }
    }

    // 인벤토리에서 클릭한 액티브 아이템을 사용하고, 성공하면 아이콘의 사용 상태를 다시 표시합니다.
    private void TryUseActiveInventoryItem(ItemInstance itemInstance)
    {
        if (!_game.TryUseActiveItem(itemInstance)) return;
        RefreshInventoryUi();
        RefreshBattleUi();
    }

    // 인벤토리·상점·보상 UI에서 공용 아이콘 프리팹을 생성할 수 있도록 아이템 아이콘을 반환합니다.
    public ItemIconView CreateItemIcon(ItemDefinition itemDefinition, Transform parent, bool isUsed = false, UnityEngine.Events.UnityAction clickAction = null, string footerText = null)
    {
        if (itemDefinition == null || parent == null || _itemIconPrefab == null) return null;
        ItemIconView _view = Instantiate(_itemIconPrefab, parent);
        _view.gameObject.SetActive(true);
        _view.SetItem(itemDefinition, _itemDescriptionView, isUsed, clickAction, footerText);
        return _view;
    }

    // 이전에 생성한 아이콘 슬롯을 제거합니다. 템플릿 오브젝트는 목록에 넣지 않으므로 보존됩니다.
    private void ClearInventoryItemViews(List<ItemIconView> views)
    {
        ClearItemIconViews(views);
    }

    // 이전에 생성한 공용 아이콘 슬롯을 제거합니다.
    private void ClearItemIconViews(List<ItemIconView> views)
    {
        foreach (ItemIconView _view in views)
        {
            if (_view != null) Destroy(_view.gameObject);
        }
        views.Clear();
    }

    // 인벤토리 목록을 줄 단위 텍스트로 만듭니다.
    private string BuildInventoryText(string header, IReadOnlyList<ItemInstance> items)
    {
        StringBuilder _builder = new StringBuilder(header).Append("\n\n");
        if (items.Count == 0) return _builder.Append("없음").ToString();
        foreach (ItemInstance _item in items) _builder.Append("• ").Append(_item.Definition.ItemName).Append("\n");
        return _builder.ToString();
    }

    // 카드 자체가 만든 원점수만 반환합니다. 점수 보정은 별도 Bonus Text에서 표시합니다.
    private string GetScoreText(BlackjackHand hand)
    {
        return hand == null ? string.Empty : hand.Score.ToString();
    }

    // 원점수와 실제 판정 점수의 차이를 (+1), (-1)처럼 별도 표기합니다.
    private string GetScoreBonusText(int rawScore, int judgedScore)
    {
        int _bonus = judgedScore - rawScore;
        if (_bonus == 0) return string.Empty;
        return _bonus > 0 ? $"(+{_bonus})" : $"({_bonus})";
    }

    // 카드 컨테이너와 진영별 템플릿 설정에 따라 카드 크기와 간격을 배치합니다.
    private void RebuildCards(BlackjackHand hand, RectTransform cardContainer, List<BlackjackCardView> cardViews, BlackjackCardView cardTemplate, float cardWidth, float maximumCardSpacing, bool isDealer = false, bool hideDealerCards = false)
    {
        if (cardContainer == null || cardTemplate == null) return;
        ClearCardViews(cardViews);
        int _count = hand.Cards.Count;
        if (_count == 0) return;

        float _safeCardWidth = Mathf.Max(1f, cardWidth);
        RectTransform _templateTransform = cardTemplate.GetComponent<RectTransform>();
        float _aspectRatio = _templateTransform != null && _templateTransform.rect.width > 0f
            ? _templateTransform.rect.height / _templateTransform.rect.width
            : 1.4f;
        float _containerWidth = Mathf.Max(_safeCardWidth, cardContainer.rect.width);
        float _spacing = _count == 1 ? 0f : Mathf.Min(maximumCardSpacing, (_containerWidth - _safeCardWidth) / (_count - 1));
        float _totalWidth = _safeCardWidth + _spacing * (_count - 1);
        float _startX = -_totalWidth * 0.5f + _safeCardWidth * 0.5f;

        for (int _index = 0; _index < _count; _index++)
        {
            BlackjackCardView _cardView = Instantiate(cardTemplate, cardContainer);
            _cardView.gameObject.SetActive(true);
            RectTransform _cardTransform = _cardView.GetComponent<RectTransform>();
            _cardTransform.anchorMin = _cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _cardTransform.pivot = new Vector2(0.5f, 0.5f);
            _cardTransform.sizeDelta = new Vector2(_safeCardWidth, _safeCardWidth * _aspectRatio);
            _cardTransform.anchoredPosition = new Vector2(_startX + _spacing * _index, 0f);
            if (isDealer && hideDealerCards && _game.MonsterAbilities.IsDealerCardHidden(_index)) _cardView.SetHidden();
            else _cardView.SetCard(hand.Cards[_index]);
            _cardView.transform.SetAsLastSibling();
            cardViews.Add(_cardView);
        }
    }

    // 씬에 놓은 원본 템플릿은 복제 전용으로 숨깁니다.
    private void HideCardTemplate(BlackjackCardView cardTemplate)
    {
        if (cardTemplate != null && cardTemplate.gameObject.scene.IsValid()) cardTemplate.gameObject.SetActive(false);
    }

    // 이전에 생성한 카드 UI를 제거합니다.
    private void ClearCardViews(List<BlackjackCardView> cardViews)
    {
        foreach (BlackjackCardView _cardView in cardViews)
        {
            if (_cardView != null) Destroy(_cardView.gameObject);
        }
        cardViews.Clear();
    }

    // 현재 매치 상태에 맞는 안내 문구를 반환합니다.
    private string GetHintText()
    {
        if (_isShowingMatchResult) return _matchResultText;
        return _game.Match.State switch
        {
            MatchState.OpeningDeal => "카드를 나누는 중...",
            MatchState.PlayerForcedDraw => "강제 드로우 진행 중...",
            MatchState.PlayerTurn => "카드를 선택하세요: HIT / STAND / DOUBLE DOWN",
            MatchState.DealerTurn => "몬스터가 카드를 뽑는 중...",
            _ => "결과를 정리하는 중..."
        };
    }

    // 인벤토리 패널을 열거나 닫습니다.
    private void ToggleInventory()
    {
        if (_inventoryPanel == null) return;
        _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
        if (_inventoryPanel.activeSelf)
        {
            RefreshInventoryUi();
            ShowInventoryTab(_selectedInventoryType);
        }
        else _itemDescriptionView?.Hide();
    }

    // 공용 설명 프리팹을 Canvas 아래 지정한 루트에 한 번 생성해 모든 아이콘이 공유하게 합니다.
    private void CreateItemDescriptionView()
    {
        if (_itemDescriptionPrefab == null || _itemDescriptionRoot == null) return;
        _itemDescriptionRoot.SetAsLastSibling();
        _itemDescriptionView = Instantiate(_itemDescriptionPrefab, _itemDescriptionRoot);
        _itemDescriptionView.transform.SetAsLastSibling();
        _itemDescriptionView.gameObject.SetActive(false);
    }

    // 버튼 하위 TMP 텍스트 컴포넌트에 후보 이름을 표시합니다.
    private void SetButtonText(Button button, string text)
    {
        TMP_Text _text = button.GetComponentInChildren<TMP_Text>();
        if (_text != null) _text.text = text;
    }

    // 등록한 이벤트 구독을 해제합니다.
    private void OnDestroy()
    {
        if (_game == null) return;
        _game.Match.CardDrawn -= OnCardDrawn;
        _game.Match.StateChanged -= OnMatchStateChanged;
        _game.Match.MatchEnded -= OnMatchEnded;
        _game.DamageApplied -= OnDamageApplied;
        _game.GoldChanged -= OnGoldChanged;
        _game.StageStarted -= OnStageStarted;
        _game.StageItemDropsGenerated -= QueueRewardPanel;
        _game.ShopVisitRequested -= OnShopVisitRequested;
        _game.ShopStockChanged -= OnShopStockChanged;
        _game.PlayerStateChanged -= RefreshStatusUi;
        _game.ItemAdded -= OnItemAdded;
    }
}
