using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject whiteTilePrefab;
    public GameObject blackTilePrefab;
    public float tileSize = 1f;

    [Header("Border Settings")]
    public bool showBorders = true;
    public Color borderColor = new Color(0.4f, 0.3f, 0.2f);
    public float borderWidth = 0.5f; // 테두리 두께


    [Header("Coordinate Settings")]
    public bool showCoordinates = true;
    public Color coordinateColor = Color.white;
    public int fontSize = 50;

    [Header("Camera Settings")]
    public float positionx = 3.5f;
    public float positiony = 8f;
    public float positionz = 3.5f;
    public float lookatx = 3.5f;
    public float lookaty = 0f;
    public float lookatz = 3.5f;

    private GameObject[,] tiles = new GameObject[8, 8];
    void Start()
    {
        GenerateBoard();

        if (showBorders)
        {
            CreateBorder();
        }

        if(showCoordinates)
        {
            AddCoordinates();
        }
    }

    void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                bool isWhite = (x + y) % 2 == 0;
                GameObject tilePrefab = isWhite ? whiteTilePrefab : blackTilePrefab;

                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile ({x}, {y})";
                tiles[x, y] = tile;

                Collider collider = tile.GetComponent<Collider>();
                //if (collider != null)
                //{
                //    Destroy(collider);
                //}
            }
        }
        Camera.main.transform.position = new Vector3(positionx, positiony, positionz);
        Camera.main.transform.LookAt(new Vector3(lookatx, lookaty, lookatz));
    }

    void CreateBorder()
    {
        float boardSize = 8 * tileSize;
        float frameThickness = 0.4f;
        float frameHeight = 0.4f;
        float boardCenter = 3.5f;

        // 베이스 프레임 (받침)
        GameObject baseFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseFrame.transform.parent = transform;
        baseFrame.transform.position = new Vector3(boardCenter, -0.2f, boardCenter);
        baseFrame.transform.localScale = new Vector3(
            boardSize + frameThickness * 2,
            frameHeight,
            boardSize + frameThickness * 2
        );
        baseFrame.name = "Border_Base";

        Renderer baseRenderer = baseFrame.GetComponent<Renderer>();
        baseRenderer.material.color = borderColor;
        Destroy(baseFrame.GetComponent<Collider>());

       
    }

    void AddCoordinates()
    {
        string[] letters = { "A", "B", "C", "D", "E", "F", "G", "H" };

        // 문자 좌표 (A-H) - 아래쪽

        for (int i = 0; i < 8; i++)
        {
            CreateCoordinateText(letters[i], new Vector3(i * tileSize, 0.01f, -0.7f), Quaternion.Euler(90, 0, 0));
        }
        // 문자 좌표 (A-H) - 위쪽
        for (int i = 0; i < 8; i++)
        {
            CreateCoordinateText(
                letters[i],
                new Vector3(i * tileSize, 0.01f, 7 * tileSize + 0.7f),
                Quaternion.Euler(90, 0, 0)
            );
        }

        // 숫자 좌표 (1-8) - 왼쪽
        for (int i = 0; i < 8; i++)
        {
            CreateCoordinateText(
                (i + 1).ToString(),
                new Vector3(-0.7f, 0.01f, i * tileSize),
                Quaternion.Euler(90, 0, 0)
            );
        }

        // 숫자 좌표 (1-8) - 오른쪽
        for (int i = 0; i < 8; i++)
        {
            CreateCoordinateText(
                (i + 1).ToString(),
                new Vector3(7 * tileSize + 0.7f, 0.01f, i * tileSize),
                Quaternion.Euler(90, 0, 0)
            );
        }
    }

    void CreateCoordinateText(string text, Vector3 position, Quaternion rotation)
    {
        GameObject textObj = new GameObject($"Coordinate_{text}");
        textObj.transform.parent = transform;
        textObj.transform.position = position;
        textObj.transform.rotation = rotation;

        // TextMeshPro 사용
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 3;
        tmp.color = coordinateColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
    }



}
