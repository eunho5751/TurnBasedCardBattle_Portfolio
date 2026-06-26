using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Shieldbearer")]
public class ShieldbearerCardData : CardDataBase
{
    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, 1, true);
        ctx.AppendAction(attackAction);
    }
}