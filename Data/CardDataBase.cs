using UnityEngine;

public abstract class CardDataBase : ScriptableObject
{
    [SerializeField]
    private string _cardName;
    [SerializeField, TextArea]
    private string _description;
    [SerializeField]
    private Sprite _cardSprite;
    [SerializeField]
    private int _baseHp = 5;

    public virtual void OnDeployed(CardInstance self, BattleController ctx) { }
    public virtual void OnTurnStart(CardInstance self, BattleController ctx) { }
    public virtual void OnTurnEnd(CardInstance self, BattleController ctx) { }
    public virtual void OnUsed(CardInstance self, CardInstance target, BattleController ctx) { }

    public string CardName => _cardName;
    public string Description => _description;
    public Sprite CardSprite => _cardSprite;
    public int BaseHp => _baseHp;
}
