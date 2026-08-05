using System;
using System.Drawing;
using System.Windows.Forms;
using GhostlineChess.GameLogic;

namespace GhostlineChess
{
    /// <summary>
    /// Contains the FEN import, export,
    /// and starting-position controls.
    /// </summary>
    public partial class FrmMain
    {
        private readonly Label fenLabel =
            new Label();

        private readonly TextBox fenTextBox =
            new TextBox();

        private readonly Button loadFenButton =
            new Button();

        private readonly Button copyFenButton =
            new Button();

        private readonly Button startingPositionButton =
            new Button();

        /// <summary>
        /// Creates the controls used to import,
        /// display, and copy FEN positions.
        /// </summary>
        private void BuildFenControls()
        {
            fenLabel.Text =
                "Current FEN:";

            fenLabel.AutoSize =
                true;

            fenLabel.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            fenLabel.Location =
                new Point(
                    boardPanel.Left,
                    statusLabel.Bottom + 20);

            // The text box is editable so the player
            // can paste a FEN position into it.
            fenTextBox.ReadOnly =
                false;

            fenTextBox.Font =
                new Font(
                    "Consolas",
                    10F,
                    FontStyle.Regular);

            fenTextBox.Location =
                new Point(
                    boardPanel.Left,
                    fenLabel.Bottom + 8);

            fenTextBox.Size =
                new Size(
                    boardPanel.Width,
                    30);

            fenTextBox.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left;

            loadFenButton.Text =
                "Load FEN";

            loadFenButton.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            loadFenButton.Size =
                new Size(
                    120,
                    38);

            loadFenButton.Location =
                new Point(
                    boardPanel.Left,
                    fenTextBox.Bottom + 10);

            loadFenButton.Click +=
                LoadFenButton_Click;

            copyFenButton.Text =
                "Copy FEN";

            copyFenButton.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            copyFenButton.Size =
                new Size(
                    120,
                    38);

            copyFenButton.Location =
                new Point(
                    loadFenButton.Right + 10,
                    fenTextBox.Bottom + 10);

            copyFenButton.Click +=
                CopyFenButton_Click;

            startingPositionButton.Text =
                "Starting Position";

            startingPositionButton.Font =
                new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold);

            startingPositionButton.Size =
                new Size(
                    170,
                    38);

            startingPositionButton.Location =
                new Point(
                    copyFenButton.Right + 10,
                    fenTextBox.Bottom + 10);

            startingPositionButton.Click +=
                StartingPositionButton_Click;

            Controls.Add(
                fenLabel);

            Controls.Add(
                fenTextBox);

            Controls.Add(
                loadFenButton);

            Controls.Add(
                copyFenButton);

            Controls.Add(
                startingPositionButton);
        }

        /// <summary>
        /// Updates the FEN text to match
        /// the current chess position.
        /// </summary>
        private void UpdateFenDisplay()
        {
            fenTextBox.Text =
                FenService.ExportFen(
                    chessGame);
        }

        /// <summary>
        /// Loads the position entered
        /// in the FEN text box.
        /// </summary>
        private void LoadFenButton_Click(
            object? sender,
            EventArgs e)
        {
            string enteredFen =
                fenTextBox.Text.Trim();

            bool positionLoaded =
                FenService.TryImportFen(
                    chessGame,
                    enteredFen,
                    out string message);

            if (positionLoaded)
            {
                RefreshBoard(message);
                return;
            }

            // Do not refresh the board when the FEN
            // is invalid because RefreshBoard would
            // replace the entered text.
            UpdateStatusLabel(message);

            fenTextBox.Focus();
            fenTextBox.SelectAll();
        }

        /// <summary>
        /// Copies the current board position
        /// to the Windows clipboard.
        /// </summary>
        private void CopyFenButton_Click(
            object? sender,
            EventArgs e)
        {
            // Export the actual current board,
            // not unsaved text entered by the user.
            UpdateFenDisplay();

            if (string.IsNullOrWhiteSpace(
                    fenTextBox.Text))
            {
                return;
            }

            Clipboard.SetText(
                fenTextBox.Text);

            UpdateStatusLabel(
                "FEN copied to clipboard.");
        }

        /// <summary>
        /// Restores the standard chess
        /// starting position.
        /// </summary>
        private void StartingPositionButton_Click(
            object? sender,
            EventArgs e)
        {
            fenTextBox.Text =
                FenService.StartingPositionFen;

            bool positionLoaded =
                FenService.TryImportFen(
                    chessGame,
                    fenTextBox.Text,
                    out string message);

            if (positionLoaded)
            {
                RefreshBoard(
                    "Starting position loaded.");

                return;
            }

            UpdateStatusLabel(message);
        }
    }
}