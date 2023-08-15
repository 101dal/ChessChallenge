using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{

    int positions = 0; //#DEBUG
    int minDepth = 3;
    int maxDepth = 100;
    int infinity = 1000000;
    int maxTime = 100; // In ms

    int sideMated = 0; //#DEBUG 0 if none, 1 if whote -1 of black

    public Move Think(Board board, Timer timer)
    {
        maxTime = (timer.GameStartTimeMilliseconds + timer.IncrementMilliseconds * 100)/100;
        positions = 0; //#DEBUG
        int d;//#DEBUG
        Move bestMove = Move.NullMove;
        int bestScore = -infinity;
        for (d = minDepth; d <= maxDepth; d++)
        {
            (Move move, int score) = Negamax(board, d, -infinity, infinity, board.IsWhiteToMove ? 1 : -1, ref timer);
            if(timer.MillisecondsElapsedThisTurn >= maxTime && d!=minDepth) break;
            bestMove = move;
            bestScore = score; //#DEBUG
        }
        Console.WriteLine($"{positions} PV / BS {bestScore} / BM {bestMove} / SideMated {sideMated} / T {timer.MillisecondsElapsedThisTurn}ms / MT {maxTime} / D {d-1}");//#DEBUG
        if (bestMove == Move.NullMove) bestMove = OrderedMoves(board)[0];
        return bestMove;
    }

    private (Move, int) Negamax(Board board, int depth, int alpha, int beta, int color, ref Timer timer)
    {
        if(timer.MillisecondsElapsedThisTurn >= maxTime) return (Move.NullMove, Evaluate(board, color));
        Move[] allMoves = board.GetLegalMoves();
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) return (Move.NullMove, Evaluate(board, color));

        int bestScore = -infinity;
        Move bestMove = allMoves[0];
        foreach (Move move in OrderedMoves(board))
        {
            board.MakeMove(move);
            (_, int score) = Negamax(board, depth - 1, -beta, -alpha, -color, ref timer);
            score = -score;
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
                break;
        }

        return (bestMove, bestScore);
    }

    private readonly int[] pieceValues = { 0, 100, 300, 320, 500, 900, 10000 };
    // Positive if the current side is winning
    private int Evaluate(Board board, int color)
    {
        positions++;//#DEBUG

        if (board.IsInCheckmate())
        {
            sideMated = board.IsWhiteToMove ? 1 : -1; //#DEBUG
            // Return a large positive or negative value based on color
            return (infinity - board.PlyCount) * color;
        }

        if (board.IsFiftyMoveDraw()) return -20 * color;
        if (board.IsInStalemate()) return 0;
        if (board.IsInsufficientMaterial()) return -5 * color;
        if (board.IsRepeatedPosition()) return -40 * color;

        int evaluation = 0;
        for (int i = 1; i <= 6; i++) // Start from index 1 to skip None piece
            evaluation += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceValues[i];

        return evaluation * color;
    }

    private Move[] OrderedMoves(Board board)
    {
        Move[] allMoves = board.GetLegalMoves();
        Array.Sort(allMoves, (m1, m2) => GetMoveScore(m2) - GetMoveScore(m1));
        return allMoves;
    }

    private int GetMoveScore(Move move)
    {
        int score = 0;

        if (move.IsPromotion)
            score += pieceValues[(int)move.PromotionPieceType];

        if (move.IsCapture)
            score += pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];

        if (move.IsCastles)
            score += 1000;

        if (move.MovePieceType == PieceType.King || move.MovePieceType == PieceType.Queen)
            score -= infinity;

        return score;
    }
}
