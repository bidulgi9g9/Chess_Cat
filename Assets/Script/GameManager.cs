using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class PieceData
{
    public PieceType type;
    public TeamColor color;
    public int x;
    public int y;
    public bool hasMoved;
    public ChessPiece visualPiece;

    public PieceData(PieceType type, TeamColor color, int x, int y, bool hasMoved, ChessPiece visual)
    {
        this.type = type;
        this.color = color;
        this.x = x;
        this.y = y;
        this.hasMoved = false;
        this.visualPiece = visual;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Piece Prefabs")]
    public GameObject whitePawnPrefab;
    public GameObject whiteRookPrefab;
    public GameObject whiteKnightPrefab;
    public GameObject whiteBishopPrefab;
    public GameObject whiteQueenPrefab;
    public GameObject whiteKingPrefab;

    // 흑
    public GameObject BlackPawnPrefab;
    public GameObject blackRookPrefab;
    public GameObject blackKnightPrefab;
    public GameObject blackBishopPrefab;
    public GameObject blackQueenPrefab;
    public GameObject blackKingPrefab;


    [Header("Visual")]
    public GameObject highlightPrefab;

    [Header("Promotion UI")]
    public GameObject promotionPanel;
    public UnityEngine.UI.Button queenButton;
    public UnityEngine.UI.Button rookButton;
    public UnityEngine.UI.Button bishopButton;
    public UnityEngine.UI.Button knightButton;

    private PieceData[,] board = new PieceData[8, 8];
    private PieceData selectedPiece = null;
    private PieceData promotingPiece = null;
    private TeamColor currentTurn = TeamColor.White;
    private List<GameObject> highlights = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnAllPieces();
        AdjustTableHeight();

        // 프로모션 UI 초기화
        if(promotionPanel != null)
        {
            promotionPanel.SetActive(false);

            // 버튼 클릭 이벤트 설정
            queenButton.onClick.AddListener(() => PromoteTo(PieceType.Queen));
            rookButton.onClick.AddListener(() => PromoteTo(PieceType.Rook));
            bishopButton.onClick.AddListener(() => PromoteTo(PieceType.Bishop));
            knightButton.onClick.AddListener(() => PromoteTo(PieceType.Knight));
        }
    }
    void AdjustTableHeight()
    {
        GameObject table = GameObject.Find("Table");

        if (table != null)
        {
            // 테이블 상판 높이를 체스판보다 살짝 아래로
            float tableY = -0.1f;  // 체스판 Y=0 기준
            table.transform.position = new Vector3(3.5f, tableY, 3.5f);

            Debug.Log($"테이블 위치:  {table.transform.position}");
        }
    }

    void SpawnAllPieces()
    {
        // 백 폰 8개
        for(int i = 0; i < 8; i++)
        {
            SpawnPiece(whitePawnPrefab, PieceType.Pawn, TeamColor.White, i, 1);
        }
        
        // 백 룩 2개
        SpawnPiece(whiteRookPrefab, PieceType.Rook, TeamColor.White, 0, 0);
        SpawnPiece(whiteRookPrefab, PieceType.Rook, TeamColor.White, 7, 0);

        // 백 나이트 2개
        SpawnPiece(whiteKnightPrefab, PieceType.Knight, TeamColor.White, 1, 0); 
        SpawnPiece(whiteKnightPrefab, PieceType.Knight, TeamColor.White, 6, 0);

        // 백 비숍 2개
        SpawnPiece(whiteBishopPrefab, PieceType.Bishop, TeamColor.White, 2, 0);  // C1
        SpawnPiece(whiteBishopPrefab, PieceType.Bishop, TeamColor.White, 5, 0);  // F1

        // 백 퀸
        SpawnPiece(whiteQueenPrefab, PieceType.Queen, TeamColor.White, 3, 0);  // D1

        // 백 킹
        SpawnPiece(whiteKingPrefab, PieceType.King, TeamColor.White, 4, 0);  // E1

        // 흑 폰 8개
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(BlackPawnPrefab, PieceType.Pawn, TeamColor.Black, i, 6);
        }

        // 흑 룩 2개 (Y=7, 8행)
        SpawnPiece(blackRookPrefab, PieceType.Rook, TeamColor.Black, 0, 7);  // A8
        SpawnPiece(blackRookPrefab, PieceType.Rook, TeamColor.Black, 7, 7);  // H8

        // 흑 나이트 2개
        SpawnPiece(blackKnightPrefab, PieceType.Knight, TeamColor.Black, 1, 7);  // B8
        SpawnPiece(blackKnightPrefab, PieceType.Knight, TeamColor.Black, 6, 7);  // G8

        // 흑 비숍 2개
        SpawnPiece(blackBishopPrefab, PieceType.Bishop, TeamColor.Black, 2, 7);  // C8
        SpawnPiece(blackBishopPrefab, PieceType.Bishop, TeamColor.Black, 5, 7);  // F8

        // 흑 퀸
        SpawnPiece(blackQueenPrefab, PieceType.Queen, TeamColor.Black, 3, 7);  // D8

        // 흑 킹
        SpawnPiece(blackKingPrefab, PieceType.King, TeamColor.Black, 4, 7);  // E8

        Debug.Log("모든 말 배치 완료");
    }

    void SpawnPiece(GameObject prefab, PieceType type, TeamColor color, int x, int y)
    {
        // 3D 오브젝트 생성
        GameObject obj = Instantiate(prefab, Vector3.zero, prefab.transform.rotation);
        ChessPiece visual = obj.GetComponent<ChessPiece>();

        // 시각 설정
        visual.pieceType = type;
        visual.teamColor = color;
        visual.SetPosition(x, y);

        // 데이터 생성 및 저장
        PieceData pieceData = new PieceData(type, color, x, y, false, visual);
        board[x, y] = pieceData;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }


    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane boardPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;

        RaycastHit hit;

        if (boardPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            int x = Mathf.FloorToInt(hitPoint.x + 0.5f);
            int y = Mathf.FloorToInt(hitPoint.z + 0.5f);

            Debug.Log($"클릭 좌표: ({x}, {y})");

            if (x < 0 || x >= 8 || y < 0 || y >= 8)
            {
                Debug.Log("❌ 범위 밖");
                return;
            }

            if(selectedPiece != null)
            {
                Debug.Log("→ 이동 시도");
                TryMove(x, y);
            }
            else
            {
                Debug.Log("→ 말 선택 시도");
                PieceData piece = board[x, y];
                if(piece != null && piece.color == currentTurn)
                {
                    selectedPiece = piece;
                    ShowAvailableMoves();
                }         
            }
        }
    }

    void ShowAvailableMoves()
    {
        ClearHighlights();

        if (selectedPiece == null)
        {
            return;
        }

        List<Vector2Int> moves = GetAvailableMoves(selectedPiece);

        Debug.Log($"이동 가능한 곳: {moves.Count}개");

        foreach(Vector2Int move in moves)
        {
            // 하이라이트 생성
            if(highlightPrefab != null)
            {
                GameObject highlight = Instantiate(highlightPrefab);
                highlight.transform.position = new Vector3(move.x, 0.1f, move.y);
                highlights.Add(highlight);
            }
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject highlight in highlights)
        {
            Destroy(highlight);
        }
        highlights.Clear();
    }

    void TryMove(int x, int y)
    {
        List<Vector2Int> availavleMoves = GetAvailableMoves(selectedPiece);
        Vector2Int targetMove = new Vector2Int(x, y);

        if (availavleMoves.Contains(targetMove))
        {
            Debug.Log($"✅ 이동:  ({selectedPiece.x}, {selectedPiece.y}) → ({x}, {y})");

            // 이동 실행
            MovePiece(selectedPiece, x, y);

            // 턴 변경
            currentTurn = (currentTurn == TeamColor.White) ? TeamColor.Black : TeamColor.White;
            Debug.Log($"턴: {currentTurn}");
        }
        else
        {
              Debug.Log("❌ 이동 불가능한 위치!");                       
        }

        // 선택 해제 및 하이라이트 제거
        selectedPiece = null;
        ClearHighlights();
    }

    void MovePiece(PieceData piece, int x, int y)
    {
        if (board[x, y] != null)
        {
            PieceData capturedPiece = board[x, y];
            
            // 기존 기물 제거
            Destroy(board[x, y].visualPiece.gameObject);
        }
        // 보드 상태 업데이트
        board[piece.x, piece.y] = null;

        piece.x = x;
        piece.y = y;
        piece.hasMoved = true;

        board[x, y] = piece;

        // 기물 이동
        piece.visualPiece.MoveTo(x, y);

        //프로모션 체크
        CheckPromotion(piece);

    }

    void CheckPromotion(PieceData piece)
    {
        if (piece.type != PieceType.Pawn)
            return;

        if((piece.color == TeamColor.White && piece.y == 7) ||
           (piece.color == TeamColor.Black && piece.y == 0))
        {
            StartPromotion(piece);
        }
    }

    List<Vector2Int> GetAvailableMoves(PieceData piece)
    {
        switch (piece.type)
        {
            case PieceType.Pawn:
                return GetPawnMoves(piece);
            case PieceType.Rook:
                return GetRookMoves(piece);
            case PieceType.Knight:
                return GetKnightMoves(piece);
            case PieceType.Bishop:
                return GetBishopMoves(piece);
            case PieceType.Queen:
                return GetQueenMoves(piece);
            case PieceType.King:
                return GetKingMoves(piece);
            default:
                return new List<Vector2Int>();
        }
    }

    List<Vector2Int> GetPawnMoves(PieceData piece) // 폰 이동 로직
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        int direction = (piece.color == TeamColor.White) ? 1 : -1;

        // 1. 앞으로 이동
        int forwardY = piece.y + direction;
        if(IsValidPosition(piece.x, forwardY) && board[piece.x, forwardY] == null)
        {
            moves.Add(new Vector2Int(piece.x, forwardY));

            // 2. 처음 이동 시 2칸 이동
            if(!piece.hasMoved)
            {
                int doubleY = piece.y + 2 * direction;
                if(IsValidPosition(piece.x, doubleY) && board[piece.x, doubleY] == null)
                {
                    moves.Add(new Vector2Int(piece.x, doubleY));
                }
            }
        }

        // 3. 대각선 왼쪽 공격
        int diagLeftX = piece.x - 1;
        int diagLeftY = piece.y + direction;
        if(IsValidPosition(diagLeftX, diagLeftY) && board[diagLeftX, diagLeftY] != null && board[diagLeftX, diagLeftY].color != piece.color)
        {
            moves.Add(new Vector2Int(diagLeftX, diagLeftY));
        }

        // 4. 대각선 오른쪽 공격
        int diagRightX = piece.x + 1;
        int diagRightY = piece.y + direction;
        if(IsValidPosition(diagRightX, diagRightY) && board[diagRightX, diagRightY] != null && board[diagRightX, diagRightY].color != piece.color)
        {
            moves.Add(new Vector2Int(diagRightX, diagRightY));
        }
        return moves;
    }

    List<Vector2Int> GetRookMoves(PieceData piece)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 상하좌우 이동
        Vector2Int[] directions =
        {
            new Vector2Int(0, 1),   // 위
            new Vector2Int(0, -1),  // 아래
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(-1, 0)   // 왼쪽
        };

        foreach(Vector2Int dir in directions)
        {
            // 한 방향으로 계속 이동
            for (int i = 1; i < 8; i++)
            {
                int x = piece.x + dir.x * i;
                int y = piece.y + dir.y * i;

                if(!IsValidPosition(x, y))
                {
                    break; // 범위 벗어남
                }

                if (board[x, y] == null)
                {
                    // 빈 칸
                    moves.Add(new Vector2Int(x, y));
                }
                else
                {
                    // 말이 있음
                    if (board[x, y].color != piece.color)
                    {
                        // 상대 말이면 공격 가능
                        moves.Add(new Vector2Int(x, y));
                    }
                    break; // 막힘
                }
            }
        }
        return moves;
    }

    List<Vector2Int> GetKnightMoves(PieceData piece) // 나이트 이동 로직
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 나이트 이동 패턴
        Vector2Int[] knightMoves =
        {
            new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2)
        };

        foreach(Vector2Int move in knightMoves)
        {
            int x = piece.x + move.x;
            int y = piece.y + move.y;

            if(IsValidPosition(x, y))
            {
                if(board[x, y] == null || board[x, y].color != piece.color)
                {
                    moves.Add(new Vector2Int(x, y));
                }
            }   
        }
        return moves;
    }

    List<Vector2Int> GetBishopMoves(PieceData piece) // 비숍 이동 로직
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 대각선 이동
        Vector2Int[] directions =
        {
            new Vector2Int(1, 1),   // 우상
            new Vector2Int(1, -1),  // 우하
            new Vector2Int(-1, 1),  // 좌상
            new Vector2Int(-1, -1)  // 좌하
        };

        foreach(Vector2Int dir in directions)
        {
            for(int i = 1; i < 8; i++)
            {
                int x = piece.x + dir.x * i;
                int y = piece.y + dir.y * i;

                if(!IsValidPosition(x, y))
                {
                    break;
                }
                if(board[x, y] == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                else
                {
                    if(board[x, y].color != piece.color)
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                    break;
                }
            }
        }
        return moves;
    }

    List<Vector2Int> GetQueenMoves(PieceData piece) // 퀸 이동 로직
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        // 퀸은 룩과 비숍의 이동을 합친 것
        moves.AddRange(GetRookMoves(piece));
        moves.AddRange(GetBishopMoves(piece));
        return moves;
    }

    List<Vector2Int> GetKingMoves(PieceData piece) // 킹 이동 로직
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 킹 이동 패턴 (한 칸씩)
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if(x == 0 && y == 0)
                {
                    continue; // 현재 위치
                }

                int newX = piece.x + x;
                int newY = piece.y + y;

                if(IsValidPosition(newX, newY))
                {
                    if(board[newX, newY] == null || board[newX, newY].color != piece.color)
                    {
                        moves.Add(new Vector2Int(newX, newY));
                    }
                }
            }
        }
        
        return moves;
    }

    void StartPromotion(PieceData piece)
    {
        promotingPiece = piece;

        // 프로모션 UI 활성화
        if (promotionPanel != null)
        {
            promotionPanel.SetActive(true);
        }
        else
        {
            // UI가 없으면 자동으로 퀸으로 프로모션
            PromoteTo(PieceType.Queen);
        }
    }

    void PromoteTo(PieceType newType)
    {
        if(promotingPiece == null)
        {
            return;
        }
        // 기존 폰 제거
        Destroy(promotingPiece.visualPiece.gameObject);

        // 새 기물 생성
        GameObject newPrefab = GetPiecePrefab(newType, promotingPiece.color);

        if (newPrefab == null) 
        {
            return;
        }

        GameObject obj = Instantiate(newPrefab, Vector3.zero, newPrefab.transform.rotation);
        ChessPiece visual = obj.GetComponent<ChessPiece>();

        visual.pieceType = newType;
        visual.teamColor = promotingPiece.color;
        visual.SetPosition(promotingPiece.x, promotingPiece.y);

        // 데이터 업데이트
        promotingPiece.type = newType;
        promotingPiece.visualPiece = visual;

        // UI 비활성화
        if (promotionPanel != null)
        {
            promotionPanel.SetActive(false);
        }
        promotingPiece = null;
    }

    GameObject GetPiecePrefab(PieceType type, TeamColor color)
    {
        bool isWhite = (color == TeamColor.White);

        switch (type)
        {
            case PieceType.Queen:
                return isWhite ? whiteQueenPrefab : blackQueenPrefab;
            case PieceType.Rook:
                return isWhite ? whiteRookPrefab : blackRookPrefab;
            case PieceType.Bishop:
                return isWhite ? whiteBishopPrefab : blackBishopPrefab;
            case PieceType.Knight:
                return isWhite ? whiteKnightPrefab : blackKnightPrefab;
            default:
                return null;
        }
    }
    
    bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
}



