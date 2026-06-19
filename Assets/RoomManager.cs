using UnityEngine;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // Note: We don't need a GameObject reference for Photon. 
    // Photon loads prefabs by their exact spelling as a string.

    void Start()
    {
        Debug.Log("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinOrCreateRoom("Room1", null, null);
    }

    // --- NEW CODE BELOW ---
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Successfully Joined Room! Spawning player...");

        // This tells Photon to spawn your player prefab at coordinates (0,0,0) with no rotation.
        // Make sure your prefab is named EXACTLY "Player" (case-sensitive).
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }
}