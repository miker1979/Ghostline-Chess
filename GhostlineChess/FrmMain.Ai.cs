using System.Drawing;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;
using GhostlineChess.Models;

namespace GhostlineChess
{
    /// <summary>
    /// Connects a computer opponent to the existing board,
    /// Chronicle, Graveyard, FEN, and audio pipelines.
    /// </summary>
    public partial class FrmMain
    {
        private readonly Button aiModeButton =
            new Button();

        private readonly ComputerPlayer computerPlayer =
            new ComputerPlayer();

        private readonly PieceColor aiColor =
            PieceColor.Black;

        private bool aiOpponentEnabled;
        private bool aiMovePending;

        /// <summary>
        /// Adds the first single-player control beside the
        /// existing audio controls. The Damned are AI-controlled
        /// during this initial milestone.
        /// </summary>
        private void InitializeAiExperience()
        {
            aiModeButton.Text =
                "AI Opponent: Off";

            aiModeButton.Location =
                new Point(
                    muteButton.Right + 12,
                    muteButton.Top);

            aiModeButton.Size =
                new Size(
                    175,
                    muteButton.Height);

            StyleGothicButton(
                aiModeButton);

            aiModeButton.Click +=
                AiModeButton_Click;

            Controls.Add(
                aiModeButton);

            aiModeButton.BringToFront();

            newGameButton.Click +=
                AiBoardStateChanged;

            loadFenButton.Click +=
                AiBoardStateChanged;

            startingPositionButton.Click +=
                AiBoardStateChanged;
        }

        private void AiModeButton_Click(
            object? sender,
            EventArgs e)
        {
            aiOpponentEnabled =
                !aiOpponentEnabled;

            aiModeButton.Text =
                aiOpponentEnabled
                    ? "AI Opponent: On"
                    : "AI Opponent: Off";

            UpdateStatusLabel(
                aiOpponentEnabled
                    ? "The Damned awaken under computer control."
                    : "Local two-player control restored.");

            ScheduleAiMoveIfNeeded();
        }

        /// <summary>
        /// Rechecks AI ownership after New Game or FEN controls
        /// finish changing the current board position.
        /// </summary>
        private void AiBoardStateChanged(
            object? sender,
            EventArgs e)
        {
            ScheduleAiMoveIfNeeded();
        }

        /// <summary>
        /// Waits briefly so the opponent feels intentional,
        /// chooses a legal move, then feeds it through the same
        /// interface pipeline used by a local player.
        /// </summary>
        private async void ScheduleAiMoveIfNeeded()
        {
            if (!aiOpponentEnabled ||
                aiMovePending ||
                chessGame.Result !=
                    GameResult.InProgress ||
                chessGame.PromotionPending ||
                chessGame.Turn != aiColor)
            {
                return;
            }

            aiMovePending = true;
            aiModeButton.Enabled = false;

            SetBoardInteractionEnabled(
                false);

            UpdateStatusLabel(
                "The Damned contemplate their move...");

            try
            {
                await Task.Delay(650);

                if (!aiOpponentEnabled ||
                    chessGame.Result !=
                        GameResult.InProgress ||
                    chessGame.Turn != aiColor)
                {
                    return;
                }

                ChessMove? move =
                    computerPlayer.ChooseMove(
                        chessGame);

                if (move == null)
                {
                    return;
                }

                RefreshBoard(
                    "The Damned choose their path.");

                ExecuteAiMove(
                    move);
            }
            finally
            {
                aiMovePending = false;
                aiModeButton.Enabled = true;

                if (chessGame.Result ==
                        GameResult.InProgress &&
                    !chessGame.PromotionPending)
                {
                    SetBoardInteractionEnabled(
                        true);
                }
            }
        }

        /// <summary>
        /// Selects the AI piece and destination while preparing
        /// Chronicle state exactly as a mouse-down would for a
        /// human move.
        /// </summary>
        private void ExecuteAiMove(
            ChessMove move)
        {
            Button startingButton =
                squareButtons[
                    move.StartRow,
                    move.StartColumn];

            Button destinationButton =
                squareButtons[
                    move.EndRow,
                    move.EndColumn];

            SquareButton_Click(
                startingButton,
                EventArgs.Empty);

            pendingChronicleMove =
                CreateJournalMoveContext(
                    new Point(
                        move.EndRow,
                        move.EndColumn));

            previousMoveBeforeChronicleClick =
                chessGame.PreviousMove;

            JournalMoveContext? completedMove =
                pendingChronicleMove;

            LastMove? previousMove =
                previousMoveBeforeChronicleClick;

            SquareButton_Click(
                destinationButton,
                EventArgs.Empty);

            pendingChronicleMove = null;
            previousMoveBeforeChronicleClick = null;

            if (completedMove != null &&
                DidMoveOccur(previousMove))
            {
                RecordMoveAfterInterfaceCompletes(
                    completedMove);
            }
        }

        /// <summary>
        /// Prevents a human click while the computer is thinking.
        /// </summary>
        private void SetBoardInteractionEnabled(
            bool enabled)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Button squareButton =
                        squareButtons[row, column];

                    squareButton.Enabled = enabled;
                    squareButton.Cursor =
                        enabled
                            ? Cursors.Hand
                            : Cursors.Default;
                }
            }
        }
    }
}
