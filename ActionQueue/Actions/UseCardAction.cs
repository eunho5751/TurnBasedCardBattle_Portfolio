using System.Threading;
using Cysharp.Threading.Tasks;

public class UseCardAction : IBattleAction
{
    private readonly CardInstance _attacker;
    private readonly CardInstance _target;

    public UseCardAction(CardInstance attacker, CardInstance target)
    {
        _attacker = attacker;
        _target = target;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        if (!_attacker.IsAlive || !_target.IsAlive)
            return UniTask.CompletedTask;

        _attacker.Data.OnUsed(_attacker, _target, ctx);
        ctx.AppendAction(new PutDownCardAction(_attacker));
        ctx.AppendAction(new EndTurnAction());
        return UniTask.CompletedTask;
    }
}
