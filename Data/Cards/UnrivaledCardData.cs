using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Unrivaled")]
public class UnrivaledNormalCardData : CardDataBase
{
    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, self.CurrentHp, true);
        ctx.AppendAction(attackAction);

        var targetSide = ctx.Board.GetSide(target);
        var targetAdjacentCards = targetSide.GetAdjacentFieldCards(target);
        if (targetAdjacentCards.Count > 0)
        {
            var randomTarget = targetAdjacentCards[Random.Range(0, targetAdjacentCards.Count)];
            int damage = Mathf.FloorToInt(self.CurrentHp * 0.5f);
            AttackCardAction adjacentAttackAction = new(self, randomTarget, damage, false);
            ctx.AppendAction(adjacentAttackAction);
        }
    }
}