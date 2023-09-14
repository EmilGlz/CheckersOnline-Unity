using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button quickPlayButton;
    [SerializeField] Button createPrivateRoomButton;
    [SerializeField] Button joinPrivateRoomButton;
    [SerializeField] Button playOfflineButton;
    [SerializeField] Button playOnlineButton;
    [SerializeField] TMP_Text[] myUsernameTexts;
    [SerializeField] TMP_InputField newUsernameInput;
    [SerializeField] TMP_InputField roomCodeInput;
    [SerializeField] GameObject loadingMenu;
    [SerializeField] GameObject[] Menus;
    public GameObject PopupsCanvas;
    public Canvas Canvas;
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
                var isOnline = GameController.Instance.IsOnline;
                if (isOnline)
                    opp = GameController.Instance.Opponent;
                var oppName = opp != null ? LobbyManager.GetPlayerName(opp) : "";
                var oppNameText = GuiUtils.FindGameObject("OppNameText", GetCurrentMenuObj()).GetComponent<TMP_Text>();
                oppNameText.transform.parent.gameObject.SetActive(isOnline);
                oppNameText.text = oppName;
            }
            else if (value == Menu.WaitingForOppToJoinMenu)
            {
                var title = GuiUtils.FindGameObject("TitleText", GetCurrentMenuObj()).GetComponent<TMP_Text>();
                title.text = LobbyManager.Instance.WaitingForOppToJoinInPrivateRoom ? 
                    "Lobby code: " + LobbyManager.Instance.CurrentPrivateRoomJoinCode : 
                    "Waiting for an opponent to join...";
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
    private GameObject GetMenuObjectByEnum(Menu searchingMenu)
    {
        for (int i = 0; i < Menus.Length; i++)
        {
            if (i == (int)searchingMenu)
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
        PopupsCanvas.SetActive(true);
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
        createPrivateRoomButton.onClick.RemoveAllListeners();
        createPrivateRoomButton.onClick.AddListener(CreatePrivateRoom);
        joinPrivateRoomButton.onClick.RemoveAllListeners();
        joinPrivateRoomButton.onClick.AddListener(JoinPrivateRoom);
        playOfflineButton.onClick.RemoveAllListeners();
        playOfflineButton.onClick.AddListener(PlayOffline);
        playOnlineButton.onClick.RemoveAllListeners();
        playOnlineButton.onClick.AddListener(PlayOnline);
    }
    private async void PlayOnline()
    {
        LoadingMenu.Show();
        var signedIn = await GameController.Instance.SignIn();
        if (signedIn)
            CurrentMenu = Menu.PlayOnlineMenu;
        else
        {
            CurrentMenu = Menu.MainMenu;
            NotificationBox.Create("Something went wrong.");
        }
        LoadingMenu.Hide();
    }
    private void PlayOffline()
    {
        GameController.Instance.InitGame(true);
        CurrentMenu = Menu.InGameMenu;
    }
    private async void CreatePrivateRoom()
    {
        LoadingMenu.Show();
        await LobbyManager.Instance.CreatePrivateRoom(GameController.Instance.OnICreatedPrivateRoom, OnError);
        LoadingMenu.Hide();
    }
    private async void JoinPrivateRoom()
    {
        var lobbyCode = roomCodeInput.text;
        if (string.IsNullOrEmpty(lobbyCode))
        {
            NotificationBox.Create("Please, enter the lobby code");
            return;
        }
        LoadingMenu.Show();
        await LobbyManager.Instance.JoinPrivateRoom(lobbyCode, GameController.Instance.OnIJoinedRoom, OnError);
        LoadingMenu.Hide();
    }
    private async void QuickPlay()
    {
        LoadingMenu.Show();
        await LobbyManager.Instance.PlayRandom(GameController.Instance.OnIJoinedRoom, GameController.Instance.OnICreatedRoom, OnError);
        LoadingMenu.Hide();
    }
    public void OnIJoinedRoom(Lobby lobby)
    {
        CurrentMenu = Menu.InGameMenu;
    }
    public void OnICreatedRoom(Lobby lobby)
    {
        NotificationBox.Create("Created new room");
    }
    public void OnOppJoinedMyRoom(Lobby lobby)
    {
        CurrentMenu = Menu.InGameMenu;
        NotificationBox.Create("Joined the room: " + LobbyManager.GetOpponent(lobby).Data["Name"].Value);
        LoadingMenu.Hide();
    }
    public void CancelWaitingForOthersToJoin()
    {
        GameController.Instance.CancelWaitingForOthersToJoin();
    }
    private void OnError(string err)
    {
        NotificationBox.Create(err);
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
            NotificationBox.Create("Username must not be less than 3 characters long");
            return;
        }
        Settings.SavedUsername = username;
        UpdateUsernameEverywhereInUI();
    }
    public void UpdateScore()
    {
        var currentMenu = GetMenuObjectByEnum(Menu.InGameMenu);
        if (currentMenu == null) 
            return;
        var whiteScoreText = GuiUtils.FindGameObject("MyScoreText", currentMenu).GetComponent<TMP_Text>();
        var blackScoreText = GuiUtils.FindGameObject("OppScoreText", currentMenu).GetComponent<TMP_Text>();
        whiteScoreText.color = Color.white;
        blackScoreText.color = Color.black;
        var whiteCount = GridManager.Instance.GetAliveCirclesCount(true);
        var blackCount = GridManager.Instance.GetAliveCirclesCount(false);
        whiteScoreText.text = whiteCount.ToString();
        blackScoreText.text = blackCount.ToString();
        bool iamWhite = GameController.Instance.IAmWhite;
        if (!GameController.Instance.IsOnline)
            iamWhite = GameController.Instance.CameraRotation.z == 0;
        bool iWon = iamWhite && blackCount == 0;
        bool iLost= !iamWhite && whiteCount == 0;
        if (iWon)
            MatchResultPopup.Create(true);
        else if (iLost)
            MatchResultPopup.Create(false);
    }
}
