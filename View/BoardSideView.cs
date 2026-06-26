using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BoardSideView : MonoBehaviour
{
    [SerializeField]
    private Transform _fieldCardsParent;
    [SerializeField]
    private Transform _handCardsParent;
    [SerializeField]
    private float _fanRadius = 5f;
    [SerializeField]
    private float _anglePerCard = 8f;
    [SerializeField]
    private float _maxFanAngle = 60f;
    [SerializeField]
    private float _dealInterval = 0.08f;
    [SerializeField]
    private float _riseDuration = 0.25f;
    [SerializeField]
    private float _riseDistance = 3f;
    [SerializeField]
    private float _showcaseHeight = 2f;
    [SerializeField]
    private float _liftDuration = 0.2f;
    [SerializeField]
    private float _flipDuration = 0.3f;
    [SerializeField]
    private float _revealHold = 0.2f;
    [SerializeField]
    private float _placeDuration = 0.3f;
    [SerializeField]
    private float _arrangeDuration = 0.2f;

    private readonly Dictionary<CardInstance, CardView> _cardViewMap = new();

    public void Initialize(CardView cardViewPrefab, IReadOnlyList<CardInstance> initialCards)
    {
        foreach (var card in initialCards)
        {
            var cardView = Instantiate(cardViewPrefab, _handCardsParent);
            cardView.Initialize(card);
            _cardViewMap.Add(card, cardView);
        }

        int count = _handCardsParent.childCount;
        for (int i = 0; i < count; i++)
        {
            GetCardLayout(i, count, out Vector3 position, out Quaternion rotation);
            _handCardsParent.GetChild(i).SetLocalPositionAndRotation(position + Vector3.down * _riseDistance, rotation);
        }
    }

    public async UniTask DeployCardAsync(int slot, CancellationToken token)
    {
        Transform card = _handCardsParent.GetChild(_handCardsParent.childCount - 1);
        CardView cardView = card.GetComponent<CardView>();

        Vector3 showcasePosition = card.localPosition + Vector3.up * _showcaseHeight;
        await MoveCardAsync(card, showcasePosition, Quaternion.identity, _liftDuration, token);

        await cardView.RevealAsync(_flipDuration, token);

        if (_revealHold > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_revealHold), cancellationToken: token);
        }

        SFXManager.Instance.Play(SFXType.DeployCard);

        Transform slotParent = _fieldCardsParent.GetChild(slot);
        Vector3 slotPosition = _handCardsParent.InverseTransformPoint(slotParent.position);
        await MoveCardAsync(card, slotPosition, Quaternion.identity, _placeDuration, token);

        card.SetParent(slotParent, false);
        card.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        cardView.SetPickable(true);
        await ArrangeHandAsync(token);
    }

    public async UniTask KillCardAsync(CardInstance card, CancellationToken token)
    {
        var cardView = GetCardView(card);
        Destroy(cardView.gameObject);

        SFXManager.Instance.Play(SFXType.KillCard);
        var diedVfx = VFXManager.Instance.Spawn(VFXType.Died);
        diedVfx.transform.position = cardView.transform.position;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        }
        finally
        {
            VFXManager.Instance.Despawn(diedVfx);    
        }
    }

    public void SetFieldCardsHighlight(bool isOn)
    {
        var cardViews = _fieldCardsParent.GetComponentsInChildren<CardView>();
        foreach (var cardView in cardViews)
        {
            cardView.SetOutlineHighlight(isOn);
        }
    }

    public void SetFieldCardsHighlightExcept(CardView except, bool isOn)
    {
        var cardViews = _fieldCardsParent.GetComponentsInChildren<CardView>();
        foreach (var cardView in cardViews)
        {
            if (cardView != except)
            {
                cardView.SetOutlineHighlight(isOn);
            }
        }
    }

    public CardView GetCardView(CardInstance card)
    {
        return _cardViewMap.TryGetValue(card, out var cardView) ? cardView : null;
    }

    private async UniTask ArrangeHandAsync(CancellationToken token)
    {
        int count = _handCardsParent.childCount;
        List<UniTask> moves = new(count);
        for (int i = 0; i < count; i++)
        {
            GetCardLayout(i, count, out Vector3 position, out Quaternion rotation);
            moves.Add(MoveCardAsync(_handCardsParent.GetChild(i), position, rotation, _arrangeDuration, token));
        }
        await UniTask.WhenAll(moves);
    }

    private void GetCardLayout(int index, int count, out Vector3 position, out Quaternion rotation)
    {
        float step = _anglePerCard;
        if (count > 1 && step * (count - 1) > _maxFanAngle)
        {
            step = _maxFanAngle / (count - 1);
        }

        float startAngle = -step * (count - 1) * 0.5f;
        float angle = startAngle + step * index;
        float rad = angle * Mathf.Deg2Rad;
        position = new Vector3(_fanRadius * Mathf.Sin(rad), _fanRadius * Mathf.Cos(rad) - _fanRadius, 0f);
        rotation = Quaternion.Euler(0f, 0f, -angle);
    }

    public async UniTask PlayHandEntranceAsync(CancellationToken token)
    {
        int count = _handCardsParent.childCount;
        List<UniTask> rises = new(count);
        for (int i = 0; i < count; i++)
        {
            GetCardLayout(i, count, out Vector3 position, out _);
            rises.Add(RiseCardAsync(_handCardsParent.GetChild(i), position, i * _dealInterval, token));
        }
        await UniTask.WhenAll(rises);
    }

    private async UniTask RiseCardAsync(Transform card, Vector3 targetPosition, float startDelay, CancellationToken token)
    {
        Vector3 startPosition = card.localPosition;

        if (startDelay > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(startDelay), cancellationToken: token);
        }

        float elapsed = 0f;
        while (elapsed < _riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / _riseDuration));
            card.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            await UniTask.Yield(token);
        }
        card.localPosition = targetPosition;
    }

    private async UniTask MoveCardAsync(Transform card, Vector3 targetPosition, Quaternion targetRotation, float duration, CancellationToken token)
    {
        card.GetLocalPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            card.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            card.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            await UniTask.Yield(token);
        }
        card.SetLocalPositionAndRotation(targetPosition, targetRotation);
    }
}
