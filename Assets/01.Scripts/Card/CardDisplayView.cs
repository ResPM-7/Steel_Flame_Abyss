using TMPro;
using UnityEngine;

//모든 카드의 공통 텍스트와 일러스트 및 희귀도 테두리 표시
public class CardDisplayView : MonoBehaviour
{
    [SerializeField] protected TMP_Text nameText;
    [SerializeField] protected TMP_Text costText;
    [SerializeField] protected TMP_Text descriptionText;
    [SerializeField] private UnityEngine.UI.Image cardOutline;
    [SerializeField] private CardRarityColors rarityColors;
    [Header("카드 공통 디자인")]
    [SerializeField] private UnityEngine.UI.Image artworkWell;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private Color steelColor = new Color(0.38f, 0.63f, 0.82f);
    [SerializeField] private Color flameColor = new Color(0.94f, 0.43f, 0.18f);
    [SerializeField] private Color abyssColor = new Color(0.66f, 0.42f, 0.85f);
    [SerializeField] private Color publicColor = new Color(0.72f, 0.66f, 0.44f);

    public virtual bool IsConfigured => nameText != null && costText != null &&
        descriptionText != null && cardOutline != null;

    //카드 정보와 희귀도 색상을 공통 화면에 반영
    protected void SetDisplay(CardData data, int cost, string description)
    {
        nameText.text = data.DisplayName;
        costText.text = cost.ToString();
        descriptionText.text = description;
        ApplyDesign(data);
        if (cardOutline == null)
        {
            GameLogger.LogError("[카드 표시] Card Outline에 테두리 Image를 연결해 주세요", this);
            return;
        }
        cardOutline.enabled = true;
        cardOutline.raycastTarget = false;
    }

    //직업별 프레임 색상과 카드 종류 및 일러스트 표시
    private void ApplyDesign(CardData data)
    {
        Color accent = data.Owner switch
        {
            CharacterClass.Warrior => steelColor,
            CharacterClass.FireMage => flameColor,
            CharacterClass.Warlock => abyssColor,
            _ => publicColor
        };
        //일반 등급은 직업·공용 색상, 최고 등급은 SO 색상 또는 기본 금색
        Color emphasis = data.Rarity == DataRarity.Special
            ? (rarityColors != null ? rarityColors.GetColor(data.Rarity) : new Color(0.88f, 0.69f, 0.3f))
            : accent;
        if (cardOutline != null) cardOutline.color = emphasis;
        ApplyArtwork(data.Artwork, accent);
        if (typeText != null)
        {
            typeText.color = accent;
            typeText.text = data.CardType switch
            {
                CardType.Attack => "공격",
                CardType.Skill => "스킬",
                CardType.Power => "파워",
                CardType.Status => "상태",
                _ => data.CardType.ToString()
            };
        }
    }

    //등록된 일러스트의 원본 색상과 비율 유지, 미등록 시 빈 배경 표시
    private void ApplyArtwork(Sprite artwork, Color accent)
    {
        if (artworkWell == null) return;
        artworkWell.overrideSprite = null;
        artworkWell.sprite = artwork;
        artworkWell.type = UnityEngine.UI.Image.Type.Simple;
        artworkWell.preserveAspect = artwork != null;
        artworkWell.color = artwork != null
            ? Color.white
            : Color.Lerp(new Color(0.055f, 0.06f, 0.08f), accent, 0.1f);
        artworkWell.raycastTarget = false;
        artworkWell.enabled = true;
    }

}
