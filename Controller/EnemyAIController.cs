using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField]
    private BattleController _battleController;

    private void OnEnable()
    {
        _battleController.EnemyTurnReady += OnEnemyTurnReady;
    }

    private void OnDisable()
    {
        _battleController.EnemyTurnReady -= OnEnemyTurnReady;
    }

    private void OnEnemyTurnReady()
    {
        Board board = _battleController.Board;
        List<CardInstance> playerCards = board.GetSide(PlayerSide.Player).GetFieldCards();
        List<CardInstance> enemyCards = board.GetSide(PlayerSide.Enemy).GetFieldCards();

        if (playerCards.Count == 0 || enemyCards.Count == 0)
        {
            _battleController.AppendAction(new EndTurnAction());
            return;
        }

        CardInstance target = SelectLowestHpTarget(playerCards);
        CardInstance attacker = SelectHighestHpAttacker(enemyCards);
        var attackerView = _battleController.BoardView.GetCardView(attacker);

        UniTask.Void(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.75f), cancellationToken: destroyCancellationToken);
            attackerView.SetPickedUp(true);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: destroyCancellationToken);
            _battleController.AppendAction(new UseCardAction(attacker, target));
        });
    }

    private CardInstance SelectLowestHpTarget(List<CardInstance> cards)
    {
        int minHp = int.MaxValue;
        foreach (var card in cards)
        {
            if (card.CurrentHp < minHp)
            {
                minHp = card.CurrentHp;
            }
        }

        List<CardInstance> lowestCards = new();
        foreach (var card in cards)
        {
            if (card.CurrentHp == minHp)
            {
                lowestCards.Add(card);
            }
        }
        return lowestCards[UnityEngine.Random.Range(0, lowestCards.Count)];
    }

    private CardInstance SelectHighestHpAttacker(List<CardInstance> cards)
    {
        int maxHp = int.MinValue;
        foreach (var card in cards)
        {
            if (card.CurrentHp > maxHp)
            {
                maxHp = card.CurrentHp;
            }
        }

        List<CardInstance> highestCards = new();
        foreach (var card in cards)
        {
            if (card.CurrentHp == maxHp)
            {
                highestCards.Add(card);
            }
        }
        return highestCards[UnityEngine.Random.Range(0, highestCards.Count)];
    }
}
