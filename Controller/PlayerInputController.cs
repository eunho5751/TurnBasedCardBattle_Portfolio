using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    private BattleController _battleController;
    [SerializeField]
    private LayerMask _cardLayer;

    private bool _isInputEnabled;
    private CardView _heldCard;
    private CardView _hoveredTarget;

    private void OnEnable()
    {
        _battleController.PlayerTurnReady += OnPlayerTurnReady;
    }

    private void OnDisable()
    {
        _battleController.PlayerTurnReady -= OnPlayerTurnReady;
    }

    private void Update()
    {
        if (!IsInputEnabled)
        {
            return;
        }

        HandlePickup();
        if (_heldCard != null)
        {
            UpdateTargeting();
        }
    }

    public void RequestEndTurn()
    {
        if (!TryConsumeInput())
        {
            return;
        }
        _battleController.AppendAction(new EndTurnAction());
    }

    private void HandlePickup()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        if (pointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverUI())
            {
                return;
            }
            TryPickUp(pointer.position.ReadValue());
        }
        else if (pointer.press.wasReleasedThisFrame)
        {
            ReleaseHeldCard(pointer.position.ReadValue());
        }
    }

    private void TryPickUp(Vector2 screenPosition)
    {
        CardView cardView = RaycastCard(screenPosition);
        if (cardView == null || cardView.Card.OwnerSide != PlayerSide.Player)
        {
            return;
        }

        _heldCard = cardView;
        _heldCard.SetPickedUp(true);

        var sideView = _battleController.BoardView.GetSideView(PlayerSide.Player);
        sideView.SetFieldCardsHighlightExcept(_heldCard, false);
        _battleController.BoardView.ShowTargetingArrow(_heldCard.transform);
    }

    private CardView RaycastCard(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _cardLayer))
        {
            return null;
        }
        return hit.collider.GetComponentInParent<CardView>();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void UpdateTargeting()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return;
        }

        CardView target = RaycastCard(pointer.position.ReadValue());
        bool isTargetEnemy = target != null && target.Card.OwnerSide == PlayerSide.Enemy;
        SetHoveredTarget(isTargetEnemy ? target : null);
    }

    private void SetHoveredTarget(CardView target)
    {
        if (_hoveredTarget == target)
        {
            return;
        }

        if (_hoveredTarget != null)
        {
            _hoveredTarget.SetOutlineHighlight(false);
        }
        _hoveredTarget = target;
        if (_hoveredTarget != null)
        {
            _hoveredTarget.SetOutlineHighlight(true);
        }
    }

    private void ReleaseHeldCard(Vector2 screenPosition)
    {
        CardView heldCard = _heldCard;
        _heldCard = null;
        _battleController.BoardView.HideTargetingArrow();
        SetHoveredTarget(null);
        if (heldCard == null)
        {
            return;
        }

        CardView target = RaycastCard(screenPosition);
        bool isTargetEnemy = target != null && target.Card.OwnerSide == PlayerSide.Enemy;
        if (isTargetEnemy && TryConsumeInput())
        {
            _battleController.AppendAction(new UseCardAction(heldCard.Card, target.Card));
        }
        else
        {
            heldCard.SetPickedUp(false);

            var sideView = _battleController.BoardView.GetSideView(PlayerSide.Player);
            sideView.SetFieldCardsHighlightExcept(heldCard, true);
        }
    }

    private bool TryConsumeInput()
    {
        if (!IsInputEnabled)
        {
            return false;
        }
        IsInputEnabled = false;
        return true;
    }

    private void OnPlayerTurnReady()
    {
        IsInputEnabled = true;
    }

    private bool IsInputEnabled
    {
        get
        {
            return _isInputEnabled;
        }   

        set
        {
            if (_isInputEnabled == value)
            {
                return;
            }
            _isInputEnabled = value;

            var sideView = _battleController.BoardView.GetSideView(PlayerSide.Player);
            sideView.SetFieldCardsHighlight(value);
        }
    }
}
