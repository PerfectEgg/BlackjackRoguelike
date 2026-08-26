using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>로비의 게임 시작과 공용 음소거 UI를 담당합니다.</summary>
public sealed class LobbyManager : MonoBehaviour
{
    [Header("씬 이동")]
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private string _gameSceneName = "2_Game";

    [Header("음소거 UI")]
    [SerializeField] private SoundController _soundController;
    [SerializeField] private Button _muteButton;
    [SerializeField] private Sprite _soundOnIcon;
    [SerializeField] private Sprite _soundMutedIcon;

    // 버튼 이벤트와 음소거 상태 변경 이벤트를 연결합니다.
    private void Start()
    {
        if (_soundController == null) _soundController = SoundController.Instance;
        if (_gameStartButton != null) _gameStartButton.onClick.AddListener(StartGame);
        if (_muteButton != null) _muteButton.onClick.AddListener(ToggleMute);
        if (_soundController != null)
        {
            _soundController.MuteChanged += RefreshMuteIcon;
        }
        RefreshMuteIcon(_soundController != null && _soundController.IsMuted);
    }

    // 지정한 게임 씬으로 이동합니다.
    private void StartGame()
    {
        SceneTransitionController.LoadScene(_gameSceneName);
    }

    // 공용 사운드 컨트롤러의 음소거 상태를 전환합니다.
    private void ToggleMute()
    {
        if (_soundController != null) _soundController.ToggleMute();
    }

    // 현재 상태에 맞는 음소거 또는 사운드 켜짐 아이콘을 표시합니다.
    private void RefreshMuteIcon(bool isMuted)
    {
        if (_muteButton != null && _muteButton.image != null) _muteButton.image.sprite = isMuted ? _soundMutedIcon : _soundOnIcon;
    }

    // 씬 전환 시 이벤트 구독을 해제합니다.
    private void OnDestroy()
    {
        if (_soundController != null) _soundController.MuteChanged -= RefreshMuteIcon;
    }
}
