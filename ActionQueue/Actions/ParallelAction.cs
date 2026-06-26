using System.Threading;
using Cysharp.Threading.Tasks;

public class ParallelAction : IBattleAction
{
    private readonly IBattleAction[] _actions;

    public ParallelAction(params IBattleAction[] actions)
    {
        _actions = actions;
    }

    public async UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        UniTask[] tasks = new UniTask[_actions.Length];
        for (int i = 0; i < _actions.Length; i++)
        {
            tasks[i] = _actions[i].ExecuteAsync(ctx, token);
        }
        await UniTask.WhenAll(tasks);
    }
}
