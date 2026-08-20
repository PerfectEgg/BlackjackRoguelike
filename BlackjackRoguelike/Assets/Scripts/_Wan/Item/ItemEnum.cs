using UnityEngine;

// 아이템의 사용 방식입니다.
public enum ItemType
{
    [InspectorName("패시브")]
    Passive,
    [InspectorName("액티브")]
    Active
}

// 아이템의 희귀도입니다.
public enum ItemRarity
{
    [InspectorName("일반")]
    Common,
    [InspectorName("희귀")]
    Rare,
    [InspectorName("전설")]
    Legendary
}

// 아이템 효과가 확인되는 대표 시점입니다.
public enum ItemTrigger
{
    [InspectorName("없음")]
    None,
    [InspectorName("라운드 시작")]
    RoundStart,
    [InspectorName("드로우 전")]
    BeforeDraw,
    [InspectorName("드로우 후")]
    AfterDraw,
    [InspectorName("매치 종료")]
    OnMatchEnd,
    [InspectorName("피해를 받을 때")]
    OnDamageTaken,
    [InspectorName("피해를 준 뒤")]
    OnDamageDealt,
    [InspectorName("사용 즉시")]
    OnUse
}

// 아이템 효과의 큰 분류입니다. 세부 수치와 조건은 ItemEffectData에서 조합합니다.
public enum ItemEffectCategory
{
    [InspectorName("없음")]
    None,
    [InspectorName("공격력 계수")]
    AttackMultiplier,
    [InspectorName("최대 체력")]
    MaxHp,
    [InspectorName("체력 회복")]
    Heal,
    [InspectorName("체력 흡수")]
    LifeSteal,
    [InspectorName("배리어")]
    Barrier,
    [InspectorName("골드")]
    Gold,
    [InspectorName("상점")]
    Shop,
    [InspectorName("점수 보정")]
    ScoreAdjustment,
    [InspectorName("카드 조작")]
    CardManipulation,
    [InspectorName("드로우 카드 보정")]
    DrawCardModifier,
    [InspectorName("승패 결과")]
    MatchOutcome,
    [InspectorName("직접 피해")]
    DirectDamage,
    [InspectorName("스테이지 제어")]
    StageControl
}

// 효과 수치를 더할지, 곱할지, 고정할지 결정합니다.
public enum ItemEffectOperation
{
    [InspectorName("없음")]
    None,
    [InspectorName("더하기")]
    Add,
    [InspectorName("곱하기")]
    Multiply,
    [InspectorName("고정")]
    Set,
    [InspectorName("방지")]
    Prevent,
    [InspectorName("강제")]
    Force,
    [InspectorName("제거")]
    Remove,
    [InspectorName("교환")]
    Swap
}

// 효과가 유지될 범위입니다. None은 즉시 처리하거나 범위가 필요 없는 효과에 사용합니다.
public enum ItemEffectDurationScope
{
    [InspectorName("없음")]
    None,
    [InspectorName("영구")]
    Permanent,
    [InspectorName("이번 라운드")]
    ThisRound,
    [InspectorName("다음 라운드")]
    NextRound,
    [InspectorName("이번 스테이지")]
    ThisStage,
    [InspectorName("다음 스테이지")]
    NextStage
}

// 카드와 무관한 매치·전투 조건입니다. None이면 별도 조건 없이 발동합니다.
public enum ItemConditionType
{
    [InspectorName("없음")]
    None,
    [InspectorName("승리")]
    Victory,
    [InspectorName("패배")]
    Defeat,
    [InspectorName("무승부")]
    Draw,
    [InspectorName("블랙잭")]
    Blackjack,
    [InspectorName("더블 다운")]
    DoubleDown,
    [InspectorName("버스트")]
    Bust,
    [InspectorName("몬스터 버스트")]
    MonsterBust,
    [InspectorName("최소 드로우 수")]
    DrawCountAtLeast,
    [InspectorName("최소 골드")]
    GoldAtLeast,
    [InspectorName("스테이지 번호")]
    StageNumber
}

// 카드와 무관한 조건을 판정할 대상입니다. None이면 대상 구분이 필요 없습니다.
public enum ItemConditionTarget
{
    [InspectorName("없음")]
    None,
    [InspectorName("플레이어")]
    Player,
    [InspectorName("몬스터")]
    Monster,
    [InspectorName("승리자")]
    Winner,
    [InspectorName("패배자")]
    Loser
}

// 카드 보유 조건을 비교하는 방식입니다. None이면 카드 조건을 사용하지 않습니다.
public enum CardConditionMode
{
    [InspectorName("없음")]
    None,
    [InspectorName("숫자만 일치")]
    RankOnly,
    [InspectorName("무늬만 일치")]
    SuitOnly,
    [InspectorName("숫자와 무늬 일치")]
    ExactCard
}

// 카드 보유 조건을 확인할 패의 주체입니다.
public enum CardConditionTarget
{
    [InspectorName("없음")]
    None,
    [InspectorName("플레이어 패")]
    Player,
    [InspectorName("몬스터 패")]
    Monster,
    [InspectorName("승리자 패")]
    Winner,
    [InspectorName("패배자 패")]
    Loser
}

// 드로우 카드 보정 효과가 적용되는 시점과 범위입니다.
public enum DrawModifierScope
{
    None,
    NextDrawThisStage,
    NextDrawNextStage,
    AllDrawsThisRound,
    AllDrawsNextRound
}

// 드로우 카드 보정 효과가 적용될 대상을 지정합니다.
public enum DrawModifierTarget
{
    [InspectorName("없음")]
    None,
    [InspectorName("플레이어")]
    Player,
    [InspectorName("몬스터")]
    Monster
}

// 드로우 카드 보정의 구체적인 처리 방식입니다.
public enum DrawModifierMode
{
    [InspectorName("없음")]
    None,
    [InspectorName("특정 카드 강제")]
    SetSpecificCard,
    [InspectorName("J/Q/K 중 하나 강제")]
    SetFaceCard,
    [InspectorName("조커 강제")]
    SetJoker,
    [InspectorName("마지막 카드 복제")]
    DuplicateLastCard,
    [InspectorName("리롤")]
    Reroll,
    [InspectorName("카드 파괴 후 재드로우")]
    DestroyAndRedraw
}

// 패 교환·리롤·삭제 같은 카드 조작이 일어날 카드 영역입니다.
public enum CardManipulationScope
{
    [InspectorName("없음")]
    None,
    [InspectorName("초기 패")]
    InitialHand,
    [InspectorName("현재 패")]
    CurrentHand,
    [InspectorName("마지막 드로우 카드")]
    LastDrawnCard,
    [InspectorName("덱")]
    Deck
}

// 카드 조작의 구체적인 처리 방식입니다.
public enum CardManipulationMode
{
    [InspectorName("없음")]
    None,
    [InspectorName("초기 패 전체 교환")]
    SwapInitialHand,
    [InspectorName("무작위 카드 리롤")]
    RerollRandomCard,
    [InspectorName("마지막 카드 제거")]
    RemoveLastCard,
    [InspectorName("양측 패 교환")]
    SwapHands
}

// 여러 카드 조작 효과 중 한 가지만 사용할 수 있도록 묶는 제한 그룹입니다.
public enum CardManipulationExclusiveGroup
{
    [InspectorName("없음")]
    None,
    [InspectorName("라운드당 카드 조작 하나")]
    OneActionPerRound,
    [InspectorName("스테이지당 카드 조작 하나")]
    OneActionPerStage
}
