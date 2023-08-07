using System;
using ChessChallenge.API;


// IMPLEMENT THE BRUTAL TIMER BREAK INTO THE NEGAMAX

public class MyBot : IChessBot
{
    int positions = 0; //#DEBUG
    int minDepth = 4;
    int maxDepth = 100;
    int infinity = 1000000;
    int maxTime = 100; // In ms
    public Move Think(Board board, Timer timer)
    {
        positions = 0;
        int d;//#DEBUG
        Move bestMove = Move.NullMove;
        int bestScore=-infinity;
        for (d = minDepth; d <= maxDepth && timer.MillisecondsElapsedThisTurn<maxTime; d++)
        {
            (bestMove, bestScore) = Negamax(board, d, -infinity, infinity, board.IsWhiteToMove ? -1 : 1, ref timer);
            if(bestScore >= infinity) break;
        }
        Console.WriteLine($"{positions} PV / BS {bestScore} / BM {bestMove} / T {timer.MillisecondsElapsedThisTurn}ms / D {d}");//#DEBUG
        return bestMove;
    }

    private (Move, int) Negamax(Board board, int depth, int alpha, int beta, int color, ref Timer timer)
    {
        Move[] allMoves = OrderedMoves(board);
        Move[] nullMove = { Move.NullMove };
        allMoves = (allMoves.Length == 0) ? nullMove : allMoves;
        if (depth == 0) return (allMoves[0], Evaluate(board, -color));

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
    private int Evaluate(Board board, int color)
    {
        positions++; //#DEBUG

        int evaluation = 0;
        for (int i = 0; ++i < 7;)
            evaluation += (board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count) * pieceValues[i];

        if (board.IsDraw()) evaluation = (color==1)?-10:10;
        if (board.IsInCheckmate()) evaluation = -infinity;
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

        PieceType MovePieceType = move.MovePieceType;

        if (move.IsPromotion)
            score += pieceValues[(int)move.PromotionPieceType];

        if (move.IsCapture)
            score += pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];

        if (move.IsCastles)
            score += 1000;

        if (MovePieceType == PieceType.King || MovePieceType == PieceType.Queen)
            score -= infinity;

        return score;
    }
}
