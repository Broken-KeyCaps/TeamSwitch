using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TeamLobbyManager : MonoBehaviourPunCallbacks
{
    private const string TEAM_PROP = "Team";
    private const string IS_PRIVATE_PROP = "IsPrivateMatch";
    private const string STAKE_PROP = "PlayerStake";
    private const string MIN_STAKE_PROP = "MinStake";

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject hostPanel;

    [Header("Team Selection UI")]
    public GameObject redTeamButton;
    public GameObject blueTeamButton;

    [Header("Scroll View Contents")]
    public Transform redTeamContent;
    public Transform blueTeamContent;
    public GameObject playerTextPrefab;

    [Header("Room Info UI")]
    public TMP_Text roomCodeText;
    public TMP_InputField stakeInputField;
    public TMP_Text playerCountText;

    private Dictionary<int, GameObject> playerListItemPool = new Dictionary<int, GameObject>();

    public override void OnEnable()
    {
        base.OnEnable();
    }

    void Start()
    {
        // Instead of running setup immediately, we start a background routine
        StartCoroutine(InitializeLobbyRoutine());
    }
    IEnumerator InitializeLobbyRoutine()
    {
        // 1. THE WAITING ROOM: Wait until Photon is 100% finished syncing the room
        while (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("Waiting for network sync...");
            yield return null;
        }

        Debug.Log("✅ Network Sync Complete! Setting up UI for Player: " + PhotonNetwork.LocalPlayer.ActorNumber);


        if (hostPanel != null)
        {
            hostPanel.SetActive(PhotonNetwork.IsMasterClient);
        }
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        }

        if (stakeInputField != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MIN_STAKE_PROP))
        {
            int minStake = (int)PhotonNetwork.CurrentRoom.CustomProperties[MIN_STAKE_PROP];
            stakeInputField.text = minStake.ToString();
        }

        //Match name with prefab 
        PhotonNetwork.Instantiate("Player",Vector3.zero, Quaternion.identity);
        

        EnforcePublicPrivateUI();


        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TEAM_PROP))
        {
            AutoAssignTeam();
        }


        UpdatePlayerCountUI();
        RefreshTeamLists();
    }
    // --------------------------------------------------------
    // CORE UI UPDATES (Fixes the Sync Issues)
    // --------------------------------------------------------

    private void EnforcePublicPrivateUI()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        bool isPrivate = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(IS_PRIVATE_PROP))
        {
            isPrivate = (bool)PhotonNetwork.CurrentRoom.CustomProperties[IS_PRIVATE_PROP];
        }

        if (redTeamButton != null && blueTeamButton != null)
        {
            redTeamButton.SetActive(isPrivate);
            blueTeamButton.SetActive(isPrivate);
        }
    }

    private void UpdatePlayerCountUI()
    {
        if (playerCountText != null && PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = "Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
        }
    }

    // --- THESE CALLBACKS ENSURE EVERYONE UPDATES WHEN SOMEONE JOINS OR LEAVES ---
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCountUI();
        RefreshTeamLists();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        if (playerListItemPool.TryGetValue(otherPlayer.ActorNumber, out GameObject item))
        {
            Destroy(item);
            playerListItemPool.Remove(otherPlayer.ActorNumber);
        }

        UpdatePlayerCountUI();
        RefreshTeamLists();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(IS_PRIVATE_PROP))
        {
            EnforcePublicPrivateUI();
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //When the server confirms someone picked a team everyone's screen instantly redraws the lists.
        if (changedProps.ContainsKey(TEAM_PROP))
        {
            RefreshTeamLists();
        }
    }

    private void RefreshTeamLists()
    {
        var currentActorNumbers = new HashSet<int>();

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey(TEAM_PROP)) continue;

            currentActorNumbers.Add(p.ActorNumber);

            int team = (int)p.CustomProperties[TEAM_PROP];
            Transform targetContent = (team == 1) ? redTeamContent : blueTeamContent;

            string displayName = string.IsNullOrEmpty(p.NickName) ? "Player " + p.ActorNumber : p.NickName;

            if (playerListItemPool.TryGetValue(p.ActorNumber, out GameObject textObj))
            {
                if (textObj.transform.parent != targetContent)
                {
                    textObj.transform.SetParent(targetContent, false);
                }
                textObj.GetComponent<TMP_Text>().text = displayName;
            }
            else
            {
                textObj = Instantiate(playerTextPrefab, targetContent);
                textObj.GetComponent<TMP_Text>().text = displayName;
                playerListItemPool[p.ActorNumber] = textObj;
            }
        }

        List<int> toRemove = null;
        foreach (var kvp in playerListItemPool)
        {
            if (!currentActorNumbers.Contains(kvp.Key))
            {
                (toRemove ??= new List<int>()).Add(kvp.Key);
            }
        }
        if (toRemove != null)
        {
            foreach (int actorNumber in toRemove)
            {
                Destroy(playerListItemPool[actorNumber]);
                playerListItemPool.Remove(actorNumber);
            }
        }
    }

    // --------------------------------------------------------
    // TEAM ASSIGNMENT & STAKE LOGIC
    // --------------------------------------------------------

    private void AutoAssignTeam()
    {
        int redCount = GetPlayerCountOnTeam(1);
        int blueCount = GetPlayerCountOnTeam(2);

        if (redCount <= blueCount) SetPlayerTeam(1);
        else SetPlayerTeam(2);
    }
    public void OnStartPressed()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(2);


    }
    public void JoinRedTeam() { SetPlayerTeam(1); }
    public void JoinBlueTeam() { SetPlayerTeam(2); }

    private void SetPlayerTeam(int teamIndex)
    {
        Hashtable hash = new Hashtable();
        hash.Add(TEAM_PROP, teamIndex);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    private int GetPlayerCountOnTeam(int teamIndex)
    {
        int count = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.ContainsKey(TEAM_PROP) && (int)p.CustomProperties[TEAM_PROP] == teamIndex)
            {
                count++;
            }
        }
        return count;
    }

    public void ConfirmStakeBtn()
    {
        if (stakeInputField != null && int.TryParse(stakeInputField.text, out int enteredStake))
        {
            int minStake = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MIN_STAKE_PROP))
            {
                minStake = (int)PhotonNetwork.CurrentRoom.CustomProperties[MIN_STAKE_PROP];
            }

            if (enteredStake >= minStake)
            {
                Hashtable hash = new Hashtable();
                hash.Add(STAKE_PROP, enteredStake);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                Debug.Log("Stake accepted: " + enteredStake);
            }
            else
            {
                Debug.LogWarning("Stake too low! Minimum is: " + minStake);
                stakeInputField.text = minStake.ToString();
            }
        }
    }
}