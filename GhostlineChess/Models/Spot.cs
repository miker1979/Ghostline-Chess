namespace GhostlineChess.Models
{
    /// <summary>
    /// Represents one square on the chessboard.
    /// </summary>
    public class Spot
    {
        public int Row { get; }

        public int Column { get; }

        public Piece Piece { get; set; }

        public Spot(int row, int column)
        {
            Row = row;
            Column = column;
            Piece = Piece.Empty;
        }
    }
}