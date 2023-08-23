using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button quickPlayButton;
    [SerializeField] Button playOfflineButton;
    [SerializeField] TMP_Text[] myUsernameTexts;
    [SerializeField] TMP_Text notificationText;
    [SerializeField] TMP_InputField newUsernameInput;
    [SerializeField] GameObject loadingMenu;
    [SerializeField] GameObject[] Menus;
    private Menu _currentMenu;
    public Menu CurrentMenu
    {
        get => _currentMenu;
        set
        {
            _currentMenu = value;
            UpdateMenu();
            if (value == Menu.InGameMenu)
            {
                Player opp = null;
                if (GameController.Instance.IsOnline)
                    opp = GameController.Instance.Opponent;
                var oppName = opp != null ? LobbyManager.GetPlayerName(opp) : "";
                var oppNameText = GuiUtils.FindGameObject("OppNameText", GetCurrentMenuObj()).GetComponent<TMP_Text>();
                oppNameText.text = oppName;
            }
        }
    }
    private void UpdateMenu()
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            Menus[i].SetActive(i == (int)CurrentMenu);
        }
    }
    private GameObject GetCurrentMenuObj()
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            if(i == (int)CurrentMenu)
                return Menus[i];
        }
        return null;
    }
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
        CurrentMenu = Menu.MainMenu;
        LoadingMenu.Hide();
    }
    private void AddOnclicks()
    {
        quickPlayButton.onClick.RemoveAllListeners();
        quickPlayButton.onClick.AddListener(QuickPlay);
        playOfflineButton.onClick.RemoveAllListeners();
        playOfflineButton.onClick.AddListener(PlayOffline);
    }
    private void PlayOffline()
    {
        GameController.Instance.InitGame(true);
        CurrentMenu = Menu.InGameMenu;
    }
    private async void QuickPlay()
    {
        await LobbyManager.Instance.PlayRandom(GameController.Instance.OnIJoinedRoom, GameController.Instance.OnICreatedRoom, OnError);
    }
    public void OnIJoinedRoom(Lobby lobby)
    {
        CurrentMenu = Menu.InGameMenu;
    }
    public void OnICreatedRoom(Lobby lobby)
    {
        ShowNotificationText("Created new room");
        LoadingMenu.Show("Waiting for someone to join...");
    }
    public void OnOppJoinedMyRoom(Lobby lobby)
    {
        CurrentMenu = Menu.InGameMenu;
        ShowNotificationText("Joined the room: " + LobbyManager.GetOpponent(lobby).Data["Name"].Value);
        LoadingMenu.Hide();
    }
    public void CancelWaitingForOthersToJoin()
    {
        GameController.Instance.CancelWaitingForOthersToJoin();
    }
    private void OnError(string err)
    {
        ShowNotificationText(err);
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
