using TMPro;
using UnityEngine;

//전투 상태 아이콘과 설명창 생성 및 재사용
public class BattleStatusBarView : MonoBehaviour
{
    private readonly BattleStatusItemView[] items = new BattleStatusItemView[8];
    private readonly StatusDefinition[] definitions = new StatusDefinition[8];
    private BattleStatusItemView itemPrefab;

    //효과별 표시 데이터 보관
    private class StatusDefinition
    {
        public string Title;
        public string Description;
        public Color Color;
        public Sprite Sprite;
        public bool DescriptionOnly;

        public StatusDefinition(string title, string description, Color color, Sprite sprite, bool descriptionOnly = false)
        {
            Title = title;
            Description = description;
            Color = color;
            Sprite = sprite;
            DescriptionOnly = descriptionOnly;
        }
    }
    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform tooltip;
    private TMP_Text tooltipText;
    private BattleStatusItemView tooltipOwner;
    private TMP_FontAsset font;

    //기존 상태 텍스트 위치에 공용 상태 표시 줄 배치
    public static BattleStatusBarView Create(TMP_Text source, BattleStatusIcons icons, BattleStatusItemView prefab)
    {
        if (prefab == null) return null;
        RectTransform rect = NewRect("StatusEffects", source.transform.parent);
        RectTransform original = source.rectTransform;
        rect.anchorMin = original.anchorMin;
        rect.anchorMax = original.anchorMax;
        rect.pivot = original.pivot;
        rect.sizeDelta = new Vector2(412f, 44f);
        rect.anchoredPosition = original.anchoredPosition;
        var layout = rect.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var bar = rect.gameObject.AddComponent<BattleStatusBarView>();
        bar.font = source.font;
        bar.itemPrefab = prefab;
        bar.CreateTooltip(source.GetComponentInParent<Canvas>().rootCanvas);
        bar.definitions[0] = new StatusDefinition("화상", "턴 종료 시 수치만큼 방어도를 무시하고 체력 피해.\n발동 후 화상 수치 1 감소.", new Color(0.65f, 0.18f, 0.08f), icons != null ? icons.Burn : null);
        bar.definitions[1] = new StatusDefinition("화상\n반격", "적의 공격을 받고 생존하면 공격자에게 수치만큼 화상 부여.\n방어도로 막아도 발동. 다음 자신의 턴 시작 시 해제.", new Color(0.48f, 0.24f, 0.08f), icons != null ? icons.BurnRetaliation : null);
        bar.definitions[2] = new StatusDefinition("약화", "공격 피해가 약화 수치만큼 감소.\n자신의 턴 종료 시 전체 해제.", new Color(0.25f, 0.35f, 0.25f), icons != null ? icons.Weak : null);
        bar.definitions[3] = new StatusDefinition("취약", "받는 공격 피해 50% 증가.\n수치는 남은 턴. 자신의 턴 종료 시 1감소.", new Color(0.38f, 0.22f, 0.48f), icons != null ? icons.Vulnerable : null);
        bar.definitions[4] = new StatusDefinition("정신\n분열", "수치가 적의 현재 체력 이상이면 처형.", new Color(0.24f, 0.16f, 0.42f), icons != null ? icons.MindFracture : null);
        bar.definitions[5] = new StatusDefinition("힘", "공격 피해에 힘 수치만큼 추가.", new Color(0.55f, 0.22f, 0.2f), icons != null ? icons.Strength : null);
        bar.definitions[6] = new StatusDefinition("힘\n증가", "매 턴 시작 시 힘을 {amount} 부여.\n이번 전투 동안 지속.", new Color(0.55f, 0.32f, 0.12f), icons != null ? icons.TurnStartStrength : null, true);
        bar.definitions[7] = new StatusDefinition("화상\n부여", "매 턴 시작 시 모든 생존 적에게 화상을 {amount} 부여.\n이번 전투 동안 지속.", new Color(0.65f, 0.25f, 0.08f), icons != null ? icons.TurnStartBurn : null, true);
        return bar;
    }

    //살아 있는 유닛의 활성 상태만 표시
    public void Refresh(CombatantState state, PlayerBattleState player = null)
    {
        RefreshItem(0, state.IsAlive ? state.Burn : 0);
        RefreshItem(1, state.IsAlive ? state.BurnRetaliation : 0);
        RefreshItem(2, state.IsAlive ? state.Weak : 0);
        RefreshItem(3, state.IsAlive ? state.Vulnerable : 0);
        RefreshItem(4, state.IsAlive ? state.MindFracture : 0);
        RefreshItem(5, state.IsAlive ? state.Strength : 0);
        RefreshItem(6, state.IsAlive && player != null ? player.GetTurnStartAmount(EffectType.Strength, TargetType.Self) : 0);
        RefreshItem(7, state.IsAlive && player != null ? player.GetTurnStartAmount(EffectType.Burn, TargetType.AllEnemies) : 0);
    }

    //사용 시 최초 생성, 종료 후 비활성 상태로 보관
    private void RefreshItem(int index, int amount)
    {
        BattleStatusItemView item = items[index];
        if (item == null)
        {
            if (amount <= 0) return;
            StatusDefinition definition = definitions[index];
            item = Instantiate(itemPrefab, transform, false);
            item.name = definition.Title.Replace("\n", "");
            item.gameObject.SetActive(false);
            item.Initialize(this, definition.Title, definition.Description,
                definition.Color, definition.Sprite, definition.DescriptionOnly);
            items[index] = item;
        }
        item.Refresh(amount);
    }

    //다른 UI 입력을 가리지 않는 설명창 생성
    private void CreateTooltip(Canvas canvas)
    {
        tooltip = NewRect("StatusTooltip", canvas.transform);
        tooltip.pivot = new Vector2(0, 1);
        tooltip.sizeDelta = new Vector2(430, 150);
        var overlay = tooltip.gameObject.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingLayerID = canvas.sortingLayerID;
        //손패 Canvas_Overlay(20) 앞, 팝업 Canvas_Popup(30) 뒤
        overlay.sortingOrder = 25;
        var image = tooltip.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.08f, 0.09f, 0.13f, 0.98f);
        image.raycastTarget = false;
        tooltipText = NewText(tooltip, 21);
        tooltipText.alignment = TextAlignmentOptions.TopLeft;
        tooltipText.margin = new Vector4(16, 12, 16, 12);
        tooltip.gameObject.SetActive(false);
    }

    //아이콘 아래 설명 표시 및 화면 경계 보정
    public void ShowTooltip(BattleStatusItemView item, string title, string description, string amount)
    {
        tooltipOwner = item;
        string details = description.Replace("{amount}", amount);
        tooltipText.text = string.IsNullOrEmpty(title)
            ? details
            : $"<size=26>{title}  {amount}</size>\n{details}";
        tooltip.gameObject.SetActive(true);
        tooltip.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        RectTransform parent = (RectTransform)tooltip.parent;
        Canvas canvas = parent.GetComponent<Canvas>();
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        ((RectTransform)item.transform).GetWorldCorners(corners);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, camera, out Vector2 position);
        position.y -= 8;
        Rect bounds = parent.rect;
        if (position.y - tooltip.rect.height < bounds.yMin)
        {
            screen = RectTransformUtility.WorldToScreenPoint(camera, corners[1]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, camera, out position);
            position.y += tooltip.rect.height + 8;
        }
        position.x = Mathf.Clamp(position.x, bounds.xMin, Mathf.Max(bounds.xMin, bounds.xMax - tooltip.rect.width));
        position.y = Mathf.Clamp(position.y, Mathf.Min(bounds.yMax, bounds.yMin + tooltip.rect.height), bounds.yMax);
        tooltip.localPosition = new Vector3(position.x, position.y, 0);
    }

    //해당 아이콘이 소유한 설명만 숨김
    public void HideTooltip(BattleStatusItemView item)
    {
        if (tooltipOwner == item) HideTooltip();
    }

    //화면 종료 시 설명 숨김
    public void HideTooltip()
    {
        tooltipOwner = null;
        if (tooltip != null) tooltip.gameObject.SetActive(false);
    }

    //바 비활성화 시 외부 설명창 정리
    private void OnDisable() => HideTooltip();

    //유닛 제거 시 캔버스 아래 설명창 제거
    private void OnDestroy()
    {
        if (tooltip != null) Destroy(tooltip.gameObject);
    }

    //부모 기준 UI 사각형 생성
    private static RectTransform NewRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    //기존 전투 폰트로 비상호작용 텍스트 생성
    private TMP_Text NewText(RectTransform parent, float size)
    {
        RectTransform rect = NewRect("Text", parent);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }
}
