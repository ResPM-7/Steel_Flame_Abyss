using UnityEngine;
using DG.Tweening;

//승리 팝업의 보상과 다음 버튼 참조 제공
public class BattleWinPopupView : MonoBehaviour
{
    [Header("승리 팝업 연출")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform animatedPanel;
    [SerializeField, Min(0.01f)] private float showDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float hideDuration = 0.18f;
    [SerializeField, Range(0.1f, 1f)] private float showScale = 0.94f;
    [SerializeField, Range(0.1f, 1f)] private float hideScale = 0.97f;
    private Sequence animationSequence;
    private Vector3 originalScale;
    private bool initialized;
    private bool busy;
    [SerializeField] private CardRewardView rewardView;
    [SerializeField] private UnityEngine.UI.Button coinButton;
    [SerializeField] private TMPro.TMP_Text coinText;
    [SerializeField] private UnityEngine.UI.Button nextButton;
    public CardRewardView RewardView => rewardView;
    public UnityEngine.UI.Button CoinButton => coinButton;
    public TMPro.TMP_Text CoinText => coinText;
    public UnityEngine.UI.Button NextButton => nextButton;

    //팝업 입력 그룹과 패널 기준 크기 보관
    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (animatedPanel == null) animatedPanel = transform as RectTransform;
        originalScale = animatedPanel != null ? animatedPanel.localScale : Vector3.one;
        initialized = true;
    }

    //보상창 전체 페이드인과 소폭 확대
    private void OnEnable()
    {
        if (!initialized) return;
        ShowAnimated();
    }

    //새 등장 연출 전 기존 트윈 정리와 입력 차단
    private void ShowAnimated()
    {
        animationSequence?.Kill();
        busy = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;
        if (animatedPanel != null) animatedPanel.localScale = originalScale * showScale;
        animationSequence = DOTween.Sequence().SetUpdate(true);
        animationSequence.Append(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
        if (animatedPanel != null)
            animationSequence.Join(animatedPanel.DOScale(originalScale, showDuration).SetEase(Ease.OutCubic));
        animationSequence.OnComplete(() =>
        {
            busy = false;
            canvasGroup.interactable = true;
        });
    }

    //퇴장 완료 후 맵 이동 요청과 실패 시 보상창 복원
    public void HideAndContinue(System.Func<bool> tryContinue)
    {
        if (busy || !isActiveAndEnabled || tryContinue == null) return;
        busy = true;
        canvasGroup.interactable = false;
        //퇴장 중에도 뒤쪽 UI 입력 차단 유지
        canvasGroup.blocksRaycasts = true;
        animationSequence?.Kill();
        animationSequence = DOTween.Sequence().SetUpdate(true);
        animationSequence.Append(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad));
        if (animatedPanel != null)
            animationSequence.Join(animatedPanel.DOScale(originalScale * hideScale, hideDuration).SetEase(Ease.InQuad));
        animationSequence.OnComplete(() =>
        {
            if (!tryContinue()) ShowAnimated();
            //이동 성공 시 씬 종료까지 투명한 입력 차단 영역 유지
        });
    }

    //재사용과 씬 종료 시 트윈 중단 및 기준 상태 복원
    private void OnDisable()
    {
        animationSequence?.Kill();
        animationSequence = null;
        busy = false;
        if (animatedPanel != null) animatedPanel.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
        }
    }
}
