using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>인벤토리, 상점, 보상에서 공통으로 사용하는 아이템 아이콘과 호버 입력 처리기입니다.</summary>
public sealed class ItemIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _footerText;
    [SerializeField] private GameObject _usedIndicator;
    [SerializeField] private GameObject _selectedIndicator;

    private ItemDefinition _itemDefinition;
    private ItemDescriptionView _itemDescriptionView;
    private UnityAction _clickAction;

    // 버튼이 연결돼 있으면 아이콘 클릭을 외부에서 전달한 동작으로 처리합니다.
    private void Awake()
    {
        if (_button != null) _button.onClick.AddListener(HandleClick);
    }

    // 아이콘 제거 시 등록한 클릭 콜백을 해제합니다.
    private void OnDestroy()
    {
        if (_button != null) _button.onClick.RemoveListener(HandleClick);
    }

    // 아이템 정의와 공용 설명 뷰를 연결해 어느 목록에서든 사용할 아이콘을 초기화합니다.
    public void SetItem(ItemDefinition itemDefinition, ItemDescriptionView itemDescriptionView, bool isUsed = false, UnityAction clickAction = null, string footerText = null)
    {
        _itemDefinition = itemDefinition;
        _itemDescriptionView = itemDescriptionView;
        if (_iconImage != null)
        {
            _iconImage.sprite = _itemDefinition?.Icon;
            _iconImage.enabled = _iconImage.sprite != null;
        }
        if (_usedIndicator != null) _usedIndicator.SetActive(isUsed);
        _clickAction = clickAction;
        if (_button != null) _button.interactable = _clickAction != null;
        SetFooterText(footerText);
        SetSelected(false);
    }

    // 상점 가격이나 판매 완료 상태처럼 아이콘 아래에 표시할 보조 문구를 설정합니다.
    public void SetFooterText(string footerText)
    {
        if (_footerText == null) return;
        bool _hasFooterText = !string.IsNullOrEmpty(footerText);
        _footerText.gameObject.SetActive(_hasFooterText);
        if (_hasFooterText) _footerText.text = footerText;
    }

    // 보상 선택 횟수 소진, 골드 부족처럼 아이콘의 클릭 가능 상태를 바꿉니다.
    public void SetInteractable(bool isInteractable)
    {
        if (_button != null) _button.interactable = isInteractable && _clickAction != null;
    }

    // 보상 후보로 선택됐는지 외곽선 등 선택 전용 UI에 표시합니다.
    public void SetSelected(bool isSelected)
    {
        if (_selectedIndicator != null) _selectedIndicator.SetActive(isSelected);
    }

    // 아이콘에 마우스를 올리면 공용 설명 프리팹을 표시합니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        _itemDescriptionView?.Show(_itemDefinition, eventData.position);
    }

    // 아이콘에서 마우스가 벗어나면 공용 설명을 숨깁니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        _itemDescriptionView?.Hide();
    }

    // Button 클릭 시 현재 아이콘에 할당된 보상 선택 또는 상점 구매 동작을 호출합니다.
    private void HandleClick()
    {
        _itemDescriptionView?.Hide();
        _clickAction?.Invoke();
    }
}
