using System;
using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Validates chess-piece movement and detects
    /// whether a king is under attack.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// Determines whether a move follows the
        /// standard movement rules for a piece.
        /// </summary>
        public static bool IsLegalMove(
            Board board,
            Spot start,
            Spot end)
        {
            Piece movingPiece = start.Piece;
            Piece destinationPiece = end.Piece;

            if (movingPiece.IsEmpty)
            {
                return false;
            }

            if (start.Row == end.Row &&
                start.Column == end.Column)
            {
                return false;
            }

            // A piece cannot capture a friendly piece.
            if (!destinationPiece.IsEmpty &&
                destinationPiece.Color == movingPiece.Color)
            {
                return false;
            }

            return movingPiece.Type switch
            {
                PieceType.Pawn =>
                    IsLegalPawnMove(board, start, end),

                PieceType.Rook =>
                    IsLegalRookMove(board, start, end),

                PieceType.Knight =>
                    IsLegalKnightMove(start, end),

                PieceType.Bishop =>
                    IsLegalBishopMove(board, start, end),

                PieceType.Queen =>
                    IsLegalQueenMove(board, start, end),

                PieceType.King =>
                    IsLegalKingMove(start, end),

                _ => false
            };
        }

        /// <summary>
        /// Determines whether the specified king
        /// is currently under attack.
        /// </summary>
        public static bool IsKingInCheck(
            Board board,
            PieceColor kingColor)
        {
            Spot? kingSpot = FindKing(board, kingColor);

            if (kingSpot == null)
            {
                return true;
            }

            PieceColor attackingColor =
                kingColor == PieceColor.White
                    ? PieceColor.Black
                    : PieceColor.White;

            return IsSquareUnderAttack(
                board,
                kingSpot.Row,
                kingSpot.Column,
                attackingColor);
        }

        /// <summary>
        /// Determines whether a square is attacked
        /// by a piece of the specified color.
        /// </summary>
        public static bool IsSquareUnderAttack(
            Board board,
            int targetRow,
            int targetColumn,
            PieceColor attackingColor)
        {
            Spot targetSpot =
                board.Spots[targetRow, targetColumn];

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Spot attacker =
                        board.Spots[row, column];

                    if (attacker.Piece.IsEmpty ||
                        attacker.Piece.Color != attackingColor)
                    {
                        continue;
                    }

                    if (CanPieceAttackSquare(
                            board,
                            attacker,
                            targetSpot))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the king belonging to the specified side.
        /// </summary>
        private static Spot? FindKing(
            Board board,
            PieceColor kingColor)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Spot spot =
                        board.Spots[row, column];

                    if (!spot.Piece.IsEmpty &&
                        spot.Piece.Type == PieceType.King &&
                        spot.Piece.Color == kingColor)
                    {
                        return spot;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether one piece attacks
        /// the target square.
        /// </summary>
        private static bool CanPieceAttackSquare(
            Board board,
            Spot attacker,
            Spot target)
        {
            return attacker.Piece.Type switch
            {
                PieceType.Pawn =>
                    CanPawnAttack(attacker, target),

                PieceType.Rook =>
                    IsLegalRookMove(board, attacker, target),

                PieceType.Knight =>
                    IsLegalKnightMove(attacker, target),

                PieceType.Bishop =>
                    IsLegalBishopMove(board, attacker, target),

                PieceType.Queen =>
                    IsLegalQueenMove(board, attacker, target),

                PieceType.King =>
                    IsLegalKingMove(attacker, target),

                _ => false
            };
        }

        /// <summary>
        /// Checks normal pawn movement and capturing.
        /// </summary>
        private static bool IsLegalPawnMove(
            Board board,
            Spot start,
            Spot end)
        {
            Piece pawn = start.Piece;

            int rowChange =
                end.Row - start.Row;

            int columnChange =
                Math.Abs(end.Column - start.Column);

            int direction =
                pawn.Color == PieceColor.White ? -1 : 1;

            int startingRow =
                pawn.Color == PieceColor.White ? 6 : 1;

            // Move forward one square.
            if (columnChange == 0 &&
                rowChange == direction &&
                end.Piece.IsEmpty)
            {
                return true;
            }

            // Move forward two squares from starting row.
            if (columnChange == 0 &&
                start.Row == startingRow &&
                rowChange == direction * 2 &&
                end.Piece.IsEmpty)
            {
                int middleRow =
                    start.Row + direction;

                Spot middleSpot =
                    board.Spots[middleRow, start.Column];

                return middleSpot.Piece.IsEmpty;
            }

            // Capture diagonally.
            if (columnChange == 1 &&
                rowChange == direction &&
                !end.Piece.IsEmpty &&
                end.Piece.Color != pawn.Color)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a pawn attacks a target square.
        /// </summary>
        private static bool CanPawnAttack(
            Spot start,
            Spot target)
        {
            int direction =
                start.Piece.Color == PieceColor.White
                    ? -1
                    : 1;

            int rowChange =
                target.Row - start.Row;

            int columnChange =
                Math.Abs(target.Column - start.Column);

            return rowChange == direction &&
                   columnChange == 1;
        }

        /// <summary>
        /// Checks horizontal and vertical rook movement.
        /// </summary>
        private static bool IsLegalRookMove(
            Board board,
            Spot start,
            Spot end)
        {
            bool movesHorizontally =
                start.Row == end.Row;

            bool movesVertically =
                start.Column == end.Column;

            if (!movesHorizontally &&
                !movesVertically)
            {
                return false;
            }

            return IsPathClear(board, start, end);
        }

        /// <summary>
        /// Checks L-shaped knight movement.
        /// </summary>
        private static bool IsLegalKnightMove(
            Spot start,
            Spot end)
        {
            int rowDistance =
                Math.Abs(end.Row - start.Row);

            int columnDistance =
                Math.Abs(end.Column - start.Column);

            return
                (rowDistance == 2 && columnDistance == 1) ||
                (rowDistance == 1 && columnDistance == 2);
        }

        /// <summary>
        /// Checks diagonal bishop movement.
        /// </summary>
        private static bool IsLegalBishopMove(
            Board board,
            Spot start,
            Spot end)
        {
            int rowDistance =
                Math.Abs(end.Row - start.Row);

            int columnDistance =
                Math.Abs(end.Column - start.Column);

            if (rowDistance != columnDistance)
            {
                return false;
            }

            return IsPathClear(board, start, end);
        }

        /// <summary>
        /// Checks straight or diagonal queen movement.
        /// </summary>
        private static bool IsLegalQueenMove(
            Board board,
            Spot start,
            Spot end)
        {
            int rowDistance =
                Math.Abs(end.Row - start.Row);

            int columnDistance =
                Math.Abs(end.Column - start.Column);

            bool movesStraight =
                start.Row == end.Row ||
                start.Column == end.Column;

            bool movesDiagonally =
                rowDistance == columnDistance;

            if (!movesStraight &&
                !movesDiagonally)
            {
                return false;
            }

            return IsPathClear(board, start, end);
        }

        /// <summary>
        /// Checks normal one-square king movement.
        /// </summary>
        private static bool IsLegalKingMove(
            Spot start,
            Spot end)
        {
            int rowDistance =
                Math.Abs(end.Row - start.Row);

            int columnDistance =
                Math.Abs(end.Column - start.Column);

            return rowDistance <= 1 &&
                   columnDistance <= 1 &&
                   rowDistance + columnDistance > 0;
        }

        /// <summary>
        /// Determines whether all squares between
        /// the starting and ending squares are empty.
        /// </summary>
        private static bool IsPathClear(
            Board board,
            Spot start,
            Spot end)
        {
            int rowStep =
                Math.Sign(end.Row - start.Row);

            int columnStep =
                Math.Sign(end.Column - start.Column);

            int currentRow =
                start.Row + rowStep;

            int currentColumn =
                start.Column + columnStep;

            while (currentRow != end.Row ||
                   currentColumn != end.Column)
            {
                if (!board.Spots[currentRow, currentColumn]
                    .Piece.IsEmpty)
                {
                    return false;
                }

                currentRow += rowStep;
                currentColumn += columnStep;
            }

            return true;
        }
    }
}