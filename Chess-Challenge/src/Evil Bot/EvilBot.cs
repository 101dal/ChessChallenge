using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {


        private static readonly Random RandomGenerator = new Random();
        // Null = 0, Pawn = 100, Knight = 300, Bishop = 320, Rook = 500, Queen = 900, King = 10000
        private static readonly int[] PieceValues = { 0, 100, 300, 320, 500, 900, 10000 };

        private int positionsEvaluated = 0;
        private Dictionary<ulong, TranspositionEntry> transpositionTable = new Dictionary<ulong, TranspositionEntry>();

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = OrderMoves(board);
            bool isWhite = board.IsWhiteToMove;
            int bestScore = (isWhite) ? int.MinValue : int.MaxValue;
            int depth = 3;
            Move bestMove = allMoves[0];

            for (int d = 1; d <= depth; d++)
            {
                positionsEvaluated = 0;
                foreach (Move move in allMoves)
                {
                    board.MakeMove(move);
                    int score = MinimaxWithTransposition(board, d, int.MinValue, int.MaxValue, !isWhite);
                    board.UndoMove(move);

                    if ((isWhite && score > bestScore) || (!isWhite && score < bestScore))
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
            }

            return bestMove;
        }

        private int MinimaxWithTransposition(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth <= 0 || board.IsDraw() || board.IsInCheckmate())
            {
                positionsEvaluated++;
                return Evaluate(board, maximizingPlayer);
            }

            ulong zobristKey = board.ZobristKey;
            if (transpositionTable.TryGetValue(zobristKey, out TranspositionEntry? entry) && entry != null && entry.Depth >= depth)
            {
                if (entry.EntryType == TranspositionEntryType.Exact)
                    return entry.Score;
                if (entry.EntryType == TranspositionEntryType.LowerBound)
                    alpha = Math.Max(alpha, entry.Score);
                if (entry.EntryType == TranspositionEntryType.UpperBound)
                    beta = Math.Min(beta, entry.Score);

                if (alpha >= beta)
                    return entry.Score;
            }

            if (maximizingPlayer)
            {
                int maxScore = int.MinValue;
                foreach (Move move in OrderMoves(board))
                {
                    board.MakeMove(move);
                    int score = MinimaxWithTransposition(board, depth - 1, alpha, beta, false);
                    board.UndoMove(move);
                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, maxScore);
                    if (beta <= alpha)
                    {
                        break; // Beta cutoff
                    }
                }

                TranspositionEntryType entryType = (maxScore <= alpha) ? TranspositionEntryType.UpperBound : TranspositionEntryType.Exact;
                transpositionTable[zobristKey] = new TranspositionEntry(entryType, depth, maxScore);
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move move in OrderMoves(board))
                {
                    board.MakeMove(move);
                    int score = MinimaxWithTransposition(board, depth - 1, alpha, beta, true);
                    board.UndoMove(move);
                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, minScore);
                    if (beta <= alpha)
                    {
                        break; // Alpha cutoff
                    }
                }

                TranspositionEntryType entryType = (minScore >= beta) ? TranspositionEntryType.LowerBound : TranspositionEntryType.Exact;
                transpositionTable[zobristKey] = new TranspositionEntry(entryType, depth, minScore);
                return minScore;
            }
        }

        private int Evaluate(Board board, bool isWhiteToMove)
        {
            int evaluation = 0;

            // Evaluate the pieces value
            evaluation += EvaluatePieces(board);

            //ABSOLUTE RULES(integer limits)//
            if (board.IsInCheckmate())
            {
                if (isWhiteToMove)
                {
                    evaluation = int.MinValue;
                }
                else
                {
                    evaluation = int.MaxValue;
                }
            }
            if (board.IsDraw())
            {
                return 0;
            }


            return evaluation;
        }

        private int EvaluatePieces(Board board)
        {
            int evaluation = 0;

            foreach (PieceList pieces in board.GetAllPieceLists())
            {
                foreach (Piece piece in pieces)
                {
                    int value = PieceValues[(int)piece.PieceType];
                    if (piece.IsWhite)
                        evaluation += value;
                    else
                        evaluation -= value;
                }
            }

            return evaluation;
        }

        private Move[] OrderMoves(Board board)
        {
            Move[] legalMoves = board.GetLegalMoves();
            List<KeyValuePair<int, Move>> finalMoves = new List<KeyValuePair<int, Move>>();

            foreach (Move move in legalMoves)
            {
                int score = 0;

                //BONUSES//
                if (move.IsCapture)
                {
                    score += PieceValues[(int)move.CapturePieceType] - PieceValues[(int)move.MovePieceType];
                }
                if (move.IsCastles)
                {
                    score += 10000;
                }

                //MALUSES//
                if (PieceValues[(int)move.CapturePieceType] < PieceValues[(int)move.MovePieceType])
                {
                    score -= PieceValues[(int)move.MovePieceType];
                }

                //ABSOLUTE RULES (intiger limits)//

                board.MakeMove(move);

                //If the board is in checkmate then I prioritize this move
                if (board.IsInCheckmate())
                {
                    score = int.MaxValue;
                }
                board.UndoMove(move);


                // Add the move with its value to the finalMoves list
                finalMoves.Add(new KeyValuePair<int, Move>(score, move));
            }

            // Sort the list to get ordered from the highest score to lowest
            finalMoves.Sort((a, b) => b.Key.CompareTo(a.Key));

            // Extract the sorted moves from the list
            Move[] sortedMoves = finalMoves.Select(entry => entry.Value).ToArray();

            return sortedMoves;
        }
    }

    public enum TranspositionEntryType
    {
        Exact,
        LowerBound,
        UpperBound
    }

    public class TranspositionEntry
    {
        public TranspositionEntryType EntryType { get; }
        public int Depth { get; }
        public int Score { get; }

        public TranspositionEntry(TranspositionEntryType entryType, int depth, int score)
        {
            EntryType = entryType;
            Depth = depth;
            Score = score;
        }

    }
}