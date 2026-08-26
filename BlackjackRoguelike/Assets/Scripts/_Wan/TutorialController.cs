using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>한 장의 이미지와 설명으로 구성된 도움말 페이지 데이터입니다.</summary>
[Serializable]
public sealed class TutorialPage
{
    [Tooltip("페이지 상단에 표시할 게임 화면 캡처 또는 안내 이미지입니다.")]
    public Sprite Image;
    [TextArea(2, 5)] [Tooltip("페이지 하단에 표시할 설명입니다.")]
    public string Description;
    [Tooltip("기본 Description Text 위치에서 이 페이지에만 적용할 이동량입니다.")]
    public Vector2 DescriptionPositionOffset;
}

/// <summary>튜토리얼 페이지 UI의 표시와 페이지 이동만 담당합니다.</summary>
public sealed class TutorialController : MonoBehaviour
{
    [Header("사운드")]
    [Tooltip("비어 있으면 실행 중인 공용 SoundController를 자동으로 사용합니다.")]
    [SerializeField] private SoundController _soundController;

    [Header("튜토리얼 UI")]
    [SerializeField] private GameObject _tutorialPanel;
    [SerializeField] private Image _pageImage;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _pageNumberText;
    [SerializeField] private Button _previousButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _closeButton;

    [Header("페이지 데이터")]
    [SerializeField] private TutorialPage[] _pages;

    private int _currentPageIndex;
    private Vector2 _defaultDescriptionPosition;

    public bool IsOpen => _tutorialPanel != null && _tutorialPanel.activeSelf;
    public bool HasPages => _pages != null && _pages.Length > 0;

    // 페이지 이동 버튼을 연결하고 시작 시 도움말 창을 닫습니다.
    private void Awake()
    {
        if (_soundController == null) _soundController = SoundController.Instance;
        if (_previousButton != null)
        {
            _previousButton.onClick.AddListener(ShowPreviousPage);
            _previousButton.onClick.AddListener(PlayCommonButtonSound);
        }
        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(ShowNextPage);
            _nextButton.onClick.AddListener(PlayCommonButtonSound);
        }
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
            _closeButton.onClick.AddListener(PlayCommonButtonSound);
        }
        if (_descriptionText != null) _defaultDescriptionPosition = _descriptionText.rectTransform.anchoredPosition;
        HideImmediately();
    }

    // 외부 시스템이 시간 정지와 배경 처리를 한 후 튜토리얼을 표시합니다.
    public void Open()
    {
        if (!HasPages || _tutorialPanel == null) return;
        _currentPageIndex = 0;
        _tutorialPanel.SetActive(true);
        _tutorialPanel.transform.SetAsLastSibling();
        RefreshPage();
    }

    // 현재 도움말을 닫고 호출자에게 닫힘 상태를 전달합니다.
    public void Close()
    {
        if (!IsOpen) return;
        _tutorialPanel.SetActive(false);
        Closed?.Invoke();
    }

    // 씬 시작 시나 전환 직전에 별도 이벤트 없이 즉시 숨깁니다.
    public void HideImmediately()
    {
        if (_tutorialPanel != null) _tutorialPanel.SetActive(false);
    }

    // 이전 페이지가 있으면 한 장 앞 페이지를 표시합니다.
    private void ShowPreviousPage()
    {
        if (!IsOpen || _currentPageIndex <= 0) return;
        _currentPageIndex--;
        RefreshPage();
    }

    // 다음 페이지가 있으면 한 장 뒤 페이지를 표시합니다.
    private void ShowNextPage()
    {
        if (!IsOpen || _currentPageIndex >= _pages.Length - 1) return;
        _currentPageIndex++;
        RefreshPage();
    }

    // 현재 페이지 데이터와 이전·다음 버튼 상태를 UI에 반영합니다.
    private void RefreshPage()
    {
        if (!HasPages) return;
        TutorialPage _page = _pages[_currentPageIndex];

        if (_pageImage != null)
        {
            _pageImage.sprite = _page.Image;
            _pageImage.enabled = _page.Image != null;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = _page.Description;
            _descriptionText.rectTransform.anchoredPosition = _defaultDescriptionPosition + _page.DescriptionPositionOffset;
        }
        if (_pageNumberText != null) _pageNumberText.text = $"{_currentPageIndex + 1} / {_pages.Length}";
        if (_previousButton != null) _previousButton.interactable = _currentPageIndex > 0;
        if (_nextButton != null) _nextButton.interactable = _currentPageIndex < _pages.Length - 1;
    }

    // 튜토리얼 페이지 버튼에 공용 버튼 클릭 효과음을 재생합니다.
    private void PlayCommonButtonSound()
    {
        if (_soundController == null) _soundController = SoundController.Instance;
        _soundController?.Play(SoundCue.CommonButton);
    }

    public event Action Closed;
}
