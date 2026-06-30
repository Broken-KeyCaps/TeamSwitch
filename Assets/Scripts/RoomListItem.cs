using UnityEngine;
using TMPro;
using Photon.Realtime; // Required for RoomInfo

public class RoomListItem : MonoBehaviour
{
    public TMP_Text roomInfoText; // Link your TextMeshPro element here
    private string roomName;
    private MainMenuManager manager;

    // We now pass the whole 'RoomInfo' object instead of just the string
    public void Setup(RoomInfo info, MainMenuManager menuManager)
    {
        roomName = info.Name;
        manager = menuManager;

        // Try to grab the stake amount from the room's properties
        int stakeAmount = 0;
        if (info.CustomProperties.ContainsKey("MinStake"))
        {
            stakeAmount = (int)info.CustomProperties["MinStake"];
        }

        // Format the text to show both the code and the stake!
        roomInfoText.text = $"Code: {roomName}   |   Stake: {stakeAmount}";
    }

    public void OnClick()
    {
        manager.SelectRoom(roomName);
    }
}