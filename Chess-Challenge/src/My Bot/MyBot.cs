using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int positions = 0; //#DEBUG

    int infinity = 1000000;
    Board board;

    public Move Think(Board mboard, Timer timer)
    {
        board = mboard;
        (int bestEval, Move bestMove) = Negamax(-infinity, infinity, 6, board.IsWhiteToMove ? -1 : 1);
        Console.WriteLine($"Positions {positions} | Time {timer.MillisecondsElapsedThisTurn} | Best Eval {-bestEval} | First Move {bestMove}");//#DEBUG
        Move[] mvs = board.GetLegalMoves();
        if (bestMove == Move.NullMove) bestMove = mvs[new Random().Next(mvs.Length)];
        return bestMove;
    }

    (int, Move) Negamax(int alpha, int beta, int depthleft, int color)
    {
        if (depthleft == 0 || board.IsInCheckmate() || board.IsDraw())
            return (Evaluate() * color, Move.NullMove); // Corrected this line
        Move bm = Move.NullMove;
        int bestScore = -infinity;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            (int score, _) = Negamax(-beta, -alpha, depthleft - 1, -color);
            board.UndoMove(move);
            score = -score; // Corrected this line
            if (score > bestScore)
            {
                bestScore = score;
                bm = move;
            }
            alpha = Math.Max(alpha, score);

            if (alpha >= beta)
                break;
        }
        return (bestScore, bm); // Corrected this line
    }

    int[] p_values = { 0, 100, 300, 320, 500, 900, 10000 };
    int Evaluate()
    {
        if (board.IsDraw()) return 0;
        if (board.IsInCheckmate()) return -infinity;
        positions++;//#DEBUG

        int eval = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int v = p_values[(int)piece.PieceType];
                eval += piece.IsWhite ? v : -v;
            }
        }
        return eval;
    }
}