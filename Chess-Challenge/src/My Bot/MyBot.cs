using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int positions = 0; //#DEBUG
    private const int infinity = 1000000;
    private const int minDepth = 3; // Minimum depth for iterative deepening
    private const int maxDepth = 10; // Maximum depth for iterative deepening
    private const int maxTime = 100; // Maximum time for iterative deepening (in ms)

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = OrderedMoves(board);
        Move bestMove = Move.NullMove;
        int bestScore = -infinity;
        int alpha = -infinity;
        int beta = infinity;

        for (int depth = minDepth; depth <= maxDepth; depth++)
        {
            foreach (var move in allMoves)
            {
                board.MakeMove(move);
                int color = board.IsWhiteToMove ? 1 : -1;
                int score = -Negamax(board, depth - 1, -beta, -alpha, color);
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

            if (timer.MillisecondsElapsedThisTurn >= maxTime)
                break;
        }

        Console.WriteLine($"{positions} PV with best score of {bestScore}"); //#DEBUG

        return bestMove == Move.NullMove ? allMoves[0] : bestMove;
    }

    private int Negamax(Board board, int depth, int alpha, int beta, int color)
    {
        if (depth == 0)
            return Evaluate(board, depth) * color;

        int bestScore = -infinity;

        foreach (var move in OrderedMoves(board))
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);

            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
                break;
        }

        return bestScore;
    }

    private readonly int[] pieceValues = { 0, 100, 300, 320, 500, 900, 10000 };

    private int Evaluate(Board board, int depth)
    {
        positions++; //#DEBUG

        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -infinity - depth : infinity + depth;

        if (board.IsDraw())
            return 0;

        int evaluation = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int score = pieceValues[(int)piece.PieceType];
                evaluation += piece.IsWhite ? score : -score;
            }
        }

        return evaluation;
    }

    private Move[] OrderedMoves(Board board)
    {
        Move[] allMoves = board.GetLegalMoves();
        int[] scores = new int[allMoves.Length];

        for (int i = 0; i < allMoves.Length; i++)
        {
            int score = 0;
            Move move = allMoves[i];

            if (move.IsPromotion)
                score += pieceValues[(int)move.PromotionPieceType];

            if (move.IsCapture)
                score = 1000 * pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];

            if (move.IsCastles)
                score += 10000;

            if (move.MovePieceType == PieceType.King)
                score -= infinity;

            board.MakeMove(move);
            if (board.IsInCheckmate())
                score = infinity;
            board.UndoMove(move);

            scores[i] = score;
        }

        Array.Sort(scores, allMoves);
        Array.Reverse(allMoves);

        return allMoves;
    }
}
