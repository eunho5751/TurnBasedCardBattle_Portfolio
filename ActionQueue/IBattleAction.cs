using System.Threading;
using Cysharp.Threading.Tasks;

public interface IBattleAction
{
    UniTask ExecuteAsync(BattleController ctx, CancellationToken token);
}