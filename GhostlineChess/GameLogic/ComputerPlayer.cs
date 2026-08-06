using GhostlineChess.Enums;
using GhostlineChess.Models;

namespace GhostlineChess.GameLogic
{
    /// <summary>
    /// Supplies computer-controlled moves without bypassing
    /// the existing Ghostline Chess legality rules.
    /// </summary>
    public sealed class ComputerPlayer
    {
        private const int CheckmateScore = 100_000;

        private readonly Random random = new Random();

        /// <summary>
        /// Chooses a move by scoring material and looking ahead
        /// at the opponent's strongest reply. Search positions
        /// are reconstructed from FEN so the live game is never
        /// mutated while the computer is thinking.
        /// </summary>
        public ChessMove? ChooseMove(
            string fen,
            PieceColor aiColor,
            int searchDepth = 2)
        {
            ChessGame? game =
                CreateGameFromFen(fen);

            if (game == null ||
                game.Result != GameResult.InProgress ||
                game.Turn != aiColor)
            {
                return null;
            }

            IReadOnlyList<ChessMove> legalMoves =
                game.GetLegalMovesForTurn();

            if (legalMoves.Count == 0)
            {
                return null;
            }

            int bestScore = int.MinValue;
            List<ChessMove> bestMoves =
                new List<ChessMove>();

            foreach (ChessMove move in
                     OrderMoves(game, legalMoves))
            {
                ChessGame? child =
                    CloneAndApplyMove(game, move);

                if (child == null)
                {
                    continue;
                }

                int score =
                    Search(
                        child,
                        Math.Max(0, searchDepth - 1),
                        int.MinValue + 1,
                        int.MaxValue - 1,
                        aiColor);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }

            if (bestMoves.Count == 0)
            {
                return null;
            }

            // Keep equal positions from feeling mechanical while
            // never choosing a move with a lower evaluation.
            return bestMoves[
                random.Next(bestMoves.Count)];
        }

        /// <summary>
        /// Minimax search with alpha-beta pruning. The AI tries
        /// to maximize the evaluation while its opponent is
        /// assumed to choose the strongest available reply.
        /// </summary>
        private static int Search(
            ChessGame game,
            int depth,
            int alpha,
            int beta,
            PieceColor aiColor)
        {
            if (depth == 0 ||
                game.Result != GameResult.InProgress)
            {
                return Evaluate(game, aiColor, depth);
            }

            IReadOnlyList<ChessMove> legalMoves =
                game.GetLegalMovesForTurn();

            if (legalMoves.Count == 0)
            {
                return Evaluate(game, aiColor, depth);
            }

            bool maximizing =
                game.Turn == aiColor;

            int bestScore =
                maximizing
                    ? int.MinValue
                    : int.MaxValue;

            foreach (ChessMove move in
                     OrderMoves(game, legalMoves))
            {
                ChessGame? child =
                    CloneAndApplyMove(game, move);

                if (child == null)
                {
                    continue;
                }

                int score =
                    Search(
                        child,
                        depth - 1,
                        alpha,
                        beta,
                        aiColor);

                if (maximizing)
                {
                    bestScore =
                        Math.Max(bestScore, score);

                    alpha =
                        Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore =
                        Math.Min(bestScore, score);

                    beta =
                        Math.Min(beta, bestScore);
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestScore;
        }

        /// <summary>
        /// Values decisive results first, then material, checks,
        /// central activity, and modest pawn advancement.
        /// </summary>
        private static int Evaluate(
            ChessGame game,
            PieceColor aiColor,
            int remainingDepth)
        {
            if (IsWinFor(game.Result, aiColor))
            {
                return CheckmateScore + remainingDepth;
            }

            PieceColor opponentColor =
                aiColor == PieceColor.White
                    ? PieceColor.Black
                    : PieceColor.White;

            if (IsWinFor(game.Result, opponentColor))
            {
                return -CheckmateScore - remainingDepth;
            }

            if (game.Result != GameResult.InProgress)
            {
                return 0;
            }

            int score = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Piece piece =
                        game.Board.Spots[row, column].Piece;

                    if (piece.IsEmpty)
                    {
                        continue;
                    }

                    int pieceScore =
                        GetPieceValue(piece.Type) +
                        GetPositionBonus(
                            piece,
                            row,
                            column);

                    score +=
                        piece.Color == aiColor
                            ? pieceScore
                            : -pieceScore;
                }
            }

            if (MoveValidator.IsKingInCheck(
                    game.Board,
                    opponentColor))
            {
                score += 35;
            }

            if (MoveValidator.IsKingInCheck(
                    game.Board,
                    aiColor))
            {
                score -= 35;
            }

            return score;
        }

        private static bool IsWinFor(
            GameResult result,
            PieceColor color)
        {
            return
                (color == PieceColor.White &&
                 result == GameResult.WhiteWon) ||
                (color == PieceColor.Black &&
                 result == GameResult.BlackWon);
        }

        private static int GetPieceValue(
            PieceType type)
        {
            return type switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 320,
                PieceType.Bishop => 330,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 20_000,
                _ => 0
            };
        }

        /// <summary>
        /// Adds deliberately small positional preferences so
        /// material and tactics always dominate the decision.
        /// </summary>
        private static int GetPositionBonus(
            Piece piece,
            int row,
            int column)
        {
            int bonus = 0;

            bool centralSquare =
                row >= 2 && row <= 5 &&
                column >= 2 && column <= 5;

            if (centralSquare &&
                (piece.Type == PieceType.Knight ||
                 piece.Type == PieceType.Bishop))
            {
                bonus += 12;
            }

            if (piece.Type == PieceType.Pawn)
            {
                int advancement =
                    piece.Color == PieceColor.White
                        ? 6 - row
                        : row - 1;

                bonus += Math.Max(0, advancement) * 4;
            }

            return bonus;
        }

        /// <summary>
        /// Searches captures first, improving alpha-beta pruning
        /// and making forcing tactical moves cheaper to find.
        /// </summary>
        private static IEnumerable<ChessMove> OrderMoves(
            ChessGame game,
            IReadOnlyList<ChessMove> moves)
        {
            return moves
                .OrderByDescending(
                    move =>
                        GetPieceValue(
                            game.Board.Spots[
                                move.EndRow,
                                move.EndColumn]
                            .Piece.Type));
        }

        private static ChessGame? CloneAndApplyMove(
            ChessGame game,
            ChessMove move)
        {
            ChessGame? clone =
                CreateGameFromFen(
                    FenService.ExportFen(game));

            if (clone == null ||
                !ApplyMove(clone, move))
            {
                return null;
            }

            return clone;
        }

        private static ChessGame? CreateGameFromFen(
            string fen)
        {
            ChessGame game =
                new ChessGame();

            return FenService.TryImportFen(
                    game,
                    fen,
                    out _)
                ? game
                : null;
        }

        private static bool ApplyMove(
            ChessGame game,
            ChessMove move)
        {
            if (!game.SelectSpot(
                    move.StartRow,
                    move.StartColumn,
                    out _) ||
                !game.SelectSpot(
                    move.EndRow,
                    move.EndColumn,
                    out _))
            {
                return false;
            }

            if (game.PromotionPending)
            {
                return game.CompletePromotion(
                    PieceType.Queen,
                    out _);
            }

            return true;
        }
    }
}
