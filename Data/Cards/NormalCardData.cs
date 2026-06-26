using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Normal")]
public class NormalCardData : CardDataBase
{
    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, self.CurrentHp, true);
        ctx.AppendAction(attackAction);
    }
}