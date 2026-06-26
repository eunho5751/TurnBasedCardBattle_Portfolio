using System.Threading;
using Cysharp.Threading.Tasks;

public class KillCardAction : IBattleAction
{
    private readonly CardInstance _target;

    public KillCardAction(CardInstance target)
    {
        _target = target;
    }

    public async UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        var side = ctx.Board.GetSide(_target);
        side.RemoveCard(_target);

        var sideView = ctx.BoardView.GetSideView(_target);
        await sideView.KillCardAsync(_target, token);

        if (side.IsDefeated)
        {
            PlayerSide winner = _target.OwnerSide == PlayerSide.Player ? PlayerSide.Enemy : PlayerSide.Player;
            ctx.PrependAction(new GameOverAction(winner));
        }
    }
}