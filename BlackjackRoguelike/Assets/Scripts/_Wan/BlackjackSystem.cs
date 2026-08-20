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
    [SerializeField] private TMP_Text _passiveInventoryText;
    [SerializeField] private TMP_Text _activeInventoryText;
    [SerializeField] private Button _inventoryCloseButton;

    [Header("보상 UI")]
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private Button[] _passiveRewardButtons = new Button[2];
    [SerializeField] private Button[] _activeRewardButtons = new Button[3];
    [SerializeField] private Button _rewardConfirmButton;

    private readonly List<BlackjackCardView> _dealerCardViews = new();
    private readonly List<BlackjackCardView> _playerCardViews = new();

    private GameManager _game;
    private StageItemDropResult _pendingReward;
    private int _passivePickCount;
    private int _activePickCount;

    // 게임 매니저와 UI 버튼을 준비하고, 데이터베이스가 있으면 1스테이지 런을 시작합니다.
    private void Start()
    {
        _game = new GameManager();
        SubscribeGameEvents();
        BindUiButtons();

        if (_cardTemplate != null && _cardTemplate.gameObject.scene.IsValid()) _cardTemplate.gameObject.SetActive(false);
        if (_inventoryPanel != null) _inventoryPanel.SetActive(false);
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
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
        if (_rewardConfirmButton != null) _rewardConfirmButton.onClick.AddListener(CloseRewardPanel);

        BindRewardButtons(_passiveRewardButtons, ItemType.Passive);
        BindRewardButtons(_activeRewardButtons, ItemType.Active);
    }

    // 보상 버튼 배열에 각 슬롯의 선택 콜백을 등록합니다.
    private void BindRewardButtons(Button[] buttons, ItemType itemType)
    {
        for (int _index = 0; _index < buttons.Length; _index++)
        {
            int _buttonIndex = _index;
            if (buttons[_buttonIndex] != null)
            {
                buttons[_buttonIndex].onClick.AddListener(() => TrySelectReward(itemType, _buttonIndex));
            }
        }
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

    // 처치 보상 후보를 Inspector에 연결한 보상 패널에 표시합니다.
    private void ShowRewardPanel(StageItemDropResult itemDrops)
    {
        _pendingReward = itemDrops;
        _passivePickCount = 0;
        _activePickCount = 0;
        ConfigureRewardButtons(_passiveRewardButtons, itemDrops.PassiveCandidates);
        ConfigureRewardButtons(_activeRewardButtons, itemDrops.ActiveCandidates);
        if (_rewardPanel != null) _rewardPanel.SetActive(true);
    }

    // 후보 목록을 고정된 보상 버튼 슬롯에 표시합니다.
    private void ConfigureRewardButtons(Button[] buttons, IReadOnlyList<ItemDefinition> candidates)
    {
        for (int _index = 0; _index < buttons.Length; _index++)
        {
            Button _button = buttons[_index];
            if (_button == null) continue;

            bool _hasCandidate = _index < candidates.Count;
            _button.gameObject.SetActive(_hasCandidate);
            _button.interactable = _hasCandidate;
            if (_hasCandidate) SetButtonText(_button, $"{candidates[_index].ItemName}\n[{candidates[_index].Rarity}]");
        }
    }

    // 선택 가능 횟수가 남아 있으면 해당 후보를 인벤토리에 추가합니다.
    private void TrySelectReward(ItemType itemType, int index)
    {
        if (_pendingReward == null) return;
        IReadOnlyList<ItemDefinition> _candidates = itemType == ItemType.Passive
            ? _pendingReward.PassiveCandidates
            : _pendingReward.ActiveCandidates;
        if (index >= _candidates.Count) return;
        if (itemType == ItemType.Passive && _passivePickCount >= _pendingReward.PassivePickCount) return;
        if (itemType == ItemType.Active && _activePickCount >= _pendingReward.ActivePickCount) return;
        if (!_game.TryAddItemToInventory(_candidates[index])) return;

        Button[] _buttons = itemType == ItemType.Passive ? _passiveRewardButtons : _activeRewardButtons;
        if (_buttons[index] != null) _buttons[index].interactable = false;
        if (itemType == ItemType.Passive) _passivePickCount++;
        else _activePickCount++;
    }

    // 보상 창을 닫고 인벤토리 표기를 갱신합니다.
    private void CloseRewardPanel()
    {
        if (_rewardPanel != null) _rewardPanel.SetActive(false);
        _pendingReward = null;
        RefreshInventoryUi();
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
        if (_inventoryPanel.activeSelf) RefreshInventoryUi();
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
        _game.PlayerStateChanged -= RefreshStatusUi;
        _game.ItemAdded -= OnItemAdded;
    }
}
