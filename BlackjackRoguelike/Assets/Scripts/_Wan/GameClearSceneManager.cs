using UnityEngine;
using UnityEngine.UI;

/// <summary>게임클리어 씬의 로비 복귀 버튼을 관리합니다.</summary>
public sealed class GameClearSceneManager : MonoBehaviour
{
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private string _lobbySceneName = "1_Lobby";

    // 씬에 배치한 로비 버튼을 공용 커튼 전환에 연결합니다.
    private void Start()
    {
        if (_lobbyButton != null) _lobbyButton.onClick.AddListener(ReturnToLobby);
    }

    // 로비 씬으로 돌아갑니다.
    private void ReturnToLobby()
    {
        SceneTransitionController.LoadScene(_lobbySceneName);
    }
}
