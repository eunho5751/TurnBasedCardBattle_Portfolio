using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DelayAction : IBattleAction
{
    private readonly float _seconds;

    public DelayAction(float seconds)
    {
        _seconds = seconds;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        return UniTask.Delay(TimeSpan.FromSeconds(_seconds), cancellationToken: token);
    }
}
