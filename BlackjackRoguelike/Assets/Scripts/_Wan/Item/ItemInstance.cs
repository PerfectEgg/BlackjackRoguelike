/// <summary>플레이 중 보유한 아이템의 개별 상태를 관리합니다.</summary>
public class ItemInstance
{
    // 공유되는 아이템 정의 에셋입니다.
    public ItemDefinition Definition;
    // 중첩 횟수입니다.
    public int StackCount;
    // 액티브 아이템의 사용 여부입니다.
    public bool IsUsed;
}
