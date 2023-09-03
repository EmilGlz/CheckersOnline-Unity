using System.Collections.Generic;
using UnityEngine;

public static class Mock
{
    public static float movementDuration = 1f;
    public static bool ShowMockWhiteCircles = false;
    public static bool ShowMockBlackCircles = false;
    public static bool MakeAllWhitePlayerKings = false;
    public static bool MakeAllBlackPlayerKings = false;
    public static List<Vector2> WhitePositions = new List<Vector2> {
        new Vector2(6, 4),
        //new Vector2(5, 5),
        //new Vector2(5, 3),
        //new Vector2(3, 3),
    };
    public static List<Vector2> BlackPositions = new List<Vector2> {
        new Vector2(5, 5),
        new Vector2(5, 3),
        new Vector2(3, 3),
        new Vector2(4, 2),
    };
}
