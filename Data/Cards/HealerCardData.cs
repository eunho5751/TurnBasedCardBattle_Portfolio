using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Healer")]
public class HealerCardData : CardDataBase
{
    [SerializeField, Min(0)]
    private int _healAmount = 1;

    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, self.CurrentHp, true);
        ctx.AppendAction(attackAction);
    }

    public override void OnTurnStart(CardInstance self, BattleController ctx)
    {
        var side = ctx.Board.GetSide(self);
        var fieldCards = side.GetFieldCards();
        fieldCards.Remove(self);

        HealCardAction[] healActions = new HealCardAction[fieldCards.Count];
        for (int i = 0; i < healActions.Length; i++)
        {
            HealCardAction healAction = new(self, fieldCards[i], _healAmount);
            healActions[i] = healAction;
        }

        ParallelAction parallelAction = new(healActions);
        ctx.AppendAction(parallelAction);
    }
}