using System.Threading;
using Cysharp.Threading.Tasks;

public class HealCardAction : IBattleAction
{
    private readonly CardInstance _caster;
    private readonly CardInstance _target;
    private readonly int _amount;

    public HealCardAction(CardInstance caster, CardInstance target, int amount)
    {
        _caster = caster;
        _target = target;
        _amount = amount;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        if (!_caster.IsAlive || !_target.IsAlive)
            return UniTask.CompletedTask;

        _target.CurrentHp += _amount;
        var targetView = ctx.BoardView.GetCardView(_target);
        return targetView.HealAsync(token);;
    }
}