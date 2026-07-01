using UnityEngine;
using Photon.Pun;
public class RoomManager : MonoBehaviourPunCallbacks {
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Scene loaded. We are in a room with " + PhotonNetwork.CurrentRoom.PlayerCount + " players.");
            GamePlay();
        }
    }
   
    private void GamePlay()
    {
        // This tells Photon to spawn your player prefab at coordinates (0,0,0) with no rotation. 
        // Make sure your prefab is named EXACTLY "Player" (case-sensitive).
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity); 
    } 
}