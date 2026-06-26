using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ActionQueue : IDisposable
{
    private readonly BattleController _ctx;
    private readonly LinkedList<IBattleAction> _actions = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _isProcessing;

    public ActionQueue(BattleController ctx)
    {
        _ctx = ctx;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public void Prepend(IBattleAction action)
    {
        _actions.AddFirst(action);
        if (!_isProcessing)
        {
            ProcessAsync(_cts.Token).Forget();
        }
    }

    public void Append(IBattleAction action)
    {
        _actions.AddLast(action);
        if (!_isProcessing)
        {
            ProcessAsync(_cts.Token).Forget();
        }
    }

    public void Clear()
    {
        _actions.Clear();
    }

    private async UniTask ProcessAsync(CancellationToken token)
    {
        _isProcessing = true;
        while (!token.IsCancellationRequested && _actions.Count > 0)
        {
            IBattleAction action = _actions.First.Value;
            _actions.RemoveFirst();
            await action.ExecuteAsync(_ctx, token);
        }
        _isProcessing = false;
    }

    public bool IsProcessing => _isProcessing;
}
