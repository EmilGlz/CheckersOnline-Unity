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
    private List<Vector2> topRightMovesAvailable;
    private List<Vector2> topLeftMovesAvailable;
    private List<Vector2> bottomLeftMovesAvailable;
    private List<Vector2> bottomRightMovesAvailable;
    public int WhiteCirclesCount = 12;
    public int BlackCirclesCount = 12;
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
    }
    #endregion
    private void CreateWhiteCirclesMock()
    {
        for (int i = 0; i < Mock.WhitePositions.Count; i++)
        {
            var circlePosition = Mock.WhitePositions[i];
            var tile = GetTileAtPosition(circlePosition);
            Circle circle = Instantiate(circlePrefab, tile.transform.position + Vector3.back, Quaternion.identity, circlesParent);
            circle.transform.localScale = tile.transform.localScale;
            circle.name = $"Circle {circlePosition.x} {circlePosition.y}";
            circle.Init(true, circlePosition, Mock.MakeAllWhitePlayerKings);
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
            circle.Init(false, circlePosition, Mock.MakeAllBlackPlayerKings);
            _circles[circlePosition] = circle;
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
                circle.Init(!iAmWhite, circlePosition, !iAmWhite ? Mock.MakeAllWhitePlayerKings : Mock.MakeAllBlackPlayerKings);
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
                circle.Init(iAmWhite, circlePosition, iAmWhite ? Mock.MakeAllWhitePlayerKings : Mock.MakeAllBlackPlayerKings);
                _circles[circlePosition] = circle;

            }
        }
    }
    private bool TilesAreBorders(Vector2 tilePos1, Vector2 tilePos2)
    {
        var distance = Vector2.Distance(tilePos1, tilePos2);
        return Mathf.Abs(distance) <= Mathf.Sqrt(2);
    }
    private List<Circle> GetCirclesBetween(Tile tile1, Tile tile2)
    {
        var res = new List<Circle>();
        var tile = GetTopRightTile(tile1);
        while (tile != null && tile.Position != tile2.Position)
        {
            var circleAtThePosition = GetCircleAtPosition(tile.Position);
            if (circleAtThePosition != null)
            {
                res.Add(circleAtThePosition);
            }
            tile = GetTopRightTile(tile);
            if (tile == null) // reached the end
            {
                res.Clear();
                break;
            }
        }
        if (res.Count > 0)
            return res;

        tile = GetTopLeftTile(tile1);
        while (tile != null && tile.Position != tile2.Position)
        {
            var circleAtThePosition = GetCircleAtPosition(tile.Position);
            if (circleAtThePosition != null)
            {
                res.Add(circleAtThePosition);
            }
            tile = GetTopLeftTile(tile);
            if (tile == null) // reached the end
            {
                res.Clear();
                break;
            }
        }
        if (res.Count > 0)
            return res;

        tile = GetBottomLeftTile(tile1);
        while (tile != null && tile.Position != tile2.Position)
        {
            var circleAtThePosition = GetCircleAtPosition(tile.Position);
            if (circleAtThePosition != null)
            {
                res.Add(circleAtThePosition);
            }
            tile = GetBottomLeftTile(tile);
            if (tile == null) // reached the end
            {
                res.Clear();
                break;
            }
        }
        if (res.Count > 0)
            return res;

        tile = GetBottomRightTile(tile1);
        while (tile != null && tile.Position != tile2.Position)
        {
            var circleAtThePosition = GetCircleAtPosition(tile.Position);
            if (circleAtThePosition != null)
            {
                res.Add(circleAtThePosition);
            }
            tile = GetBottomRightTile(tile);
            if (tile == null) // reached the end
            {
                res.Clear();
                break;
            }
        }
        return res;
    }
    private List<Tile> GetAllTopRightTiles(Tile selectedTile)
    {
        var res = new List<Tile>();
        var tile = GetTopRightTile(selectedTile);
        while (tile != null)
        {
            res.Add(tile);
            tile = GetTopRightTile(tile);
        }
        return res;
    }
    private List<Tile> GetAllTopLeftTiles(Tile selectedTile)
    {
        var res = new List<Tile>();
        var tile = GetTopLeftTile(selectedTile);
        while (tile != null)
        {
            res.Add(tile);
            tile = GetTopLeftTile(tile);
        }
        return res;
    }
    private List<Tile> GetAllBottomLeftTiles(Tile selectedTile)
    {
        var res = new List<Tile>();
        var tile = GetBottomLeftTile(selectedTile);
        while (tile != null)
        {
            res.Add(tile);
            tile = GetBottomLeftTile(tile);
        }
        return res;
    }
    private List<Tile> GetAllBottomRightTiles(Tile selectedTile)
    {
        var res = new List<Tile>();
        var tile = GetBottomRightTile(selectedTile);
        while (tile != null)
        {
            res.Add(tile);
            tile = GetBottomRightTile(tile);
        }
        return res;
    }
    private Tile GetTopRightTile(Tile tile)
    {
        if (tile.Position.x == 7 || tile.Position.y == 7)
            return null;
        Vector2 nextPos = tile.Position + Vector2.up + Vector2.right;
        return GetTileAtPosition(nextPos);
    }
    private Tile GetTopLeftTile(Tile tile)
    {
        if (tile.Position.x == 7 || tile.Position.y == 0)
            return null;
        Vector2 nextPos = tile.Position + Vector2.down + Vector2.right;
        return GetTileAtPosition(nextPos);
    }
    private Tile GetBottomLeftTile(Tile tile)
    {
        if (tile.Position.x == 0 || tile.Position.y == 0)
            return null;
        Vector2 nextPos = tile.Position + Vector2.down + Vector2.left;
        return GetTileAtPosition(nextPos);
    }
    private Tile GetBottomRightTile(Tile tile)
    {
        if (tile.Position.x == 0 || tile.Position.y == 7)
            return null;
        Vector2 nextPos = tile.Position + Vector2.up + Vector2.left;
        return GetTileAtPosition(nextPos);
    }
    private void GetTopRightAvailableMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var isKing = circleAtCurrentPos.IsKing;
        while (!(tilePos.x == 7 || tilePos.y == 7))
        {
            Vector2 nextPos = tilePos + Vector2.up + Vector2.right;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    topRightMovesAvailable.Add(nextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                    var previousCircle = GetCircleAtPosition(tilePos);
                    if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
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
                        if (TilesAreBorders(lastMove, nextPos))
                            break;
                    }
                    topRightMovesAvailable.Add(nextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != circleAtCurrentPos.IsWhite;
                    if (!isOpponent)
                        break;
                    else
                    {
                        var previousCircle = GetCircleAtPosition(tilePos);
                        if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
                            break;
                    }
                }
                if (topRightMovesAvailable.Count == 2)
                {
                    var lastMove = topRightMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile.Position))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetTopLeftAvailableMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var isKing = circleAtCurrentPos.IsKing;
        while (!(tilePos.x == 7 || tilePos.y == 0))
        {
            Vector2 nextPos = tilePos + Vector2.down + Vector2.right;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    topLeftMovesAvailable.Add(nextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                    var previousCircle = GetCircleAtPosition(tilePos);
                    if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
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
                        if (TilesAreBorders(lastMove, nextPos))
                            break;
                    }
                    topLeftMovesAvailable.Add(nextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != circleAtCurrentPos.IsWhite;
                    if (!isOpponent)
                        break;
                    else
                    {
                        var previousCircle = GetCircleAtPosition(tilePos);
                        if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
                            break;
                    }
                }
                if (topLeftMovesAvailable.Count == 2)
                {
                    var lastMove = topLeftMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile.Position))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetBottomLeftAvailableMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var isKing = circleAtCurrentPos.IsKing;
        while (!(tilePos.x == 0 || tilePos.y == 0))
        {
            Vector2 nextPos = tilePos + Vector2.down + Vector2.left;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    bottomLeftMovesAvailable.Add(nextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                    var previousCircle = GetCircleAtPosition(tilePos);
                    if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
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
                        if (TilesAreBorders(lastMove, nextPos))
                            break;
                    }
                    bottomLeftMovesAvailable.Add(nextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != circleAtCurrentPos.IsWhite;
                    if (!isOpponent)
                        break;
                    else
                    {
                        var previousCircle = GetCircleAtPosition(tilePos);
                        if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
                            break;
                    }
                }
                if (bottomLeftMovesAvailable.Count == 2)
                {
                    var lastMove = bottomLeftMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile.Position))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void GetBottomRightAvailableMoves(Tile selectedTile)
    {
        var tilePos = selectedTile.Position;
        var circleAtCurrentPos = GetCircleAtPosition(tilePos);
        var isKing = circleAtCurrentPos.IsKing;
        while (!(tilePos.x == 0 || tilePos.y == 7))
        {
            Vector2 nextPos = tilePos + Vector2.up + Vector2.left;
            var circleAtNextPos = GetCircleAtPosition(nextPos);
            if (isKing)
            {
                if (circleAtNextPos == null) //  empty tile
                    bottomRightMovesAvailable.Add(nextPos);
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != GameController.Instance.IAmWhite;
                    if (!isOpponent)
                        break;
                    var previousCircle = GetCircleAtPosition(tilePos);
                    if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
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
                        if (TilesAreBorders(lastMove, nextPos))
                            break;
                    }
                    bottomRightMovesAvailable.Add(nextPos);
                }
                else
                {
                    var isOpponent = circleAtNextPos.IsWhite != circleAtCurrentPos.IsWhite;
                    if (!isOpponent)
                        break;
                    else
                    {
                        var previousCircle = GetCircleAtPosition(tilePos);
                        if (previousCircle != null && previousCircle.IsWhite == circleAtNextPos.IsWhite)
                            break;
                    }
                }
                if (bottomRightMovesAvailable.Count == 2)
                {
                    var lastMove = bottomRightMovesAvailable[^1];
                    if (TilesAreBorders(lastMove, selectedTile.Position))
                        break;
                }
            }
            tilePos = nextPos;
        }
    }
    private void HighlightTiles(List<Vector2> tilePositions)
    {
        for (int i = 0; i < tilePositions.Count; i++)
        {
            GetTileAtPosition(tilePositions[i]).SetHightlight();
        }
    }
    private void RemoveCircles(List<Circle> circles)
    {
        foreach (var circle in circles)
        {
            _circles.Remove(circle.Position);
            circle.Die();
        }
    }
    private void ShowNextKillingMoves1(Tile selectedTile)
    {
        var selectedCircle = GetCircleAtPosition(selectedTile.Position);
        GameController.Instance.SelectedCircle = selectedCircle;
        if (selectedCircle == null)
        {
            GameController.Instance.UpdateMatchMovementState(MatchMovementState.None);
            return;
        }
        GameController.Instance.UpdateMatchMovementState(MatchMovementState.ShowingAvailableMoves);
        topRightMovesAvailable = new() { selectedTile.Position };
        topLeftMovesAvailable = new() { selectedTile.Position };
        bottomLeftMovesAvailable = new() { selectedTile.Position };
        bottomRightMovesAvailable = new() { selectedTile.Position };
        GetTopRightAvailableMoves(selectedTile);
        GetTopLeftAvailableMoves(selectedTile);
        GetBottomLeftAvailableMoves(selectedTile);
        GetBottomRightAvailableMoves(selectedTile);
        foreach (var item in topRightMovesAvailable)
        {
            var circleAtPos = GetCircleAtPosition(item);
            if (circleAtPos == null)
            {
                topRightMovesAvailable = new();
                break;
            }
        }
        foreach (var item in topLeftMovesAvailable)
        {
            var circleAtPos = GetCircleAtPosition(item);
            if (circleAtPos == null)
            {
                topRightMovesAvailable = new();
                break;
            }
        }
        foreach (var item in bottomLeftMovesAvailable)
        {
            var circleAtPos = GetCircleAtPosition(item);
            if (circleAtPos == null)
            {
                topRightMovesAvailable = new();
                break;
            }
        }
        foreach (var item in bottomRightMovesAvailable)
        {
            var circleAtPos = GetCircleAtPosition(item);
            if (circleAtPos == null)
            {
                topRightMovesAvailable = new();
                break;
            }
        }
        HightlightAllTilesAvailable();
    }
    private bool ShowNextKillingMoves(Circle selectedCircle)
    {
        topRightMovesAvailable = new List<Vector2> { selectedCircle.Position };
        topLeftMovesAvailable = new List<Vector2> { selectedCircle.Position };
        bottomLeftMovesAvailable = new List<Vector2> { selectedCircle.Position };
        bottomRightMovesAvailable = new List<Vector2> { selectedCircle.Position };
        var isKing = selectedCircle.IsKing;
        // TOP RIGHT MOVES
        var allMovesForOneDirection = GetAllTopRightTiles(GetTileAtPosition(selectedCircle.Position));
        var allMovesForOneDirectionInt = GetMovesAsIntegers(allMovesForOneDirection, selectedCircle.IsWhite);
        bool iamKingAndFoundOpponent = false;
        for (int i = 0; i < allMovesForOneDirectionInt.Count; i++)
        {
            if (allMovesForOneDirectionInt[i] == -1)
                break;
            if (allMovesForOneDirectionInt[i] == 1)
                continue;
            if (!isKing)
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if the start pos is empty or is my own circle, then stop
                    break;
                if (allMovesForOneDirectionInt[i] == 0 && allMovesForOneDirectionInt[i - 1] != 1) // if it is empty cell, and before is not opponent, then stop
                    break;
            }
            else
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if first place is empty, wait for the next ones
                    continue;
                if (allMovesForOneDirectionInt[i] == 0 && !iamKingAndFoundOpponent)
                    continue;
                if (allMovesForOneDirectionInt[i] == 1)
                    iamKingAndFoundOpponent = true;
                if (i > 0 && allMovesForOneDirectionInt[i] == 1 && allMovesForOneDirectionInt[i - 1] == 1) // if two respective opponent, then stop
                    break;
            }
            topRightMovesAvailable.Add(allMovesForOneDirection[i].Position);
        }
        allMovesForOneDirection.Clear();
        allMovesForOneDirectionInt.Clear();
        iamKingAndFoundOpponent = false;
        // TOP LEFT MOVES
        allMovesForOneDirection = GetAllTopLeftTiles(GetTileAtPosition(selectedCircle.Position));
        allMovesForOneDirectionInt = GetMovesAsIntegers(allMovesForOneDirection, selectedCircle.IsWhite);
        for (int i = 0; i < allMovesForOneDirectionInt.Count; i++)
        {
            if (allMovesForOneDirectionInt[i] == -1)
                break;
            if (allMovesForOneDirectionInt[i] == 1)
                continue;
            if (!isKing)
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if the start pos is empty or is my own circle, then stop
                    break;
                if (allMovesForOneDirectionInt[i] == 0 && allMovesForOneDirectionInt[i - 1] != 1) // if it is empty cell, and before is not opponent, then stop
                    break;
            }
            else
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if first place is empty, wait for the next ones
                    continue;
                if (allMovesForOneDirectionInt[i] == 0 && !iamKingAndFoundOpponent)
                    continue;
                if (allMovesForOneDirectionInt[i] == 1)
                    iamKingAndFoundOpponent = true;
                if (i > 0 && allMovesForOneDirectionInt[i] == 1 && allMovesForOneDirectionInt[i - 1] == 1) // if two respective opponent, then stop
                    break;
            }
            topLeftMovesAvailable.Add(allMovesForOneDirection[i].Position);
        }
        allMovesForOneDirection.Clear();
        allMovesForOneDirectionInt.Clear();
        iamKingAndFoundOpponent = false;
        // BOTTOM LEFT MOVES
        allMovesForOneDirection = GetAllBottomLeftTiles(GetTileAtPosition(selectedCircle.Position));
        allMovesForOneDirectionInt = GetMovesAsIntegers(allMovesForOneDirection, selectedCircle.IsWhite);
        for (int i = 0; i < allMovesForOneDirectionInt.Count; i++)
        {
            if (allMovesForOneDirectionInt[i] == -1)
                break;
            if (allMovesForOneDirectionInt[i] == 1)
                continue;
            if (!isKing)
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if the start pos is empty or is my own circle, then stop
                    break;
                if (allMovesForOneDirectionInt[i] == 0 && allMovesForOneDirectionInt[i - 1] != 1) // if it is empty cell, and before is not opponent, then stop
                    break;
            }
            else
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if first place is empty, wait for the next ones
                    continue;
                if (allMovesForOneDirectionInt[i] == 0 && !iamKingAndFoundOpponent)
                    continue;
                if (allMovesForOneDirectionInt[i] == 1)
                    iamKingAndFoundOpponent = true;
                if (i > 0 && allMovesForOneDirectionInt[i] == 1 && allMovesForOneDirectionInt[i - 1] == 1) // if two respective opponent, then stop
                    break;
            }
            bottomLeftMovesAvailable.Add(allMovesForOneDirection[i].Position);
        }
        allMovesForOneDirection.Clear();
        allMovesForOneDirectionInt.Clear();
        iamKingAndFoundOpponent = false;
        // BOTTOM Right MOVES
        allMovesForOneDirection = GetAllBottomRightTiles(GetTileAtPosition(selectedCircle.Position));
        allMovesForOneDirectionInt = GetMovesAsIntegers(allMovesForOneDirection, selectedCircle.IsWhite);
        for (int i = 0; i < allMovesForOneDirectionInt.Count; i++)
        {
            if (allMovesForOneDirectionInt[i] == -1)
                break;
            if (allMovesForOneDirectionInt[i] == 1)
                continue;
            if (!isKing)
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if the start pos is empty or is my own circle, then stop
                    break;
                if (allMovesForOneDirectionInt[i] == 0 && allMovesForOneDirectionInt[i - 1] != 1) // if it is empty cell, and before is not opponent, then stop
                    break;
            }
            else
            {
                if (i == 0 && allMovesForOneDirectionInt[i] == 0) // if first place is empty, wait for the next ones
                    continue;
                if (allMovesForOneDirectionInt[i] == 0 && !iamKingAndFoundOpponent)
                    continue;
                if (allMovesForOneDirectionInt[i] == 1)
                    iamKingAndFoundOpponent = true;
                if (i > 0 && allMovesForOneDirectionInt[i] == 1 && allMovesForOneDirectionInt[i - 1] == 1) // if two respective opponent, then stop
                    break;
            }
            bottomRightMovesAvailable.Add(allMovesForOneDirection[i].Position);
        }
        allMovesForOneDirection.Clear();
        allMovesForOneDirectionInt.Clear();
        HightlightAllTilesAvailable();
        return topRightMovesAvailable.Count + topLeftMovesAvailable.Count + bottomLeftMovesAvailable.Count + bottomRightMovesAvailable.Count > 4;
    }
    private void HightlightAllTilesAvailable()
    {
        HighlightTiles(topRightMovesAvailable);
        HighlightTiles(topLeftMovesAvailable);
        HighlightTiles(bottomLeftMovesAvailable);
        HighlightTiles(bottomRightMovesAvailable);
    }
    public List<int> GetMovesAsIntegers(List<Tile> tiles, bool isWhite)
    {
        var res = new List<int>();
        for (int i = 0; i < tiles.Count; i++)
        {
            var circleAtPos = GetCircleAtPosition(tiles[i].Position);
            if (circleAtPos == null)
                res.Add(0);
            else
            {
                var isOpponent = circleAtPos.IsWhite != isWhite;
                if (isOpponent)
                    res.Add(1);
                else
                    res.Add(-1);
            }
        }
        return res;
    }
    public void ClearAvailableMoves()
    {
        var allMoves = new List<Vector2>();
        allMoves.AddRange(topRightMovesAvailable);
        allMoves.AddRange(topLeftMovesAvailable);
        allMoves.AddRange(bottomRightMovesAvailable);
        allMoves.AddRange(bottomLeftMovesAvailable);
        foreach (var itemPos in allMoves)
        {
            var tile = GetTileAtPosition(itemPos);
            tile.SetHightlight(false);
        }
        topRightMovesAvailable.Clear();
        topLeftMovesAvailable.Clear();
        bottomRightMovesAvailable.Clear();
        bottomLeftMovesAvailable.Clear();
    }
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
        float bottommostPositionY = -tileSize * 3.5f;

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
            if (Mock.ShowMockWhiteCircles) // opp is mock
                CreateWhiteCirclesMock();
            else // opp real
                CreateOppCircles(iAmWhite);
        }
    }
    public void ShowAvailableMoves(Tile selectedTile)
    {
        var selectedCircle = GetCircleAtPosition(selectedTile.Position);
        GameController.Instance.SelectedCircle = selectedCircle;
        if (selectedCircle == null)
        {
            GameController.Instance.UpdateMatchMovementState(MatchMovementState.None);
            return;
        }
        GameController.Instance.UpdateMatchMovementState(MatchMovementState.ShowingAvailableMoves);
        topRightMovesAvailable = new() { selectedTile.Position };
        topLeftMovesAvailable = new() { selectedTile.Position };
        bottomLeftMovesAvailable = new() { selectedTile.Position };
        bottomRightMovesAvailable = new() { selectedTile.Position };
        if (!selectedCircle.IsKing)
        {
            if (selectedCircle.IsWhite)
            {
                GetTopRightAvailableMoves(selectedTile);
                GetTopLeftAvailableMoves(selectedTile);
            }
            else
            {
                GetBottomLeftAvailableMoves(selectedTile);
                GetBottomRightAvailableMoves(selectedTile);
            }
        }
        else
        {
            GetTopRightAvailableMoves(selectedTile);
            GetTopLeftAvailableMoves(selectedTile);
            GetBottomLeftAvailableMoves(selectedTile);
            GetBottomRightAvailableMoves(selectedTile);
        }
        HightlightAllTilesAvailable();
    }
    private bool CircleReachedEnd(Circle theCircle, Tile destination)
    {
        if (theCircle.IsWhite)
        {
            return destination.Position.x == 7;
        }
        else
        {
            return destination.Position.x == 0;
        }
    }
    public void MoveCircle(Circle circle, Tile destination)
    {
        var dyingCircles = GetCirclesBetween(GetTileAtPosition(circle.Position), destination);
        RemoveCircles(dyingCircles);
        _circles.Remove(circle.Position);
        _circles[destination.Position] = circle;
        GameController.Instance.UpdateMatchMovementState(MatchMovementState.None);
        ClearAvailableMoves();
        var killedOpponent = dyingCircles.Count > 0;
        void onFinishMove(Circle circle)
        {
            if (!circle.IsKing && CircleReachedEnd(circle, destination))
                circle.IsKing = true;
            if (killedOpponent)
            {
                var thereIsKillingMoves = ShowNextKillingMoves(circle);
                if (thereIsKillingMoves)
                {
                    GameController.Instance.UpdateMatchMovementState(MatchMovementState.ShowingAvailableMoves);
                    GameController.Instance.MyTurn = true;
                }
                else
                {
                    GameController.Instance.UpdateMatchMovementState(MatchMovementState.None);
                    ClearAvailableMoves();
                    GameController.Instance.MyTurn = false;
                }
                UIManager.Instance.UpdateScore();
            }
            else
                GameController.Instance.MyTurn = false;
        }
        circle.Move(destination, onFinishMove);
    }
    public void MoveCircleByPos(Vector2 startPos, Vector2 endPos)
    {
        var circle = GetCircleAtPosition(startPos);
        var destination = GetTileAtPosition(endPos);
        var dyingCircles = GetCirclesBetween(GetTileAtPosition(circle.Position), destination);
        RemoveCircles(dyingCircles);
        _circles.Remove(circle.Position);
        _circles[destination.Position] = circle;
        circle.Move(destination, onFinishMove);
        void onFinishMove(Circle circle)
        {
            if (!circle.IsKing && CircleReachedEnd(circle, destination))
                circle.IsKing = true;
            UIManager.Instance.UpdateScore();
        }
    }
    public int GetAliveCirclesCount(bool white)
    {
        var res = 0;
        foreach (var circle in _circles.Values)
        {
            res += white == circle.IsWhite ? 1 : 0;
        }
        return res;
    }
    public void DisposeAllTiles()
    {
        foreach (var tile in _tiles.Values)
        {
            Object.Destroy(tile.gameObject);
        }
    }
    public void DisposeAllCircles()
    {
        foreach (var tile in _circles.Values)
        {
            Object.Destroy(tile.gameObject);
        }
    }
}