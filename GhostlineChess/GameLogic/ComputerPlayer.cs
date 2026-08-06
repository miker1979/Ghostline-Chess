using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Supplies computer-controlled moves without bypassing
    /// the existing Ghostline Chess legality rules.
    /// </summary>
    public sealed class ComputerPlayer
    {
        private readonly Random random = new Random();

        /// <summary>
        /// Chooses one fully legal move for the current turn.
        /// This first milestone is intentionally unpredictable;
        /// stronger evaluation will be layered on next.
        /// </summary>
        public ChessMove? ChooseMove(
            ChessGame game)
        {
            IReadOnlyList<ChessMove> legalMoves =
                game.GetLegalMovesForTurn();

            if (legalMoves.Count == 0)
            {
                return null;
            }

            return legalMoves[
                random.Next(
                    legalMoves.Count)];
        }
    }
}
