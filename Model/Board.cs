
public class Board
{
    private readonly BoardSide _player;
    private readonly BoardSide _enemy;

    public Board(BoardSide player, BoardSide enemy)
    {
        _player = player;
        _enemy = enemy;
    }

    public BoardSide GetSide(PlayerSide side)
    {
        return side == PlayerSide.Player ? _player : _enemy;
    }

    public BoardSide GetSide(CardInstance card)
    {
        return GetSide(card.OwnerSide);
    }

    public BoardSide Player => _player;
    public BoardSide Enemy => _enemy;
}
