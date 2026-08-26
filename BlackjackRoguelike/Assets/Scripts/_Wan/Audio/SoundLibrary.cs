using UnityEngine;

/// <summary>게임에서 공통으로 사용하는 사운드 종류입니다.</summary>
public enum SoundBgmTrack
{
    Lobby,
    Stage1To3,
    Stage4To6,
    Stage7,
    Stage8,
    Stage9,
    Stage10,
    Shop,
    GameOver,
    GameClear
}

/// <summary>게임에서 공통으로 사용하는 효과음 종류입니다.</summary>
public enum SoundCue
{
    StageClear,
    CardDraw,
    InitialHandSwap,
    BlackjackDamage,
    BlackjackHit,
    NormalDamage,
    DoubleDown,
    NormalHit,
    BarrierHit,
    ItemUse,
    CommonButton,
    InventoryToggle,
    ShopExit,
    ShopRefresh,
    RewardSelect,
    RewardCancel
}

/// <summary>클립과 개별 볼륨·피치를 묶어 Inspector에서 조정할 수 있는 사운드 데이터입니다.</summary>
[System.Serializable]
public sealed class SoundEntry
{
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(0.1f, 3f)] public float Pitch = 1f;
    [Range(0f, 0.5f)] public float RandomPitchRange;
}

/// <summary>블랙잭 런에서 재생할 BGM과 효과음을 한 에셋에서 관리합니다.</summary>
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Blackjack/Sound Library")]
public sealed class SoundLibrary : ScriptableObject
{
    [Header("BGM")]
    public SoundEntry LobbyBgm = new();
    public SoundEntry Stage1To3Bgm = new();
    public SoundEntry Stage4To6Bgm = new();
    public SoundEntry Stage7Bgm = new();
    public SoundEntry Stage8Bgm = new();
    public SoundEntry Stage9Bgm = new();
    public SoundEntry Stage10Bgm = new();
    public SoundEntry ShopBgm = new();
    public SoundEntry GameOverBgm = new();
    public SoundEntry GameClearBgm = new();

    [Header("전투")]
    public SoundEntry StageClear = new();
    public SoundEntry CardDraw = new();
    public SoundEntry InitialHandSwap = new();
    public SoundEntry BlackjackDamage = new();
    public SoundEntry BlackjackHit = new();
    public SoundEntry NormalDamage = new();
    public SoundEntry DoubleDown = new();
    public SoundEntry NormalHit = new();
    public SoundEntry BarrierHit = new();

    [Header("UI와 상점")]
    public SoundEntry CommonButton = new();
    public SoundEntry InventoryToggle = new();
    public SoundEntry ShopExit = new();
    public SoundEntry ShopRefresh = new();
    public SoundEntry RewardSelect = new();
    public SoundEntry RewardCancel = new();
    public SoundEntry ItemUse = new();

    // 요청한 BGM 종류에 맞는 설정을 반환합니다.
    public SoundEntry GetBgmEntry(SoundBgmTrack track)
    {
        return track switch
        {
            SoundBgmTrack.Lobby => LobbyBgm,
            SoundBgmTrack.Stage1To3 => Stage1To3Bgm,
            SoundBgmTrack.Stage4To6 => Stage4To6Bgm,
            SoundBgmTrack.Stage7 => Stage7Bgm,
            SoundBgmTrack.Stage8 => Stage8Bgm,
            SoundBgmTrack.Stage9 => Stage9Bgm,
            SoundBgmTrack.Stage10 => Stage10Bgm,
            SoundBgmTrack.Shop => ShopBgm,
            SoundBgmTrack.GameOver => GameOverBgm,
            SoundBgmTrack.GameClear => GameClearBgm,
            _ => null
        };
    }

    // 요청한 효과음 종류에 맞는 설정을 반환합니다.
    public SoundEntry GetSfxEntry(SoundCue cue)
    {
        return cue switch
        {
            SoundCue.StageClear => StageClear,
            SoundCue.CardDraw => CardDraw,
            SoundCue.InitialHandSwap => InitialHandSwap,
            SoundCue.BlackjackDamage => BlackjackDamage,
            SoundCue.BlackjackHit => BlackjackHit,
            SoundCue.NormalDamage => NormalDamage,
            SoundCue.DoubleDown => DoubleDown,
            SoundCue.NormalHit => NormalHit,
            SoundCue.BarrierHit => BarrierHit,
            SoundCue.CommonButton => CommonButton,
            SoundCue.InventoryToggle => InventoryToggle,
            SoundCue.ShopExit => ShopExit,
            SoundCue.ShopRefresh => ShopRefresh,
            SoundCue.RewardSelect => RewardSelect,
            SoundCue.RewardCancel => RewardCancel,
            SoundCue.ItemUse => ItemUse,
            _ => null
        };
    }
}
