using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>SoundLibrary 설정을 실제 BGM·효과음 AudioSource로 재생하는 컴포넌트입니다.</summary>
[DisallowMultipleComponent]
public sealed class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    [SerializeField] private SoundLibrary _soundLibrary;
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private string _lobbySceneName = "1_Lobby";
    [SerializeField] private string _gameOverSceneName = "3_GameOver";
    [SerializeField] private string _gameClearSceneName = "4_GameClear";

    // 씬 전체에서 하나의 BGM·SFX 재생기만 유지합니다.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 씬 진입 시 로비·게임오버·클리어 BGM을 자동으로 전환합니다.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlaySceneBgm(SceneManager.GetActiveScene().name);
    }

    // 씬 이벤트 구독을 해제합니다.
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 유지 중인 컨트롤러가 파괴될 때만 정적 참조를 비웁니다.
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 지정한 효과음을 한 번 재생합니다. 비어 있는 클립은 조용히 무시합니다.
    public void Play(SoundCue cue)
    {
        if (_sfxSource == null) return;

        SoundEntry _entry = _soundLibrary?.GetSfxEntry(cue);
        if (_entry == null || _entry.Clip == null) return;
        _sfxSource.pitch = _entry.Pitch + Random.Range(-_entry.RandomPitchRange, _entry.RandomPitchRange);
        _sfxSource.PlayOneShot(_entry.Clip, _entry.Volume);
    }

    // 현재 스테이지 번호에 맞는 전투 BGM을 반복 재생합니다.
    public void PlayStageBgm(int stageNumber)
    {
        SoundBgmTrack _track = stageNumber switch
        {
            <= 3 => SoundBgmTrack.Stage1To3,
            <= 6 => SoundBgmTrack.Stage4To6,
            7 => SoundBgmTrack.Stage7,
            8 => SoundBgmTrack.Stage8,
            9 => SoundBgmTrack.Stage9,
            _ => SoundBgmTrack.Stage10
        };
        PlayBgm(_track);
    }

    // 상점 진입 시 상점 전용 BGM으로 전환합니다.
    public void PlayShopBgm()
    {
        PlayBgm(SoundBgmTrack.Shop);
    }

    // 선택한 BGM 항목을 반복 재생하도록 현재 라이브러리 설정을 반영합니다.
    public void PlayBgm(SoundBgmTrack track)
    {
        if (_bgmSource == null) return;
        SoundEntry _entry = _soundLibrary?.GetBgmEntry(track);
        if (_entry == null || _entry.Clip == null) return;
        if (_bgmSource.clip == _entry.Clip && _bgmSource.isPlaying) return;

        _bgmSource.clip = _entry.Clip;
        _bgmSource.volume = _entry.Volume;
        _bgmSource.pitch = _entry.Pitch;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    // 씬 이름으로 자동 전환 가능한 BGM만 처리합니다. 전투 스테이지 BGM은 BlackjackSystem이 지정합니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneBgm(scene.name);
    }

    // 로비·게임오버·게임클리어 씬에 맞는 BGM을 재생합니다.
    private void PlaySceneBgm(string sceneName)
    {
        if (sceneName == _lobbySceneName) PlayBgm(SoundBgmTrack.Lobby);
        else if (sceneName == _gameOverSceneName) PlayBgm(SoundBgmTrack.GameOver);
        else if (sceneName == _gameClearSceneName) PlayBgm(SoundBgmTrack.GameClear);
    }
}
