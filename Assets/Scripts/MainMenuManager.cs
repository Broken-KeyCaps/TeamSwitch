using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    private const string IS_PRIVATE_PROP = "IsPrivateMatch";
    private const string MIN_STAKE_PROP = "MinStake";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject loadingPanel;
    public GameObject createRoomSettingsPanel;
    public GameObject nicknamePopupPanel;

    [Header("Nickname Setup")]
    public TMP_InputField nicknameInputField;

    [Header("Join via Code")]
    public TMP_InputField roomCodeInputField;

    [Header("Room Creation Settings")]
    public TMP_InputField minStakeInputField;
    public Toggle isPrivateToggle;

    [Header("Room List (Scroll View)")]
    public Transform roomListContent;
    public GameObject roomListItemPrefab;

    private string selectedRoomName = "";
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private GameObject currentPanel;

    private Dictionary<string, GameObject> roomListItemPool = new Dictionary<string, GameObject>();

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ShowPanel(loadingPanel);

        // --- ADD THIS CHECK SO WE DON'T CONNECT TWICE ---
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Already connected! Joining Lobby directly...");
            PhotonNetwork.JoinLobby();
        }
        else
        {
            Debug.Log("Connecting to Server...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master. Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");
        ShowPanel(nicknamePopupPanel);
    }
    public void ConfirmNicknameBtn()
    {
        string chosenName = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(chosenName))
        {
            chosenName = "Player " + Random.Range(1000, 9999);
        }

        // Tell the Photon Server what our name is!
        PhotonNetwork.NickName = chosenName;
        Debug.Log("Nickname set to: " + PhotonNetwork.NickName);

        // Now we can safely enter the Main Menu
        ShowPanel(mainMenuPanel);
    }

    // --------------------------------------------------------
    // UI NAVIGATION HELPER
    // --------------------------------------------------------
    private void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel == panelToShow) return;

        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(false);
        createRoomSettingsPanel.SetActive(false);
        nicknamePopupPanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
        currentPanel = panelToShow;
    }
    public void JoinByCodeBtn()
    {
        string code = roomCodeInputField.text.Trim();
        if (string.IsNullOrEmpty(code)) return;

        ShowPanel(loadingPanel);
        PhotonNetwork.JoinRoom(code);
    }

    public void OpenCreateRoomSettingsBtn()
    {
        if (minStakeInputField != null) minStakeInputField.text = "";
        if (isPrivateToggle != null) isPrivateToggle.isOn = false;
        ShowPanel(createRoomSettingsPanel);
    }

    public void ConfirmAndCreateRoomBtn()
    {
        int minStake = 0;
        if (minStakeInputField != null && !string.IsNullOrEmpty(minStakeInputField.text))
        {
            if (int.TryParse(minStakeInputField.text, out int parsedStake))
            {
                if (parsedStake < 0) return;
                minStake = parsedStake;
            }
            else return;
        }

        ShowPanel(loadingPanel);

        string randomCode = Random.Range(1000, 9999).ToString();
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 10;

        bool isPrivate = isPrivateToggle != null && isPrivateToggle.isOn;
        options.IsVisible = !isPrivate;

        Hashtable roomProps = new Hashtable();
        roomProps.Add(IS_PRIVATE_PROP, isPrivate);
        roomProps.Add(MIN_STAKE_PROP, minStake);

        options.CustomRoomProperties = roomProps;
        options.CustomRoomPropertiesForLobby = new string[] { IS_PRIVATE_PROP, MIN_STAKE_PROP }; //imp

        PhotonNetwork.CreateRoom(randomCode, options);
    }

    public void CancelCreateRoomBtn()
    {
        ShowPanel(mainMenuPanel);
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.LoadLevel(1);
    }

    // --------------------------------------------------------
    // SCROLLABLE ROOM LIST LOGIC
    // --------------------------------------------------------
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                if (roomListItemPool.TryGetValue(info.Name, out GameObject staleItem))
                {
                    Destroy(staleItem);
                    roomListItemPool.Remove(info.Name);
                }
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }

        foreach (RoomInfo info in cachedRoomList.Values)
        {
            if (roomListItemPool.TryGetValue(info.Name, out GameObject existingItem))
            {
                // Already on screen — just refresh its data (player count, stake, etc.)
                existingItem.GetComponent<RoomListItem>().Setup(info, this);
            }
            else
            {
                GameObject listItem = Instantiate(roomListItemPrefab, roomListContent);
                listItem.GetComponent<RoomListItem>().Setup(info, this);
                roomListItemPool[info.Name] = listItem;
            }
        }
    }

    public void SelectRoom(string roomName)
    {
        selectedRoomName = roomName;
        Debug.Log("Selected Room: " + selectedRoomName);
    }

    public void JoinSelectedRoomBtn()
    {
        if (string.IsNullOrEmpty(selectedRoomName)) return;

        ShowPanel(loadingPanel);
        PhotonNetwork.JoinRoom(selectedRoomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        ShowPanel(mainMenuPanel);
    }
}