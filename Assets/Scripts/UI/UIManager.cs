using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button quickPlayButton;
    [SerializeField] TMP_Text[] myUsernameTexts;
    [SerializeField] TMP_Text notificationText;
    [SerializeField] TMP_InputField newUsernameInput;
    [SerializeField] GameObject loadingMenu;
    public GameObject LoadingMenuObj => loadingMenu;
    #region Singleton
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }
    private void Awake()
    {
        _instance = this;
        AddOnclicks();
    }
    #endregion
    private void Start()
    {
        LoadingMenu.HideLoadingText();
    }
    private void AddOnclicks()
    {
        quickPlayButton.onClick.RemoveAllListeners();
        quickPlayButton.onClick.AddListener(QuickPlay);
    }
    private async void QuickPlay()
    {
        await LobbyManager.Instance.PlayRandom(OnIJoinedRoom, OnICreatedRoom, OnError);
    }
    private void OnIJoinedRoom(Lobby lobby)
    {
        OpenMatch(lobby);
    }
    private void OnICreatedRoom(Lobby lobby)
    {
        ShowNotificationText("Created new room");
        LoadingMenu.ShowLoadingText("Waiting for someone to join...");
        LobbyManager.Instance.OnOppJoinedMe += OnOppJoinedMyRoom;
    }
    private void OnOppJoinedMyRoom(Lobby lobby)
    {
        ShowNotificationText("Joined the room: " + LobbyManager.GetOpponent(lobby).Data["Name"].Value);
        LobbyManager.Instance.OnOppJoinedMe -= OnOppJoinedMyRoom;
        LoadingMenu.HideLoadingText();
        OpenMatch(lobby);
    }
    private void OnError(string err)
    {
        ShowNotificationText(err);
    }
    private void OpenMatch(Lobby lobby)
    {
        GameController.Instance.InitGame(Settings.hostIsWhiteLogicActivated && LobbyManager.IAmHost(lobby));
    }
    public void UpdateUsernameEverywhereInUI()
    {
        foreach (var text in myUsernameTexts)
        {
            text.text = Settings.SavedUsername;
        }
    }
    public void ChangeUsername()
    {
        var username = newUsernameInput.text;
        if (username.Length < 3)
        {
            ShowNotificationText("Username must not be less than 3 characters long");
            return;
        }
        Settings.SavedUsername = username;
        UpdateUsernameEverywhereInUI();
    }

    private void ShowNotificationText(string text)
    {
        void MakeActionForNSeconds(Action startAction, Action finishAction, float duration, float delay)
        {
            StartCoroutine(MakeActionForNSecondsIE(startAction, finishAction, duration, delay));
        }
        notificationText.text = "";
        MakeActionForNSeconds(() => {
            notificationText.text = text;
        }, () => {
            notificationText.text = "";
        }, 1.5f, 0f
        );
    }
    private IEnumerator MakeActionForNSecondsIE(Action startAction, Action finishAction, float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        startAction.Invoke();
        yield return new WaitForSeconds(duration);
        finishAction.Invoke();
    }
}
