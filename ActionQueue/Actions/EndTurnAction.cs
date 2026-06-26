using System.Threading;
using Cysharp.Threading.Tasks;

public class EndTurnAction : IBattleAction
{
    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        PlayerSide current = ctx.CurrentSide;
        BoardSide side = ctx.Board.GetSide(current);
        foreach (var card in side.GetFieldCards())
        {
            card.Data.OnTurnEnd(card, ctx);
        }

        PlayerSide next = current == PlayerSide.Player ? PlayerSide.Enemy : PlayerSide.Player;
        ctx.AppendAction(new TakeTurnAction(next));
        return UniTask.CompletedTask;
    }
}
