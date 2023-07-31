using System;
using System.Collections.Generic;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int positions = 0; //#DEBUG
    readonly int[] piecesValue = { 0, 100, 310, 330, 500, 900, 0 };
    const int infinity = 10000; // Value for infinite;
    const int timerMax = 50; // In ms
    const int depthMax = 100; // Adjust the depth to your desired level

    public Move Think(Board board, Timer timer)
    {
        int bestEval = -infinity;
        Move bestMove = Move.NullMove;
        int alpha = -infinity;
        int beta = infinity;
        int d;

        for (d = 1; d <= depthMax; d++)
        {
            foreach (Move move in OrderMoves(board.GetLegalMoves(), board))
            {
                board.MakeMove(move);
                int eval = -Negamax(board, -beta, -alpha, d - 1);
                board.UndoMove(move);
                if (eval > bestEval)
                {
                    bestEval = eval;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, eval);
                if (alpha >= beta)
                    break;
            }

            if (timer.MillisecondsElapsedThisTurn >= timerMax || board.IsInCheckmate() || board.IsDraw())
                break;
        }

        Console.WriteLine($"Depth {d} search with {positions} positions"); //#DEBUG
        return bestMove;
    }

    int Negamax(Board board, int alpha, int beta, int depth)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
        {
            positions++; //#DEBUG
            return Evaluate(board);
        }

        foreach (Move move in OrderMoves(board.GetLegalMoves(), board))
        {
            board.MakeMove(move);
            int score = -Negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);
            alpha = Math.Max(alpha, score);
            if (alpha >= beta)
            {
                return beta;
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
                if (board.SquareIsAttackedByOpponent(piece.Square))
                {
                    int value = piecesValue[(int)piece.PieceType];
                    evaluation += (piece.IsWhite) ? value : -value;
                }
            }
        }
        int coeff = board.IsWhiteToMove ? 1 : -1;

        // PRIORITY LAWS
        if (board.IsInCheckmate()) {
            return -infinity;
        }
        if(board.IsDraw()) {
            return 0;
        }
        return evaluation * coeff;
    }

    Move[] OrderMoves(Move[] moves, Board board)
    {
        List<(Move, int)> orderedMoves = new List<(Move, int)>();

        foreach (Move move in moves)
        {
            int moveScore = GetMoveScore(move, board);

            orderedMoves.Add((move, moveScore));
        }

        orderedMoves.Sort((x, y) => y.Item2.CompareTo(x.Item2));

        Move[] sortedMoves = new Move[moves.Length];
        for (int i = 0; i < orderedMoves.Count; i++)
        {
            sortedMoves[i] = orderedMoves[i].Item1;
        }

        return sortedMoves;
    }

    int GetMoveScore(Move move, Board board)
    {
        int capturePieceValue = piecesValue[(int)move.CapturePieceType];
        int promotionPieceValue = piecesValue[(int)move.PromotionPieceType];
        int movePieceValue = piecesValue[(int)move.MovePieceType];
        int score = 0;

        // Add the value of the captured piece
        if (move.IsCapture)
        {
            score += capturePieceValue;
        }

        // Add the value of the promotion piece
        if (move.IsPromotion)
        {
            score += promotionPieceValue;
        }

        // Add some bonus for castling moves
        if (move.IsCastles)
        {
            score += 50;
        }

        // Remove some points if the captured piece is lower type
        if (capturePieceValue < movePieceValue) {
            score -= movePieceValue * 2;
        }

        return score;
    }
}
