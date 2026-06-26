using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Thief")]
public class ThiefCardData : CardDataBase
{
    [SerializeField, Range(0f, 1f)]
    private float _assasinateChance = 0.3f;

    public override void OnUsed(CardInstance self, CardInstance target, BattleController ctx)
    {
        AttackCardAction attackAction = new(self, target, self.CurrentHp, true);
        ctx.AppendAction(attackAction);

        if (Random.value < _assasinateChance)
        {
            var targetSide = ctx.Board.GetSide(target);
            int targetSlotIndex = targetSide.GetSlotIndex(target);
            ctx.AppendAction(new DelegateAction(() =>
            {
                if (self.IsAlive && !target.IsAlive)
                {
                    var targetAdjacentCards = targetSide.GetAdjacentFieldCards(targetSlotIndex);
                    if (targetAdjacentCards.Count > 0)
                    {
                        var randomTarget = targetAdjacentCards[Random.Range(0, targetAdjacentCards.Count)];
                        AttackCardAction assasinateAction = new(self, randomTarget, randomTarget.CurrentHp, false, VFXType.Assasinate, SFXType.Assasinate);
                        ctx.PrependAction(assasinateAction);
                    }
                }
            }));
        }
    }
}