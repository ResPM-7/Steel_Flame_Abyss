using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

//유물 아이콘 표시와 포인터 진입·이탈 전달
public class RelicIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnityEngine.UI.Image icon;
    [SerializeField] private TMP_Text fallbackLabel;
    private RelicData data;
    private RelicPanelView owner;

    //아이콘 우선 표시, 미등록 유물은 흰 배경과 이름 표시
    public void Bind(RelicData value, RelicPanelView panel)
    {
        data = value;
        owner = panel;
        icon.sprite = value.Icon;
        icon.overrideSprite = null;
        icon.type = UnityEngine.UI.Image.Type.Simple;
        icon.preserveAspect = true;
        icon.color = Color.white;
        fallbackLabel.text = value.DisplayName;
        fallbackLabel.gameObject.SetActive(value.Icon == null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null) owner.ShowTooltip(this, data);
    }

    //풀 반환 전에 이전 유물과 패널 참조 제거
    public void Unbind()
    {
        owner?.HideTooltip(this);
        owner = null;
        data = null;
        icon.sprite = null;
        icon.overrideSprite = null;
        fallbackLabel.text = string.Empty;
    }

    public void OnPointerExit(PointerEventData eventData) => owner?.HideTooltip(this);
    private void OnDisable() => owner?.HideTooltip(this);
}
