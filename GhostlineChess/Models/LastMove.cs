using GhostlineChess.Models;

namespace GhostlineChess.Models
{
    /// <summary>
    /// Stores information about the most recent move.
    /// En passant uses this information to determine
    /// whether an adjacent pawn just moved two squares.
    /// </summary>
    public class LastMove
    {
        public Piece Piece { get; }

        public int StartRow { get; }

        public int StartColumn { get; }

        public int EndRow { get; }

        public int EndColumn { get; }

        public LastMove(
            Piece piece,
            int startRow,
            int startColumn,
            int endRow,
            int endColumn)
        {
            Piece = piece;
            StartRow = startRow;
            StartColumn = startColumn;
            EndRow = endRow;
            EndColumn = endColumn;
        }

        /// <summary>
        /// Returns true when the previous move was
        /// a pawn moving exactly two rows.
        /// </summary>
        public bool WasTwoSquarePawnMove
        {
            get
            {
                return
                    Piece.Type == Enums.PieceType.Pawn &&
                    Math.Abs(EndRow - StartRow) == 2;
            }
        }
    }
}