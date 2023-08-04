using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button serverBtn;
    [SerializeField] Button hostBtn;
    [SerializeField] Button clientBtn;
    [SerializeField] Button createLobby;
    [SerializeField] Button listLobbiesBtn;
    [SerializeField] Button joinLobby;
    private void Awake()
    {
        serverBtn.onClick.AddListener(()=> {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
        createLobby.onClick.AddListener(() => {
            TestLobby.Instance.CreateLobby();
        });
        listLobbiesBtn.onClick.AddListener(() => {
            TestLobby.Instance.ListLobbies();
        });
        joinLobby.onClick.AddListener(() => {
            TestLobby.Instance.JoinLobby();
        });
    }
}
