using System.Collections.Generic;
using UnityEngine;

//전투 편성의 적 ID와 배치 순서 보관
public class EncounterData : GameDataEntry
{
    [Tooltip("EnemyPoint1~3 배치 순서, 0은 빈 자리")]
    [SerializeField] private int[] enemyIds = new int[3];

    [Header("전투 승리 코인 보상")]
    [Min(0)] [SerializeField] private int minRewardCoins = 12;
    [Min(0)] [SerializeField] private int maxRewardCoins = 18;

    public IReadOnlyList<int> EnemyIds => enemyIds;
    public int MinRewardCoins => minRewardCoins;
    public int MaxRewardCoins => maxRewardCoins;

#if UNITY_EDITOR
    //검증된 시트의 적 배치 정보 받음
    public void EditorApply(int[] newEnemyIds, int? newMinRewardCoins = null, int? newMaxRewardCoins = null)
    {
        enemyIds = (int[])newEnemyIds.Clone();
        if (newMinRewardCoins.HasValue) minRewardCoins = newMinRewardCoins.Value;
        if (newMaxRewardCoins.HasValue) maxRewardCoins = newMaxRewardCoins.Value;
        OnValidate();
    }

    //음수와 역전된 보상 범위 보정
    private void OnValidate()
    {
        minRewardCoins = Mathf.Max(0, minRewardCoins);
        maxRewardCoins = Mathf.Max(minRewardCoins, maxRewardCoins);
    }
#endif
}
