using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

//상태 이름과 수치 표시 및 마우스 설명 연결
public class BattleStatusItemView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BattleStatusBarView owner;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private UnityEngine.UI.Image icon;
    [SerializeField] private TMP_Text fallbackName;
    private string title;
    private string description;
    private bool hovered;

    //상태 표시 데이터 연결
    public void Initialize(BattleStatusBarView bar, string name, string details, Color fallbackColor, Sprite sprite, bool descriptionOnly)
    {
        owner = bar;
        title = descriptionOnly ? string.Empty : name.Replace("\n", " ");
        description = details;
        if (icon != null)
        {
            icon.overrideSprite = null;
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.color = sprite != null ? Color.white : fallbackColor;
            icon.raycastTarget = true;
        }
        if (fallbackName != null)
        {
            fallbackName.text = name;
            fallbackName.gameObject.SetActive(sprite == null);
            fallbackName.raycastTarget = false;
        }
        if (amountText != null)
        {
            amountText.textWrappingMode = TextWrappingModes.NoWrap;
            amountText.enableAutoSizing = true;
            amountText.fontSizeMax = amountText.fontSize;
            amountText.fontSizeMin = 10f;
            amountText.raycastTarget = false;
        }
        else GameLogger.LogError("[상태 아이콘] 수치 텍스트를 연결해 주세요", this);
    }

    //새 효과는 표시 줄 끝에 추가, 중첩 수치 변경 시 순서 유지
    public void Refresh(int amount)
    {
        bool visible = amount > 0;
        if (visible && !gameObject.activeSelf) transform.SetAsLastSibling();
        if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        if (amountText != null) amountText.text = amount.ToString();
        if (hovered && amount > 0) Show();
    }

    //아이콘 진입 시 설명 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        Show();
    }

    //아이콘 이탈 시 설명 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        owner.HideTooltip(this);
    }

    //현재 수치를 포함한 설명 전달
    private void Show()
    {
        if (owner != null && amountText != null)
            owner.ShowTooltip(this, title, description, amountText.text);
    }

    //효과 종료 및 화면 비활성화 시 설명 정리
    private void OnDisable()
    {
        hovered = false;
        if (owner != null) owner.HideTooltip(this);
    }
}
