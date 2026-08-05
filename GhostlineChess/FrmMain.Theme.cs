using System.Drawing;
using System.Windows.Forms;

namespace GhostlineChess
{
    /// <summary>
    /// Contains the haunted Gothic colors,
    /// framing, coordinates, layout, and styling.
    /// </summary>
    public partial class FrmMain
    {
        // Main window colors.
        private readonly Color gothicBackground =
            Color.FromArgb(12, 9, 13);

        private readonly Color gothicPanel =
            Color.FromArgb(25, 19, 24);

        private readonly Color gothicBone =
            Color.FromArgb(225, 216, 194);

        private readonly Color gothicGold =
            Color.FromArgb(177, 136, 58);

        private readonly Color gothicBurgundy =
            Color.FromArgb(76, 10, 26);

        private readonly Color gothicButtonHover =
            Color.FromArgb(110, 18, 40);

        // Chessboard colors.
        private readonly Color gothicLightSquare =
            Color.FromArgb(184, 173, 145);

        private readonly Color gothicDarkSquare =
            Color.FromArgb(43, 14, 20);

        // Deep blood-burgundy selected square.
        // This allows the amber Hallowed Saints pieces
        // to remain clearly visible.
        private readonly Color gothicSelectedSquare =
            Color.FromArgb(86, 12, 28);

        private readonly Color gothicLegalMoveSquare =
            Color.FromArgb(76, 120, 96);

        private readonly Color gothicCaptureSquare =
            Color.FromArgb(155, 20, 39);

        // Original faction colors retained
        // for compatibility with the form.
        private readonly Color gothicWhitePiece =
            Color.FromArgb(239, 224, 187);

        private readonly Color gothicWhitePieceOutline =
            Color.FromArgb(27, 18, 23);

        private readonly Color gothicBlackPiece =
            Color.FromArgb(19, 17, 20);

        private readonly Color gothicBlackPieceGlow =
            Color.FromArgb(137, 168, 178);

        // Main interface controls.
        private readonly Label titleLabel =
            new Label();

        private readonly Label subtitleLabel =
            new Label();

        private readonly Panel boardFramePanel =
            new Panel();

        // Gothic coordinate labels.
        private readonly Label[] fileLabels =
            new Label[8];

        private readonly Label[] rankLabels =
            new Label[8];

        /// <summary>
        /// Applies the haunted Gothic appearance
        /// to the main form and its controls.
        /// </summary>
        private void ApplyGothicTheme()
        {
            SuspendLayout();

            Text =
                "Ghostline Chess";

            BackColor =
                gothicBackground;

            // Prevent the designer's AutoSize setting from
            // shrinking the form around the board and clipping
            // the right-side Chronicle panel.
            AutoSize =
                false;

            ForeColor =
                gothicBone;

            ClientSize =
                new Size(
                    1000,
                    950);

            MinimumSize =
                new Size(
                    820,
                    900);

            StartPosition =
                FormStartPosition.CenterScreen;

            CreateGothicBoardFrame();
            CreateGothicTitle();
            CreateBoardCoordinates();

            ConfigureStatusLabel();
            ConfigureFenControls();
            ConfigureMainButtons();

            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Creates the main game title and subtitle.
        /// </summary>
        private void CreateGothicTitle()
        {
            titleLabel.Text =
                "GHOSTLINE CHESS";

            titleLabel.AutoSize =
                false;

            titleLabel.Location =
                new Point(
                    boardFramePanel.Left,
                    15);

            titleLabel.Size =
                new Size(
                    boardFramePanel.Width,
                    48);

            titleLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            titleLabel.Font =
                new Font(
                    "Georgia",
                    25F,
                    FontStyle.Bold);

            titleLabel.ForeColor =
                gothicGold;

            titleLabel.BackColor =
                gothicBackground;

            subtitleLabel.Text =
                "Every move awakens something.";

            subtitleLabel.AutoSize =
                false;

            subtitleLabel.Location =
                new Point(
                    boardFramePanel.Left,
                    62);

            subtitleLabel.Size =
                new Size(
                    boardFramePanel.Width,
                    30);

            subtitleLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            subtitleLabel.Font =
                new Font(
                    "Georgia",
                    11F,
                    FontStyle.Italic);

            subtitleLabel.ForeColor =
                gothicBone;

            subtitleLabel.BackColor =
                gothicBackground;

            Controls.Add(
                titleLabel);

            Controls.Add(
                subtitleLabel);

            titleLabel.BringToFront();
            subtitleLabel.BringToFront();
        }

        /// <summary>
        /// Places the board inside an ornate frame
        /// with room for coordinates.
        /// </summary>
        private void CreateGothicBoardFrame()
        {
            const int framePadding = 8;
            const int coordinateMargin = 24;

            boardFramePanel.Location =
                new Point(
                    38,
                    112);

            boardFramePanel.Size =
                new Size(
                    boardPanel.Width +
                        framePadding * 2 +
                        coordinateMargin,

                    boardPanel.Height +
                        framePadding * 2 +
                        coordinateMargin);

            boardFramePanel.BackColor =
                gothicBackground;

            boardFramePanel.Paint +=
                BoardFramePanel_Paint;

            Controls.Add(
                boardFramePanel);

            boardPanel.Parent =
                boardFramePanel;

            boardPanel.Location =
                new Point(
                    framePadding + coordinateMargin,
                    framePadding);

            boardPanel.BackColor =
                gothicPanel;

            boardFramePanel.BringToFront();
        }

        /// <summary>
        /// Creates the A-H and 1-8 coordinate labels.
        /// </summary>
        private void CreateBoardCoordinates()
        {
            const int squareSize = 70;
            const int coordinateWidth = 24;

            string[] files =
            {
                "A", "B", "C", "D",
                "E", "F", "G", "H"
            };

            string[] ranks =
            {
                "8", "7", "6", "5",
                "4", "3", "2", "1"
            };

            for (int index = 0;
                 index < 8;
                 index++)
            {
                Label fileLabel =
                    new Label
                    {
                        Text =
                            files[index],

                        AutoSize =
                            false,

                        TextAlign =
                            ContentAlignment.MiddleCenter,

                        Font =
                            new Font(
                                "Georgia",
                                9F,
                                FontStyle.Bold),

                        ForeColor =
                            gothicGold,

                        BackColor =
                            gothicBackground,

                        Location =
                            new Point(
                                boardPanel.Left +
                                    index * squareSize,

                                boardPanel.Bottom),

                        Size =
                            new Size(
                                squareSize,
                                coordinateWidth)
                    };

                fileLabels[index] =
                    fileLabel;

                boardFramePanel.Controls.Add(
                    fileLabel);

                fileLabel.BringToFront();

                Label rankLabel =
                    new Label
                    {
                        Text =
                            ranks[index],

                        AutoSize =
                            false,

                        TextAlign =
                            ContentAlignment.MiddleCenter,

                        Font =
                            new Font(
                                "Georgia",
                                9F,
                                FontStyle.Bold),

                        ForeColor =
                            gothicGold,

                        BackColor =
                            gothicBackground,

                        Location =
                            new Point(
                                4,

                                boardPanel.Top +
                                    index * squareSize),

                        Size =
                            new Size(
                                coordinateWidth,
                                squareSize)
                    };

                rankLabels[index] =
                    rankLabel;

                boardFramePanel.Controls.Add(
                    rankLabel);

                rankLabel.BringToFront();
            }
        }

        /// <summary>
        /// Draws the gold and burgundy frame.
        /// </summary>
        private void BoardFramePanel_Paint(
            object? sender,
            PaintEventArgs e)
        {
            Rectangle outerBorder =
                new Rectangle(
                    1,
                    1,
                    boardFramePanel.Width - 3,
                    boardFramePanel.Height - 3);

            Rectangle innerBorder =
                new Rectangle(
                    5,
                    5,
                    boardFramePanel.Width - 11,
                    boardFramePanel.Height - 11);

            using Pen outerPen =
                new Pen(
                    gothicGold,
                    2F);

            using Pen innerPen =
                new Pen(
                    gothicBurgundy,
                    2F);

            e.Graphics.DrawRectangle(
                outerPen,
                outerBorder);

            e.Graphics.DrawRectangle(
                innerPen,
                innerBorder);

            DrawFrameCorner(
                e.Graphics,
                3,
                3,
                true,
                true);

            DrawFrameCorner(
                e.Graphics,
                boardFramePanel.Width - 4,
                3,
                false,
                true);

            DrawFrameCorner(
                e.Graphics,
                3,
                boardFramePanel.Height - 4,
                true,
                false);

            DrawFrameCorner(
                e.Graphics,
                boardFramePanel.Width - 4,
                boardFramePanel.Height - 4,
                false,
                false);
        }

        /// <summary>
        /// Draws one decorative frame corner.
        /// </summary>
        private void DrawFrameCorner(
            Graphics graphics,
            int x,
            int y,
            bool extendsRight,
            bool extendsDown)
        {
            const int ornamentLength = 18;

            int horizontalEnd =
                extendsRight
                    ? x + ornamentLength
                    : x - ornamentLength;

            int verticalEnd =
                extendsDown
                    ? y + ornamentLength
                    : y - ornamentLength;

            using Pen ornamentPen =
                new Pen(
                    gothicGold,
                    3F);

            graphics.DrawLine(
                ornamentPen,
                x,
                y,
                horizontalEnd,
                y);

            graphics.DrawLine(
                ornamentPen,
                x,
                y,
                x,
                verticalEnd);
        }

        /// <summary>
        /// Positions and styles the turn
        /// and game-state message.
        /// </summary>
        private void ConfigureStatusLabel()
        {
            statusLabel.AutoSize =
                false;

            statusLabel.Location =
                new Point(
                    boardFramePanel.Left,
                    boardFramePanel.Bottom + 12);

            statusLabel.Size =
                new Size(
                    ClientSize.Width -
                        boardFramePanel.Left * 2,

                    44);

            statusLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            statusLabel.BackColor =
                gothicBackground;

            statusLabel.ForeColor =
                gothicBone;

            statusLabel.Font =
                new Font(
                    "Georgia",
                    12F,
                    FontStyle.Bold);

            statusLabel.UseMnemonic =
                false;
        }

        /// <summary>
        /// Positions and styles the FEN label
        /// and editable FEN text box.
        /// </summary>
        private void ConfigureFenControls()
        {
            fenLabel.AutoSize =
                false;

            fenLabel.Location =
                new Point(
                    boardFramePanel.Left,
                    statusLabel.Bottom + 8);

            fenLabel.Size =
                new Size(
                    boardFramePanel.Width,
                    25);

            fenLabel.Text =
                "Current FEN:";

            fenLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            fenLabel.BackColor =
                gothicBackground;

            fenLabel.ForeColor =
                gothicGold;

            fenLabel.Font =
                new Font(
                    "Georgia",
                    10F,
                    FontStyle.Bold);

            fenTextBox.Location =
                new Point(
                    boardFramePanel.Left,
                    fenLabel.Bottom + 5);

            fenTextBox.Size =
                new Size(
                    boardFramePanel.Width,
                    30);

            fenTextBox.BackColor =
                gothicPanel;

            fenTextBox.ForeColor =
                gothicBone;

            fenTextBox.Font =
                new Font(
                    "Consolas",
                    10F,
                    FontStyle.Regular);

            fenTextBox.BorderStyle =
                BorderStyle.FixedSingle;
        }

        /// <summary>
        /// Positions and styles the four
        /// primary interface buttons.
        /// </summary>
        private void ConfigureMainButtons()
        {
            int buttonY =
                fenTextBox.Bottom + 10;

            loadFenButton.Text =
                "Load FEN";

            loadFenButton.Size =
                new Size(
                    120,
                    38);

            loadFenButton.Location =
                new Point(
                    boardFramePanel.Left,
                    buttonY);

            copyFenButton.Text =
                "Copy FEN";

            copyFenButton.Size =
                new Size(
                    120,
                    38);

            copyFenButton.Location =
                new Point(
                    loadFenButton.Right + 10,
                    buttonY);

            startingPositionButton.Text =
                "Starting Position";

            startingPositionButton.Size =
                new Size(
                    175,
                    38);

            startingPositionButton.Location =
                new Point(
                    copyFenButton.Right + 10,
                    buttonY);

            newGameButton.Text =
                "New Game";

            newGameButton.Size =
                new Size(
                    130,
                    38);

            newGameButton.Location =
                new Point(
                    startingPositionButton.Right + 10,
                    buttonY);

            StyleGothicButton(
                loadFenButton);

            StyleGothicButton(
                copyFenButton);

            StyleGothicButton(
                startingPositionButton);

            StyleGothicButton(
                newGameButton);
        }

        /// <summary>
        /// Applies haunted Gothic styling
        /// to one interface button.
        /// </summary>
        private void StyleGothicButton(
            Button button)
        {
            button.UseVisualStyleBackColor =
                false;

            button.FlatStyle =
                FlatStyle.Flat;

            button.BackColor =
                gothicBurgundy;

            button.ForeColor =
                gothicBone;

            button.Font =
                new Font(
                    "Georgia",
                    10F,
                    FontStyle.Bold);

            button.FlatAppearance.BorderSize =
                1;

            button.FlatAppearance.BorderColor =
                gothicGold;

            button.FlatAppearance.MouseOverBackColor =
                gothicButtonHover;

            button.FlatAppearance.MouseDownBackColor =
                gothicPanel;

            button.Cursor =
                Cursors.Hand;
        }
    }
}
