using UnityEngine;
using UnityEngine.UI;

/// <summary>게임오버 씬의 재시작과 로비 복귀 버튼을 관리합니다.</summary>
public sealed class GameOverSceneManager : MonoBehaviour
{
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private string _gameSceneName = "2_Game";
    [SerializeField] private string _lobbySceneName = "1_Lobby";

    // 씬에 배치한 두 버튼을 공용 커튼 전환에 연결합니다.
    private void Start()
    {
        if (_restartButton != null) _restartButton.onClick.AddListener(RestartGame);
        if (_lobbyButton != null) _lobbyButton.onClick.AddListener(ReturnToLobby);
    }

    // 새 런을 시작할 게임 씬으로 이동합니다.
    private void RestartGame()
    {
        SceneTransitionController.LoadScene(_gameSceneName);
    }

    // 로비 씬으로 돌아갑니다.
    private void ReturnToLobby()
    {
        SceneTransitionController.LoadScene(_lobbySceneName);
    }
}
