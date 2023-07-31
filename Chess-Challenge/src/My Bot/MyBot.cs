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
        for (int d = 0; (d < depthMax) && (timer.MillisecondsElapsedThisTurn < timerMax) && (!board.IsInCheckmate()); d++)
        {
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                int eval = Negamax(board, infinity, -infinity, d);
                board.UndoMove(move);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
            }
        }

        return board.GetLegalMoves()[0];
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
        return 0;
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
