using UnityEngine;


public enum PieceType
{
    Pawn, Rook, Knight, Bishop, Queen, King
}

public enum TeamColor
{
    White, Black
}

public class  ChessPiece : MonoBehaviour
{
    [HideInInspector]
    public PieceType pieceType;
    [HideInInspector]
    public TeamColor teamColor;

    [HideInInspector]
    public int boardX;
    [HideInInspector]
    public int boardY;

    [Header("Visual")]
    public float yOffset = 0.05f;

    public void SetPosition(int x, int y) //위치 설정
    {
        boardX = x;
        boardY = y;
        transform.position = new Vector3(x , yOffset, y );
    }

    public void MoveTo(int x, int y) //움직이기
    {
        boardX = x;
        boardY = y;
        transform.position = new Vector3(x, yOffset, y);
    }
}
