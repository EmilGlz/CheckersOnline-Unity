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
    private List<Tile> topRightMovesAvailable;
    private List<Tile> topLeftMovesAvailable;
    private List<Tile> bottomLeftMovesAvailable;
    private List<Tile> bottomRightMovesAvailable;
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    public void CreateGrid()
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
                circle.Init(iAmWhite, circlePosition, Mock.MakeAllMyPlayerKings);
                _circles[circlePosition] = circle;

            }
        }
    }
    public void ShowAvailableMoves(Tile selectedTile)
    {
        var selectedCircle = GetCircleAtPosition(selectedTile.Position);
        if (selectedCircle == null)
            return;
        var topLeftCorner = GetTopLeftBorder(selectedTile, selectedCircle.IsKing);
        var topRightCorner = GetTopRightBorder(selectedTile, selectedCircle.IsKing);
        var bottomRightCorner = GetBottomRightBorder(selectedTile, selectedCircle.IsKing);
        var bottomLeftCorner = GetBottomLeftBorder(selectedTile, selectedCircle.IsKing);
        List<Tile> allAvailableMoves = new();
        topRightMovesAvailable = new() { selectedTile };
        while (topLeftCorner != null)
        {
            allAvailableMoves.Add(topLeftCorner);
            topLeftCorner = GetTopLeftBorder(topLeftCorner, selectedCircle.IsKing);
        }
        while (topRightCorner != null)
        {
            allAvailableMoves.Add(topRightCorner);
            topRightMovesAvailable.Add(topRightCorner);
            topRightCorner = GetTopRightBorder(topRightCorner, selectedCircle.IsKing);
        }
        while (bottomRightCorner != null)
        {
            allAvailableMoves.Add(bottomRightCorner);
            bottomRightCorner = GetBottomRightBorder(bottomRightCorner, selectedCircle.IsKing);
        }
        while (bottomLeftCorner != null)
        {
            allAvailableMoves.Add(bottomLeftCorner);
            bottomLeftCorner = GetBottomLeftBorder(bottomLeftCorner, selectedCircle.IsKing);
        }
        HighlightTiles(allAvailableMoves);
    }
    public void ShowAvailableMovesNew(Tile selectedTile)
    {
        var selectedCircle = GetCircleAtPosition(selectedTile.Position);
        if (selectedCircle == null)
            return;
        topRightMovesAvailable = new() { selectedTile };
        topLeftMovesAvailable = new() { selectedTile };
        bottomLeftMovesAvailable = new() { selectedTile };
        bottomRightMovesAvailable = new() { selectedTile };
        GetTopRightMoves(selectedTile);
        GetTopLeftMoves(selectedTile);
        GetBottomLeftMoves(selectedTile);
        GetBottomRightMoves(selectedTile);
        HighlightTiles(topRightMovesAvailable);
    }
    private Tile GetTopLeftBorder(Tile centerTile, bool isKing = false)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 7 || tilePos.y == 0)
            return null;
        var nextPos = tilePos + Vector2.down + Vector2.right;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        var tileAtNextPos = GetTileAtPosition(nextPos);
        if (circleAtNextPos == null)
        {
            if (circleAtCurrentPos == null)
            {
                if (isKing)
                    return tileAtNextPos;
                return null;
            }
            else
                return tileAtNextPos;
        }
        else
        {
            var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
            if (!isOpponent)
                return null;
            else
                if(isKing)
                    return GetTopLeftBorder(tileAtNextPos, isKing);
        }
        return null;
    }
    private Tile GetTopRightBorder(Tile centerTile, bool isKing = false)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 7 || tilePos.y == 7)
            return null;
        var nextPos = tilePos + Vector2.up + Vector2.right;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        var tileAtNextPos = GetTileAtPosition(nextPos);
        if (circleAtNextPos == null)
        {
            if (circleAtCurrentPos == null)
            {
                if (isKing)
                    return tileAtNextPos;
                return null;
            }
            else
                return tileAtNextPos;
        }
        else
        {
            var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
            if (!isOpponent)
                return null;
            else
            {
                //topRightMovesAvailable[^1]
                return GetTopRightBorder(tileAtNextPos, isKing);
            }    
        }
    }
    private Tile GetBottomRightBorder(Tile centerTile, bool isKing = false)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 0 || tilePos.y == 7)
            return null;
        var nextPos = tilePos + Vector2.up + Vector2.left;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        var tileAtNextPos = GetTileAtPosition(nextPos);
        if (circleAtNextPos == null)
        {
            if (circleAtCurrentPos == null)
            {
                if (isKing)
                    return tileAtNextPos;
                return null;
            }
            else
                return tileAtNextPos;
        }
        else
        {
            var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
            if (!isOpponent)
                return null;
            else
                if (isKing)
                    return GetBottomRightBorder(tileAtNextPos, isKing);
        }
        return null;
    }
    private Tile GetBottomLeftBorder(Tile centerTile, bool isKing = false)
    {
        var tilePos = centerTile.Position;
        if (tilePos.x == 0 || tilePos.y == 0)
            return null;
        var nextPos = tilePos + Vector2.down + Vector2.left;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var circleAtNextPos = GetCircleAtPosition(nextPos);
        var tileAtNextPos = GetTileAtPosition(nextPos);
        if (circleAtNextPos == null)
        {
            if (circleAtCurrentPos == null)
            {
                if (isKing)
                    return tileAtNextPos;
                return null;
            }
            else
                return tileAtNextPos;
        }
        else
        {
            var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
            if (!isOpponent)
                return null;
            else
                if (isKing)
                    return GetBottomLeftBorder(tileAtNextPos, isKing);
        }
        return null;
    }
    private bool TilesAreBorders(Tile tile1, Tile tile2)
    {
        var distance = Vector2.Distance(tile1.Position, tile2.Position);
        return Mathf.Abs(distance) <= Mathf.Sqrt(2);
    }

    private void GetTopRightMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        while (!(tilePos.x == 7 || tilePos.y == 7))
        {
            Vector2 nextPos = tilePos + Vector2.up + Vector2.right;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            var tileAtNextPos = GetTileAtPosition(nextPos);
            var isKing = circleAtCurrentPos.IsKing;
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    topRightMovesAvailable.Add(tileAtNextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
            }
            else
            {
                if (circleAtNextPos == null) //  empty tile
                {
                    if (topRightMovesAvailable.Count > 1)
                    {
                        var lastMove = topRightMovesAvailable[^1];
                        if (TilesAreBorders(lastMove, tileAtNextPos))
                            break;
                    }
                    topRightMovesAvailable.Add(tileAtNextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
                if (topRightMovesAvailable.Count == 2)
                {
                    var lastMove = topRightMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetTopLeftMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        while (!(tilePos.x == 7 || tilePos.y == 0))
        {
            Vector2 nextPos = tilePos + Vector2.down + Vector2.right;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            var tileAtNextPos = GetTileAtPosition(nextPos);
            var isKing = circleAtCurrentPos.IsKing;
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    topLeftMovesAvailable.Add(tileAtNextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
            }
            else
            {
                if (circleAtNextPos == null) //  empty tile
                {
                    if (topLeftMovesAvailable.Count > 1)
                    {
                        var lastMove = topLeftMovesAvailable[^1];
                        if (TilesAreBorders(lastMove, tileAtNextPos))
                            break;
                    }
                    topLeftMovesAvailable.Add(tileAtNextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
                if (topLeftMovesAvailable.Count == 2)
                {
                    var lastMove = topLeftMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetBottomLeftMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        while (!(tilePos.x == 0 || tilePos.y == 0))
        {
            Vector2 nextPos = tilePos + Vector2.down + Vector2.left;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            var tileAtNextPos = GetTileAtPosition(nextPos);
            var isKing = circleAtCurrentPos.IsKing;
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    bottomLeftMovesAvailable.Add(tileAtNextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
            }
            else
            {
                if (circleAtNextPos == null) //  empty tile
                {
                    if (bottomLeftMovesAvailable.Count > 1)
                    {
                        var lastMove = bottomLeftMovesAvailable[^1];
                        if (TilesAreBorders(lastMove, tileAtNextPos))
                            break;
                    }
                    bottomLeftMovesAvailable.Add(tileAtNextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
                if (bottomLeftMovesAvailable.Count == 2)
                {
                    var lastMove = bottomLeftMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetBottomRightMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        while (!(tilePos.x == 0 || tilePos.y == 7))
        {
            Vector2 nextPos = tilePos + Vector2.up + Vector2.left;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            var tileAtNextPos = GetTileAtPosition(nextPos);
            var isKing = circleAtCurrentPos.IsKing;
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    bottomRightMovesAvailable.Add(tileAtNextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
            }
            else
            {
                if (circleAtNextPos == null) //  empty tile
                {
                    if (bottomRightMovesAvailable.Count > 1)
                    {
                        var lastMove = bottomRightMovesAvailable[^1];
                        if (TilesAreBorders(lastMove, tileAtNextPos))
                            break;
                    }
                    bottomRightMovesAvailable.Add(tileAtNextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                }
                if (bottomRightMovesAvailable.Count == 2)
                {
                    var lastMove = bottomRightMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }



    private void HighlightTiles(List<Tile> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].SetHightlight();
        }
    }    
}