using UnityEditor;

internal static class EditorTests
{
    [MenuItem("Tests/Popups/Show Match Result Menu")]
    private static void OpenShowMatchResultPopup()
    {
        MatchResultPopup.Create(true, "Congratulations, you won!");
    }
}
