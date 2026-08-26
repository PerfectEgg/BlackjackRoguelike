using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>0_Conversion 씬의 좌우 커튼과 Additive 씬 전환 순서를 담당합니다.</summary>
public sealed class SceneTransitionController : MonoBehaviour
{
    private const string ConversionSceneName = "0_Conversion";
    private static bool _isTransitioning;

    [Header("커튼 UI")]
    [SerializeField] private RectTransform _leftCurtain;
    [SerializeField] private RectTransform _rightCurtain;
    [SerializeField] private Vector2 _leftClosedPosition = new(384f, 0f);
    [SerializeField] private Vector2 _rightClosedPosition = new(-384f, 0f);
    [Min(0f)] [SerializeField] private float _curtainTravelDistance = 768f;
    [Min(0f)] [SerializeField] private float _closeDuration = 0.45f;
    [Min(0f)] [SerializeField] private float _openDuration = 0.45f;
    [Header("로딩 표기")]
    [SerializeField] private TMP_Text _loadingText;
    [Min(0f)] [SerializeField] private float _loadingDisplayDelay = 1f;
    [Min(0.05f)] [SerializeField] private float _loadingDotInterval = 0.3f;

    // 외부 씬에서 호출하는 공용 전환 진입점입니다.
    public static void LoadScene(string targetSceneName)
    {
        if (_isTransitioning || string.IsNullOrWhiteSpace(targetSceneName)) return;
        GameObject _runnerObject = new("Scene Transition Runner");
        DontDestroyOnLoad(_runnerObject);
        _runnerObject.AddComponent<TransitionRunner>().Begin(targetSceneName);
    }

    // 전환 시작 시 Canvas가 다른 UI보다 위에 표시되도록 설정하고 커튼을 화면 밖에 둡니다.
    private void Awake()
    {
        Canvas _canvas = GetComponent<Canvas>();
        if (_canvas != null)
        {
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = short.MaxValue;
        }
        EnsureLoadingText();
        SetLoadingVisible(false);
        SetCurtainProgress(0f);
    }

    // 커튼 닫힘 정도를 0(열림)부터 1(완전히 닫힘)까지 적용합니다.
    private void SetCurtainProgress(float progress)
    {
        float _progress = Mathf.Clamp01(progress);
        if (_leftCurtain != null)
        {
            Vector2 _openPosition = _leftClosedPosition + Vector2.left * _curtainTravelDistance;
            _leftCurtain.anchoredPosition = Vector2.Lerp(_openPosition, _leftClosedPosition, _progress);
        }
        if (_rightCurtain != null)
        {
            Vector2 _openPosition = _rightClosedPosition + Vector2.right * _curtainTravelDistance;
            _rightCurtain.anchoredPosition = Vector2.Lerp(_openPosition, _rightClosedPosition, _progress);
        }
    }

    // 지정 시간에 맞춰 커튼을 부드럽게 열거나 닫습니다. 일시 정지 중에도 진행됩니다.
    private IEnumerator AnimateCurtains(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetCurtainProgress(to);
            yield break;
        }

        float _elapsed = 0f;
        while (_elapsed < duration)
        {
            _elapsed += Time.unscaledDeltaTime;
            float _progress = Mathf.SmoothStep(from, to, Mathf.Clamp01(_elapsed / duration));
            SetCurtainProgress(_progress);
            yield return null;
        }
        SetCurtainProgress(to);
    }

    // 프리팹에 별도 TMP를 연결하지 않았을 때도 전환 전용 로딩 문구를 생성합니다.
    private void EnsureLoadingText()
    {
        if (_loadingText != null) return;
        GameObject _textObject = new("Loading Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        _textObject.transform.SetParent(transform, false);
        RectTransform _rectTransform = _textObject.GetComponent<RectTransform>();
        _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        _rectTransform.pivot = new Vector2(0.5f, 0.5f);
        _rectTransform.anchoredPosition = Vector2.zero;
        _rectTransform.sizeDelta = new Vector2(400f, 80f);

        TextMeshProUGUI _text = _textObject.GetComponent<TextMeshProUGUI>();
        _text.font = TMP_Settings.defaultFontAsset;
        _text.fontSize = 36f;
        _text.alignment = TextAlignmentOptions.Center;
        _text.color = Color.white;
        _text.raycastTarget = false;
        _loadingText = _text;
    }

    // 로드 대기 시간이 기준을 넘겼을 때만 점 개수가 순환하는 로딩 문구를 표시합니다.
    private void UpdateLoadingText(float loadingElapsed)
    {
        if (_loadingText == null || loadingElapsed < _loadingDisplayDelay) return;
        int _dotCount = Mathf.FloorToInt((loadingElapsed - _loadingDisplayDelay) / _loadingDotInterval) % 3 + 1;
        _loadingText.text = "Loading" + new string('.', _dotCount);
        SetLoadingVisible(true);
    }

    // 로딩 문구의 표시 여부를 변경합니다.
    private void SetLoadingVisible(bool isVisible)
    {
        if (_loadingText != null) _loadingText.gameObject.SetActive(isVisible);
    }

    // 전환 씬을 Additive로 붙이고 목적지 교체 뒤 커튼을 여는 실행용 오브젝트입니다.
    private sealed class TransitionRunner : MonoBehaviour
    {
        public void Begin(string targetSceneName)
        {
            StartCoroutine(Run(targetSceneName));
        }

        private IEnumerator Run(string targetSceneName)
        {
            _isTransitioning = true;
            Scene _sourceScene = SceneManager.GetActiveScene();
            Scene _conversionScene = SceneManager.GetSceneByName(ConversionSceneName);
            if (!_conversionScene.isLoaded)
            {
                AsyncOperation _loadConversion = SceneManager.LoadSceneAsync(ConversionSceneName, LoadSceneMode.Additive);
                while (!_loadConversion.isDone) yield return null;
                _conversionScene = SceneManager.GetSceneByName(ConversionSceneName);
            }

            SceneTransitionController _controller = FindAnyObjectByType<SceneTransitionController>();
            if (_controller == null)
            {
                _isTransitioning = false;
                Destroy(gameObject);
                yield break;
            }

            yield return _controller.AnimateCurtains(0f, 1f, _controller._closeDuration);

            // 목적지 씬을 Additive로 열기 전에 이전 씬을 내립니다.
            // 각 일반 씬이 EventSystem을 하나씩 가지므로, 전환 중 중복 EventSystem 경고를 막습니다.
            if (_sourceScene.IsValid() && _sourceScene.isLoaded)
            {
                AsyncOperation _unloadSource = SceneManager.UnloadSceneAsync(_sourceScene);
                while (_unloadSource != null && !_unloadSource.isDone) yield return null;
            }

            AsyncOperation _loadTarget = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            float _loadingElapsed = 0f;
            while (!_loadTarget.isDone)
            {
                _loadingElapsed += Time.unscaledDeltaTime;
                _controller.UpdateLoadingText(_loadingElapsed);
                yield return null;
            }
            _controller.SetLoadingVisible(false);
            Scene _targetScene = SceneManager.GetSceneByName(targetSceneName);
            SceneManager.SetActiveScene(_targetScene);
            SoundController.Instance?.PlayInitialSceneBgm(targetSceneName);

            yield return _controller.AnimateCurtains(1f, 0f, _controller._openDuration);
            if (_conversionScene.IsValid() && _conversionScene.isLoaded) SceneManager.UnloadSceneAsync(_conversionScene);
            _isTransitioning = false;
            Destroy(gameObject);
        }
    }
}
