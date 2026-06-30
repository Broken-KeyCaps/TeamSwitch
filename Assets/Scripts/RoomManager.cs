using UnityEngine;
using Photon.Pun;
public class RoomManager : MonoBehaviourPunCallbacks {
    // Note: We don't need a GameObject reference for Photon.
    // Photon loads prefabs by their exact spelling as a string.
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Scene loaded. We are in a room with " + PhotonNetwork.CurrentRoom.PlayerCount + " players.");
            GamePlay();
        }
    }
   
    // --- NEW CODE BELOW ---
    private void GamePlay()
    {
        // This tells Photon to spawn your player prefab at coordinates (0,0,0) with no rotation. 
        // Make sure your prefab is named EXACTLY "Player" (case-sensitive).
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity); 
    } 
}