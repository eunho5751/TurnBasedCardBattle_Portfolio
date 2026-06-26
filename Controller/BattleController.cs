using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    [SerializeField]
    private DeckData _playerDeck;
    [SerializeField]
    private DeckData _enemyDeck;

    [Space(5)]

    [SerializeField]
    private BoardView _boardView;
    [SerializeField]
    private HudView _hudView;

    private Board _board;
    private ActionQueue _actionQueue;
    private PlayerSide _currentSide;
    private bool _isGameOver;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        _actionQueue = new(this);
    }

    private void OnDestroy()
    {
        _actionQueue.Dispose();
    }

    private void Update()
    {
        if (_isGameOver)
        {
            if (Pointer.current.press.wasReleasedThisFrame)
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void StartGame()
    {
        SetupBoard();
    }

    public void FinishGame()
    {
        _actionQueue.Clear();
        _isGameOver = true;
    }

    public void PrependAction(IBattleAction action)
    {
        _actionQueue.Prepend(action);
    }

    public void AppendAction(IBattleAction action)
    {
        _actionQueue.Append(action);
    }

    public void SetTurnReady(PlayerSide side)
    {
        _currentSide = side;
        #if UNITY_EDITOR
        Debug.Log($"Current Turn : {side}");
        #endif

        if (side == PlayerSide.Player)
        {
            PlayerTurnReady?.Invoke();
        }
        else
        {
            EnemyTurnReady?.Invoke();
        }
    }

    private void SetupBoard()
    {
        BoardSide playerSide = CreateSide(PlayerSide.Player, _playerDeck);
        BoardSide enemySide = CreateSide(PlayerSide.Enemy, _enemyDeck);
        _board = new(playerSide, enemySide);
        _boardView.Initialize(playerSide.HandCards, enemySide.HandCards);

        _actionQueue.Append(new HandEntranceAction());
        _actionQueue.Append(new DelayAction(0.7f));
        for (int i = 0; i < BoardSide.FieldSlotCount; i++)
        {
            _actionQueue.Append(new ParallelAction(
                new DeployCardAction(PlayerSide.Player),
                new DeployCardAction(PlayerSide.Enemy)));
        }
        _actionQueue.Append(new TakeTurnAction(PlayerSide.Player));
    }

    private BoardSide CreateSide(PlayerSide side, DeckData deck)
    {
        List<CardInstance> initialCards = new();
        if (deck.Count > 0)
        {
            for (int i = 0; i < 6; i++)
            {
                var cardData = deck[UnityEngine.Random.Range(0, deck.Count)];
                CardInstance card = new(cardData, side);
                initialCards.Add(card);
            }
        }
        return new BoardSide(side, initialCards);
    }

    public event Action PlayerTurnReady;
    public event Action EnemyTurnReady;

    public Board Board => _board;
    public BoardView BoardView => _boardView;
    public HudView HudView => _hudView;
    public PlayerSide CurrentSide => _currentSide;
}
