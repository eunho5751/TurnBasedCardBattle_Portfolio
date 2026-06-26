using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class TakeTurnAction : IBattleAction
{
    private readonly PlayerSide _side;

    public TakeTurnAction(PlayerSide side)
    {
        _side = side;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        BoardSide side = ctx.Board.GetSide(_side);
        int emptyCount = side.EmptySlotCount;
        for (int i = 0; i < emptyCount; i++)
        {
            ctx.AppendAction(new DeployCardAction(_side));
        }

        ctx.AppendAction(new DelegateAction(PlayTurnBannerAsync));
        ctx.AppendAction(new DelegateAction(ProcessTurnStartAsync));
        return UniTask.CompletedTask;

        UniTask PlayTurnBannerAsync(BattleController ctx, CancellationToken token)
        {
            return ctx.HudView.PlayTurnBannerAsync(_side, token);
        }

        void ProcessTurnStartAsync()
        {
            foreach (var card in side.GetFieldCards())
            {
                card.Data.OnTurnStart(card, ctx);
            }

            void turnReadyAction() => ctx.SetTurnReady(_side);
            ctx.AppendAction(new DelegateAction(turnReadyAction));
        }
    }
}