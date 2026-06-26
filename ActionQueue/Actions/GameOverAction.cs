using System.Threading;
using Cysharp.Threading.Tasks;

public class GameOverAction : IBattleAction
{
    private readonly PlayerSide _winner;

    public GameOverAction(PlayerSide winner)
    {
        _winner = winner;
    }

    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        ctx.FinishGame();
        ctx.HudView.ShowGameOver(_winner);
        return UniTask.CompletedTask;
    }
}
