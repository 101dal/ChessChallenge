using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int maxDepth = 3
    public Move Think(Board board, Timer timer)
    {
        (Move,int) moveScore = Negamax(board, maxDepth);
        return board.GetLegalMoves()[0];
    }

    private (Move,int) Negamax(Board board, int depth){
        board.MakeMove(move);
        Move[] allMoves = OrderedMoves(board);

        board.UndoMove(move);
        return (Move.NullMove, 0);
    }

    Move[] OrderedMoves(Board board){
        return board.GetLegalMoves();
    }
}
