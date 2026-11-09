using UnityEngine;

//씬 BGM과 버튼 효과음의 인스펙터 연결
public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private string soundId;
    [SerializeField] private bool playOnStart;
    private SoundManager soundManager;

    //씬 또는 프리팹 생성자에게 공통 재생 관리자 받음
    public void Inject(SoundManager manager)
    {
        soundManager = manager;
    }

    //씬 시작 시 등록된 사운드 재생
    private void Start()
    {
        if (playOnStart) Play();
    }

    //버튼 이벤트에서 지정한 ID 재생
    public void Play() => PlaySound(soundId);

    //외부 이벤트에서 받은 ID 재생
    public async void PlaySound(string id)
    {
        SoundManager manager = soundManager;
        if (manager == null)
        {
            GameLogger.LogError("[사운드] SoundPlayer에 재생 관리자를 주입해 주세요", this);
            return;
        }
        //로드 대기 중 씬 종료 또는 비활성화 시 재생 취소
        int requestVersion = playbackVersion;
        if (!await manager.InitializeAsync()) return;
        if (this == null || !isActiveAndEnabled || manager == null ||
            manager != soundManager || requestVersion != playbackVersion) return;
        manager.Play(id);
    }

    private int playbackVersion;

    //비활성화 전 요청한 지연 재생 취소
    private void OnDisable() { playbackVersion++; }

    //반복 사운드 정지
    public void Stop()
    {
        playbackVersion++;
        if (soundManager != null) soundManager.Stop(soundId);
    }
}
