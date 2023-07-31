using System;
using ChessChallenge.API;
public class MyBot : IChessBot
{
    readonly int[] piecesValue = { 0, 100, 310, 330, 500, 900, 10000 };
    const int infinity = 10000;//Value for infinite;
    const int timerMax = 1000;//In ms
    const int depthMax = 1000;//Integer
    public Move Think(Board board, Timer timer)
    {
        int bestEval = (board.IsWhiteToMove) ? -infinity : infinity;
        Move bestMove = Move.NullMove;
        int d;
        for (d = 0; (d < depthMax) && (timer.MillisecondsElapsedThisTurn < timerMax) && (!board.IsInCheckmate()); d++)
        {
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                int eval = Negamax(board, infinity, -infinity, d);
                board.UndoMove(move);
                if ((board.IsWhiteToMove && (eval > bestEval)) || (!board.IsWhiteToMove && (eval < bestEval)))
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }
        }
        Console.WriteLine(d);

        return bestMove;
    }

    int Negamax(Board board, int alpha, int beta, int depth)
    {
        if (depth == 0)
        {
            return Evaluate(board);
        }
        if (board.GetLegalMoves().Length == 0)
        {
            if (board.IsInCheck())
            {
                return -infinity;
            }
            return 1;
        }

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -Negamax(board, -alpha, -beta, depth - 1);
            board.UndoMove(move);
            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
            }
        }
        return alpha;
    }

    int Evaluate(Board board)
    {
        int evaluation = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int value = piecesValue[(int)piece.PieceType];
                evaluation += (piece.IsWhite) ? value : -value;
            }
        }
        return evaluation;
    }
}
