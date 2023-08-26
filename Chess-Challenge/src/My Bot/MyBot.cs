using System;
using ChessChallenge.API;


public class MyBot : IChessBot
{

    int inf = 10000;

    int maxDepth = 1;
    Board board;
    int[] p_value = { 0, 100, 300, 320, 500, 900, 10000 };
    public Move Think(Board mainBoard, Timer timer)
    {
        board = mainBoard;
        Move bm = Move.NullMove;
        for (int depth = 0; depth < maxDepth; depth++)
        {
            (bm, _) = Negamax(depth, inf, -inf);
        }
        return bm;
    }

    (Move, int) Negamax(int depth, int beta, int alpha)
    {
        if (depth == 0) return (Move.NullMove, Evaluate());

        Move b_Move = Move.NullMove;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            (_, int score) = Negamax(depth - 1, -alpha, -beta);
            board.UndoMove(move);

            if (score >= beta)
            {
                return (move, beta);
            }
            if (score > alpha)
            {
                alpha = score;
                b_Move = move;
            }
        }
        return (b_Move, alpha);
    }

    int Evaluate()
    {
        int eval = 0;
        for (int i = 0; i < 6; i++)
        {
            eval += (board.GetPieceList((PieceType)i, true).Count + board.GetPieceList((PieceType)i, false).Count) * p_value[i];
        }
        return eval * (board.IsWhiteToMove ? 1 : -1);
    }


}