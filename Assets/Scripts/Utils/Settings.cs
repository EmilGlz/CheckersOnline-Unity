using UnityEngine;

public static class Settings
{
    private static readonly string SavedUsernameKey = "SavedUsername";
    public static readonly float LobbyPollTime = 1.1f;
    public static readonly float LobbyHeartBeat = 15f;
    public static readonly bool hostIsWhiteLogicActivated = true;
    public static string SavedUsername
    {
        get => PlayerPrefs.GetString(SavedUsernameKey);
        set => PlayerPrefs.SetString(SavedUsernameKey, value);
    }

}
