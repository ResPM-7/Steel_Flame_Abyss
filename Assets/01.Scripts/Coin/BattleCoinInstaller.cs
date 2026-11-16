using TMPro;
using UnityEngine;
using UnityEngine.UI;

//공통 코인 표시에 전투 승리 보상 추가
public class BattleCoinInstaller : CoinInstaller
{
    [SerializeField] private BattleParticipants participants;
    [SerializeField] private Button rewardButton;
    [SerializeField] private TMP_Text rewardText;
    private CoinReward reward;
    private EnemyBattleState rewardEnemy;
    private readonly System.Random random = new System.Random();

    //공통 화면과 전투 보상 연결 확인
    protected override bool ValidateBindings()
    {
        if (!base.ValidateBindings()) return false;
        if (participants != null) return true;
        GameLogger.LogError("[전투 코인] Participants를 연결해 주세요", this);
        return false;
    }

    //전투 결과와 보상 버튼 구독
    protected override void BindAdditional()
    {
        participants.ResultChanged += RefreshReward;
        if (rewardButton != null) rewardButton.onClick.AddListener(ClaimReward);
        RefreshReward(participants.Result);
    }

    //동적 승리 팝업의 코인 보상 화면 연결
    public void BindRewardView(UnityEngine.UI.Button button, TMP_Text text)
    {
        if (rewardButton != null) rewardButton.onClick.RemoveListener(ClaimReward);
        rewardButton = button;
        rewardText = text;
        rewardButton.onClick.AddListener(ClaimReward);
        RefreshReward(participants.Result);
    }

    //승리한 적의 보상 범위에서 일회 지급 금액 결정
    private void RefreshReward(BattleResult result)
    {
        if (rewardEnemy != participants.Enemy)
        {
            rewardEnemy = participants.Enemy;
            reward = null;
        }
        if (result == BattleResult.Victory && rewardEnemy != null && reward == null)
        {
            //편성 보상 범위에서 전투당 한 번 추첨
            long total = 0;
            if (participants.Encounter != null)
            {
                total = RollCoins(participants.Encounter.MinRewardCoins, participants.Encounter.MaxRewardCoins);
            }
            else
            {
                //편성 없는 단일 적 테스트의 기존 보상 유지
                foreach (var enemy in participants.Enemies)
                    total += RollCoins(enemy.Data.MinGold, enemy.Data.MaxGold);
            }
            reward = new CoinReward((int)System.Math.Min(int.MaxValue, total));
        }
        if (rewardButton != null) rewardButton.interactable = result == BattleResult.Victory && reward != null &&
            !reward.IsClaimed && reward.Amount > 0;
        if (rewardText != null)
            rewardText.text = reward == null ? "Coin" :
                reward.IsClaimed ? "수령 완료" : $"Coin +{reward.Amount}";
    }

    //최소와 최대를 포함한 코인 추첨
    private int RollCoins(int minimum, int maximum)
    {
        int min = Mathf.Max(0, minimum);
        int max = Mathf.Max(min, maximum);
        return (int)(min + (long)(random.NextDouble() * ((long)max - min + 1)));
    }

    //승리 보상 요청 확인과 보유 코인 추가
    private void ClaimReward()
    {
        if (participants.Result != BattleResult.Victory || reward == null) return;
        reward.TryClaim(Model);
        RefreshReward(participants.Result);
    }

    //전투 결과와 버튼 및 공통 코인 구독 해제
    protected override void ReleaseBindings()
    {
        if (participants != null) participants.ResultChanged -= RefreshReward;
        if (rewardButton != null) rewardButton.onClick.RemoveListener(ClaimReward);
        base.ReleaseBindings();
    }
}
