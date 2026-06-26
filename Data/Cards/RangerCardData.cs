using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Ranger")]
public class RangerCardData : CardDataBase
{
    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, self.CurrentHp, false);
        ctx.AppendAction(attackAction);
    }
}