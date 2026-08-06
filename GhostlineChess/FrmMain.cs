using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;
using GhostlineChess.Models;

namespace GhostlineChess
{
    public partial class FrmMain : Form
    {
        // Stores the 64 buttons that display
        // the chessboard.
        private readonly Button[,] squareButtons =
            new Button[8, 8];

        // Stores legal destination locations
        // for the painted runic markers.
        private readonly bool[,] legalMoveSquares =
            new bool[8, 8];

        private readonly bool[,] legalCaptureSquares =
            new bool[8, 8];

        // Stores the square currently under the mouse.
        private int hoveredRow = -1;
        private int hoveredColumn = -1;

        // Stores the current chess game.
        private ChessGame chessGame =
            new ChessGame();

        // Hallowed Saints colors: ivory, cathedral gold, and sacred blue.
        private readonly Color hallowedSaintsFill =
            Color.FromArgb(231, 215, 181);

        private readonly Color hallowedSaintsBlueGlow =
            Color.FromArgb(52, 102, 190);

        private readonly Color hallowedSaintsGoldOutline =
            Color.FromArgb(205, 150, 55);

        // The Damned colors: blackened metal and infernal crimson.
        private readonly Color damnedFill =
            Color.FromArgb(18, 17, 20);

        private readonly Color damnedCrimsonGlow =
            Color.FromArgb(190, 28, 48);

        // Cathedral-stone details.
        private readonly Color cathedralMortar =
            Color.FromArgb(15, 12, 14);

        private readonly Color cathedralEdgeHighlight =
            Color.FromArgb(
                55,
                225,
                216,
                194);

        // Interactive aura colors.
        private readonly Color hoverAuraColor =
            Color.FromArgb(
                220,
                220,
                235,
                226);

        private readonly Color selectedAuraColor =
            Color.FromArgb(
                190,
                124,
                215,
                166);

        private readonly Color selectedSealColor =
            Color.FromArgb(
                220,
                170,
                22,
                45);

        private readonly Color selectedSealInnerColor =
            Color.FromArgb(
                220,
                202,
                151,
                62);

        private readonly Color legalRuneColor =
            Color.FromArgb(
                225,
                116,
                205,
                158);

        private readonly Color captureRuneColor =
            Color.FromArgb(
                245,
                210,
                30,
                54);

        // Check warning colors. These are deliberately
        // brighter than the normal capture rune so the
        // threatened king remains obvious even when the
        // player has not selected it.
        private readonly Color checkedKingAuraColor =
            Color.FromArgb(
                235,
                190,
                18,
                42);

        private readonly Color checkedKingInnerColor =
            Color.FromArgb(
                245,
                255,
                92,
                58);

        public FrmMain()
        {
            InitializeComponent();

            BuildBoard();
            BuildFenControls();
            ApplyGothicTheme();
            RefreshBoard();
        }

        /// <summary>
        /// Creates the 64 buttons used as
        /// chessboard squares.
        /// </summary>
        private void BuildBoard()
        {
            const int squareSize = 70;

            boardPanel.AutoSize =
                false;

            boardPanel.Dock =
                DockStyle.None;

            boardPanel.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left;

            boardPanel.Size =
                new Size(
                    squareSize * 8,
                    squareSize * 8);

            boardPanel.SuspendLayout();

            boardPanel.Controls.Clear();
            boardPanel.ColumnStyles.Clear();
            boardPanel.RowStyles.Clear();

            boardPanel.ColumnCount = 8;
            boardPanel.RowCount = 8;

            boardPanel.GrowStyle =
                TableLayoutPanelGrowStyle.FixedSize;

            boardPanel.Padding =
                Padding.Empty;

            boardPanel.Margin =
                Padding.Empty;

            for (int column = 0;
                 column < 8;
                 column++)
            {
                boardPanel.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        12.5F));
            }

            for (int row = 0;
                 row < 8;
                 row++)
            {
                boardPanel.RowStyles.Add(
                    new RowStyle(
                        SizeType.Percent,
                        12.5F));
            }

            for (int row = 0;
                 row < 8;
                 row++)
            {
                for (int column = 0;
                     column < 8;
                     column++)
                {
                    Button squareButton =
                        new Button
                        {
                            Dock =
                                DockStyle.Fill,

                            Margin =
                                Padding.Empty,

                            FlatStyle =
                                FlatStyle.Flat,

                            Font =
                                new Font(
                                    "Segoe UI Symbol",
                                    32F,
                                    FontStyle.Regular),

                            // X stores the row.
                            // Y stores the column.
                            Tag =
                                new Point(
                                    row,
                                    column),

                            TabStop =
                                false,

                            Text =
                                string.Empty,

                            Cursor =
                                Cursors.Hand,

                            UseVisualStyleBackColor =
                                false
                        };

                    squareButton
                        .FlatAppearance
                        .BorderSize = 0;

                    squareButton.Click +=
                        SquareButton_Click;

                    squareButton.Paint +=
                        SquareButton_Paint;

                    squareButton.MouseEnter +=
                        SquareButton_MouseEnter;

                    squareButton.MouseLeave +=
                        SquareButton_MouseLeave;

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
        /// Records the square under the mouse.
        /// </summary>
        private void SquareButton_MouseEnter(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button squareButton ||
                squareButton.Tag is not Point position)
            {
                return;
            }

            int previousRow =
                hoveredRow;

            int previousColumn =
                hoveredColumn;

            hoveredRow =
                position.X;

            hoveredColumn =
                position.Y;

            if (previousRow >= 0 &&
                previousColumn >= 0)
            {
                squareButtons[
                    previousRow,
                    previousColumn]
                    .Invalidate();
            }

            squareButton.Invalidate();
        }

        /// <summary>
        /// Removes the hover aura when the
        /// mouse leaves a board square.
        /// </summary>
        private void SquareButton_MouseLeave(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button squareButton)
            {
                return;
            }

            hoveredRow = -1;
            hoveredColumn = -1;

            squareButton.Invalidate();
        }

        /// <summary>
        /// Paints the cathedral tile, chess piece,
        /// interactive aura, and runic markers.
        /// </summary>
        private void SquareButton_Paint(
            object? sender,
            PaintEventArgs e)
        {
            if (sender is not Button squareButton)
            {
                return;
            }

            DrawCathedralTileBorder(
                e.Graphics,
                squareButton.ClientRectangle);

            if (squareButton.Tag is not Point position)
            {
                return;
            }

            Piece piece =
                chessGame
                    .Board
                    .Spots[
                        position.X,
                        position.Y]
                    .Piece;

            bool isHovered =
                hoveredRow == position.X &&
                hoveredColumn == position.Y;

            bool isSelected =
                chessGame.SelectedSpot != null &&
                chessGame.SelectedSpot.Row ==
                    position.X &&
                chessGame.SelectedSpot.Column ==
                    position.Y;

            bool isCheckedKing =
                IsCheckedKing(
                    piece);

            if (!piece.IsEmpty)
            {
                string symbol =
                    GetSolidPieceSymbol(
                        piece.Type);

                if (!string.IsNullOrEmpty(symbol))
                {
                    using GraphicsPath piecePath =
                        CreatePiecePath(
                            e.Graphics,
                            squareButton,
                            symbol);

                    e.Graphics.SmoothingMode =
                        SmoothingMode.AntiAlias;

                    e.Graphics.PixelOffsetMode =
                        PixelOffsetMode.HighQuality;

                    if (isCheckedKing)
                    {
                        DrawCheckedPieceAura(
                            e.Graphics,
                            piecePath);
                    }

                    DrawInteractivePieceAura(
                        e.Graphics,
                        piecePath,
                        isHovered,
                        isSelected);

                    if (piece.Color ==
                        PieceColor.White)
                    {
                        DrawHallowedSaintsPiece(
                            e.Graphics,
                            piecePath);
                    }
                    else
                    {
                        DrawDamnedPiece(
                            e.Graphics,
                            piecePath);
                    }
                }
            }

            if (isCheckedKing)
            {
                DrawCheckedKingWarning(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }

            // Draw the selected-piece seal after
            // the piece so it remains visible.
            if (isSelected)
            {
                DrawSelectedPieceSeal(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }

            // Capture markers use a sharp,
            // blood-red danger sigil.
            if (legalCaptureSquares[
                    position.X,
                    position.Y])
            {
                DrawCaptureRune(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }
            else if (legalMoveSquares[
                         position.X,
                         position.Y])
            {
                DrawLegalMoveRune(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }
        }

        /// <summary>
        /// Returns true when the supplied piece is a king
        /// that is currently attacked. The check marker is
        /// calculated from the board rather than from the
        /// last status message, so it also works after FEN
        /// loading and after rejected moves.
        /// </summary>
        private bool IsCheckedKing(
            Piece piece)
        {
            return
                !piece.IsEmpty &&
                piece.Type == PieceType.King &&
                MoveValidator.IsKingInCheck(
                    chessGame.Board,
                    piece.Color);
        }

        /// <summary>
        /// Draws a blood-red aura around a Unicode king
        /// while it is in check.
        /// </summary>
        private void DrawCheckedPieceAura(
            Graphics graphics,
            GraphicsPath piecePath)
        {
            using Pen outerAura =
                new Pen(
                    Color.FromArgb(
                        100,
                        checkedKingAuraColor),
                    11F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            using Pen innerAura =
                new Pen(
                    checkedKingInnerColor,
                    3.4F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                outerAura,
                piecePath);

            graphics.DrawPath(
                innerAura,
                piecePath);
        }

        /// <summary>
        /// Draws a persistent warning border and ritual ring
        /// on the square occupied by a checked king.
        /// </summary>
        private void DrawCheckedKingWarning(
            Graphics graphics,
            Rectangle squareRectangle)
        {
            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            Rectangle outerBorder =
                Rectangle.Inflate(
                    squareRectangle,
                    -3,
                    -3);

            Rectangle innerBorder =
                Rectangle.Inflate(
                    squareRectangle,
                    -6,
                    -6);

            int centerX =
                squareRectangle.Width / 2;

            int sealY =
                squareRectangle.Height - 18;

            Rectangle warningSeal =
                new Rectangle(
                    centerX - 25,
                    sealY - 9,
                    50,
                    18);

            using Pen borderGlow =
                new Pen(
                    Color.FromArgb(
                        95,
                        checkedKingAuraColor),
                    7F);

            using Pen outerPen =
                new Pen(
                    checkedKingAuraColor,
                    2.8F);

            using Pen innerPen =
                new Pen(
                    checkedKingInnerColor,
                    1.4F);

            graphics.DrawRectangle(
                borderGlow,
                outerBorder);

            graphics.DrawRectangle(
                outerPen,
                outerBorder);

            graphics.DrawRectangle(
                innerPen,
                innerBorder);

            graphics.DrawEllipse(
                borderGlow,
                warningSeal);

            graphics.DrawEllipse(
                outerPen,
                warningSeal);

            // Four short warning marks make the check
            // indicator distinct from the selection seal.
            graphics.DrawLine(
                innerPen,
                centerX - 30,
                sealY,
                centerX - 25,
                sealY);

            graphics.DrawLine(
                innerPen,
                centerX + 25,
                sealY,
                centerX + 30,
                sealY);

            graphics.DrawLine(
                innerPen,
                centerX,
                sealY - 13,
                centerX,
                sealY - 9);

            graphics.DrawLine(
                innerPen,
                centerX,
                sealY + 9,
                centerX,
                sealY + 13);
        }

        /// <summary>
        /// Creates the drawing path for one
        /// solid Unicode chess silhouette.
        /// </summary>
        private static GraphicsPath CreatePiecePath(
            Graphics graphics,
            Button squareButton,
            string symbol)
        {
            RectangleF textBounds =
                new RectangleF(
                    0,
                    -2,
                    squareButton.ClientSize.Width,
                    squareButton.ClientSize.Height);

            using StringFormat textFormat =
                new StringFormat
                {
                    Alignment =
                        StringAlignment.Center,

                    LineAlignment =
                        StringAlignment.Center
                };

            float fontSize =
                graphics.DpiY *
                32F /
                72F;

            GraphicsPath piecePath =
                new GraphicsPath();

            piecePath.AddString(
                symbol,
                squareButton.Font.FontFamily,
                (int)squareButton.Font.Style,
                fontSize,
                textBounds,
                textFormat);

            return piecePath;
        }

        /// <summary>
        /// Intensifies the glow of hovered
        /// and selected chess pieces.
        /// </summary>
        private void DrawInteractivePieceAura(
            Graphics graphics,
            GraphicsPath piecePath,
            bool isHovered,
            bool isSelected)
        {
            if (isSelected)
            {
                // A controlled spectral aura.
                // This is narrower than the previous
                // version so it does not wash out
                // the ivory-and-gold Hallowed Saints pieces.
                using Pen outerAura =
                    new Pen(
                        Color.FromArgb(
                            65,
                            selectedAuraColor),
                        8F)
                    {
                        LineJoin =
                            LineJoin.Round
                    };

                using Pen innerAura =
                    new Pen(
                        Color.FromArgb(
                            180,
                            selectedAuraColor),
                        3.8F)
                    {
                        LineJoin =
                            LineJoin.Round
                    };

                graphics.DrawPath(
                    outerAura,
                    piecePath);

                graphics.DrawPath(
                    innerAura,
                    piecePath);

                return;
            }

            if (isHovered)
            {
                using Pen hoverAura =
                    new Pen(
                        Color.FromArgb(
                            140,
                            hoverAuraColor),
                        5F)
                    {
                        LineJoin =
                            LineJoin.Round
                    };

                graphics.DrawPath(
                    hoverAura,
                    piecePath);
            }
        }

        /// <summary>
        /// Draws a ritual seal beneath
        /// the currently selected piece.
        /// </summary>
        private void DrawSelectedPieceSeal(
            Graphics graphics,
            Rectangle squareRectangle)
        {
            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            int centerX =
                squareRectangle.Width / 2;

            int sealY =
                squareRectangle.Height - 19;

            Rectangle outerSeal =
                new Rectangle(
                    centerX - 22,
                    sealY - 8,
                    44,
                    16);

            Rectangle innerSeal =
                new Rectangle(
                    centerX - 16,
                    sealY - 5,
                    32,
                    10);

            using Pen crimsonGlow =
                new Pen(
                    Color.FromArgb(
                        85,
                        selectedSealColor),
                    6F);

            using Pen crimsonPen =
                new Pen(
                    selectedSealColor,
                    2.3F);

            using Pen goldPen =
                new Pen(
                    selectedSealInnerColor,
                    1.5F);

            graphics.DrawEllipse(
                crimsonGlow,
                outerSeal);

            graphics.DrawEllipse(
                crimsonPen,
                outerSeal);

            graphics.DrawEllipse(
                goldPen,
                innerSeal);

            // Small ritual marks.
            graphics.DrawLine(
                crimsonPen,
                centerX - 27,
                sealY,
                centerX - 22,
                sealY);

            graphics.DrawLine(
                crimsonPen,
                centerX + 22,
                sealY,
                centerX + 27,
                sealY);

            graphics.DrawLine(
                crimsonPen,
                centerX,
                sealY - 12,
                centerX,
                sealY - 8);

            graphics.DrawLine(
                crimsonPen,
                centerX,
                sealY + 8,
                centerX,
                sealY + 12);
        }

        /// <summary>
        /// Draws a glowing spectral ring on
        /// an empty legal destination square.
        /// </summary>
        private void DrawLegalMoveRune(
            Graphics graphics,
            Rectangle squareRectangle)
        {
            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            int centerX =
                squareRectangle.Width / 2;

            int centerY =
                squareRectangle.Height / 2;

            Rectangle runeCircle =
                new Rectangle(
                    centerX - 12,
                    centerY - 12,
                    24,
                    24);

            using Pen glowPen =
                new Pen(
                    Color.FromArgb(
                        75,
                        legalRuneColor),
                    7F);

            using Pen runePen =
                new Pen(
                    legalRuneColor,
                    2.4F);

            graphics.DrawEllipse(
                glowPen,
                runeCircle);

            graphics.DrawEllipse(
                runePen,
                runeCircle);

            graphics.DrawLine(
                runePen,
                centerX,
                centerY - 18,
                centerX,
                centerY - 12);

            graphics.DrawLine(
                runePen,
                centerX,
                centerY + 12,
                centerX,
                centerY + 18);

            graphics.DrawLine(
                runePen,
                centerX - 18,
                centerY,
                centerX - 12,
                centerY);

            graphics.DrawLine(
                runePen,
                centerX + 12,
                centerY,
                centerX + 18,
                centerY);
        }

        /// <summary>
        /// Draws a sharp blood-red danger sigil
        /// around an opposing capturable piece.
        /// </summary>
        private void DrawCaptureRune(
            Graphics graphics,
            Rectangle squareRectangle)
        {
            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            int centerX =
                squareRectangle.Width / 2;

            int centerY =
                squareRectangle.Height / 2;

            Rectangle outerCircle =
                new Rectangle(
                    7,
                    7,
                    squareRectangle.Width - 15,
                    squareRectangle.Height - 15);

            Rectangle innerCircle =
                new Rectangle(
                    13,
                    13,
                    squareRectangle.Width - 27,
                    squareRectangle.Height - 27);

            using Pen glowPen =
                new Pen(
                    Color.FromArgb(
                        95,
                        captureRuneColor),
                    8F);

            using Pen runePen =
                new Pen(
                    captureRuneColor,
                    2.8F);

            using Pen innerPen =
                new Pen(
                    Color.FromArgb(
                        210,
                        255,
                        90,
                        90),
                    1.5F);

            graphics.DrawEllipse(
                glowPen,
                outerCircle);

            graphics.DrawEllipse(
                runePen,
                outerCircle);

            graphics.DrawEllipse(
                innerPen,
                innerCircle);

            // Four sharp inward danger marks.
            graphics.DrawLine(
                runePen,
                centerX,
                3,
                centerX,
                13);

            graphics.DrawLine(
                runePen,
                centerX,
                squareRectangle.Height - 4,
                centerX,
                squareRectangle.Height - 14);

            graphics.DrawLine(
                runePen,
                3,
                centerY,
                13,
                centerY);

            graphics.DrawLine(
                runePen,
                squareRectangle.Width - 4,
                centerY,
                squareRectangle.Width - 14,
                centerY);

            // Diagonal ritual cuts.
            const int markLength = 10;

            graphics.DrawLine(
                runePen,
                8,
                8,
                8 + markLength,
                8 + markLength);

            graphics.DrawLine(
                runePen,
                squareRectangle.Width - 9,
                8,
                squareRectangle.Width - 9 - markLength,
                8 + markLength);

            graphics.DrawLine(
                runePen,
                8,
                squareRectangle.Height - 9,
                8 + markLength,
                squareRectangle.Height - 9 - markLength);

            graphics.DrawLine(
                runePen,
                squareRectangle.Width - 9,
                squareRectangle.Height - 9,
                squareRectangle.Width - 9 - markLength,
                squareRectangle.Height - 9 - markLength);
        }

        /// <summary>
        /// Draws subtle mortar and raised edges
        /// around one cathedral-stone tile.
        /// </summary>
        private void DrawCathedralTileBorder(
            Graphics graphics,
            Rectangle tileRectangle)
        {
            if (tileRectangle.Width <= 1 ||
                tileRectangle.Height <= 1)
            {
                return;
            }

            SmoothingMode originalSmoothing =
                graphics.SmoothingMode;

            graphics.SmoothingMode =
                SmoothingMode.None;

            Rectangle mortarRectangle =
                new Rectangle(
                    0,
                    0,
                    tileRectangle.Width - 1,
                    tileRectangle.Height - 1);

            using Pen mortarPen =
                new Pen(
                    cathedralMortar,
                    1F);

            graphics.DrawRectangle(
                mortarPen,
                mortarRectangle);

            using Pen highlightPen =
                new Pen(
                    cathedralEdgeHighlight,
                    1F);

            graphics.DrawLine(
                highlightPen,
                1,
                1,
                tileRectangle.Width - 2,
                1);

            graphics.DrawLine(
                highlightPen,
                1,
                1,
                1,
                tileRectangle.Height - 2);

            graphics.SmoothingMode =
                originalSmoothing;
        }

        /// <summary>
        /// Draws a Hallowed Saints piece using an
        /// ivory body, sacred blue glow, and gold edge.
        /// </summary>
        private void DrawHallowedSaintsPiece(
            Graphics graphics,
            GraphicsPath piecePath)
        {
            using Pen blueGlowPen =
                new Pen(
                    Color.FromArgb(
                        95,
                        hallowedSaintsBlueGlow),
                    6.5F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                blueGlowPen,
                piecePath);

            using Pen goldGlowPen =
                new Pen(
                    Color.FromArgb(
                        145,
                        hallowedSaintsGoldOutline),
                    4F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                goldGlowPen,
                piecePath);

            using SolidBrush fillBrush =
                new SolidBrush(
                    hallowedSaintsFill);

            graphics.FillPath(
                fillBrush,
                piecePath);

            using Pen outlinePen =
                new Pen(
                    hallowedSaintsGoldOutline,
                    2.2F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                outlinePen,
                piecePath);
        }

        /// <summary>
        /// Draws a Damned piece using a
        /// charcoal body and infernal crimson glow.
        /// </summary>
        private void DrawDamnedPiece(
            Graphics graphics,
            GraphicsPath piecePath)
        {
            using Pen glowPen =
                new Pen(
                    Color.FromArgb(
                        90,
                        damnedCrimsonGlow),
                    5F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                glowPen,
                piecePath);

            using SolidBrush fillBrush =
                new SolidBrush(
                    damnedFill);

            graphics.FillPath(
                fillBrush,
                piecePath);

            using Pen outlinePen =
                new Pen(
                    damnedCrimsonGlow,
                    2.3F)
                {
                    LineJoin =
                        LineJoin.Round
                };

            graphics.DrawPath(
                outlinePen,
                piecePath);
        }

        /// <summary>
        /// Returns a solid Unicode silhouette.
        /// </summary>
        private static string GetSolidPieceSymbol(
            PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => "♟",
                PieceType.Rook => "♜",
                PieceType.Knight => "♞",
                PieceType.Bishop => "♝",
                PieceType.Queen => "♛",
                PieceType.King => "♚",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Handles clicks on board squares.
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

            bool actionCompleted =
                chessGame.SelectSpot(
                    position.X,
                    position.Y,
                    out string message);

            RefreshBoard(
                message);

            if (actionCompleted &&
                chessGame.PromotionPending)
            {
                if (aiMovePending &&
                    chessGame.Turn == aiColor)
                {
                    chessGame.CompletePromotion(
                        PieceType.Queen,
                        out string promotionMessage);

                    RefreshBoard(
                        promotionMessage);
                }
                else
                {
                    ShowPromotionDialog();
                }
            }
        }

        /// <summary>
        /// Displays a Gothic pawn-promotion window.
        /// </summary>
        private void ShowPromotionDialog()
        {
            using Form promotionDialog =
                new Form
                {
                    Text =
                        "Pawn Promotion",

                    StartPosition =
                        FormStartPosition.CenterParent,

                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,

                    ClientSize =
                        new Size(
                            460,
                            150),

                    MaximizeBox = false,
                    MinimizeBox = false,
                    ControlBox = false,
                    ShowInTaskbar = false,

                    BackColor =
                        gothicBackground,

                    ForeColor =
                        gothicBone
                };

            Label instructionLabel =
                new Label
                {
                    Text =
                        "Choose the form your pawn will awaken as:",

                    AutoSize =
                        false,

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Font =
                        new Font(
                            "Georgia",
                            12F,
                            FontStyle.Bold),

                    ForeColor =
                        gothicGold,

                    BackColor =
                        gothicBackground,

                    Location =
                        new Point(
                            10,
                            10),

                    Size =
                        new Size(
                            440,
                            35)
                };

            promotionDialog.Controls.Add(
                instructionLabel);

            PieceType[] promotionTypes =
            {
                PieceType.Queen,
                PieceType.Rook,
                PieceType.Bishop,
                PieceType.Knight
            };

            string[] buttonTexts =
            {
                "Queen",
                "Rook",
                "Bishop",
                "Knight"
            };

            for (int index = 0;
                 index < promotionTypes.Length;
                 index++)
            {
                PieceType promotionType =
                    promotionTypes[index];

                Button promotionButton =
                    new Button
                    {
                        Text =
                            buttonTexts[index],

                        Size =
                            new Size(
                                100,
                                55),

                        Location =
                            new Point(
                                15 + index * 110,
                                65),

                        Tag =
                            promotionType
                    };

                StyleGothicButton(
                    promotionButton);

                promotionButton.Click +=
                    PromotionButton_Click;

                promotionDialog.Controls.Add(
                    promotionButton);
            }

            promotionDialog.ShowDialog(
                this);
        }

        /// <summary>
        /// Handles the pawn-promotion choice.
        /// </summary>
        private void PromotionButton_Click(
            object? sender,
            EventArgs e)
        {
            if (sender is not Button promotionButton)
            {
                return;
            }

            if (promotionButton.Tag is not
                PieceType promotionType)
            {
                return;
            }

            chessGame.CompletePromotion(
                promotionType,
                out string message);

            RefreshBoard(
                message);

            Form? promotionDialog =
                promotionButton.FindForm();

            if (promotionDialog != null)
            {
                promotionDialog.DialogResult =
                    DialogResult.OK;

                promotionDialog.Close();
            }
        }

        /// <summary>
        /// Updates pieces, colors, runes,
        /// messages, and FEN.
        /// </summary>
        private void RefreshBoard(
            string message = "")
        {
            bool boardCanBeUsed =
                chessGame.Result ==
                    GameResult.InProgress &&
                !chessGame.PromotionPending;

            Array.Clear(
                legalMoveSquares,
                0,
                legalMoveSquares.Length);

            Array.Clear(
                legalCaptureSquares,
                0,
                legalCaptureSquares.Length);

            foreach (Spot destination in
                     chessGame
                         .GetLegalMovesForSelectedPiece())
            {
                bool isCapture =
                    IsCaptureDestination(
                        destination);

                if (isCapture)
                {
                    legalCaptureSquares[
                        destination.Row,
                        destination.Column] = true;
                }
                else
                {
                    legalMoveSquares[
                        destination.Row,
                        destination.Column] = true;
                }
            }

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

                    bool isLightSquare =
                        (row + column) % 2 == 0;

                    squareButton.Text =
                        string.Empty;

                    squareButton.BackColor =
                        isLightSquare
                            ? gothicLightSquare
                            : gothicDarkSquare;

                    // The selected square now uses
                    // deep blood burgundy instead of gold.
                    if (chessGame.SelectedSpot != null &&
                        chessGame.SelectedSpot.Row == row &&
                        chessGame.SelectedSpot.Column ==
                            column)
                    {
                        squareButton.BackColor =
                            gothicSelectedSquare;
                    }

                    squareButton
                        .FlatAppearance
                        .MouseOverBackColor =
                            squareButton.BackColor;

                    squareButton
                        .FlatAppearance
                        .MouseDownBackColor =
                            squareButton.BackColor;

                    squareButton.Enabled =
                        boardCanBeUsed;

                    squareButton.Cursor =
                        boardCanBeUsed
                            ? Cursors.Hand
                            : Cursors.Default;

                    squareButton.Invalidate();
                }
            }

            UpdateStatusLabel(
                message);

            UpdateFenDisplay();
        }

        /// <summary>
        /// Returns true when a legal destination
        /// captures an opposing piece.
        /// </summary>
        private bool IsCaptureDestination(
            Spot destination)
        {
            if (!destination.Piece.IsEmpty)
            {
                return true;
            }

            Spot? selectedSpot =
                chessGame.SelectedSpot;

            if (selectedSpot == null)
            {
                return false;
            }

            bool isEnPassant =
                selectedSpot.Piece.Type ==
                    PieceType.Pawn &&
                destination.Piece.IsEmpty &&
                Math.Abs(
                    destination.Column -
                    selectedSpot.Column) == 1;

            return isEnPassant;
        }

        /// <summary>
        /// Displays the turn or final result.
        /// </summary>
        private void UpdateStatusLabel(
            string message)
        {
            switch (chessGame.Result)
            {
                case GameResult.WhiteWon:
                    statusLabel.Text =
                        "Checkmate — The Damned throne has fallen. " +
                        "The Hallowed Saints prevail!";

                    statusLabel.ForeColor =
                        gothicCaptureSquare;
                    break;

                case GameResult.BlackWon:
                    statusLabel.Text =
                        "Checkmate — The Hallowed Saints' throne has fallen. " +
                        "The Damned prevail!";

                    statusLabel.ForeColor =
                        gothicCaptureSquare;
                    break;

                case GameResult.Draw:
                    statusLabel.Text =
                        "Stalemate — Both factions are trapped " +
                        "within the darkness.";

                    statusLabel.ForeColor =
                        gothicGold;
                    break;

                case GameResult.InsufficientMaterial:
                    statusLabel.Text =
                        "Draw — Neither faction has enough power " +
                        "remaining to deliver checkmate.";

                    statusLabel.ForeColor =
                        gothicGold;
                    break;

                case GameResult.ThreefoldRepetition:
                    statusLabel.Text =
                        "Draw — The same fate has returned " +
                        "three times through the Ghostline.";

                    statusLabel.ForeColor =
                        gothicGold;
                    break;

                case GameResult.FiftyMoveRule:
                    statusLabel.Text =
                        "Draw — Fifty moves have passed without " +
                        "bloodshed or a pawn's advance.";

                    statusLabel.ForeColor =
                        gothicGold;
                    break;

                default:
                    bool currentKingInCheck =
                        MoveValidator.IsKingInCheck(
                            chessGame.Board,
                            chessGame.Turn);

                    string displayMessage =
                        RemoveRedundantCheckNotice(
                            message,
                            chessGame.Turn);

                    if (currentKingInCheck)
                    {
                        statusLabel.ForeColor =
                            gothicCaptureSquare;

                        string checkedKingName =
                            chessGame.Turn ==
                                PieceColor.White
                                ? "The Hallowed Saints' King"
                                : "The Damned King";

                        string factionTurn =
                            GetFactionName(
                                chessGame.Turn);

                        statusLabel.Text =
                            $"{factionTurn} to move — " +
                            $"{checkedKingName} is in check.";
                    }
                    else
                    {
                        statusLabel.ForeColor =
                            gothicBone;

                        string atmosphere =
                            chessGame.Turn ==
                                PieceColor.White
                                ? "The Hallowed Saints awaken."
                                : "The Damned advance.";

                        statusLabel.Text =
                            $"{GetFactionName(chessGame.Turn)} to move — " +
                            atmosphere;
                    }

                    if (!string.IsNullOrWhiteSpace(
                            displayMessage))
                    {
                        statusLabel.Text +=
                            $" | {displayMessage}";
                    }

                    break;
            }
        }

        /// <summary>
        /// Returns the public faction name used by
        /// the themed interface for a piece color.
        /// </summary>
        private static string GetFactionName(
            PieceColor color)
        {
            return color == PieceColor.White
                ? "The Hallowed Saints"
                : "The Damned";
        }

        /// <summary>
        /// ChessGame appends a plain-language check notice to
        /// successful move messages. The status banner now has
        /// a dedicated check headline, so remove that duplicate
        /// suffix while preserving the rest of the message.
        /// </summary>
        private static string
            RemoveRedundantCheckNotice(
                string message,
                PieceColor checkedColor)
        {
            if (string.IsNullOrWhiteSpace(
                    message))
            {
                return string.Empty;
            }

            string checkSuffix =
                $"{GetFactionName(checkedColor)} are in check.";

            string trimmedMessage =
                message.Trim();

            if (!trimmedMessage.EndsWith(
                    checkSuffix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return trimmedMessage;
            }

            return trimmedMessage[
                    ..^checkSuffix.Length]
                .TrimEnd();
        }

        /// <summary>
        /// Starts a completely new game.
        /// </summary>
        private void newGameButton_Click(
            object sender,
            EventArgs e)
        {
            chessGame =
                new ChessGame();

            hoveredRow = -1;
            hoveredColumn = -1;

            RefreshBoard(
                "A new conflict begins.");
        }
    }
}
