using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess
{
    /// <summary>
    /// Loads and renders all custom Gothic
    /// chess-piece PNG artwork.
    /// </summary>
    public partial class FrmMain
    {
        /// <summary>
        /// Stores one sprite for each faction
        /// and chess-piece type.
        /// </summary>
        private readonly Dictionary<
            (PieceColor Color, PieceType Type),
            Bitmap> pieceSprites =
                new Dictionary<
                    (PieceColor Color, PieceType Type),
                    Bitmap>();

        private bool spriteRenderingConnected;

        /// <summary>
        /// Loads the artwork after the form
        /// has finished creating its controls.
        /// </summary>
        protected override void OnShown(
            EventArgs e)
        {
            base.OnShown(e);

            if (spriteRenderingConnected)
            {
                return;
            }

            spriteRenderingConnected = true;

            LoadAllPieceSprites();

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

                    // Remove the older Unicode-only
                    // renderer from each board square.
                    squareButton.Paint -=
                        SquareButton_Paint;

                    squareButton.Paint +=
                        SquareButton_PaintWithSprites;

                    squareButton.Invalidate();
                }
            }

            FormClosed +=
                FrmMain_PieceSprites_FormClosed;
        }

        /// <summary>
        /// Loads all twelve Gothic sprites.
        /// </summary>
        private void LoadAllPieceSprites()
        {
            DisposePieceSprites();

            string piecesDirectory =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "Pieces");

            AddPieceSprite(
                PieceColor.White,
                PieceType.Pawn,
                piecesDirectory,
                "pawn_white.png");

            AddPieceSprite(
                PieceColor.White,
                PieceType.Rook,
                piecesDirectory,
                "rook_white.png");

            AddPieceSprite(
                PieceColor.White,
                PieceType.Knight,
                piecesDirectory,
                "knight_white.png");

            AddPieceSprite(
                PieceColor.White,
                PieceType.Bishop,
                piecesDirectory,
                "bishop_white.png");

            AddPieceSprite(
                PieceColor.White,
                PieceType.Queen,
                piecesDirectory,
                "queen_white.png");

            AddPieceSprite(
                PieceColor.White,
                PieceType.King,
                piecesDirectory,
                "king_white.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.Pawn,
                piecesDirectory,
                "pawn_black.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.Rook,
                piecesDirectory,
                "rook_black.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.Knight,
                piecesDirectory,
                "knight_black.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.Bishop,
                piecesDirectory,
                "bishop_black.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.Queen,
                piecesDirectory,
                "queen_black.png");

            AddPieceSprite(
                PieceColor.Black,
                PieceType.King,
                piecesDirectory,
                "king_black.png");
        }

        /// <summary>
        /// Loads one sprite and adds it
        /// to the artwork dictionary.
        /// </summary>
        private void AddPieceSprite(
            PieceColor color,
            PieceType type,
            string piecesDirectory,
            string fileName)
        {
            string fullPath =
                Path.Combine(
                    piecesDirectory,
                    fileName);

            Bitmap? sprite =
                LoadAndTrimSprite(
                    fullPath);

            if (sprite == null)
            {
                return;
            }

            pieceSprites[
                (color, type)] = sprite;
        }

        /// <summary>
        /// Loads an image without permanently
        /// locking the original PNG file.
        /// </summary>
        private static Bitmap?
            LoadAndTrimSprite(
                string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using FileStream stream =
                    new FileStream(
                        path,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read);

                using Bitmap original =
                    new Bitmap(
                        stream);

                using Bitmap unlockedCopy =
                    new Bitmap(
                        original.Width,
                        original.Height,
                        PixelFormat.Format32bppArgb);

                using (Graphics copyGraphics =
                       Graphics.FromImage(
                           unlockedCopy))
                {
                    copyGraphics.Clear(
                        Color.Transparent);

                    copyGraphics.DrawImage(
                        original,
                        new Rectangle(
                            0,
                            0,
                            unlockedCopy.Width,
                            unlockedCopy.Height));
                }

                Rectangle visibleBounds =
                    FindVisibleBounds(
                        unlockedCopy);

                if (visibleBounds.Width <= 0 ||
                    visibleBounds.Height <= 0)
                {
                    return new Bitmap(
                        unlockedCopy);
                }

                Bitmap trimmedSprite =
                    new Bitmap(
                        visibleBounds.Width,
                        visibleBounds.Height,
                        PixelFormat.Format32bppArgb);

                using Graphics graphics =
                    Graphics.FromImage(
                        trimmedSprite);

                graphics.Clear(
                    Color.Transparent);

                graphics.DrawImage(
                    unlockedCopy,
                    new Rectangle(
                        0,
                        0,
                        trimmedSprite.Width,
                        trimmedSprite.Height),
                    visibleBounds,
                    GraphicsUnit.Pixel);

                return trimmedSprite;
            }
            catch
            {
                // Missing or damaged artwork should
                // not prevent the game from opening.
                return null;
            }
        }

        /// <summary>
        /// Finds the visible portion of a sprite
        /// and removes unnecessary transparent space.
        /// </summary>
        private static Rectangle FindVisibleBounds(
            Bitmap bitmap)
        {
            Rectangle imageRectangle =
                new Rectangle(
                    0,
                    0,
                    bitmap.Width,
                    bitmap.Height);

            BitmapData bitmapData =
                bitmap.LockBits(
                    imageRectangle,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

            try
            {
                int stride =
                    bitmapData.Stride;

                int absoluteStride =
                    Math.Abs(
                        stride);

                byte[] pixels =
                    new byte[
                        absoluteStride *
                        bitmap.Height];

                Marshal.Copy(
                    bitmapData.Scan0,
                    pixels,
                    0,
                    pixels.Length);

                int minimumX =
                    bitmap.Width;

                int minimumY =
                    bitmap.Height;

                int maximumX =
                    -1;

                int maximumY =
                    -1;

                // Ignore extremely faint smoke so the
                // primary piece remains large in-square.
                const byte alphaThreshold = 35;

                for (int y = 0;
                     y < bitmap.Height;
                     y++)
                {
                    int rowOffset =
                        stride >= 0
                            ? y * absoluteStride
                            : (bitmap.Height - 1 - y) *
                              absoluteStride;

                    for (int x = 0;
                         x < bitmap.Width;
                         x++)
                    {
                        int alphaIndex =
                            rowOffset +
                            x * 4 +
                            3;

                        if (pixels[alphaIndex] <=
                            alphaThreshold)
                        {
                            continue;
                        }

                        minimumX =
                            Math.Min(
                                minimumX,
                                x);

                        minimumY =
                            Math.Min(
                                minimumY,
                                y);

                        maximumX =
                            Math.Max(
                                maximumX,
                                x);

                        maximumY =
                            Math.Max(
                                maximumY,
                                y);
                    }
                }

                if (maximumX < minimumX ||
                    maximumY < minimumY)
                {
                    return Rectangle.Empty;
                }

                return Rectangle.FromLTRB(
                    minimumX,
                    minimumY,
                    maximumX + 1,
                    maximumY + 1);
            }
            finally
            {
                bitmap.UnlockBits(
                    bitmapData);
            }
        }

        /// <summary>
        /// Paints board squares using custom
        /// artwork whenever it is available.
        /// </summary>
        private void SquareButton_PaintWithSprites(
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

            if (squareButton.Tag is not
                Point position)
            {
                return;
            }

            Piece piece =
                chessGame.Board
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

            bool spriteDrawn =
                false;

            if (!piece.IsEmpty)
            {
                spriteDrawn =
                    TryDrawPieceSprite(
                        e.Graphics,
                        squareButton,
                        piece,
                        isHovered,
                        isSelected,
                        isCheckedKing);
            }

            // Use the original Unicode artwork
            // when a custom file cannot be loaded.
            if (!piece.IsEmpty &&
                !spriteDrawn)
            {
                DrawUnicodePieceForSpriteRenderer(
                    e.Graphics,
                    squareButton,
                    piece,
                    isHovered,
                    isSelected,
                    isCheckedKing);
            }

            if (isCheckedKing)
            {
                DrawCheckedKingWarning(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }

            if (isSelected)
            {
                DrawSelectedPieceSeal(
                    e.Graphics,
                    squareButton.ClientRectangle);
            }

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
        /// Draws one custom Gothic sprite.
        /// </summary>
        private bool TryDrawPieceSprite(
            Graphics graphics,
            Button squareButton,
            Piece piece,
            bool isHovered,
            bool isSelected,
            bool isCheckedKing)
        {
            if (!pieceSprites.TryGetValue(
                    (piece.Color, piece.Type),
                    out Bitmap? sprite))
            {
                return false;
            }

            Rectangle destination =
                CreateSpriteDestination(
                    squareButton.ClientRectangle,
                    sprite);

            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            graphics.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            graphics.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            graphics.CompositingQuality =
                CompositingQuality.HighQuality;

            if (isCheckedKing)
            {
                DrawSpriteGlow(
                    graphics,
                    sprite,
                    destination,
                    checkedKingAuraColor,
                    7,
                    0.64F);
            }

            // Keep normal board pieces clean. Auras are
            // displayed only during hover or selection.
            if (isSelected)
            {
                DrawSpriteGlow(
                    graphics,
                    sprite,
                    destination,
                    selectedAuraColor,
                    5,
                    0.42F);
            }
            else if (isHovered)
            {
                DrawSpriteGlow(
                    graphics,
                    sprite,
                    destination,
                    hoverAuraColor,
                    3,
                    0.34F);
            }

            graphics.DrawImage(
                sprite,
                destination);

            return true;
        }

        /// <summary>
        /// Scales one sprite while preserving
        /// its proportions and full base.
        /// </summary>
        private static Rectangle
            CreateSpriteDestination(
                Rectangle squareRectangle,
                Image sprite)
        {
            // A 58-pixel box leaves six pixels of breathing
            // room on every side of a 70-pixel board square.
            // The extra space prevents hover and selection
            // auras from touching adjacent squares.
            const int maximumWidth = 58;
            const int maximumHeight = 58;
            const int bottomPadding = 6;

            float scale =
                Math.Min(
                    maximumWidth /
                        (float)sprite.Width,

                    maximumHeight /
                        (float)sprite.Height);

            int width =
                Math.Max(
                    1,
                    (int)Math.Round(
                        sprite.Width *
                        scale));

            int height =
                Math.Max(
                    1,
                    (int)Math.Round(
                        sprite.Height *
                        scale));

            int x =
                squareRectangle.Left +
                (squareRectangle.Width -
                 width) / 2;

            int y =
                squareRectangle.Bottom -
                height -
                bottomPadding;

            return new Rectangle(
                x,
                y,
                width,
                height);
        }

        /// <summary>
        /// Draws tinted copies of a sprite
        /// to simulate a soft aura.
        /// </summary>
        private static void DrawSpriteGlow(
            Graphics graphics,
            Image sprite,
            Rectangle destination,
            Color glowColor,
            int radius,
            float opacity)
        {
            using ImageAttributes attributes =
                CreateTintAttributes(
                    glowColor,
                    opacity);

            Point[] offsets =
            {
                new Point(-radius, 0),
                new Point(radius, 0),
                new Point(0, -radius),
                new Point(0, radius),

                new Point(-radius, -radius),
                new Point(radius, -radius),
                new Point(-radius, radius),
                new Point(radius, radius)
            };

            foreach (Point offset in offsets)
            {
                Rectangle glowDestination =
                    new Rectangle(
                        destination.X +
                            offset.X,

                        destination.Y +
                            offset.Y,

                        destination.Width,
                        destination.Height);

                graphics.DrawImage(
                    sprite,
                    glowDestination,
                    0,
                    0,
                    sprite.Width,
                    sprite.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }
        }

        /// <summary>
        /// Creates the color filter used
        /// for custom sprite auras.
        /// </summary>
        private static ImageAttributes
            CreateTintAttributes(
                Color color,
                float opacity)
        {
            float red =
                color.R / 255F;

            float green =
                color.G / 255F;

            float blue =
                color.B / 255F;

            ColorMatrix matrix =
                new ColorMatrix(
                    new[]
                    {
                        new float[]
                        {
                            0, 0, 0, 0, 0
                        },
                        new float[]
                        {
                            0, 0, 0, 0, 0
                        },
                        new float[]
                        {
                            0, 0, 0, 0, 0
                        },
                        new float[]
                        {
                            0, 0, 0, opacity, 0
                        },
                        new float[]
                        {
                            red,
                            green,
                            blue,
                            0,
                            1
                        }
                    });

            ImageAttributes attributes =
                new ImageAttributes();

            attributes.SetColorMatrix(
                matrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);

            attributes.SetWrapMode(
                WrapMode.TileFlipXY);

            return attributes;
        }

        /// <summary>
        /// Preserves the existing Unicode fallback
        /// for any unavailable artwork.
        /// </summary>
        private void
            DrawUnicodePieceForSpriteRenderer(
                Graphics graphics,
                Button squareButton,
                Piece piece,
                bool isHovered,
                bool isSelected,
                bool isCheckedKing)
        {
            string symbol =
                GetSolidPieceSymbol(
                    piece.Type);

            if (string.IsNullOrEmpty(symbol))
            {
                return;
            }

            using GraphicsPath piecePath =
                CreatePiecePath(
                    graphics,
                    squareButton,
                    symbol);

            graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            graphics.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            if (isCheckedKing)
            {
                DrawCheckedPieceAura(
                    graphics,
                    piecePath);
            }

            DrawInteractivePieceAura(
                graphics,
                piecePath,
                isHovered,
                isSelected);

            if (piece.Color ==
                PieceColor.White)
            {
                DrawHallowedSaintsPiece(
                    graphics,
                    piecePath);
            }
            else
            {
                DrawDamnedPiece(
                    graphics,
                    piecePath);
            }
        }

        /// <summary>
        /// Releases every loaded image when
        /// the main window closes.
        /// </summary>
        private void FrmMain_PieceSprites_FormClosed(
            object? sender,
            FormClosedEventArgs e)
        {
            DisposePieceSprites();
        }

        /// <summary>
        /// Releases all loaded bitmap resources.
        /// </summary>
        private void DisposePieceSprites()
        {
            foreach (Bitmap sprite in
                     pieceSprites.Values)
            {
                sprite.Dispose();
            }

            pieceSprites.Clear();
        }
    }
}