using System;
using System.Collections.Generic;

public class BoardSide
{
    public const int FieldSlotCount = 3;

    private readonly PlayerSide _side;
    private readonly CardInstance[] _fieldSlots;
    private readonly List<CardInstance> _handCards;

    public BoardSide(PlayerSide side, IReadOnlyList<CardInstance> initialCards)
    {
        _side = side;
        _fieldSlots = new CardInstance[FieldSlotCount];
        _handCards = new(initialCards);
    }

    public bool TryDeployCard(out CardInstance outCard, out int outSlot)
    {
        for (int i = 0; i < _fieldSlots.Length; i++)
        {
            if (_fieldSlots[i] == null && _handCards.Count > 0)
            {
                var card = _handCards[^1];
                _fieldSlots[i] = card;
                _handCards.RemoveAt(_handCards.Count - 1);
                outCard = card;
                outSlot = i;
                return true;
            }
        }

        outCard = null;
        outSlot = -1;
        return false;
    }

    public void RemoveCard(CardInstance card)
    {
        for (int i = 0; i < _fieldSlots.Length; i++)
        {
            if (_fieldSlots[i] == card)
            {
                _fieldSlots[i] = null;
                return;
            }
        }
    }

    public List<CardInstance> GetFieldCards()
    {
        List<CardInstance> result = new();
        foreach (var card in _fieldSlots)
        {
            if (card != null)
            {
                result.Add(card);
            }
        }
        return result;
    }

    public List<CardInstance> GetAdjacentFieldCards(CardInstance card)
    {
        return GetAdjacentFieldCards(GetSlotIndex(card));
    }

    public List<CardInstance> GetAdjacentFieldCards(int slotIndex)
    {
        List<CardInstance> result = new();
        int prevIdx = slotIndex - 1;
        int nextIdx = slotIndex + 1;
        if (prevIdx >= 0 && _fieldSlots[prevIdx] != null)
        {
            result.Add(_fieldSlots[prevIdx]);
        }
        if (nextIdx < _fieldSlots.Length && _fieldSlots[nextIdx] != null)
        {
            result.Add(_fieldSlots[nextIdx]);
        }
        return result;
    }

    public int GetSlotIndex(CardInstance card)
    {
        return Array.IndexOf(_fieldSlots, card);
    }

    public int EmptySlotCount
    {
        get
        {
            int count = 0;
            foreach (var card in _fieldSlots)
            {
                if (card == null)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public bool IsDefeated
    {
        get
        {
            if (_handCards.Count > 0)
            {
                return false;
            }
            foreach (var card in _fieldSlots)
            {
                if (card != null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public PlayerSide Side => _side;
    public IReadOnlyList<CardInstance> HandCards => _handCards;
}
