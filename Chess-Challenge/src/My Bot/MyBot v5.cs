using System;
using ChessChallenge.API;


public class MyBotv5 : IChessBot
{
    double[] EncryptedMiddleGameArray = {
        21020001020050000, 31010001010040000, 11010001010030000, 1004001010030001, 1004001010030001, 11010001010030000, 31010001010040000, 21020001020050000,
        21010005010040052, 21001011001020052, 1001011001001051, 1001011001001051, 1001011001001051, 1001011001001051, 21001011001020052, 21010005010040052,
        10010004010030012, 20001001001001012, 20005001005011020, 20005001011015030, 20005001011015030, 20005001005011020, 20001001001001012, 10010004010030012,
        20004004010030004, 30001001005005004, 30005001005015012, 40005001011021020, 40005001011021020, 30005001005015012, 30001001005005004, 20004004010030004,
        30001004010030000, 40001001001001000, 40005001011015000, 50005001011021020, 50005001011021020, 40005001011015000, 40001001001001000, 30004004010030000,
        30010004010030004, 40005001011005010, 40005001011011010, 50005001011015000, 50005001011015000, 40005001011011010, 40001001011005010, 30010004010030004,
        30010004010040004, 40001001005020010, 40005001001001010, 50001001001005020, 50001001001005020, 40001001001001010, 40001001005020010, 30010004010040004,
        30020001020050000, 40010001010040000, 40010001010030000, 50004005010030000, 50004005010030000, 40010001010030000, 40010001010040000, 30020001020050000
    };

    double[] EncryptedEndGameArray = {
        50020001020050000, 40010001010040000, 30010001010030000, 20004001010030000, 20004001010030000, 30010001010030000, 40010001010040000, 50020001020050000,
        30010005010040052, 20001011001020052, 10001011001001052, 1001011001001051, 1001011001001051, 10001011001001052, 20001011001020052, 30010005010040052,
        30010004010030012, 10001001001001012, 21005001005011020, 31005001011015030, 31005001011015030, 21005001005011020, 10001001001001012, 30010004010030012,
        30004004010030004, 10001001005005004, 31005001005015012, 41005001011021020, 41005001011021020, 31005001005015012, 10001001005005004, 30004004010030004,
        30001004010030000, 10001001001001000, 31005001011015000, 41005001011021020, 41005001011021020, 31005001011015000, 10001001001001000, 30001004010030000,
        30010004010030004, 10005001011005004, 21005001011011010, 31005001011015000, 31005001011015000, 21005001011011010, 10001001011005004, 30010004010030004,
        30010004010040004, 30001001005020012, 1005001001001011, 1001001001005020, 1001001001005020, 1001001001001011, 30001001005020012, 30010004010040004,
        50020001020050000, 30010001010040000, 30010001010030000, 30004005010030000, 30004005010030000, 30010001010030000, 30010001010040000, 50020001020050000
    };

    int positions = 0; //#DEBUG
    int minDepth = 3;
    int maxDepth = 100;
    int infinity = 1000000;
    int maxTime = 100; // In ms

    public Move Think(Board board, Timer timer)
    {
        maxTime = (timer.GameStartTimeMilliseconds + timer.IncrementMilliseconds * 60) / 60;
        positions = 0;
        int d;
        Move bestMove = Move.NullMove;
        int bestScore = -infinity;

        for (d = minDepth; d <= maxDepth && timer.MillisecondsElapsedThisTurn < maxTime; d++)
        {
            (Move move, int score) = Negamax(board, d, -infinity, infinity, board.IsWhiteToMove ? 1 : -1, ref timer);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Console.WriteLine($"{positions} PV / BS {bestScore} / BM {bestMove} / T {timer.MillisecondsElapsedThisTurn}ms / MT {maxTime} / D {d - 1}"); //#DEBUG

        if (bestMove == Move.NullMove)
        {
            Console.WriteLine("A null move was played");
            bestMove = OrderedMoves(board)[0];
        }

        return bestMove;
    }

    private (Move, int) Negamax(Board board, int depth, int alpha, int beta, int color, ref Timer timer)
    {
        if (timer.MillisecondsElapsedThisTurn >= maxTime && depth != minDepth) return (Move.NullMove, Evaluate(board, color));
        if (depth == 0 || board.IsInCheckmate() || board.IsDraw()) return (Move.NullMove, Evaluate(board, color));

        int bestScore = -infinity;
        Move bestMove = Move.NullMove;
        foreach (Move move in OrderedMoves(board))
        {
            board.MakeMove(move);
            (_, int score) = Negamax(board, depth - 1, -beta, -alpha, -color, ref timer);
            score = -score;
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

        return (bestMove, bestScore);
    }

    private readonly int[] pieceValues = { 0, 100, 300, 320, 500, 900, 10000 };
    // Positive if the current side is winning
    private int Evaluate(Board board, int color)
    {
        positions++;//#DEBUG

        if (board.IsInCheckmate())
        {
            return (infinity - board.PlyCount) * color;
        }

        if (board.IsFiftyMoveDraw()) return -50 * color;
        if (board.IsInStalemate()) return 0;
        if (board.IsInsufficientMaterial()) return -50 * color;
        if (board.IsRepeatedPosition()) return -100 * color;

        int evaluation = 0;
        foreach (PieceList pieces in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieces)
            {
                int value = pieceValues[(int)piece.PieceType];
                evaluation += piece.IsWhite ? value : -value;
                if (piece.PieceType != PieceType.None) evaluation += (int)Math.Round(calcPiecePos(board, piece) * (piece.IsWhite ? 1 : -1) * 0.5);
            }
        }
        return evaluation * color;
    }

    private int calcPiecePos(Board board, Piece piece)
    {
        int square = piece.Square.Index;
        int t = (int)piece.PieceType - 1;
        double[] table = isEndGame(board) ? EncryptedEndGameArray : EncryptedMiddleGameArray;

        if (!piece.IsWhite)
        {
            Array.Reverse(table);
        }

        return DecryptValue(table, square, t);
    }

    private bool isEndGame(Board board)
    {
        int diffPieceCount = 0;

        for (int i = 1; i <= 6; i++)
        {
            diffPieceCount += Math.Abs(board.GetPieceList((PieceType)i, true).Count - board.GetPieceList((PieceType)i, false).Count);
        }

        return diffPieceCount <= 10;
    }

    private Move[] OrderedMoves(Board board)
    {
        Move[] allMoves = board.GetLegalMoves();
        Array.Sort(allMoves, (m1, m2) => GetMoveScore(m2) - GetMoveScore(m1));
        return allMoves;
    }

    private int GetMoveScore(Move move)
    {
        int score = 0;

        if (move.IsPromotion)
            score += pieceValues[(int)move.PromotionPieceType];

        if (move.IsCapture)
            score += pieceValues[(int)move.CapturePieceType] - pieceValues[(int)move.MovePieceType];

        if (move.IsCastles)
            score += 1000;

        if (move.MovePieceType == PieceType.King || move.MovePieceType == PieceType.Queen)
            score -= infinity;

        return score;
    }



    static int DecryptValue(double[] encryptedValue, int index, int piece)
    {
        string encryptedString = encryptedValue[index].ToString();

        int result = 0;
        for (int i = 0; i <= piece && encryptedString.Length >= 3; i++)
        {
            encryptedString = encryptedString.Remove(encryptedString.Length - 3);
        }

        int a = encryptedString.Length - 3;
        int c = 3;
        if (a < 0)
        {
            a = 0; c = encryptedString.Length;
        }
        result = int.Parse(encryptedString.Substring(a, c));
        if (result % 2 == 0)
        {
            result = -result;
        }

        return result;
    }





    //static double[] EncryptValues(int[,] values)
    //{
    //    List<double> encryptedTables = new List<double>();
    //
    //    int rows = values.GetLength(0);
    //    int cols = values.GetLength(1);
    //
    //    for (int a = 0; a < cols; a++) // Loop over columns
    //    {
    //        List<double> table = new List<double>();
    //        for (int i = 0; i < rows; i++) // Loop over rows
    //        {
    //            table.Add(values[i, a]);
    //        }
    //
    //        double o = 0;
    //        int power = 0;
    //        foreach (double value in table) // Iterate over values in the table list
    //        {
    //            double v = value;
    //            if (value < 0)
    //            {
    //                if (value % 2 != 0)
    //                {
    //                    v++;
    //                }
    //            }
    //            else
    //            {
    //                if (value % 2 == 0)
    //                {
    //                    v++;
    //                }
    //            }
    //            double k = Math.Abs(v);
    //            o += k * Math.Pow(1000, power);
    //            power++;
    //        }
    //
    //        encryptedTables.Add(o);
    //    }
    //
    //    return encryptedTables.ToArray();
    //}
}
