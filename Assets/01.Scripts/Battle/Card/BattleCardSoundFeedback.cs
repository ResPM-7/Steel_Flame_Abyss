using UnityEngine;

//카드 사용 완료 시 캐릭터 공격음과 방어음 재생
public class BattleCardSoundFeedback : MonoBehaviour
{
    [System.Serializable]
    public class AttackSound
    {
        public CharacterClass character;
        public string soundId;
    }

    [SerializeField] private string shieldSoundId = "Shield";
    [SerializeField] private AttackSound[] attackSounds =
    {
        new AttackSound { character = CharacterClass.Warrior, soundId = "SwordAttack" },
        new AttackSound { character = CharacterClass.FireMage, soundId = "FireAttack" },
        new AttackSound { character = CharacterClass.Warlock, soundId = "VoidAttack" }
    };
    private CardPlayController controller;
    private SoundManager soundManager;
    private int playbackVersion;

    //전투 카드 사용 알림과 세션 사운드 관리자 연결
    public void Inject(CardPlayController cardController, SoundManager manager)
    {
        Unsubscribe();
        controller = cardController;
        soundManager = manager;
        playbackVersion++;
        if (isActiveAndEnabled) Subscribe();
    }

    //활성화 시 카드 사용 알림 구독
    private void OnEnable() => Subscribe();

    //비활성화 시 구독 해제와 대기 중 재생 취소
    private void OnDisable()
    {
        playbackVersion++;
        Unsubscribe();
    }

    //중복 없는 사용 완료 구독
    private void Subscribe()
    {
        if (controller == null) return;
        controller.CardPlayed -= OnCardPlayed;
        controller.CardPlayed += OnCardPlayed;
    }

    //이전 카드 컨트롤러 구독 해제
    private void Unsubscribe()
    {
        if (controller != null) controller.CardPlayed -= OnCardPlayed;
    }

    //광역과 연타도 카드 한 장 기준으로 각 소리 한 번 재생
    private async void OnCardPlayed(CardInstance card, CharacterClass character, bool gainedBlock)
    {
        string attackId = card.Data.CardType == CardType.Attack ? FindAttackSound(character) : null;
        string shieldId = gainedBlock ? shieldSoundId : null;
        if (string.IsNullOrWhiteSpace(attackId) && string.IsNullOrWhiteSpace(shieldId)) return;
        SoundManager manager = soundManager;
        if (manager == null) return;
        int version = playbackVersion;
        if (!await manager.InitializeAsync()) return;
        if (this == null || !isActiveAndEnabled || version != playbackVersion || manager == null) return;
        if (!string.IsNullOrWhiteSpace(attackId)) manager.Play(attackId);
        if (!string.IsNullOrWhiteSpace(shieldId)) manager.Play(shieldId);
    }

    //카드 소유 직업 대신 실제 플레이 중인 캐릭터 기준 조회
    private string FindAttackSound(CharacterClass character)
    {
        if (attackSounds == null) return null;
        foreach (AttackSound entry in attackSounds)
            if (entry != null && entry.character == character) return entry.soundId;
        return null;
    }
}
