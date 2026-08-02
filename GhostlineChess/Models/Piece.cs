using GhostlineChess.Enums;

namespace GhostlineChess.Models
{
    /// <summary>
    /// Represents one chess piece on the board.
    /// </summary>
    public class Piece
    {
        public PieceType Type { get; }

        public PieceColor Color { get; }

        /// <summary>
        /// Tracks whether this piece has moved during the current game.
        /// This will be used for castling and pawn movement rules.
        /// </summary>
        public bool HasMoved { get; set; }

        /// <summary>
        /// Returns true when this object represents an empty square.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Type == PieceType.None;
            }
        }

        public Piece(PieceType type, PieceColor color)
        {
            Type = type;
            Color = color;
            HasMoved = false;
        }

        /// <summary>
        /// Creates an empty piece object for an unoccupied square.
        /// </summary>
        public static Piece Empty
        {
            get
            {
                return new Piece(PieceType.None, PieceColor.White);
            }
        }

        /// <summary>
        /// Returns the Unicode chess symbol for this piece.
        /// </summary>
        public override string ToString()
        {
            return (Type, Color) switch
            {
                (PieceType.Pawn, PieceColor.White) => "♙",
                (PieceType.Rook, PieceColor.White) => "♖",
                (PieceType.Knight, PieceColor.White) => "♘",
                (PieceType.Bishop, PieceColor.White) => "♗",
                (PieceType.Queen, PieceColor.White) => "♕",
                (PieceType.King, PieceColor.White) => "♔",

                (PieceType.Pawn, PieceColor.Black) => "♟",
                (PieceType.Rook, PieceColor.Black) => "♜",
                (PieceType.Knight, PieceColor.Black) => "♞",
                (PieceType.Bishop, PieceColor.Black) => "♝",
                (PieceType.Queen, PieceColor.Black) => "♛",
                (PieceType.King, PieceColor.Black) => "♚",

                _ => string.Empty
            };
        }
    }
}