using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>중앙 몬스터 스프라이트에 마우스를 올리면 몬스터 설명과 특성을 표시합니다.</summary>
public sealed class MonsterHoverView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MonsterDefinition _monsterDefinition;
    private ItemDescriptionView _descriptionView;

    // 현재 전투 몬스터 데이터와 공용 설명창을 연결합니다.
    public void Setup(MonsterDefinition monsterDefinition, ItemDescriptionView descriptionView)
    {
        _monsterDefinition = monsterDefinition;
        _descriptionView = descriptionView;
    }

    // 포인터가 몬스터 스프라이트에 들어오면 공용 설명창을 표시합니다.
    public void OnPointerEnter(PointerEventData eventData)
    {
        _descriptionView?.Show(_monsterDefinition, eventData.position);
    }

    // 포인터가 몬스터 스프라이트에서 벗어나면 설명창을 숨깁니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        _descriptionView?.Hide();
    }
}
