using TMPro;
using UnityEngine;

/// <summary>어느 UI에서든 아이템 아이콘 호버 시 이름, 등급, 설명을 화면 안쪽에 표시합니다.</summary>
public sealed class ItemDescriptionView : MonoBehaviour
{
    [SerializeField] private TMP_Text _detailText;
    [SerializeField] private Vector2 _cursorOffset = new(18f, -18f);
    [Header("등급별 이름 색상")]
    [SerializeField] private Color _commonNameColor = new(0.38f, 0.72f, 1f);
    [SerializeField] private Color _rareNameColor = new(1f, 0.85f, 0.35f);
    [SerializeField] private Color _legendaryNameColor = new(1f, 0.38f, 0.38f);
    [SerializeField] private Color _monsterNameColor = new(0.88f, 0.88f, 0.88f);

    private Canvas _targetCanvas;
    private RectTransform _canvasRect;
    private RectTransform _descriptionRect;

    // 설명 패널이 아이콘의 포인터 이벤트를 가로채 호버가 반복되지 않도록 Raycast를 차단합니다.
    private void Awake()
    {
        CanvasGroup _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // 아이템 이름·등급·설명을 설정하고 마우스 위치의 화면 안쪽 사분면에 설명을 표시합니다.
    public void Show(ItemDefinition itemDefinition, Vector2 screenPosition)
    {
        if (itemDefinition == null) return;
        string _colorCode = ColorUtility.ToHtmlStringRGB(GetRarityNameColor(itemDefinition.Rarity));
        Show(itemDefinition.ItemName, $"[{GetRarityName(itemDefinition.Rarity)}]\n\n{itemDefinition.Description}", _colorCode, screenPosition);
    }

    // 몬스터 이름·설명·특성을 아이템 설명창과 같은 위치 규칙으로 표시합니다.
    public void Show(MonsterDefinition monsterDefinition, Vector2 screenPosition)
    {
        if (monsterDefinition == null) return;
        string _colorCode = ColorUtility.ToHtmlStringRGB(_monsterNameColor);
        Show(monsterDefinition.MonsterName, monsterDefinition.GetTooltipDescription(), _colorCode, screenPosition);
    }

    // 제목 색상과 본문을 받아 공용 설명창을 표시합니다.
    private void Show(string title, string body, string titleColorCode, Vector2 screenPosition)
    {
        if (_descriptionRect == null) _descriptionRect = transform as RectTransform;
        if (_targetCanvas == null) _targetCanvas = GetComponentInParent<Canvas>();
        if (_descriptionRect == null || _targetCanvas == null) return;

        _canvasRect = _targetCanvas.transform as RectTransform;
        if (_detailText != null)
        {
            _detailText.text = string.IsNullOrWhiteSpace(body)
                ? $"<color=#{titleColorCode}>{title}</color>"
                : $"<color=#{titleColorCode}>{title}</color>\n\n{body}";
        }

        // 설명 루트와 현재 설명을 마지막 형제로 보내 다른 패널보다 위에 표시합니다.
        transform.parent?.SetAsLastSibling();
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        UpdatePosition(screenPosition);
    }

    // 현재 마우스가 화면 중심의 어느 쪽에 있는지에 맞춰 설명이 화면 안쪽으로 펼쳐지게 배치합니다.
    public void UpdatePosition(Vector2 screenPosition)
    {
        if (!gameObject.activeSelf || _descriptionRect == null || _canvasRect == null) return;

        Camera _camera = _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _targetCanvas.worldCamera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPosition, _camera, out Vector2 _localPosition)) return;

        bool _isRightSide = screenPosition.x >= Screen.width * 0.5f;
        float _horizontalOffset = _isRightSide ? -Mathf.Abs(_cursorOffset.x) : Mathf.Abs(_cursorOffset.x);
        float _downwardOffset = -Mathf.Abs(_cursorOffset.y);
        float _upwardOffset = Mathf.Abs(_cursorOffset.y);
        float _bottomEdge = _canvasRect.rect.yMin;
        bool _wouldOverflowBottom = _localPosition.y + _downwardOffset - _descriptionRect.rect.height < _bottomEdge;

        _descriptionRect.pivot = new Vector2(_isRightSide ? 1f : 0f, _wouldOverflowBottom ? 0f : 1f);
        Vector2 _offset = new Vector2(_horizontalOffset, _wouldOverflowBottom ? _upwardOffset : _downwardOffset);
        _descriptionRect.anchoredPosition = _localPosition + _offset;
    }

    // 아이콘에서 마우스가 벗어나면 설명을 숨깁니다.
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 인스펙터 열거형 대신 사용자에게 표시할 한국어 등급 이름을 반환합니다.
    private string GetRarityName(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "일반",
            ItemRarity.Rare => "희귀",
            ItemRarity.Legendary => "전설",
            _ => "없음"
        };
    }

    // 아이템 이름에 적용할 등급별 밝은 색상을 반환합니다.
    private Color GetRarityNameColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => _commonNameColor,
            ItemRarity.Rare => _rareNameColor,
            ItemRarity.Legendary => _legendaryNameColor,
            _ => Color.white
        };
    }
}
