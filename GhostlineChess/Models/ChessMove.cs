namespace GhostlineChess.Models
{
    /// <summary>
    /// Describes one legal chess move using board coordinates.
    /// The same lightweight message can later be supplied by
    /// a local player, computer opponent, or remote opponent.
    /// </summary>
    public sealed class ChessMove
    {
        public int StartRow { get; }
        public int StartColumn { get; }
        public int EndRow { get; }
        public int EndColumn { get; }

        public ChessMove(
            int startRow,
            int startColumn,
            int endRow,
            int endColumn)
        {
            StartRow = startRow;
            StartColumn = startColumn;
            EndRow = endRow;
            EndColumn = endColumn;
        }
    }
}
