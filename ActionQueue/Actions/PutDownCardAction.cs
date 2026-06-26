using System.Threading;
using Cysharp.Threading.Tasks;

public class PutDownCardAction : IBattleAction
{
    private readonly CardInstance _target;

    public PutDownCardAction(CardInstance target)
    {
        _target = target;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        if (!_target.IsAlive)
            return UniTask.CompletedTask;

        var cardView = ctx.BoardView.GetCardView(_target);
        cardView.SetPickedUp(false);
        return UniTask.CompletedTask;
    }
}
