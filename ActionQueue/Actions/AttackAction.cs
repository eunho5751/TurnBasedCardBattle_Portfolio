using System.Threading;
using Cysharp.Threading.Tasks;

public class AttackCardAction : IBattleAction
{
    private readonly CardInstance _attacker;
    private readonly CardInstance _target;
    private readonly int _amount;
    private readonly bool _provokesCounter;
    private readonly VFXType _vfxType;
    private readonly SFXType _sfxType;

    public AttackCardAction(CardInstance attacker, CardInstance target, int amount, bool provokesCounter, VFXType vfxType = VFXType.BasicAttack, SFXType sfxType = SFXType.BasicAttack)
    {
        _attacker = attacker;
        _target = target;
        _amount = amount;
        _provokesCounter = provokesCounter;
        _vfxType = vfxType;
        _sfxType = sfxType;
    }

    public async UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        if (!_attacker.IsAlive || !_target.IsAlive)
            return;

        _target.CurrentHp -= _amount;
        if (!_target.IsAlive)
        {
            KillCardAction killAction = new(_target);
            ctx.PrependAction(killAction);    
        }
        else if (_provokesCounter)
        {
            AttackCardAction counterAction = new(_target, _attacker, _target.CurrentHp, false);
            ctx.PrependAction(counterAction);
        }

        var targetView = ctx.BoardView.GetCardView(_target);
        await targetView.TakeDamageAsync(_vfxType, _sfxType, token);
    }
}