using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    readonly int infinite = 100000;
    readonly int depth = 3;

    public Move Think(Board board, Timer timer)
    {
        Move bestMove = Move.NullMove;
        int max = -infinite;

        foreach(Move move in board.GetLegalMoves()) {
            board.MakeMove(move);
            int eval = Negamax(board, depth - 1);
            board.UndoMove(move);
            if (eval > max) {
                max = eval;
                bestMove = move;
            }
        }
        return bestMove;
    }

    int Negamax(Board board, int depth)
    {
        if (depth == 0) return Evaluation(board);
        int max = -infinite;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1);
            board.UndoMove(move);
            if (score > max)
            {
                max = score;
            }
        }
        return max;
    }

    // Positive if white and negative if black
    int Evaluation(Board board)
    {
        return 0;
    }
}
