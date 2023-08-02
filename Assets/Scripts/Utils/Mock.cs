using System.Collections.Generic;
using UnityEngine;

public static class Mock
{
    public static bool ShowMockWhiteCircles = true;
    public static bool ShowMockBlackCircles = false;
    public static bool MakeAllMyPlayerKings = false;
    public static List<Vector2> WhitePositions = new List<Vector2> {
        new Vector2(5, 1),
        new Vector2(4, 2),
        new Vector2(3, 3),
        new Vector2(5, 5),
        new Vector2(5, 3),
    };
    public static List<Vector2> BlackPositions = new List<Vector2> {
    };
}
