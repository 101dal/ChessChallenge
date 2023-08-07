using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int positions = 0; //#DEBUG
    private const int infinity = 1000000;
    private const int minDepth = 3;
    private const int maxDepth = 100;
    private const int maxTime = 50;

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = OrderedMoves(board);
        Move bestMove = Move.NullMove;
        int bestScore = -infinity;
        int alpha = -infinity;
        int beta = infinity;
        int depth;
        for (depth = minDepth; depth <= maxDepth; depth++)
        {
            foreach (var move in allMoves)
            {
                board.MakeMove(move);
                int color = board.IsWhiteToMove ? 1 : -1;
                int score = -Negamax(board, depth - 1, -beta, -alpha, color);
                //Console.WriteLine($"{score} {move}"); //#DEBUG
                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
                
                if(bestScore>=infinity) break;

                alpha = Math.Max(alpha, score);

                if (alpha >= beta)
                    break;
            }

            if (timer.MillisecondsElapsedThisTurn >= maxTime || bestScore >= infinity)
                break;
        }

        Console.WriteLine($"{positions} PV with best score of {bestScore} and depth {depth} in {timer.MillisecondsElapsedThisTurn}ms"); //#DEBUG

        return bestMove == Move.NullMove ? allMoves[0] : bestMove;
    }

    private int Negamax(Board board, int depth, int alpha, int beta, int color)
    {
        if (depth == 0)
            return Evaluate(board) * color;

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

    private int Evaluate(Board board)
    {
        positions++; //#DEBUG

        if (board.IsInCheckmate())
            return board.IsWhiteToMove ? -infinity : infinity;

        if (board.IsDraw())
            return board.IsWhiteToMove ? 1 :-1;

        int evaluation = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int score = pieceValues[(int)piece.PieceType];
                evaluation += piece.IsWhite ? score : -score;
            }
        }

        evaluation += PieceMobility(board);

        return evaluation;
    }

    private int PieceMobility(Board board) {
        int mobility = 0;
        mobility += board.GetLegalMoves().Length - board.GetLegalMoves(true).Length;
        board.ForceSkipTurn();
        mobility -= board.GetLegalMoves().Length - board.GetLegalMoves(true).Length;
        board.UndoSkipTurn();
        //Console.WriteLine((mobility * (board.IsWhiteToMove?1:-1)).ToString()); //#DEBUG
        return mobility * (board.IsWhiteToMove?1:-1);
    }

    private Move[] OrderedMoves(Board board)
    {
        Move[] allMoves = board.GetLegalMoves();
        Array.Sort(allMoves, (m1, m2) => GetMoveScore(m2,board) - GetMoveScore(m1,board));
        return allMoves;
    }

    private int GetMoveScore(Move move, Board board)
    {
        int score = 0;

        PieceType MovePieceType = move.MovePieceType;

        board.MakeMove(move);
        if(board.IsInCheckmate()) score+= infinity;

        if (move.IsPromotion)
            score += pieceValues[(int)move.PromotionPieceType];

        if (move.IsCapture)
            score += pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];

        if (move.IsCastles)
            score += 1000;

        if (MovePieceType == PieceType.King || MovePieceType == PieceType.Queen)
            score -= infinity;
        
        board.UndoMove(move);

        return score;
    }
}
