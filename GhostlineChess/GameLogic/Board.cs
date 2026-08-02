using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Represents the chessboard and its starting arrangement.
    /// </summary>
    public class Board
    {
        public Spot[,] Spots { get; } = new Spot[8, 8];

        public Board()
        {
            CreateSpots();
            SetupPieces();
        }

        /// <summary>
        /// Creates all 64 squares on the chessboard.
        /// </summary>
        private void CreateSpots()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Spots[row, column] =
                        new Spot(row, column);
                }
            }
        }

        /// <summary>
        /// Places all chess pieces in their standard
        /// starting positions.
        /// </summary>
        private void SetupPieces()
        {
            PieceType[] backRank =
            {
                PieceType.Rook,
                PieceType.Knight,
                PieceType.Bishop,
                PieceType.Queen,
                PieceType.King,
                PieceType.Bishop,
                PieceType.Knight,
                PieceType.Rook
            };

            for (int column = 0; column < 8; column++)
            {
                // Black back rank.
                Spots[0, column].Piece =
                    new Piece(
                        backRank[column],
                        PieceColor.Black);

                // Black pawns.
                Spots[1, column].Piece =
                    new Piece(
                        PieceType.Pawn,
                        PieceColor.Black);

                // White pawns.
                Spots[6, column].Piece =
                    new Piece(
                        PieceType.Pawn,
                        PieceColor.White);

                // White back rank.
                Spots[7, column].Piece =
                    new Piece(
                        backRank[column],
                        PieceColor.White);
            }
        }
    }
}