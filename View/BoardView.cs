using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BoardView : MonoBehaviour
{
    [SerializeField]
    private CardView _cardViewPrefab;
    [SerializeField]
    private BoardSideView _playerSideView;
    [SerializeField]
    private BoardSideView _enemySideView;
    [SerializeField]
    private TargetingArrow _targetingArrow;

    public void Initialize(IReadOnlyList<CardInstance> playerCards, IReadOnlyList<CardInstance> enemyCards)
    {
        _playerSideView.Initialize(_cardViewPrefab, playerCards);
        _enemySideView.Initialize(_cardViewPrefab, enemyCards);
    }

    public async UniTask PlayHandEntranceAsync(CancellationToken token)
    {
        await UniTask.WhenAll(
            _playerSideView.PlayHandEntranceAsync(token),
            _enemySideView.PlayHandEntranceAsync(token));
    }

    public CardView GetCardView(CardInstance card)
    {
        return GetSideView(card).GetCardView(card);
    }

    public BoardSideView GetSideView(PlayerSide side)
    {
        return side switch
        {
            PlayerSide.Player => _playerSideView,
            PlayerSide.Enemy => _enemySideView,
            _ => null
        };
    }

    public BoardSideView GetSideView(CardInstance card)
    {
        return GetSideView(card.OwnerSide);
    }

    public void ShowTargetingArrow(Transform heldCardTransform)
    {
        _targetingArrow.Show(heldCardTransform);
    }

    public void HideTargetingArrow()
    {
        _targetingArrow.Hide();
    }
}