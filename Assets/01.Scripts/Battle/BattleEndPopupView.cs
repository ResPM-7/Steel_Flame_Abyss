using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

//클리어·패배 팝업의 공통 연출과 메뉴 입력 연결
public class BattleEndPopupView : MonoBehaviour
{
    [SerializeField] private Button titleButton;
    [SerializeField] private Button exitButton;
    [Header("팝업 연출")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform animatedPanel;
    [SerializeField, Min(0.01f)] private float showDuration = 0.25f;
    [SerializeField, Range(0.1f, 1f)] private float showScale = 0.94f;
    private SceneMenuActions menu;
    private Sequence sequence;
    private Vector3 originalScale;
    private bool busy;

    //입력 그룹과 패널 기준 크기 보관
    protected virtual void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (animatedPanel == null) animatedPanel = transform as RectTransform;
        originalScale = animatedPanel != null ? animatedPanel.localScale : Vector3.one;
    }

    //공통 메뉴 받음과 중복 버튼 구독 방지
    public void Inject(SceneMenuActions actions)
    {
        if (actions == null) throw new System.ArgumentNullException(nameof(actions));
        menu = actions;
        if (titleButton != null)
        {
            titleButton.onClick.RemoveListener(ReturnToTitle);
            titleButton.onClick.AddListener(ReturnToTitle);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitGame);
            exitButton.onClick.AddListener(ExitGame);
        }
        if (!busy && canvasGroup != null) canvasGroup.interactable = true;
    }

    //활성화마다 등장 연출 시작
    protected virtual void OnEnable() => ShowAnimated();

    //페이드인과 기준 크기로 확대, 완료 후 입력 허용
    private void ShowAnimated()
    {
        sequence?.Kill();
        busy = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0f;
        if (animatedPanel != null) animatedPanel.localScale = originalScale * showScale;
        sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
        if (animatedPanel != null)
            sequence.Join(animatedPanel.DOScale(originalScale, showDuration).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            busy = false;
            canvasGroup.interactable = menu != null;
        });
    }

    //즉시 타이틀 이동 요청
    private void ReturnToTitle()
    {
        ContinueImmediately(() =>
        {
            if (menu == null) return false;
            menu.ReturnToTitle();
            return menu.IsLoading;
        });
    }

    //즉시 게임 종료
    private void ExitGame()
    {
        ContinueImmediately(() =>
        {
            if (menu == null) return false;
            menu.ExitGame();
            return true;
        });
    }

    //퇴장 연출 없이 메뉴 실행, 실패 시 입력 복구
    private void ContinueImmediately(System.Func<bool> action)
    {
        if (busy || !isActiveAndEnabled || menu == null || menu.IsLoading) return;
        busy = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        if (!action() && this != null && isActiveAndEnabled)
        {
            busy = false;
            canvasGroup.interactable = menu != null;
        }
    }

    //비활성화 시 트윈 정리와 기준 상태 복원
    protected virtual void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
        busy = false;
        if (animatedPanel != null) animatedPanel.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
        }
    }

    //팝업 제거 시 버튼 구독과 메뉴 참조 정리
    protected virtual void OnDestroy()
    {
        sequence?.Kill();
        if (titleButton != null) titleButton.onClick.RemoveListener(ReturnToTitle);
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
        menu = null;
    }
}
