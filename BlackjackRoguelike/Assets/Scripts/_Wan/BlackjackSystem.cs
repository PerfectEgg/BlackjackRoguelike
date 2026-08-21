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

    [Header("공용 아이템 UI 프리팹")]
    [SerializeField] private ItemIconView _itemIconPrefab;
    [SerializeField] private ItemDescriptionView _itemDescriptionPrefab;
    [SerializeField] private Transform _itemDescriptionRoot;

    [Header("전투 정보 UI")]
    [SerializeField] private TMP_Text _stageText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _dealerNameText;
    [SerializeField] private TMP_Text _dealerHpText;
    [SerializeField] private TMP_Text _dealerAttackMultiplierText;
    [SerializeField] private TMP_Text _dealerScoreText;
    [SerializeField] private TMP_Text _playerHpText;
    [SerializeField] private TMP_Text _playerAttackMultiplierText;
    [SerializeField] private GameObject _playerBarrierIndicator;
    [SerializeField] private TMP_Text _playerScoreText;
    [SerializeField] private TMP_Text _hintText;

    [Header("카드 UI")]
    [SerializeField] private RectTransform _dealerCardContainer;
    [SerializeField] private RectTransform _playerCardContainer;
    [SerializeField] private BlackjackCardView _cardTemplate;
    [SerializeField] private float _cardWidth = 150f;
    [SerializeField] private float _maximumCardSpacing = 165f;

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
    [SerializeField] private GameObject _passiveRewardContent;
    [SerializeField] private GameObject _activeRewardContent;
    [SerializeField] private Button _rewardConfirmButton;

    [Header("상점 UI")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private TMP_Text _shopTitleText;
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

    // 게임 매니저와 UI 버튼을 준비하고, 데이터베이스가 있으면 1스테이지 런을 시작합니다.
    private void Start()
    {
        _game = new GameManager();
        SubscribeGameEvents();
        BindUiButtons();

        if (_cardTemplate != null && _cardTemplate.gameObject.scene.IsValid()) _cardTemplate.gameObject.SetActive(false);
        if (_inventoryPanel != null) _inventoryPanel.SetActive(false);
        CreateItemDescriptionView();
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        if (_shopPanel != null) _shopPanel.SetActive(false);
        if (_itemDatabase != null) _game.ConfigureItemDrops(_itemDatabase);
        if (_monsterDatabase != null) _game.StartRun(_monsterDatabase);
        else _game.StartMatch();

        RefreshBattleUi();
    }

    // Space는 카드 공개와 딜러 턴 진행에만 사용하고, 선택은 화면 버튼으로 처리합니다.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) _game.AdvanceMatch();
        if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.R)) StartNextRound();
        if (Input.GetKeyDown(KeyCode.B)) _game.ForceNextPlayerOpeningBlackjack();
        if (Input.GetKeyDown(KeyCode.T)) _game.ForceNextThreePlayerDrawsToSeven();
    }

    // 몬스터 처치 뒤에는 다음 스테이지로, 그 외에는 같은 상대와 다음 라운드를 시작합니다.
    private void StartNextRound()
    {
        if (_game.IsWaitingForShop) return;
        if (_game.Match.IsMatchActive) return;
        if (_game.CanProceedToNextStage && _game.ProceedToNextStage()) return;
        _game.RestartCurrentMatch();
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
        _game.StageItemDropsGenerated += ShowRewardPanel;
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
    }

    // 매치가 끝난 뒤 전투 정보를 갱신합니다.
    private void OnMatchEnded(MatchResult result)
    {
        RefreshBattleUi();
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
        RefreshBattleUi();
    }

    // 아이템 획득으로 인벤토리·공격력·카드 조작 버튼 상태를 갱신합니다.
    private void OnItemAdded(ItemDefinition itemDefinition)
    {
        RefreshBattleUi();
    }

    // 4·9스테이지 몬스터 처치 뒤 다음 스테이지 전 상점 방문이 요청되면 패널을 표시합니다.
    private void OnShopVisitRequested(int nextStageNumber)
    {
        if (_rewardPanel != null && _rewardPanel.activeSelf) return;
        ShowShopPanel();
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
        RebuildRewardItems(ItemType.Passive, itemDrops.PassiveCandidates, _passiveRewardContent, _passiveRewardViews);
        RebuildRewardItems(ItemType.Active, itemDrops.ActiveCandidates, _activeRewardContent, _activeRewardViews);
        UpdateRewardSelectionUi(ItemType.Passive);
        UpdateRewardSelectionUi(ItemType.Active);
        UpdateRewardConfirmButton();
        if (_rewardPanel != null) _rewardPanel.SetActive(true);
    }

    // 보상 후보를 공용 아이콘 프리팹으로 생성하고 각 아이콘에 선택 동작을 연결합니다.
    private void RebuildRewardItems(ItemType itemType, IReadOnlyList<ItemDefinition> candidates, GameObject content, List<ItemIconView> views)
    {
        ClearItemIconViews(views);
        if (content == null) return;

        for (int _index = 0; _index < candidates.Count; _index++)
        {
            int _candidateIndex = _index;
            ItemIconView _view = CreateItemIcon(candidates[_index], content.transform, false, () => TrySelectReward(itemType, _candidateIndex));
            if (_view != null) views.Add(_view);
        }
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

    // 보상 창을 닫고 인벤토리 표기를 갱신합니다.
    private void CloseRewardPanel()
    {
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        _pendingReward = null;
        RefreshInventoryUi();
        if (_game.IsWaitingForShop) ShowShopPanel();
    }

    // 현재 상점의 진열 정보를 표시하고 상점 패널을 엽니다.
    private void ShowShopPanel()
    {
        if (_shopPanel == null || !_game.IsWaitingForShop) return;
        _shopPanel.SetActive(true);
        RefreshShopUi();
    }

    // 상품 이름·희귀도·가격과 새로 고침 가격을 상점 UI에 표시합니다.
    private void RefreshShopUi()
    {
        if (_game == null || !_game.Shop.IsOpen) return;
        ShopManager _shop = _game.Shop;
        if (_shopTitleText != null) _shopTitleText.text = $"SHOP | STAGE {_shop.StageNumber}";

        ClearItemIconViews(_shopOfferViews);
        if (_shopOfferContent != null)
        {
            for (int _index = 0; _index < _shop.Offers.Count; _index++)
            {
                int _offerIndex = _index;
                ShopOffer _offer = _shop.Offers[_index];
                string _footerText = _offer.IsSold ? "SOLD OUT" : $"{_offer.Price} GOLD";
                ItemIconView _view = CreateItemIcon(_offer.Item, _shopOfferContent.transform, false, () => TryBuyShopOffer(_offerIndex), _footerText);
                if (_view == null) continue;
                _view.SetInteractable(!_offer.IsSold && _game.Gold >= _offer.Price);
                _shopOfferViews.Add(_view);
            }
        }

        if (_shopRefreshCostText != null) _shopRefreshCostText.text = $"REFRESH: {_shop.CurrentRefreshCost} GOLD";
        if (_shopRefreshButton != null) _shopRefreshButton.interactable = _game.Gold >= _shop.CurrentRefreshCost;
    }

    // 지정한 상점 진열 아이템을 구매하고 버튼 표기를 갱신합니다.
    private void TryBuyShopOffer(int index)
    {
        if (_game.TryBuyShopOffer(index)) RefreshShopUi();
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
        if (_shopPanel != null) _shopPanel.SetActive(false);
        RefreshBattleUi();
    }

    // 체력, 점수, 카드, 버튼 상태와 안내 문구를 갱신합니다.
    private void RefreshBattleUi()
    {
        if (_game == null) return;
        RefreshStatusUi();
        RebuildCards(_game.Match.DealerHand, _dealerCardContainer, _dealerCardViews);
        RebuildCards(_game.Match.PlayerHand, _playerCardContainer, _playerCardViews);
        if (_dealerScoreText != null) _dealerScoreText.text = GetScoreText("MONSTER HAND", _game.Match.DealerHand, _game.Match.DealerScore, _game.Match.TargetScore);
        if (_playerScoreText != null) _playerScoreText.text = GetScoreText("PLAYER HAND", _game.Match.PlayerHand, _game.Match.PlayerScore, _game.Match.TargetScore, _game.PlayerScoreAdjustmentRange);

        bool _isPlayerTurn = _game.Match.IsMatchActive && _game.Match.State == MatchState.PlayerTurn;
        if (_hitButton != null) _hitButton.interactable = _isPlayerTurn;
        if (_standButton != null) _standButton.interactable = _isPlayerTurn;
        if (_doubleDownButton != null) _doubleDownButton.interactable = _game.Match.CanDoubleDown;
        if (_handSwapButton != null)
        {
            _handSwapButton.gameObject.SetActive(_game.HasInitialHandSwapPassive);
            _handSwapButton.interactable = _game.CanSwapInitialHand;
        }
        if (_hintText != null) _hintText.text = GetHintText();
    }

    // 이름, 체력, 골드와 인벤토리 텍스트를 갱신합니다.
    private void RefreshStatusUi()
    {
        if (_game == null) return;
        if (_stageText != null) _stageText.text = $"STAGE {_game.CurrentStage}";
        if (_goldText != null) _goldText.text = $"GOLD {_game.Gold}";
        if (_dealerNameText != null) _dealerNameText.text = _game.Monster.Name.ToUpperInvariant();
        if (_dealerHpText != null) _dealerHpText.text = $"HP {_game.Monster.CurrentHp} / {_game.Monster.MaxHp}";
        if (_dealerAttackMultiplierText != null) _dealerAttackMultiplierText.text = $"ATK x{_game.Monster.AttackMultiplier:0.##}";
        if (_playerHpText != null) _playerHpText.text = $"HP {_game.Player.CurrentHp} / {_game.Player.MaxHp}";
        if (_playerAttackMultiplierText != null) _playerAttackMultiplierText.text = $"ATK x{_game.Player.AttackMultiplier:0.##}";
        if (_playerBarrierIndicator != null) _playerBarrierIndicator.SetActive(_game.Player.HasBarrier);
        RefreshInventoryUi();
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

    // 에이스가 1점 또는 11점으로 계산되는 현재 상태를 점수 옆에 표시합니다.
    private string GetScoreText(string handName, BlackjackHand hand, int judgedScore, int targetScore, int adjustmentRange = 0)
    {
        string _aceValue = hand.HasAce ? (hand.IsSoft ? " | A = 11" : " | A = 1") : string.Empty;
        string _rawScore = hand.Score == judgedScore ? string.Empty : $" | RAW {hand.Score}";
        string _adjustment = adjustmentRange > 0 ? $" | ADJ ±{adjustmentRange}" : string.Empty;
        return $"{handName}  |  SCORE {judgedScore} / {targetScore}{_rawScore}{_adjustment}{_aceValue}";
    }

    // 카드 컨테이너의 폭에 따라 간격을 줄여 카드가 겹쳐 보이게 배치합니다.
    private void RebuildCards(BlackjackHand hand, RectTransform cardContainer, List<BlackjackCardView> cardViews)
    {
        if (cardContainer == null || _cardTemplate == null) return;
        ClearCardViews(cardViews);
        int _count = hand.Cards.Count;
        if (_count == 0) return;

        float _containerWidth = Mathf.Max(_cardWidth, cardContainer.rect.width);
        float _spacing = _count == 1 ? 0f : Mathf.Min(_maximumCardSpacing, (_containerWidth - _cardWidth) / (_count - 1));
        float _totalWidth = _cardWidth + _spacing * (_count - 1);
        float _startX = -_totalWidth * 0.5f + _cardWidth * 0.5f;

        for (int _index = 0; _index < _count; _index++)
        {
            BlackjackCardView _cardView = Instantiate(_cardTemplate, cardContainer);
            _cardView.gameObject.SetActive(true);
            RectTransform _cardTransform = _cardView.GetComponent<RectTransform>();
            _cardTransform.anchorMin = _cardTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _cardTransform.pivot = new Vector2(0.5f, 0.5f);
            _cardTransform.anchoredPosition = new Vector2(_startX + _spacing * _index, 0f);
            _cardView.SetCard(hand.Cards[_index]);
            _cardView.transform.SetAsLastSibling();
            cardViews.Add(_cardView);
        }
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
        return _game.Match.State switch
        {
            MatchState.OpeningDeal => "SPACE: 시작 카드를 한 장씩 공개",
            MatchState.PlayerTurn => "카드를 선택하세요: HIT / STAND / DOUBLE DOWN",
            MatchState.DealerTurn => "SPACE: 몬스터의 다음 카드를 공개",
            _ => "N / R: 다음 라운드 시작 | B: 다음 패 블랙잭 | T: 다음 3장 7 고정"
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
        _game.StageItemDropsGenerated -= ShowRewardPanel;
        _game.ShopVisitRequested -= OnShopVisitRequested;
        _game.ShopStockChanged -= OnShopStockChanged;
        _game.PlayerStateChanged -= RefreshStatusUi;
        _game.ItemAdded -= OnItemAdded;
    }
}
