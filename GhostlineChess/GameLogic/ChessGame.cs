using System;
using System.Collections.Generic;
using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Controls turns, piece selection, movement,
    /// captures, castling, en passant, promotion,
    /// king safety, legal moves, and game results.
    /// </summary>
    public class ChessGame
    {
        public Board Board { get; }

        public PieceColor Turn { get; private set; }

        public Spot? SelectedSpot { get; private set; }

        public GameResult Result { get; private set; }

        /// <summary>
        /// Counts how many times each repetition-relevant
        /// position has occurred during the current game.
        /// The key excludes the FEN move counters.
        /// </summary>
        private readonly Dictionary<string, int>
            positionOccurrences =
                new Dictionary<string, int>();

        /// <summary>
        /// Counts reversible halfmoves for FEN export.
        /// Pawn moves and captures reset this value to zero.
        /// </summary>
        public int HalfmoveClock { get; private set; }

        /// <summary>
        /// Stores the FEN fullmove number. It begins at one
        /// and increases after each completed Black move.
        /// </summary>
        public int FullmoveNumber { get; private set; }

        /// <summary>
        /// Stores the most recently completed move.
        /// En passant uses this information.
        /// </summary>
        public LastMove? PreviousMove { get; private set; }

        /// <summary>
        /// Returns true while the player must choose
        /// a piece for pawn promotion.
        /// </summary>
        public bool PromotionPending { get; private set; }

        /// <summary>
        /// Stores the square containing the pawn
        /// that is waiting to be promoted.
        /// </summary>
        public Spot? PromotionSpot { get; private set; }

        public ChessGame()
        {
            Board = new Board();
            Turn = PieceColor.White;
            SelectedSpot = null;
            Result = GameResult.InProgress;
            HalfmoveClock = 0;
            FullmoveNumber = 1;
            PreviousMove = null;
            PromotionPending = false;
            PromotionSpot = null;

            RecordCurrentPosition();
        }

        /// <summary>
        /// Applies the turn and previous-move information
        /// after FenService imports a new board position.
        /// </summary>
        internal void ApplyImportedFenState(
            PieceColor turn,
            LastMove? previousMove,
            int halfmoveClock,
            int fullmoveNumber)
        {
            Turn = turn;
            PreviousMove = previousMove;
            HalfmoveClock = halfmoveClock;
            FullmoveNumber = fullmoveNumber;

            SelectedSpot = null;

            PromotionPending = false;
            PromotionSpot = null;

            Result = GameResult.InProgress;

            // A FEN string describes only the current position,
            // not the earlier repetition history. Begin a fresh
            // history with the imported position counted once.
            positionOccurrences.Clear();
            RecordCurrentPosition();

            // Determine whether the imported position
            // is active, checkmate, stalemate, or a draw
            // caused by insufficient mating material.
            UpdateGameResult();
        }

        /// <summary>
        /// Selects a piece or attempts to move
        /// the currently selected piece.
        /// </summary>
        public bool SelectSpot(
            int row,
            int column,
            out string message)
        {
            if (Result != GameResult.InProgress)
            {
                message = "The game is over.";
                return false;
            }

            if (PromotionPending)
            {
                message = "Choose a promotion piece first.";
                return false;
            }

            Spot clickedSpot =
                Board.Spots[row, column];

            if (SelectedSpot == null)
            {
                return SelectPiece(
                    clickedSpot,
                    out message);
            }

            if (SelectedSpot == clickedSpot)
            {
                SelectedSpot = null;
                message = "Selection cleared.";
                return true;
            }

            if (!clickedSpot.Piece.IsEmpty &&
                clickedSpot.Piece.Color == Turn)
            {
                SelectedSpot = clickedSpot;

                message =
                    $"{clickedSpot.Piece.Type} selected.";

                return true;
            }

            // Kings are checkmated rather than captured.
            if (!clickedSpot.Piece.IsEmpty &&
                clickedSpot.Piece.Type == PieceType.King)
            {
                message = "The king cannot be captured.";
                return false;
            }

            if (!MoveValidator.IsLegalMove(
                    Board,
                    SelectedSpot,
                    clickedSpot,
                    PreviousMove))
            {
                message =
                    $"Illegal {SelectedSpot.Piece.Type} move.";

                return false;
            }

            if (WouldLeaveKingInCheck(
                    SelectedSpot,
                    clickedSpot))
            {
                message =
                    "That move would leave your king in check.";

                return false;
            }

            return MoveSelectedPiece(
                clickedSpot,
                out message);
        }

        /// <summary>
        /// Returns all fully legal destination squares
        /// for the currently selected piece.
        /// </summary>
        public IReadOnlyList<Spot>
            GetLegalMovesForSelectedPiece()
        {
            if (SelectedSpot == null ||
                PromotionPending ||
                Result != GameResult.InProgress)
            {
                return Array.Empty<Spot>();
            }

            return GetLegalMoves(
                SelectedSpot);
        }

        /// <summary>
        /// Returns all fully legal destination squares
        /// for a particular starting square.
        /// </summary>
        private List<Spot> GetLegalMoves(
            Spot startingSpot)
        {
            List<Spot> legalMoves =
                new List<Spot>();

            if (startingSpot.Piece.IsEmpty)
            {
                return legalMoves;
            }

            for (int endRow = 0;
                 endRow < 8;
                 endRow++)
            {
                for (int endColumn = 0;
                     endColumn < 8;
                     endColumn++)
                {
                    Spot destination =
                        Board.Spots[
                            endRow,
                            endColumn];

                    // A king is checkmated,
                    // not directly captured.
                    if (!destination.Piece.IsEmpty &&
                        destination.Piece.Type ==
                        PieceType.King)
                    {
                        continue;
                    }

                    if (!MoveValidator.IsLegalMove(
                            Board,
                            startingSpot,
                            destination,
                            PreviousMove))
                    {
                        continue;
                    }

                    if (WouldLeaveKingInCheck(
                            startingSpot,
                            destination))
                    {
                        continue;
                    }

                    legalMoves.Add(
                        destination);
                }
            }

            return legalMoves;
        }

        /// <summary>
        /// Selects a piece belonging to the current player.
        /// </summary>
        private bool SelectPiece(
            Spot clickedSpot,
            out string message)
        {
            if (clickedSpot.Piece.IsEmpty)
            {
                message = "Select one of your pieces.";
                return false;
            }

            if (clickedSpot.Piece.Color != Turn)
            {
                message =
                    $"It is {GetFactionPossessiveName(Turn)} turn.";
                return false;
            }

            SelectedSpot = clickedSpot;

            message =
                $"{clickedSpot.Piece.Type} selected.";

            return true;
        }

        /// <summary>
        /// Temporarily performs a move to determine
        /// whether the moving player's king remains safe.
        /// </summary>
        private bool WouldLeaveKingInCheck(
            Spot start,
            Spot destination)
        {
            Piece movingPiece =
                start.Piece;

            Piece capturedDestinationPiece =
                destination.Piece;

            bool isCastling =
                IsCastlingMove(
                    start,
                    destination);

            bool isEnPassant =
                IsEnPassantMove(
                    start,
                    destination);

            Spot? rookStartSpot = null;
            Spot? rookDestinationSpot = null;

            Piece? rookPiece = null;
            Piece? originalRookDestinationPiece = null;

            Spot? enPassantCapturedSpot = null;
            Piece? enPassantCapturedPiece = null;

            destination.Piece = movingPiece;
            start.Piece = Piece.Empty;

            // Temporarily remove the pawn captured
            // through en passant.
            if (isEnPassant)
            {
                enPassantCapturedSpot =
                    Board.Spots[
                        start.Row,
                        destination.Column];

                enPassantCapturedPiece =
                    enPassantCapturedSpot.Piece;

                enPassantCapturedSpot.Piece =
                    Piece.Empty;
            }

            // Temporarily move the rook during castling.
            if (isCastling)
            {
                bool isKingSide =
                    destination.Column >
                    start.Column;

                int rookStartColumn =
                    isKingSide ? 7 : 0;

                int rookDestinationColumn =
                    isKingSide ? 5 : 3;

                rookStartSpot =
                    Board.Spots[
                        start.Row,
                        rookStartColumn];

                rookDestinationSpot =
                    Board.Spots[
                        start.Row,
                        rookDestinationColumn];

                rookPiece =
                    rookStartSpot.Piece;

                originalRookDestinationPiece =
                    rookDestinationSpot.Piece;

                rookDestinationSpot.Piece =
                    rookPiece;

                rookStartSpot.Piece =
                    Piece.Empty;
            }

            bool kingInCheck =
                MoveValidator.IsKingInCheck(
                    Board,
                    movingPiece.Color);

            // Restore the moving piece.
            start.Piece =
                movingPiece;

            destination.Piece =
                capturedDestinationPiece;

            // Restore the en passant pawn.
            if (isEnPassant &&
                enPassantCapturedSpot != null &&
                enPassantCapturedPiece != null)
            {
                enPassantCapturedSpot.Piece =
                    enPassantCapturedPiece;
            }

            // Restore the castling rook.
            if (isCastling &&
                rookStartSpot != null &&
                rookDestinationSpot != null &&
                rookPiece != null &&
                originalRookDestinationPiece != null)
            {
                rookStartSpot.Piece =
                    rookPiece;

                rookDestinationSpot.Piece =
                    originalRookDestinationPiece;
            }

            return kingInCheck;
        }

        /// <summary>
        /// Performs an approved move and updates the game.
        /// </summary>
        private bool MoveSelectedPiece(
            Spot destinationSpot,
            out string message)
        {
            if (SelectedSpot == null)
            {
                message = "No piece is selected.";
                return false;
            }

            Spot startingSpot =
                SelectedSpot;

            Piece movingPiece =
                startingSpot.Piece;

            bool isCastling =
                IsCastlingMove(
                    startingSpot,
                    destinationSpot);

            bool isEnPassant =
                IsEnPassantMove(
                    startingSpot,
                    destinationSpot);

            bool isCapture =
                !destinationSpot.Piece.IsEmpty ||
                isEnPassant;

            PieceType capturedPieceType =
                isEnPassant
                    ? PieceType.Pawn
                    : destinationSpot.Piece.Type;

            int startRow =
                startingSpot.Row;

            int startColumn =
                startingSpot.Column;

            int endRow =
                destinationSpot.Row;

            int endColumn =
                destinationSpot.Column;

            destinationSpot.Piece =
                movingPiece;

            startingSpot.Piece =
                Piece.Empty;

            // Remove the adjacent pawn during en passant.
            if (isEnPassant)
            {
                Spot capturedPawnSpot =
                    Board.Spots[
                        startRow,
                        endColumn];

                capturedPawnSpot.Piece =
                    Piece.Empty;
            }

            // Move the rook during castling.
            if (isCastling)
            {
                MoveCastlingRook(
                    startingSpot,
                    destinationSpot);
            }

            movingPiece.HasMoved = true;

            PreviousMove =
                new LastMove(
                    movingPiece,
                    startRow,
                    startColumn,
                    endRow,
                    endColumn);

            SelectedSpot = null;

            // Stop here when a pawn reaches the last row.
            // The form will ask the player which piece
            // should replace the pawn.
            if (PawnNeedsPromotion(destinationSpot))
            {
                PromotionPending = true;
                PromotionSpot = destinationSpot;

                message =
                    "Choose a piece for pawn promotion.";

                return true;
            }

            RecordCompletedMove(
                movingPiece,
                isCapture);

            SwitchTurn();
            RecordCurrentPosition();
            UpdateGameResult();

            if (Result == GameResult.WhiteWon)
            {
                message = "Checkmate. The Hallowed Saints prevail!";
                return true;
            }

            if (Result == GameResult.BlackWon)
            {
                message = "Checkmate. The Damned prevail!";
                return true;
            }

            if (Result == GameResult.Draw)
            {
                message = "Stalemate. The game is a draw.";
                return true;
            }

            if (Result == GameResult.InsufficientMaterial)
            {
                message =
                    "Draw by insufficient mating material.";

                return true;
            }

            if (Result == GameResult.ThreefoldRepetition)
            {
                message =
                    "Draw by threefold repetition.";

                return true;
            }

            if (Result == GameResult.FiftyMoveRule)
            {
                message =
                    "Draw by the fifty-move rule.";

                return true;
            }

            if (isCastling)
            {
                message = "Castling completed.";
            }
            else if (isEnPassant)
            {
                message = "Pawn captured by en passant.";
            }
            else
            {
                message = isCapture
                    ? $"{capturedPieceType} captured."
                    : "Piece moved.";
            }

            AddCheckMessage(ref message);

            return true;
        }

        /// <summary>
        /// Replaces the pawn with the player's
        /// chosen promotion piece.
        /// </summary>
        public bool CompletePromotion(
            PieceType promotionType,
            out string message)
        {
            if (!PromotionPending ||
                PromotionSpot == null)
            {
                message =
                    "There is no pawn waiting for promotion.";

                return false;
            }

            bool isValidPromotion =
                promotionType == PieceType.Queen ||
                promotionType == PieceType.Rook ||
                promotionType == PieceType.Bishop ||
                promotionType == PieceType.Knight;

            if (!isValidPromotion)
            {
                message =
                    "Choose a Queen, Rook, Bishop, or Knight.";

                return false;
            }

            Piece pawn =
                PromotionSpot.Piece;

            if (pawn.Type != PieceType.Pawn)
            {
                message =
                    "The promotion square does not contain a pawn.";

                return false;
            }

            PieceColor promotingColor =
                pawn.Color;

            PromotionSpot.Piece =
                new Piece(
                    promotionType,
                    promotingColor)
                {
                    HasMoved = true
                };

            PromotionPending = false;
            PromotionSpot = null;

            // Promotion completes the pawn move that began
            // before the player chose the new piece.
            RecordCompletedMove(
                pawn,
                isCapture: false);

            SwitchTurn();
            RecordCurrentPosition();
            UpdateGameResult();

            string promotionMessage =
                $"Pawn promoted to {promotionType}.";

            if (Result == GameResult.WhiteWon)
            {
                message =
                    $"{promotionMessage} Checkmate. " +
                    "The Hallowed Saints prevail!";

                return true;
            }

            if (Result == GameResult.BlackWon)
            {
                message =
                    $"{promotionMessage} Checkmate. " +
                    "The Damned prevail!";

                return true;
            }

            if (Result == GameResult.Draw)
            {
                message =
                    $"{promotionMessage} Stalemate. " +
                    "The game is a draw.";

                return true;
            }

            if (Result == GameResult.InsufficientMaterial)
            {
                message =
                    $"{promotionMessage} Draw by " +
                    "insufficient mating material.";

                return true;
            }

            if (Result == GameResult.ThreefoldRepetition)
            {
                message =
                    $"{promotionMessage} Draw by " +
                    "threefold repetition.";

                return true;
            }

            if (Result == GameResult.FiftyMoveRule)
            {
                message =
                    $"{promotionMessage} Draw by the " +
                    "fifty-move rule.";

                return true;
            }

            message = promotionMessage;

            AddCheckMessage(ref message);

            return true;
        }

        /// <summary>
        /// Returns true when a pawn has reached
        /// the opposite end of the board.
        /// </summary>
        private static bool PawnNeedsPromotion(
            Spot destinationSpot)
        {
            Piece piece =
                destinationSpot.Piece;

            if (piece.Type != PieceType.Pawn)
            {
                return false;
            }

            bool whitePawnReachedEnd =
                piece.Color == PieceColor.White &&
                destinationSpot.Row == 0;

            bool blackPawnReachedEnd =
                piece.Color == PieceColor.Black &&
                destinationSpot.Row == 7;

            return
                whitePawnReachedEnd ||
                blackPawnReachedEnd;
        }

        /// <summary>
        /// Returns true for a two-square
        /// horizontal king move.
        /// </summary>
        private static bool IsCastlingMove(
            Spot start,
            Spot destination)
        {
            return
                start.Piece.Type == PieceType.King &&
                start.Row == destination.Row &&
                Math.Abs(
                    destination.Column -
                    start.Column) == 2;
        }

        /// <summary>
        /// Returns true when a pawn moves diagonally
        /// onto an empty square.
        /// </summary>
        private static bool IsEnPassantMove(
            Spot start,
            Spot destination)
        {
            return
                start.Piece.Type == PieceType.Pawn &&
                destination.Piece.IsEmpty &&
                Math.Abs(
                    destination.Column -
                    start.Column) == 1 &&
                Math.Abs(
                    destination.Row -
                    start.Row) == 1;
        }

        /// <summary>
        /// Moves the correct rook during castling.
        /// </summary>
        private void MoveCastlingRook(
            Spot kingStart,
            Spot kingDestination)
        {
            bool isKingSide =
                kingDestination.Column >
                kingStart.Column;

            int rookStartColumn =
                isKingSide ? 7 : 0;

            int rookDestinationColumn =
                isKingSide ? 5 : 3;

            Spot rookStartSpot =
                Board.Spots[
                    kingStart.Row,
                    rookStartColumn];

            Spot rookDestinationSpot =
                Board.Spots[
                    kingStart.Row,
                    rookDestinationColumn];

            Piece rook =
                rookStartSpot.Piece;

            rookDestinationSpot.Piece =
                rook;

            rookStartSpot.Piece =
                Piece.Empty;

            rook.HasMoved = true;
        }

        /// <summary>
        /// Updates the two FEN move counters after a move
        /// has been fully completed.
        /// </summary>
        private void RecordCompletedMove(
            Piece movingPiece,
            bool isCapture)
        {
            bool resetsHalfmoveClock =
                movingPiece.Type == PieceType.Pawn ||
                isCapture;

            HalfmoveClock =
                resetsHalfmoveClock
                    ? 0
                    : HalfmoveClock + 1;

            if (movingPiece.Color == PieceColor.Black)
            {
                FullmoveNumber++;
            }
        }

        /// <summary>
        /// Switches to the other player's turn.
        /// </summary>
        private void SwitchTurn()
        {
            Turn =
                Turn == PieceColor.White
                    ? PieceColor.Black
                    : PieceColor.White;
        }

        /// <summary>
        /// Adds a check notice to the move message.
        /// </summary>
        private void AddCheckMessage(
            ref string message)
        {
            if (MoveValidator.IsKingInCheck(
                    Board,
                    Turn))
            {
                message +=
                    $" {GetFactionDisplayName(Turn)} are in check.";
            }
        }

        /// <summary>
        /// Returns the player-facing faction name
        /// for the specified chess color.
        /// </summary>
        private static string GetFactionDisplayName(
            PieceColor color)
        {
            return
                color == PieceColor.White
                    ? "The Hallowed Saints"
                    : "The Damned";
        }

        /// <summary>
        /// Returns the possessive faction name
        /// used in turn-status messages.
        /// </summary>
        private static string GetFactionPossessiveName(
            PieceColor color)
        {
            return
                color == PieceColor.White
                    ? "the Hallowed Saints'"
                    : "the Damned's";
        }

        /// <summary>
        /// Determines whether the current position is active,
        /// checkmate, stalemate, or a draw caused by insufficient
        /// material, repetition, or the fifty-move rule.
        /// </summary>
        private void UpdateGameResult()
        {
            if (!HasAnyLegalMove(Turn))
            {
                if (MoveValidator.IsKingInCheck(
                        Board,
                        Turn))
                {
                    Result =
                        Turn == PieceColor.White
                            ? GameResult.BlackWon
                            : GameResult.WhiteWon;

                    return;
                }

                Result =
                    GameResult.Draw;

                return;
            }

            if (HasInsufficientMaterial())
            {
                Result =
                    GameResult.InsufficientMaterial;

                return;
            }

            if (HasThreefoldRepetition())
            {
                Result =
                    GameResult.ThreefoldRepetition;

                return;
            }

            if (HalfmoveClock >= 100)
            {
                Result =
                    GameResult.FiftyMoveRule;

                return;
            }

            Result =
                GameResult.InProgress;
        }

        /// <summary>
        /// Records the current position without the halfmove
        /// and fullmove counters. Repetition requires identical
        /// piece placement, side to move, castling rights, and
        /// en passant availability.
        /// </summary>
        private void RecordCurrentPosition()
        {
            string key =
                CreatePositionKey();

            if (positionOccurrences.TryGetValue(
                    key,
                    out int occurrenceCount))
            {
                positionOccurrences[key] =
                    occurrenceCount + 1;

                return;
            }

            positionOccurrences[key] = 1;
        }

        /// <summary>
        /// Returns a repetition key containing the first four
        /// FEN fields while intentionally excluding both move
        /// counters.
        /// </summary>
        private string CreatePositionKey()
        {
            string[] fenFields =
                FenService.ExportFen(this).Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            return string.Join(
                " ",
                fenFields,
                0,
                4);
        }

        /// <summary>
        /// Returns true after the current position has appeared
        /// at least three times during this game.
        /// </summary>
        private bool HasThreefoldRepetition()
        {
            return
                positionOccurrences.TryGetValue(
                    CreatePositionKey(),
                    out int occurrenceCount) &&
                occurrenceCount >= 3;
        }

        /// <summary>
        /// Returns true when neither side has enough remaining
        /// material to produce checkmate. This covers king versus
        /// king, king and one bishop or knight versus king, and
        /// positions containing only bishops on one color complex.
        /// </summary>
        private bool HasInsufficientMaterial()
        {
            int nonKingPieceCount = 0;
            bool onlyBishopsRemain = true;
            int? bishopSquareColor = null;

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Piece piece =
                        Board.Spots[row, column].Piece;

                    if (piece.IsEmpty ||
                        piece.Type == PieceType.King)
                    {
                        continue;
                    }

                    // Any pawn, rook, or queen supplies enough
                    // potential mating material to keep playing.
                    if (piece.Type == PieceType.Pawn ||
                        piece.Type == PieceType.Rook ||
                        piece.Type == PieceType.Queen)
                    {
                        return false;
                    }

                    nonKingPieceCount++;

                    if (piece.Type == PieceType.Knight)
                    {
                        onlyBishopsRemain = false;
                        continue;
                    }

                    if (piece.Type != PieceType.Bishop)
                    {
                        return false;
                    }

                    int currentBishopSquareColor =
                        (row + column) % 2;

                    if (bishopSquareColor == null)
                    {
                        bishopSquareColor =
                            currentBishopSquareColor;
                    }
                    else if (bishopSquareColor.Value !=
                             currentBishopSquareColor)
                    {
                        onlyBishopsRemain = false;
                    }
                }
            }

            if (nonKingPieceCount == 0)
            {
                return true;
            }

            if (nonKingPieceCount == 1)
            {
                return true;
            }

            return
                onlyBishopsRemain &&
                bishopSquareColor != null;
        }

        /// <summary>
        /// Searches the board for at least one
        /// legal move for the specified player.
        /// </summary>
        private bool HasAnyLegalMove(
            PieceColor color)
        {
            for (int startRow = 0;
                 startRow < 8;
                 startRow++)
            {
                for (int startColumn = 0;
                     startColumn < 8;
                     startColumn++)
                {
                    Spot start =
                        Board.Spots[
                            startRow,
                            startColumn];

                    if (start.Piece.IsEmpty ||
                        start.Piece.Color != color)
                    {
                        continue;
                    }

                    if (GetLegalMoves(start).Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}