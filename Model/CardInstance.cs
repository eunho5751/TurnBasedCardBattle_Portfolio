using System;
using UnityEngine;

public class CardInstance
{
    private int _currentHp;

    public CardInstance(CardDataBase data, PlayerSide ownerSide)
    {
        Data = data;
        OwnerSide = ownerSide;
        _currentHp = data.BaseHp;
    }

    public event Action<int> HpChanged;

    public int CurrentHp
    {
        get
        {
            return _currentHp;
        }

        set
        {
            _currentHp = Mathf.Clamp(value, 0, Data.BaseHp);
            HpChanged?.Invoke(_currentHp);
        }
    }

    public bool IsAlive => _currentHp > 0;

    public CardDataBase Data { get; }
    public PlayerSide OwnerSide { get; }
}
