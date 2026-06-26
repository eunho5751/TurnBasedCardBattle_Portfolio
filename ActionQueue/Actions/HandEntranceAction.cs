using System.Threading;
using Cysharp.Threading.Tasks;

public class HandEntranceAction : IBattleAction
{
    public UniTask ExecuteAsync(BattleController ctx, CancellationToken token)
    {
        return ctx.BoardView.PlayHandEntranceAsync(token);
    }
}
