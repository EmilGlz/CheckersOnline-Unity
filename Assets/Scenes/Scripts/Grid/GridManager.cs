using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Circle circlePrefab;
    [SerializeField] private Transform tilesParent;
    [SerializeField] private Transform circlesParent;
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private float horizontalPaddingPercentage = 0.1f;
    private Dictionary<Vector2, Tile> _tiles;
    private Dictionary<Vector2, Circle> _circles;
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    private void Start()
    {
        CreateGrid();
        CreateCircles(false);
    }
    private void CreateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        // Calculate the world space width and height of a single tile based on the screen size and desired number of rows/columns.
        float screenHeight = 2f * Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;

        // Apply horizontal padding to both left and right sides of the screen.
        float horizontalPadding = horizontalPaddingPercentage * screenWidth;
        float totalWidth = screenWidth - 2 * horizontalPadding;

        // Calculate the world space width and height of a single tile to keep them square.
        float tileSize = Mathf.Min(totalWidth / columns, screenHeight / rows);

        // Calculate the leftmost position of the grid based on the total width and horizontal padding.
        float leftmostPositionX = -screenWidth / 2f + tileSize / 2f + horizontalPadding;
        float bottommostPositionY = - tileSize * 3.5f;

        // Create the grid of tiles.
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector3 tilePosition = new Vector3(
                    y * tileSize + leftmostPositionX,
                    x * tileSize + bottommostPositionY,
                    0f
                );

                Tile tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, tilesParent);
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
                tile.name = $"Tile {x} {y}";
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                tile.Init(isOffset, new Vector2(x, y));
                _tiles[new Vector2(x, y)] = tile;
            }
        }
    }
    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile)) return tile;
        return null;
    }
    public Circle GetCircleAtPosition(Vector2 pos)
    {
        if (_circles.TryGetValue(pos, out var circle)) return circle;
        return null;
    }
    private void CreateWhiteCirclesMock()
    {
        for (int i = 0; i < Mock.WhitePositions.Count; i++)
        {
            var circlePosition = Mock.WhitePositions[i];
            var tile = GetTileAtPosition(circlePosition);
            Circle circle = Instantiate(circlePrefab, tile.transform.position + Vector3.back, Quaternion.identity, circlesParent);
            circle.transform.localScale = tile.transform.localScale;
            circle.name = $"Circle {circlePosition.x} {circlePosition.y}";
            circle.Init(true, circlePosition);
            _circles[circlePosition] = circle;
        }
    }
    private void CreateBlackCirclesMock()
    {
        for (int i = 0; i < Mock.BlackPositions.Count; i++)
        {
            var circlePosition = Mock.BlackPositions[i];
            var tile = GetTileAtPosition(circlePosition);
            Circle circle = Instantiate(circlePrefab, tile.transform.position + Vector3.back, Quaternion.identity, circlesParent);
            circle.transform.localScale = tile.transform.localScale;
            circle.name = $"Circle {circlePosition.x} {circlePosition.y}";
            circle.Init(false, circlePosition);
            _circles[circlePosition] = circle;
        }
    }
    public void CreateCircles(bool iAmWhite)
    {
        _circles = new Dictionary<Vector2, Circle>();
        if (iAmWhite) // im white
        {
            if (Mock.ShowMockWhiteCircles) // i am mock
                CreateWhiteCirclesMock();
            else // i am real
                CreateMyCircles(iAmWhite);
            if (Mock.ShowMockBlackCircles) // opp is mock
                CreateBlackCirclesMock();
            else // opp real
                CreateOppCircles(iAmWhite);
        }
        else // im black
        {
            if (Mock.ShowMockBlackCircles) // i am mock
                CreateBlackCirclesMock();
            else // i am real
                CreateMyCircles(iAmWhite);
            if(Mock.ShowMockWhiteCircles) // opp is mock
                CreateWhiteCirclesMock();
            else // opp real
                CreateOppCircles(iAmWhite);
        }
    }
    private void CreateOppCircles(bool iAmWhite)
    {
        for (int i = 5; i < 8; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var circlePosition = new Vector2(i, i % 2 == 0 ? j * 2 : j * 2 + 1);
                var tile = GetTileAtPosition(circlePosition);
                Circle circle = Instantiate(circlePrefab, tile.transform.position + Vector3.back, Quaternion.identity, circlesParent);
                circle.transform.localScale = tile.transform.localScale;
                circle.name = $"Circle {circlePosition.x} {circlePosition.y}";
                circle.Init(!iAmWhite, circlePosition);
                _circles[circlePosition] = circle;
            }
        }
    }
    private void CreateMyCircles(bool iAmWhite)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var circlePosition = new Vector2(i, i % 2 == 0 ? j * 2 : j * 2 + 1);
                var tile = GetTileAtPosition(circlePosition);
                Circle circle = Instantiate(circlePrefab, tile.transform.position + Vector3.back, Quaternion.identity, circlesParent);
                circle.transform.localScale = tile.transform.localScale;
                circle.name = $"Circle {circlePosition.x} {circlePosition.y}";
                circle.Init(iAmWhite, circlePosition);
                _circles[circlePosition] = circle;

            }
        }
    }
    public void ShowAvailableMoves(Tile selectedTile)
    {
        var selectedCircle = GetCircleAtPosition(selectedTile.Position);
        if (selectedCircle == null)
            return;
        var topLeftCorner = GetTopLeftBorder(selectedTile);
        var topRightCorner = GetTopRightBorder(selectedTile);
        var bottomRightCorner = GetBottomRightBorder(selectedTile);
        var bottomLeftCorner = GetBottomLeftBorder(selectedTile);
        List<Tile> availableMoves = new List<Tile>();
        while (topLeftCorner != null)
        {
            availableMoves.Add(topLeftCorner);
            topLeftCorner = GetTopLeftBorder(topLeftCorner);
        }
        while (topRightCorner != null)
        {
            availableMoves.Add(topRightCorner);
            topRightCorner = GetTopRightBorder(topRightCorner);
        }
        while (bottomRightCorner != null)
        {
            availableMoves.Add(bottomRightCorner);
            bottomRightCorner = GetBottomRightBorder(bottomRightCorner);
        }
        while (bottomLeftCorner != null)
        {
            availableMoves.Add(bottomLeftCorner);
            bottomLeftCorner = GetBottomLeftBorder(bottomLeftCorner);
        }
        HighlightTiles(availableMoves);
    }
    private Tile GetTopLeftBorder(Tile centerTile)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 7 || tilePos.y == 0)
            return null;
        var nextPos = tilePos + Vector2.down + Vector2.right;
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        if (circleAtNextPos == null)
            return GetTileAtPosition(nextPos);
        return null;
    }
    private Tile GetTopRightBorder(Tile centerTile)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 7 || tilePos.y == 7)
            return null;
        var nextPos = tilePos + Vector2.up + Vector2.right;
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        if (circleAtNextPos == null)
            return GetTileAtPosition(nextPos);
        return null;
    }
    private Tile GetBottomRightBorder(Tile centerTile)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 0 || tilePos.y == 7)
            return null;
        var nextPos = tilePos + Vector2.up + Vector2.left;
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        if (circleAtNextPos == null)
            return GetTileAtPosition(nextPos);
        return null;
    }
    private Tile GetBottomLeftBorder(Tile centerTile)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 0 || tilePos.y == 0)
            return null;
        var nextPos = tilePos + Vector2.down + Vector2.left;
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        if (circleAtNextPos == null)
            return GetTileAtPosition(nextPos);
        return null;
    }
    private void HighlightTiles(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].SetHightlight();
        }
    }    
}
