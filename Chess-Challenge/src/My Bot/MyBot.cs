using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int positions = 0; //#DEBUG
    private readonly int infinity = 1000000;
    public Move Think(Board board, Timer timer)
    {
        int depth = 3; // You can adjust the search depth as needed.

        Move bestMove = Move.NullMove;
        int bestScore = -infinity;
        int alpha = -infinity;
        int beta = infinity;

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Math.Max(alpha, score);
        }

        Console.WriteLine($"{positions} PV");

        return bestMove;
    }

    private int Negamax(Board board, int depth, int alpha, int beta)
    {
        if(board.IsInCheckmate() || board.IsDraw()) {
            return Evaluate(board, depth);
        }
        if (depth == 0)
            return Evaluate(board);

        int bestScore = -infinity;

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int score = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            if (score > bestScore)
                bestScore = score;

            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
                break;
        }

        return bestScore;
    }

    private readonly int[] pieceValues = { 0, 100, 320, 330, 500, 900, 10000 };
    private int Evaluate(Board board, int depth = 0)
    {
        positions++; //#DEBUG
        // Equal / draw
        int evaluation = 0;
        // Add your evaluation logic here.
        // You can assign scores based on piece values, piece positions, mobility, and other factors.
        // Positive scores favor white, and negative scores favor black.
        // For simplicity, this example returns a random score between -100 and 100.

        //Piece evaluation
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int score = pieceValues[(int)piece.PieceType];
                evaluation += (piece.IsWhite) ? score : -score;
            }
        }

        //Absolute rules
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? -infinity - depth : infinity + depth;
        }
        if (board.IsDraw()) return 0;
        return evaluation;
    }
}
