using System;
using System.Collections.Generic;

//캐릭터 SO와 별도로 보관하는 플레이어의 전투 상태
public class PlayerBattleState
{
    public CharacterData Data { get; }
    public CombatantState Stats { get; }
    public int Energy { get; private set; }
    public event Action Changed;
    private readonly List<EffectSpec> turnStartEffects = new();
    public IReadOnlyList<EffectSpec> TurnStartEffects => turnStartEffects;

    //현재 전투에만 유지할 반복 효과 등록, 중복 사용 시 누적
    public void AddTurnStartEffect(EffectSpec effect)
    {
        if (!Stats.IsAlive || effect.Amount <= 0) return;
        turnStartEffects.Add(effect);
        Changed?.Invoke();
    }

    //동일한 반복 효과의 턴당 적용 수치 합산
    public int GetTurnStartAmount(EffectType type, TargetType target)
    {
        long total = 0;
        foreach (var effect in turnStartEffects)
            if (effect.Type == type && effect.Target == target) total += effect.Amount;
        return (int)Math.Min(int.MaxValue, total);
    }

    //캐릭터 데이터로 전투용 체력과 에너지를 초기화
    public PlayerBattleState(CharacterData data)
    {
        Data = data != null ? data : throw new ArgumentNullException(nameof(data));
        Stats = new CombatantState(data.MaxHealth);
        Energy = Math.Max(0, data.StartingEnergy);
    }

    //에너지가 충분하면 소비하고 성공 여부를 반환
    public bool TrySpendEnergy(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (!Stats.IsAlive || amount > Energy) return false;
        if (amount == 0) return true;
        Energy -= amount;
        Changed?.Invoke();
        return true;
    }

    //살아 있는 플레이어의 에너지를 증가
    public void GainEnergy(int amount)
    {
        if (amount <= 0 || !Stats.IsAlive) return;
        Energy += amount;
        Changed?.Invoke();
    }

    //살아 있는 플레이어의 에너지를 시작 수치로 복원
    public void ResetEnergy()
    {
        if (!Stats.IsAlive) return;
        Energy = Math.Max(0, Data.StartingEnergy);
        Changed?.Invoke();
    }
}
