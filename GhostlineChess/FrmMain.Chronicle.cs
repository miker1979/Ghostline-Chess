using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;
using GhostlineChess.Models;

namespace GhostlineChess
{
    /// <summary>
    /// Contains the Ghostline Tome,
    /// captured-piece Graveyard, and
    /// move-recording support.
    /// </summary>
    public partial class FrmMain
    {
        // Right-side Gothic panel.
        private readonly Panel chroniclePanel =
            new Panel();

        private readonly Label tomeTitleLabel =
            new Label();

        private readonly RichTextBox moveJournalBox =
            new RichTextBox();

        private readonly Label graveyardTitleLabel =
            new Label();

        private readonly Label hallowedCasualtiesTitleLabel =
            new Label();

        private readonly Label hallowedCasualtiesIconsLabel =
            new Label();

        private readonly Label damnedCasualtiesTitleLabel =
            new Label();

        private readonly Label damnedCasualtiesIconsLabel =
            new Label();

        // Stores completed moves for the Tome.
        private readonly List<JournalEntry>
            journalEntries =
                new List<JournalEntry>();

        // Hallowed Saints pieces that have been captured.
        private readonly List<PieceType>
            capturedHallowedPieces =
                new List<PieceType>();

        // The Damned pieces that have been captured.
        private readonly List<PieceType>
            capturedDamnedPieces =
                new List<PieceType>();

        // Promotion moves are recorded after the
        // player selects the promotion piece.
        private JournalMoveContext?
            pendingPromotionJournalMove;

        /// <summary>
        /// Represents one completed move
        /// displayed inside the Tome.
        /// </summary>
        private sealed class JournalEntry
        {
            public PieceColor Color { get; }

            public string Notation { get; }

            public JournalEntry(
                PieceColor color,
                string notation)
            {
                Color = color;
                Notation = notation;
            }
        }

        /// <summary>
        /// Stores information about a move before
        /// ChessGame changes the board.
        /// </summary>
        private sealed class JournalMoveContext
        {
            public PieceColor MovingColor { get; init; }

            public PieceType MovingType { get; init; }

            public int StartRow { get; init; }

            public int StartColumn { get; init; }

            public int EndRow { get; init; }

            public int EndColumn { get; init; }

            public bool IsCastling { get; init; }

            public PieceType? CapturedType { get; init; }

            public PieceColor? CapturedColor { get; init; }
        }

        /// <summary>
        /// Creates the right-side Tome
        /// and Graveyard interface.
        /// </summary>
        private void BuildChroniclePanel()
        {
            // Expand the window so the complete
            // Chronicle panel fits comfortably.
            ClientSize =
                new Size(
                    1090,
                    950);

            MinimumSize =
                new Size(
                    1080,
                    900);

            chroniclePanel.Location =
                new Point(
                    boardFramePanel.Right + 20,
                    boardFramePanel.Top);

            // The graveyard controls extend slightly below
            // the board frame. Give the panel enough height
            // to show the complete tray for The Damned and its
            // bottom padding without clipping.
            chroniclePanel.Size =
                new Size(
                    370,
                    boardFramePanel.Height + 20);

            chroniclePanel.BackColor =
                gothicPanel;

            chroniclePanel.AutoScroll =
                false;

            chroniclePanel.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left;

            chroniclePanel.Paint +=
                ChroniclePanel_Paint;

            ConfigureTomeControls();
            ConfigureGraveyardControls();

            Controls.Add(
                chroniclePanel);

            chroniclePanel.BringToFront();

            RefreshChroniclePanel();
        }

        /// <summary>
        /// Creates the move-journal title
        /// and scrollable move list.
        /// </summary>
        private void ConfigureTomeControls()
        {
            tomeTitleLabel.Text =
                "[ THE GHOSTLINE TOME ]";

            tomeTitleLabel.AutoSize =
                false;

            tomeTitleLabel.Location =
                new Point(
                    10,
                    12);

            tomeTitleLabel.Size =
                new Size(
                    chroniclePanel.Width - 20,
                    42);

            tomeTitleLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            tomeTitleLabel.Font =
                new Font(
                    "Georgia",
                    12F,
                    FontStyle.Bold);

            tomeTitleLabel.ForeColor =
                gothicGold;

            tomeTitleLabel.BackColor =
                gothicPanel;

            tomeTitleLabel.UseMnemonic =
                false;

            moveJournalBox.Location =
                new Point(
                    18,
                    58);

            moveJournalBox.Size =
                new Size(
                    chroniclePanel.Width - 36,
                    285);

            moveJournalBox.ReadOnly =
                true;

            moveJournalBox.BorderStyle =
                BorderStyle.None;

            moveJournalBox.BackColor =
                Color.FromArgb(
                    18,
                    14,
                    18);

            moveJournalBox.ForeColor =
                gothicBone;

            moveJournalBox.Font =
                new Font(
                    "Consolas",
                    11F,
                    FontStyle.Regular);

            moveJournalBox.ScrollBars =
                RichTextBoxScrollBars.Vertical;

            moveJournalBox.DetectUrls =
                false;

            moveJournalBox.WordWrap =
                false;

            moveJournalBox.TabStop =
                false;

            chroniclePanel.Controls.Add(
                tomeTitleLabel);

            chroniclePanel.Controls.Add(
                moveJournalBox);
        }

        /// <summary>
        /// Creates the captured-piece section.
        /// </summary>
        private void ConfigureGraveyardControls()
        {
            graveyardTitleLabel.Text =
                "[ THE GRAVEYARD ]";

            graveyardTitleLabel.AutoSize =
                false;

            graveyardTitleLabel.Location =
                new Point(
                    10,
                    360);

            graveyardTitleLabel.Size =
                new Size(
                    chroniclePanel.Width - 20,
                    40);

            graveyardTitleLabel.TextAlign =
                ContentAlignment.MiddleCenter;

            graveyardTitleLabel.Font =
                new Font(
                    "Georgia",
                    13F,
                    FontStyle.Bold);

            graveyardTitleLabel.ForeColor =
                gothicGold;

            graveyardTitleLabel.BackColor =
                gothicPanel;

            graveyardTitleLabel.UseMnemonic =
                false;

            hallowedCasualtiesTitleLabel.AutoSize =
                false;

            hallowedCasualtiesTitleLabel.Location =
                new Point(
                    20,
                    412);

            hallowedCasualtiesTitleLabel.Size =
                new Size(
                    chroniclePanel.Width - 40,
                    26);

            hallowedCasualtiesTitleLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            hallowedCasualtiesTitleLabel.Font =
                new Font(
                    "Georgia",
                    9F,
                    FontStyle.Bold);

            hallowedCasualtiesTitleLabel.ForeColor =
                hallowedSaintsGoldOutline;

            hallowedCasualtiesTitleLabel.BackColor =
                gothicPanel;

            hallowedCasualtiesIconsLabel.AutoSize =
                false;

            hallowedCasualtiesIconsLabel.Location =
                new Point(
                    20,
                    442);

            hallowedCasualtiesIconsLabel.Size =
                new Size(
                    chroniclePanel.Width - 40,
                    58);

            hallowedCasualtiesIconsLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            hallowedCasualtiesIconsLabel.Font =
                new Font(
                    "Segoe UI Symbol",
                    19F,
                    FontStyle.Regular);

            hallowedCasualtiesIconsLabel.ForeColor =
                hallowedSaintsGoldOutline;

            hallowedCasualtiesIconsLabel.BackColor =
                Color.FromArgb(
                    18,
                    14,
                    18);

            hallowedCasualtiesIconsLabel.Padding =
                new Padding(
                    10,
                    0,
                    5,
                    0);

            damnedCasualtiesTitleLabel.AutoSize =
                false;

            damnedCasualtiesTitleLabel.Location =
                new Point(
                    20,
                    518);

            damnedCasualtiesTitleLabel.Size =
                new Size(
                    chroniclePanel.Width - 40,
                    26);

            damnedCasualtiesTitleLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            damnedCasualtiesTitleLabel.Font =
                new Font(
                    "Georgia",
                    9F,
                    FontStyle.Bold);

            damnedCasualtiesTitleLabel.ForeColor =
                damnedCrimsonGlow;

            damnedCasualtiesTitleLabel.BackColor =
                gothicPanel;

            damnedCasualtiesIconsLabel.AutoSize =
                false;

            damnedCasualtiesIconsLabel.Location =
                new Point(
                    20,
                    548);

            damnedCasualtiesIconsLabel.Size =
                new Size(
                    chroniclePanel.Width - 40,
                    58);

            damnedCasualtiesIconsLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            damnedCasualtiesIconsLabel.Font =
                new Font(
                    "Segoe UI Symbol",
                    19F,
                    FontStyle.Regular);

            damnedCasualtiesIconsLabel.ForeColor =
                damnedCrimsonGlow;

            damnedCasualtiesIconsLabel.BackColor =
                Color.FromArgb(
                    18,
                    14,
                    18);

            damnedCasualtiesIconsLabel.Padding =
                new Padding(
                    10,
                    0,
                    5,
                    0);

            chroniclePanel.Controls.Add(
                graveyardTitleLabel);

            chroniclePanel.Controls.Add(
                hallowedCasualtiesTitleLabel);

            chroniclePanel.Controls.Add(
                hallowedCasualtiesIconsLabel);

            chroniclePanel.Controls.Add(
                damnedCasualtiesTitleLabel);

            chroniclePanel.Controls.Add(
                damnedCasualtiesIconsLabel);
        }

        /// <summary>
        /// Draws the Gothic frame and separator
        /// inside the right-side panel.
        /// </summary>
        private void ChroniclePanel_Paint(
            object? sender,
            PaintEventArgs e)
        {
            Rectangle outerBorder =
                new Rectangle(
                    1,
                    1,
                    chroniclePanel.Width - 3,
                    chroniclePanel.Height - 3);

            Rectangle innerBorder =
                new Rectangle(
                    5,
                    5,
                    chroniclePanel.Width - 11,
                    chroniclePanel.Height - 11);

            using Pen outerPen =
                new Pen(
                    gothicGold,
                    2F);

            using Pen innerPen =
                new Pen(
                    gothicBurgundy,
                    2F);

            using Pen separatorPen =
                new Pen(
                    gothicGold,
                    1F);

            e.Graphics.DrawRectangle(
                outerPen,
                outerBorder);

            e.Graphics.DrawRectangle(
                innerPen,
                innerBorder);

            e.Graphics.DrawLine(
                separatorPen,
                15,
                352,
                chroniclePanel.Width - 16,
                352);
        }

        /// <summary>
        /// Captures information about the attempted
        /// move before the board changes.
        /// </summary>
        private JournalMoveContext?
            CreateJournalMoveContext(
                Point destination)
        {
            Spot? startingSpot =
                chessGame.SelectedSpot;

            if (startingSpot == null ||
                startingSpot.Piece.IsEmpty)
            {
                return null;
            }

            Piece movingPiece =
                startingSpot.Piece;

            Piece destinationPiece =
                chessGame.Board
                    .Spots[
                        destination.X,
                        destination.Y]
                    .Piece;

            PieceType? capturedType =
                null;

            PieceColor? capturedColor =
                null;

            // Normal capture.
            if (!destinationPiece.IsEmpty &&
                destinationPiece.Color !=
                    movingPiece.Color)
            {
                capturedType =
                    destinationPiece.Type;

                capturedColor =
                    destinationPiece.Color;
            }
            else
            {
                // Possible en passant capture.
                bool pawnMovedDiagonally =
                    movingPiece.Type ==
                        PieceType.Pawn &&
                    destinationPiece.IsEmpty &&
                    Math.Abs(
                        destination.Y -
                        startingSpot.Column) == 1;

                if (pawnMovedDiagonally)
                {
                    Piece adjacentPiece =
                        chessGame.Board
                            .Spots[
                                startingSpot.Row,
                                destination.Y]
                            .Piece;

                    if (!adjacentPiece.IsEmpty &&
                        adjacentPiece.Type ==
                            PieceType.Pawn &&
                        adjacentPiece.Color !=
                            movingPiece.Color)
                    {
                        capturedType =
                            adjacentPiece.Type;

                        capturedColor =
                            adjacentPiece.Color;
                    }
                }
            }

            bool isCastling =
                movingPiece.Type ==
                    PieceType.King &&
                startingSpot.Row ==
                    destination.X &&
                Math.Abs(
                    destination.Y -
                    startingSpot.Column) == 2;

            return new JournalMoveContext
            {
                MovingColor =
                    movingPiece.Color,

                MovingType =
                    movingPiece.Type,

                StartRow =
                    startingSpot.Row,

                StartColumn =
                    startingSpot.Column,

                EndRow =
                    destination.X,

                EndColumn =
                    destination.Y,

                IsCastling =
                    isCastling,

                CapturedType =
                    capturedType,

                CapturedColor =
                    capturedColor
            };
        }

        /// <summary>
        /// Returns true when SelectSpot created
        /// a new completed LastMove object.
        /// </summary>
        private bool DidMoveOccur(
            LastMove? previousMoveBeforeClick)
        {
            return !ReferenceEquals(
                previousMoveBeforeClick,
                chessGame.PreviousMove);
        }

        /// <summary>
        /// Records a completed board move.
        /// Promotion notation is delayed until
        /// the promotion choice is known.
        /// </summary>
        private void RecordCompletedMove(
            JournalMoveContext context)
        {
            RecordCapturedPiece(
                context);

            if (chessGame.PromotionPending)
            {
                pendingPromotionJournalMove =
                    context;

                RefreshChroniclePanel();
                return;
            }

            AddJournalEntry(
                context,
                null);

            RefreshChroniclePanel();
        }

        /// <summary>
        /// Finishes the journal entry for a
        /// pawn-promotion move.
        /// </summary>
        private void CompletePromotionJournalEntry(
            PieceType promotionType)
        {
            if (pendingPromotionJournalMove == null)
            {
                return;
            }

            AddJournalEntry(
                pendingPromotionJournalMove,
                promotionType);

            pendingPromotionJournalMove =
                null;

            RefreshChroniclePanel();
        }

        /// <summary>
        /// Adds a captured piece to the
        /// correct Graveyard tray.
        /// </summary>
        private void RecordCapturedPiece(
            JournalMoveContext context)
        {
            if (!context.CapturedType.HasValue ||
                !context.CapturedColor.HasValue)
            {
                return;
            }

            if (context.CapturedColor.Value ==
                PieceColor.White)
            {
                capturedHallowedPieces.Add(
                    context.CapturedType.Value);
            }
            else
            {
                capturedDamnedPieces.Add(
                    context.CapturedType.Value);
            }
        }

        /// <summary>
        /// Adds one completed move to the Tome.
        /// </summary>
        private void AddJournalEntry(
            JournalMoveContext context,
            PieceType? promotionType)
        {
            string notation =
                CreateMoveNotation(
                    context,
                    promotionType);

            journalEntries.Add(
                new JournalEntry(
                    context.MovingColor,
                    notation));
        }

        /// <summary>
        /// Creates compact algebraic notation,
        /// including captures, castling,
        /// promotion, check, and checkmate.
        /// </summary>
        private string CreateMoveNotation(
            JournalMoveContext context,
            PieceType? promotionType)
        {
            string notation;

            if (context.IsCastling)
            {
                notation =
                    context.EndColumn >
                    context.StartColumn
                        ? "O-O"
                        : "O-O-O";
            }
            else
            {
                bool isCapture =
                    context.CapturedType.HasValue;

                string pieceLetter =
                    GetNotationPieceLetter(
                        context.MovingType);

                if (context.MovingType ==
                        PieceType.Pawn &&
                    isCapture)
                {
                    pieceLetter =
                        GetFileLetter(
                            context.StartColumn)
                            .ToString();
                }

                string captureMarker =
                    isCapture
                        ? "x"
                        : string.Empty;

                string destination =
                    GetSquareName(
                        context.EndRow,
                        context.EndColumn);

                notation =
                    $"{pieceLetter}" +
                    $"{captureMarker}" +
                    $"{destination}";

                if (promotionType.HasValue)
                {
                    notation +=
                        "=" +
                        GetNotationPieceLetter(
                            promotionType.Value);
                }
            }

            notation +=
                GetCheckNotationSuffix();

            return notation;
        }

        /// <summary>
        /// Returns the standard notation letter
        /// for a chess piece.
        /// </summary>
        private static string GetNotationPieceLetter(
            PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Knight => "N",
                PieceType.Bishop => "B",
                PieceType.Rook => "R",
                PieceType.Queen => "Q",
                PieceType.King => "K",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Converts a board location into
        /// a chess coordinate such as e4.
        /// </summary>
        private static string GetSquareName(
            int row,
            int column)
        {
            char file =
                GetFileLetter(
                    column);

            int rank =
                8 - row;

            return $"{file}{rank}";
        }

        /// <summary>
        /// Returns the A-H file letter.
        /// </summary>
        private static char GetFileLetter(
            int column)
        {
            return
                (char)('a' + column);
        }

        /// <summary>
        /// Adds check or checkmate notation.
        /// </summary>
        private string GetCheckNotationSuffix()
        {
            if (chessGame.Result ==
                    GameResult.WhiteWon ||
                chessGame.Result ==
                    GameResult.BlackWon)
            {
                return "#";
            }

            if (MoveValidator.IsKingInCheck(
                    chessGame.Board,
                    chessGame.Turn))
            {
                return "+";
            }

            return string.Empty;
        }

        /// <summary>
        /// Updates all text and captured-piece
        /// icons in the right-side panel.
        /// </summary>
        private void RefreshChroniclePanel()
        {
            moveJournalBox.Text =
                BuildMoveJournalText();

            moveJournalBox.SelectionStart =
                moveJournalBox.TextLength;

            moveJournalBox.ScrollToCaret();

            hallowedCasualtiesTitleLabel.Text =
                $"FALLEN OF THE HALLOWED SAINTS " +
                $"({capturedHallowedPieces.Count})";

            hallowedCasualtiesIconsLabel.Text =
                BuildCapturedPieceText(
                    capturedHallowedPieces);

            damnedCasualtiesTitleLabel.Text =
                $"FALLEN OF THE DAMNED " +
                $"({capturedDamnedPieces.Count})";

            damnedCasualtiesIconsLabel.Text =
                BuildCapturedPieceText(
                    capturedDamnedPieces);
        }

        /// <summary>
        /// Formats the journal into numbered
        /// White and Black move columns.
        /// </summary>
        private string BuildMoveJournalText()
        {
            if (journalEntries.Count == 0)
            {
                return
                    "The tome is silent.\n\n" +
                    "Make a move to begin\n" +
                    "the chronicle.";
            }

            StringBuilder journalText =
                new StringBuilder();

            int entryIndex = 0;
            int moveNumber = 1;

            while (entryIndex <
                   journalEntries.Count)
            {
                string whiteMove =
                    string.Empty;

                string blackMove =
                    string.Empty;

                JournalEntry currentEntry =
                    journalEntries[entryIndex];

                if (currentEntry.Color ==
                    PieceColor.White)
                {
                    whiteMove =
                        currentEntry.Notation;

                    entryIndex++;

                    if (entryIndex <
                            journalEntries.Count &&
                        journalEntries[entryIndex].Color ==
                            PieceColor.Black)
                    {
                        blackMove =
                            journalEntries[
                                entryIndex]
                                .Notation;

                        entryIndex++;
                    }
                }
                else
                {
                    // This can happen after loading
                    // a FEN where Black moves first.
                    blackMove =
                        currentEntry.Notation;

                    entryIndex++;
                }

                journalText.AppendLine(
                    $"{moveNumber,2}. " +
                    $"{whiteMove,-11}" +
                    $"{blackMove}");

                moveNumber++;
            }

            return journalText.ToString();
        }

        /// <summary>
        /// Converts captured piece types into
        /// small Gothic chess silhouettes.
        /// </summary>
        private static string BuildCapturedPieceText(
            IReadOnlyList<PieceType> capturedPieces)
        {
            if (capturedPieces.Count == 0)
            {
                return "None have fallen.";
            }

            StringBuilder icons =
                new StringBuilder();

            foreach (PieceType pieceType in
                     capturedPieces)
            {
                icons.Append(
                    GetSolidPieceSymbol(
                        pieceType));

                icons.Append(' ');
            }

            return icons.ToString();
        }

        /// <summary>
        /// Clears the Tome and Graveyard.
        /// Used by New Game and FEN loading.
        /// </summary>
        private void ResetChronicle()
        {
            journalEntries.Clear();
            capturedHallowedPieces.Clear();
            capturedDamnedPieces.Clear();

            pendingPromotionJournalMove =
                null;

            RefreshChroniclePanel();
        }
    }
}
