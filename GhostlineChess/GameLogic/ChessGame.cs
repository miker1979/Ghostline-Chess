using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Controls turns, piece selection, movement,
    /// captures, king safety, and game results.
    /// </summary>
    public class ChessGame
    {
        public Board Board { get; }

        public PieceColor Turn { get; private set; }

        public Spot? SelectedSpot { get; private set; }

        public GameResult Result { get; private set; }

        public ChessGame()
        {
            Board = new Board();
            Turn = PieceColor.White;
            SelectedSpot = null;
            Result = GameResult.InProgress;
        }

        /// <summary>
        /// Selects a piece or attempts to move
        /// the currently selected piece.
        /// </summary>
        public bool SelectSpot(
            int row,
            int column,
            out string message)
        {
            if (Result != GameResult.InProgress)
            {
                message = "The game is over.";
                return false;
            }

            Spot clickedSpot = Board.Spots[row, column];

            // No piece is currently selected.
            if (SelectedSpot == null)
            {
                return SelectPiece(clickedSpot, out message);
            }

            // Clicking the selected square clears the selection.
            if (SelectedSpot == clickedSpot)
            {
                SelectedSpot = null;
                message = "Selection cleared.";
                return true;
            }

            // Clicking another friendly piece changes selection.
            if (!clickedSpot.Piece.IsEmpty &&
                clickedSpot.Piece.Color == Turn)
            {
                SelectedSpot = clickedSpot;

                message =
                    $"{clickedSpot.Piece.Type} selected.";

                return true;
            }

            // Kings are checkmated rather than captured.
            if (!clickedSpot.Piece.IsEmpty &&
                clickedSpot.Piece.Type == PieceType.King)
            {
                message = "The king cannot be captured.";
                return false;
            }

            // Check the piece's normal movement rules.
            if (!MoveValidator.IsLegalMove(
                    Board,
                    SelectedSpot,
                    clickedSpot))
            {
                message =
                    $"Illegal {SelectedSpot.Piece.Type} move.";

                return false;
            }

            // Reject moves that expose the player's king.
            if (WouldLeaveKingInCheck(
                    SelectedSpot,
                    clickedSpot))
            {
                message =
                    "That move would leave your king in check.";

                return false;
            }

            return MoveSelectedPiece(
                clickedSpot,
                out message);
        }

        /// <summary>
        /// Selects a piece belonging to the current player.
        /// </summary>
        private bool SelectPiece(
            Spot clickedSpot,
            out string message)
        {
            if (clickedSpot.Piece.IsEmpty)
            {
                message = "Select one of your pieces.";
                return false;
            }

            if (clickedSpot.Piece.Color != Turn)
            {
                message = $"It is {Turn}'s turn.";
                return false;
            }

            SelectedSpot = clickedSpot;

            message =
                $"{clickedSpot.Piece.Type} selected.";

            return true;
        }

        /// <summary>
        /// Temporarily performs a move to determine whether
        /// the moving player's king would remain in check.
        /// </summary>
        private bool WouldLeaveKingInCheck(
            Spot start,
            Spot destination)
        {
            Piece movingPiece = start.Piece;
            Piece capturedPiece = destination.Piece;

            destination.Piece = movingPiece;
            start.Piece = Piece.Empty;

            bool kingInCheck =
                MoveValidator.IsKingInCheck(
                    Board,
                    movingPiece.Color);

            // Restore the original board.
            start.Piece = movingPiece;
            destination.Piece = capturedPiece;

            return kingInCheck;
        }

        /// <summary>
        /// Performs an approved move and updates the game.
        /// </summary>
        private bool MoveSelectedPiece(
            Spot destinationSpot,
            out string message)
        {
            if (SelectedSpot == null)
            {
                message = "No piece is selected.";
                return false;
            }

            bool isCapture =
                !destinationSpot.Piece.IsEmpty;

            PieceType capturedPieceType =
                destinationSpot.Piece.Type;

            destinationSpot.Piece =
                SelectedSpot.Piece;

            SelectedSpot.Piece =
                Piece.Empty;

            SelectedSpot = null;

            // Switch to the other player.
            Turn = Turn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

            UpdateGameResult();

            if (Result == GameResult.WhiteWon)
            {
                message = "Checkmate. White wins!";
                return true;
            }

            if (Result == GameResult.BlackWon)
            {
                message = "Checkmate. Black wins!";
                return true;
            }

            if (Result == GameResult.Draw)
            {
                message = "Stalemate. The game is a draw.";
                return true;
            }

            message = isCapture
                ? $"{capturedPieceType} captured."
                : "Piece moved.";

            if (MoveValidator.IsKingInCheck(Board, Turn))
            {
                message += $" {Turn} is in check.";
            }

            return true;
        }

        /// <summary>
        /// Determines whether the current player has been
        /// checkmated or placed in stalemate.
        /// </summary>
        private void UpdateGameResult()
        {
            if (HasAnyLegalMove(Turn))
            {
                Result = GameResult.InProgress;
                return;
            }

            if (MoveValidator.IsKingInCheck(Board, Turn))
            {
                Result = Turn == PieceColor.White
                    ? GameResult.BlackWon
                    : GameResult.WhiteWon;

                return;
            }

            Result = GameResult.Draw;
        }

        /// <summary>
        /// Searches the board for at least one legal move
        /// belonging to the specified player.
        /// </summary>
        private bool HasAnyLegalMove(PieceColor color)
        {
            for (int startRow = 0; startRow < 8; startRow++)
            {
                for (int startColumn = 0;
                     startColumn < 8;
                     startColumn++)
                {
                    Spot start =
                        Board.Spots[startRow, startColumn];

                    if (start.Piece.IsEmpty ||
                        start.Piece.Color != color)
                    {
                        continue;
                    }

                    for (int endRow = 0; endRow < 8; endRow++)
                    {
                        for (int endColumn = 0;
                             endColumn < 8;
                             endColumn++)
                        {
                            Spot destination =
                                Board.Spots[endRow, endColumn];

                            // Kings are never captured.
                            if (!destination.Piece.IsEmpty &&
                                destination.Piece.Type ==
                                PieceType.King)
                            {
                                continue;
                            }

                            if (!MoveValidator.IsLegalMove(
                                    Board,
                                    start,
                                    destination))
                            {
                                continue;
                            }

                            if (!WouldLeaveKingInCheck(
                                    start,
                                    destination))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}