using System;
using System.Collections.Generic;
using System.Text;
using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Imports and exports chess positions
    /// using Forsyth-Edwards Notation.
    /// </summary>
    public static class FenService
    {
        /// <summary>
        /// Standard chess starting position.
        /// </summary>
        public const string StartingPositionFen =
            "rnbqkbnr/pppppppp/8/8/8/8/" +
            "PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Creates a FEN string from the current game.
        /// </summary>
        public static string ExportFen(
            ChessGame game)
        {
            string piecePlacement =
                CreatePiecePlacement(game.Board);

            string activeColor =
                game.Turn == PieceColor.White
                    ? "w"
                    : "b";

            string castlingRights =
                CreateCastlingRights(game.Board);

            string enPassantTarget =
                CreateEnPassantTarget(
                    game.PreviousMove);

            return
                $"{piecePlacement} " +
                $"{activeColor} " +
                $"{castlingRights} " +
                $"{enPassantTarget} " +
                $"{game.HalfmoveClock} " +
                $"{game.FullmoveNumber}";
        }

        /// <summary>
        /// Validates a FEN string and loads it
        /// into the supplied chess game.
        /// </summary>
        public static bool TryImportFen(
            ChessGame game,
            string fen,
            out string message)
        {
            if (string.IsNullOrWhiteSpace(fen))
            {
                message = "Enter a FEN position first.";
                return false;
            }

            string[] fields =
                fen.Trim().Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length != 6)
            {
                message =
                    "Invalid FEN: exactly six fields are required.";

                return false;
            }

            Piece[,] importedPieces =
                CreateEmptyPieceArray();

            if (!TryParsePiecePlacement(
                    fields[0],
                    importedPieces,
                    out message))
            {
                return false;
            }

            if (!TryParseActiveColor(
                    fields[1],
                    out PieceColor activeColor,
                    out message))
            {
                return false;
            }

            if (!TryApplyMovementFlags(
                    importedPieces,
                    fields[2],
                    out message))
            {
                return false;
            }

            if (!TryCreatePreviousMove(
                    importedPieces,
                    activeColor,
                    fields[3],
                    out LastMove? previousMove,
                    out message))
            {
                return false;
            }

            if (!TryValidateMoveCounters(
                    fields[4],
                    fields[5],
                    out int halfmoveClock,
                    out int fullmoveNumber,
                    out message))
            {
                return false;
            }

            // Only modify the real board after the
            // complete FEN has passed validation.
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0;
                     column < 8;
                     column++)
                {
                    game.Board.Spots[row, column].Piece =
                        importedPieces[row, column];
                }
            }

            game.ApplyImportedFenState(
                activeColor,
                previousMove,
                halfmoveClock,
                fullmoveNumber);

            message = "FEN position loaded.";
            return true;
        }

        /// <summary>
        /// Creates an empty temporary board.
        /// </summary>
        private static Piece[,] CreateEmptyPieceArray()
        {
            Piece[,] pieces =
                new Piece[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0;
                     column < 8;
                     column++)
                {
                    pieces[row, column] =
                        Piece.Empty;
                }
            }

            return pieces;
        }

        /// <summary>
        /// Reads the first FEN field and recreates
        /// every chess piece.
        /// </summary>
        private static bool TryParsePiecePlacement(
            string field,
            Piece[,] pieces,
            out string message)
        {
            string[] ranks =
                field.Split('/');

            if (ranks.Length != 8)
            {
                message =
                    "Invalid FEN: there must be eight ranks.";

                return false;
            }

            int whiteKingCount = 0;
            int blackKingCount = 0;

            for (int row = 0; row < 8; row++)
            {
                int column = 0;

                foreach (char symbol in ranks[row])
                {
                    if (char.IsDigit(symbol))
                    {
                        int emptyCount =
                            symbol - '0';

                        if (emptyCount < 1 ||
                            emptyCount > 8)
                        {
                            message =
                                "Invalid FEN: empty-square " +
                                "numbers must be from 1 to 8.";

                            return false;
                        }

                        column += emptyCount;

                        if (column > 8)
                        {
                            message =
                                "Invalid FEN: a rank contains " +
                                "more than eight squares.";

                            return false;
                        }

                        continue;
                    }

                    if (column >= 8)
                    {
                        message =
                            "Invalid FEN: a rank contains " +
                            "more than eight squares.";

                        return false;
                    }

                    if (!TryCreatePiece(
                            symbol,
                            out Piece? piece))
                    {
                        message =
                            $"Invalid FEN piece character: {symbol}";

                        return false;
                    }

                    pieces[row, column] = piece;

                    if (piece.Type == PieceType.King)
                    {
                        if (piece.Color ==
                            PieceColor.White)
                        {
                            whiteKingCount++;
                        }
                        else
                        {
                            blackKingCount++;
                        }
                    }

                    column++;
                }

                if (column != 8)
                {
                    message =
                        $"Invalid FEN: rank {8 - row} " +
                        "does not contain eight squares.";

                    return false;
                }
            }

            if (whiteKingCount != 1 ||
                blackKingCount != 1)
            {
                message =
                    "Invalid FEN: the position must contain " +
                    "exactly one White king and one Black king.";

                return false;
            }

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Creates a Piece from one FEN character.
        /// </summary>
        private static bool TryCreatePiece(
            char symbol,
            out Piece piece)
        {
            PieceColor color =
                char.IsUpper(symbol)
                    ? PieceColor.White
                    : PieceColor.Black;

            char lowercaseSymbol =
                char.ToLowerInvariant(symbol);

            PieceType type =
                lowercaseSymbol switch
                {
                    'p' => PieceType.Pawn,
                    'r' => PieceType.Rook,
                    'n' => PieceType.Knight,
                    'b' => PieceType.Bishop,
                    'q' => PieceType.Queen,
                    'k' => PieceType.King,
                    _ => PieceType.None
                };

            if (type == PieceType.None)
            {
                piece = Piece.Empty;
                return false;
            }

            // Imported pieces are assumed to have moved
            // unless another FEN field proves otherwise.
            piece =
                new Piece(type, color)
                {
                    HasMoved = true
                };

            return true;
        }

        /// <summary>
        /// Reads whose turn it is.
        /// </summary>
        private static bool TryParseActiveColor(
            string field,
            out PieceColor activeColor,
            out string message)
        {
            if (field == "w")
            {
                activeColor = PieceColor.White;
                message = string.Empty;
                return true;
            }

            if (field == "b")
            {
                activeColor = PieceColor.Black;
                message = string.Empty;
                return true;
            }

            activeColor = PieceColor.White;

            message =
                "Invalid FEN: active color must be w or b.";

            return false;
        }

        /// <summary>
        /// Restores pawn first-move information and
        /// castling rights from the FEN.
        /// </summary>
        private static bool TryApplyMovementFlags(
            Piece[,] pieces,
            string castlingField,
            out string message)
        {
            // Pawns on their original ranks may still
            // use their initial two-square move.
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0;
                     column < 8;
                     column++)
                {
                    Piece piece =
                        pieces[row, column];

                    if (piece.Type != PieceType.Pawn)
                    {
                        continue;
                    }

                    bool whitePawnOnStartingRank =
                        piece.Color == PieceColor.White &&
                        row == 6;

                    bool blackPawnOnStartingRank =
                        piece.Color == PieceColor.Black &&
                        row == 1;

                    piece.HasMoved =
                        !whitePawnOnStartingRank &&
                        !blackPawnOnStartingRank;
                }
            }

            if (castlingField == "-")
            {
                message = string.Empty;
                return true;
            }

            HashSet<char> usedRights =
                new HashSet<char>();

            foreach (char right in castlingField)
            {
                if (!usedRights.Add(right))
                {
                    message =
                        "Invalid FEN: duplicate castling right.";

                    return false;
                }

                bool valid =
                    right switch
                    {
                        'K' => EnableCastlingRight(
                            pieces,
                            7,
                            7,
                            PieceColor.White),

                        'Q' => EnableCastlingRight(
                            pieces,
                            7,
                            0,
                            PieceColor.White),

                        'k' => EnableCastlingRight(
                            pieces,
                            0,
                            7,
                            PieceColor.Black),

                        'q' => EnableCastlingRight(
                            pieces,
                            0,
                            0,
                            PieceColor.Black),

                        _ => false
                    };

                if (!valid)
                {
                    message =
                        $"Invalid FEN castling right: {right}";

                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Confirms the required king and rook exist
        /// and marks them as eligible for castling.
        /// </summary>
        private static bool EnableCastlingRight(
            Piece[,] pieces,
            int homeRow,
            int rookColumn,
            PieceColor color)
        {
            Piece king =
                pieces[homeRow, 4];

            Piece rook =
                pieces[homeRow, rookColumn];

            if (king.Type != PieceType.King ||
                king.Color != color ||
                rook.Type != PieceType.Rook ||
                rook.Color != color)
            {
                return false;
            }

            king.HasMoved = false;
            rook.HasMoved = false;

            return true;
        }

        /// <summary>
        /// Recreates the previous two-square pawn move
        /// represented by the en passant field.
        /// </summary>
        private static bool TryCreatePreviousMove(
            Piece[,] pieces,
            PieceColor activeColor,
            string enPassantField,
            out LastMove? previousMove,
            out string message)
        {
            previousMove = null;

            if (enPassantField == "-")
            {
                message = string.Empty;
                return true;
            }

            if (enPassantField.Length != 2)
            {
                message =
                    "Invalid FEN: en passant target " +
                    "must be a square or -.";

                return false;
            }

            char fileCharacter =
                char.ToLowerInvariant(
                    enPassantField[0]);

            char rankCharacter =
                enPassantField[1];

            if (fileCharacter < 'a' ||
                fileCharacter > 'h' ||
                rankCharacter < '1' ||
                rankCharacter > '8')
            {
                message =
                    "Invalid FEN: en passant target " +
                    "is not a valid square.";

                return false;
            }

            int column =
                fileCharacter - 'a';

            int rank =
                rankCharacter - '0';

            int targetRow =
                8 - rank;

            if (!pieces[targetRow, column].IsEmpty)
            {
                message =
                    "Invalid FEN: the en passant " +
                    "target square must be empty.";

                return false;
            }

            PieceColor movedPawnColor =
                activeColor == PieceColor.White
                    ? PieceColor.Black
                    : PieceColor.White;

            int startRow;
            int endRow;
            int requiredTargetRow;

            if (movedPawnColor == PieceColor.White)
            {
                startRow = 6;
                endRow = 4;
                requiredTargetRow = 5;
            }
            else
            {
                startRow = 1;
                endRow = 3;
                requiredTargetRow = 2;
            }

            if (targetRow != requiredTargetRow)
            {
                message =
                    "Invalid FEN: en passant target " +
                    "must be on rank 3 or rank 6.";

                return false;
            }

            Piece movedPawn =
                pieces[endRow, column];

            if (movedPawn.Type != PieceType.Pawn ||
                movedPawn.Color != movedPawnColor)
            {
                message =
                    "Invalid FEN: no matching pawn " +
                    "was found for en passant.";

                return false;
            }

            if (!pieces[startRow, column].IsEmpty)
            {
                message =
                    "Invalid FEN: the pawn's original " +
                    "square must be empty.";

                return false;
            }

            movedPawn.HasMoved = true;

            previousMove =
                new LastMove(
                    movedPawn,
                    startRow,
                    column,
                    endRow,
                    column);

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Validates and returns the halfmove and
        /// fullmove fields from an imported FEN.
        /// </summary>
        private static bool TryValidateMoveCounters(
            string halfmoveField,
            string fullmoveField,
            out int halfmoveClock,
            out int fullmoveNumber,
            out string message)
        {
            halfmoveClock = 0;
            fullmoveNumber = 1;

            if (!int.TryParse(
                    halfmoveField,
                    out halfmoveClock) ||
                halfmoveClock < 0)
            {
                message =
                    "Invalid FEN: halfmove clock must " +
                    "be zero or greater.";

                return false;
            }

            if (!int.TryParse(
                    fullmoveField,
                    out fullmoveNumber) ||
                fullmoveNumber < 1)
            {
                message =
                    "Invalid FEN: fullmove number must " +
                    "be one or greater.";

                return false;
            }

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Converts the board into the first
        /// section of a FEN string.
        /// </summary>
        private static string CreatePiecePlacement(
            Board board)
        {
            StringBuilder fen =
                new StringBuilder();

            for (int row = 0; row < 8; row++)
            {
                int emptySquareCount = 0;

                for (int column = 0;
                     column < 8;
                     column++)
                {
                    Piece piece =
                        board.Spots[row, column].Piece;

                    if (piece.IsEmpty)
                    {
                        emptySquareCount++;
                        continue;
                    }

                    if (emptySquareCount > 0)
                    {
                        fen.Append(
                            emptySquareCount);

                        emptySquareCount = 0;
                    }

                    fen.Append(
                        GetFenLetter(piece));
                }

                if (emptySquareCount > 0)
                {
                    fen.Append(
                        emptySquareCount);
                }

                if (row < 7)
                {
                    fen.Append('/');
                }
            }

            return fen.ToString();
        }

        /// <summary>
        /// Returns the FEN letter for one piece.
        /// </summary>
        private static char GetFenLetter(
            Piece piece)
        {
            char letter =
                piece.Type switch
                {
                    PieceType.Pawn => 'p',
                    PieceType.Rook => 'r',
                    PieceType.Knight => 'n',
                    PieceType.Bishop => 'b',
                    PieceType.Queen => 'q',
                    PieceType.King => 'k',

                    _ => throw new InvalidOperationException(
                        "An empty square has no FEN letter.")
                };

            return piece.Color == PieceColor.White
                ? char.ToUpperInvariant(letter)
                : letter;
        }

        /// <summary>
        /// Determines which castling rights
        /// are still available.
        /// </summary>
        private static string CreateCastlingRights(
            Board board)
        {
            StringBuilder rights =
                new StringBuilder();

            Piece whiteKing =
                board.Spots[7, 4].Piece;

            if (IsUnmovedPiece(
                    whiteKing,
                    PieceType.King,
                    PieceColor.White))
            {
                if (IsUnmovedPiece(
                        board.Spots[7, 7].Piece,
                        PieceType.Rook,
                        PieceColor.White))
                {
                    rights.Append('K');
                }

                if (IsUnmovedPiece(
                        board.Spots[7, 0].Piece,
                        PieceType.Rook,
                        PieceColor.White))
                {
                    rights.Append('Q');
                }
            }

            Piece blackKing =
                board.Spots[0, 4].Piece;

            if (IsUnmovedPiece(
                    blackKing,
                    PieceType.King,
                    PieceColor.Black))
            {
                if (IsUnmovedPiece(
                        board.Spots[0, 7].Piece,
                        PieceType.Rook,
                        PieceColor.Black))
                {
                    rights.Append('k');
                }

                if (IsUnmovedPiece(
                        board.Spots[0, 0].Piece,
                        PieceType.Rook,
                        PieceColor.Black))
                {
                    rights.Append('q');
                }
            }

            return rights.Length == 0
                ? "-"
                : rights.ToString();
        }

        /// <summary>
        /// Checks whether an original king or rook
        /// remains eligible to castle.
        /// </summary>
        private static bool IsUnmovedPiece(
            Piece piece,
            PieceType expectedType,
            PieceColor expectedColor)
        {
            return
                !piece.IsEmpty &&
                piece.Type == expectedType &&
                piece.Color == expectedColor &&
                !piece.HasMoved;
        }

        /// <summary>
        /// Creates the en passant target square
        /// from the previous move.
        /// </summary>
        private static string CreateEnPassantTarget(
            LastMove? previousMove)
        {
            if (previousMove == null ||
                !previousMove.WasTwoSquarePawnMove)
            {
                return "-";
            }

            int targetRow =
                (previousMove.StartRow +
                 previousMove.EndRow) / 2;

            int targetColumn =
                previousMove.EndColumn;

            char file =
                (char)('a' + targetColumn);

            int rank =
                8 - targetRow;

            return $"{file}{rank}";
        }
    }
}