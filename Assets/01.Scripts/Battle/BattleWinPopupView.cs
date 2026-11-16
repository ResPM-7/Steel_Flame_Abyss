using UnityEngine;
using DG.Tweening;

//승리 팝업의 보상과 다음 버튼 참조 제공
public class BattleWinPopupView : MonoBehaviour
{
    [Header("승리 팝업 연출")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform animatedPanel;
    [SerializeField, Min(0.01f)] private float showDuration = 0.25f;
    [SerializeField, Range(0.1f, 1f)] private float showScale = 0.94f;
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

    //퇴장 연출 없이 즉시 맵 이동 요청, 실패 시 입력 복구
    public void ContinueImmediately(System.Func<bool> tryContinue)
    {
        if (busy || !isActiveAndEnabled || tryContinue == null) return;
        busy = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        if (!tryContinue() && this != null && isActiveAndEnabled)
        {
            busy = false;
            canvasGroup.interactable = true;
        }
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
