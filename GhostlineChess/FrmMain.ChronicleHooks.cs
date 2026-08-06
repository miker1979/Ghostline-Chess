using System;
using System.Drawing;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;
using GhostlineChess.Models;

namespace GhostlineChess
{
    /// <summary>
    /// Connects completed board moves,
    /// captures, FEN loading, and New Game
    /// to the Ghostline Tome and Graveyard.
    /// </summary>
    public partial class FrmMain
    {
        // Stores information captured immediately
        // before a board-square click is processed.
        private JournalMoveContext?
            pendingChronicleMove;

        private LastMove?
            previousMoveBeforeChronicleClick;

        private bool chronicleEventsConnected;

        /// <summary>
        /// Builds the Chronicle after the form's
        /// normal controls and Gothic theme exist.
        /// </summary>
        protected override void OnLoad(
            EventArgs e)
        {
            base.OnLoad(e);

            BuildChroniclePanel();
            ConnectChronicleEvents();
            InitializeAudioExperience();
            InitializeAiExperience();
        }

        /// <summary>
        /// Connects the Chronicle to the board
        /// and main interface buttons.
        /// </summary>
        private void ConnectChronicleEvents()
        {
            if (chronicleEventsConnected)
            {
                return;
            }

            chronicleEventsConnected = true;

            for (int row = 0;
                 row < 8;
                 row++)
            {
                for (int column = 0;
                     column < 8;
                     column++)
                {
                    Button squareButton =
                        squareButtons[
                            row,
                            column];

                    // MouseDown occurs before the existing
                    // SquareButton_Click move handler.
                    squareButton.MouseDown +=
                        ChronicleSquare_MouseDown;

                    // This additional Click handler runs
                    // after the existing move handler.
                    squareButton.Click +=
                        ChronicleSquare_Click;
                }
            }

            // These handlers are added after the original
            // handlers, so they reset the Chronicle after
            // the board has been successfully changed.
            newGameButton.Click +=
                ChronicleNewGameButton_Click;

            loadFenButton.Click +=
                ChronicleLoadFenButton_Click;

            startingPositionButton.Click +=
                ChronicleStartingPositionButton_Click;
        }

        /// <summary>
        /// Records the selected piece, destination,
        /// capture information, and previous move
        /// before ChessGame changes the board.
        /// </summary>
        private void ChronicleSquare_MouseDown(
            object? sender,
            MouseEventArgs e)
        {
            if (sender is not Button squareButton ||
                squareButton.Tag is not Point destination)
            {
                pendingChronicleMove = null;

                previousMoveBeforeChronicleClick =
                    null;

                return;
            }

            pendingChronicleMove =
                CreateJournalMoveContext(
                    destination);

            previousMoveBeforeChronicleClick =
                chessGame.PreviousMove;
        }

        /// <summary>
        /// Records the move after the existing board
        /// click handler has completed it.
        /// </summary>
        private void ChronicleSquare_Click(
            object? sender,
            EventArgs e)
        {
            JournalMoveContext? completedMove =
                pendingChronicleMove;

            LastMove? previousMove =
                previousMoveBeforeChronicleClick;

            pendingChronicleMove = null;

            previousMoveBeforeChronicleClick =
                null;

            if (completedMove == null)
            {
                return;
            }

            if (!DidMoveOccur(previousMove))
            {
                return;
            }

            RecordMoveAfterInterfaceCompletes(
                completedMove);
        }

        /// <summary>
        /// Adds a completed move to the Tome
        /// and sends any captured piece to
        /// the correct Graveyard tray.
        /// </summary>
        private void RecordMoveAfterInterfaceCompletes(
            JournalMoveContext completedMove)
        {
            RecordCapturedPiece(
                completedMove);

            PieceType? promotionType =
                GetCompletedPromotionType(
                    completedMove);

            AddJournalEntry(
                completedMove,
                promotionType);

            PlayCompletedMoveAudio(
                completedMove,
                promotionType);

            RefreshChroniclePanel();

            ScheduleAiMoveIfNeeded();
        }

        /// <summary>
        /// Determines which promotion piece was chosen
        /// after the promotion dialog has closed.
        /// </summary>
        private PieceType? GetCompletedPromotionType(
            JournalMoveContext completedMove)
        {
            bool reachedPromotionRow =
                completedMove.MovingType ==
                    PieceType.Pawn &&
                (completedMove.EndRow == 0 ||
                 completedMove.EndRow == 7);

            if (!reachedPromotionRow)
            {
                return null;
            }

            Piece finalPiece =
                chessGame.Board
                    .Spots[
                        completedMove.EndRow,
                        completedMove.EndColumn]
                    .Piece;

            bool isPromotionPiece =
                finalPiece.Type ==
                    PieceType.Queen ||
                finalPiece.Type ==
                    PieceType.Rook ||
                finalPiece.Type ==
                    PieceType.Bishop ||
                finalPiece.Type ==
                    PieceType.Knight;

            return isPromotionPiece
                ? finalPiece.Type
                : null;
        }

        /// <summary>
        /// Clears the Tome and Graveyard after
        /// the existing New Game handler runs.
        /// </summary>
        private void ChronicleNewGameButton_Click(
            object? sender,
            EventArgs e)
        {
            ResetChronicle();
        }

        /// <summary>
        /// Clears the Tome and Graveyard after
        /// a valid FEN position is loaded.
        /// </summary>
        private void ChronicleLoadFenButton_Click(
            object? sender,
            EventArgs e)
        {
            string currentBoardFen =
                FenService.ExportFen(
                    chessGame);

            string displayedFen =
                fenTextBox.Text.Trim();

            // A successful FEN load refreshes the text
            // box with the board's exported position.
            // Invalid text remains different.
            if (string.Equals(
                    displayedFen,
                    currentBoardFen,
                    StringComparison.Ordinal))
            {
                ResetChronicle();
            }
        }

        /// <summary>
        /// Clears the Tome and Graveyard after
        /// the starting position is restored.
        /// </summary>
        private void
            ChronicleStartingPositionButton_Click(
                object? sender,
                EventArgs e)
        {
            ResetChronicle();
        }
    }
}
