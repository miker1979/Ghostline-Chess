using System;
using System.Drawing;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;

namespace GhostlineChess
{
    public partial class FrmMain : Form
    {
        // Stores the 64 buttons that display the chessboard.
        private readonly Button[,] squareButtons =
            new Button[8, 8];

        // Stores the board, selected square, current turn,
        // and game result.
        private ChessGame chessGame =
        new ChessGame();

        public FrmMain()
        {
            InitializeComponent();

            BuildBoard();
            RefreshBoard();
        }

        /// <summary>
        /// Creates the 64 buttons used as chessboard squares.
        /// </summary>
        private void BuildBoard()
        {
            const int squareSize = 70;

            boardPanel.AutoSize = false;
            boardPanel.Dock = DockStyle.None;
            boardPanel.Anchor =
                AnchorStyles.Top | AnchorStyles.Left;

            boardPanel.Size = new Size(
                squareSize * 8,
                squareSize * 8);

            statusLabel.Location = new Point(
                boardPanel.Left,
                boardPanel.Bottom + 20);

            boardPanel.SuspendLayout();

            boardPanel.Controls.Clear();
            boardPanel.ColumnStyles.Clear();
            boardPanel.RowStyles.Clear();

            boardPanel.ColumnCount = 8;
            boardPanel.RowCount = 8;
            boardPanel.GrowStyle =
                TableLayoutPanelGrowStyle.FixedSize;

            boardPanel.Padding = Padding.Empty;
            boardPanel.Margin = Padding.Empty;

            // Create eight equal-width columns.
            for (int column = 0; column < 8; column++)
            {
                boardPanel.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        12.5F));
            }

            // Create eight equal-height rows.
            for (int row = 0; row < 8; row++)
            {
                boardPanel.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        12.5F));
            }

            // Create one button for every chessboard square.
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Button squareButton = new Button
                    {
                        Dock = DockStyle.Fill,
                        Margin = Padding.Empty,
                        FlatStyle = FlatStyle.Flat,

                        Font = new Font(
                            "Segoe UI Symbol",
                            30F,
                            FontStyle.Regular),

                        // X stores the row.
                        // Y stores the column.
                        Tag = new Point(row, column),

                        TabStop = false,
                        UseVisualStyleBackColor = false
                    };

                    squareButton.FlatAppearance.BorderSize = 0;
                    squareButton.Click += SquareButton_Click;

                    squareButtons[row, column] =
                        squareButton;

                    boardPanel.Controls.Add(
                        squareButton,
                        column,
                        row);
                }
            }

            boardPanel.ResumeLayout(true);
        }

        /// <summary>
        /// Handles clicks on all 64 chessboard squares.
        /// </summary>
        private void SquareButton_Click(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button squareButton)
            {
                return;
            }

            if (squareButton.Tag is not Point position)
            {
                return;
            }

            chessGame.SelectSpot(
                position.X,
                position.Y,
                out string message);

            RefreshBoard(message);
        }

        /// <summary>
        /// Updates the pieces, colors, selection,
        /// status message, and game-over state.
        /// </summary>
        private void RefreshBoard(string message = "")
        {
            bool gameIsActive =
                chessGame.Result == GameResult.InProgress;

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Button squareButton =
                        squareButtons[row, column];

                    squareButton.Text =
                        chessGame.Board
                            .Spots[row, column]
                            .Piece
                            .ToString();

                    bool isLightSquare =
                        (row + column) % 2 == 0;

                    squareButton.BackColor =
                        isLightSquare
                            ? Color.Beige
                            : Color.SaddleBrown;

                    squareButton.ForeColor = Color.Black;

                    // Highlight the selected square.
                    if (chessGame.SelectedSpot != null &&
                        chessGame.SelectedSpot.Row == row &&
                        chessGame.SelectedSpot.Column == column)
                    {
                        squareButton.BackColor = Color.Gold;
                    }

                    // Disable every square after the game ends.
                    squareButton.Enabled = gameIsActive;
                }
            }

            UpdateStatusLabel(message);
        }

        /// <summary>
        /// Displays the current turn, move message,
        /// or final game result.
        /// </summary>
        private void UpdateStatusLabel(string message)
        {
            switch (chessGame.Result)
            {
                case GameResult.WhiteWon:
                    statusLabel.Text =
                        "Game Over | Checkmate. White wins!";
                    break;

                case GameResult.BlackWon:
                    statusLabel.Text =
                        "Game Over | Checkmate. Black wins!";
                    break;

                case GameResult.Draw:
                    statusLabel.Text =
                        "Game Over | Stalemate. The game is a draw.";
                    break;

                default:
                    statusLabel.Text =
                        $"Turn: {chessGame.Turn}";

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        statusLabel.Text +=
                            $" | {message}";
                    }

                    break;
            }
        }

        private void newGameButton_Click(object sender, EventArgs e)
        {
            chessGame = new ChessGame();

            RefreshBoard("New game started.");
        }
    }
}