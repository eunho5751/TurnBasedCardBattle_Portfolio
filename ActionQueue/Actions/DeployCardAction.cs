using System.Threading;
using Cysharp.Threading.Tasks;

public class DeployCardAction : IBattleAction
{
    private readonly PlayerSide _side;

    public DeployCardAction(PlayerSide side)
    {
        _side = side;
    }

    public async UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        BoardSide side = ctx.Board.GetSide(_side);
        if (side.TryDeployCard(out var card, out int slot))
        {
            var sideView = ctx.BoardView.GetSideView(_side);
            await sideView.DeployCardAsync(slot, token);
            card.Data.OnDeployed(card, ctx);
        }
    }
}
