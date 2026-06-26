using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DeckData")]
public class DeckData : ScriptableObject, IReadOnlyList<CardDataBase>
{
    [SerializeField]
    private List<CardDataBase> _cards;

    public IEnumerator<CardDataBase> GetEnumerator() => _cards.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public CardDataBase this[int index] => _cards[index];
    public int Count => _cards.Count;
}