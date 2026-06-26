using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DelegateAction : IBattleAction
{
    private readonly Func<BattleController, CancellationToken, UniTask> _action;

    public DelegateAction(Func<BattleController, CancellationToken, UniTask> action)
    {
        _action = action;
    }

    public DelegateAction(Action action)
    {
        _action = (ctx, token) =>
        {
            action();
            return UniTask.CompletedTask;
        };
    }

    public async UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        await _action(ctx, token);
    }
}