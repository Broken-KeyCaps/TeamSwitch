using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Photon.Realtime; 

public class VoiceChatManager : MonoBehaviourPun
{
    private PhotonVoiceView voiceView;
    private Recorder primaryRecorder;

    private const string TEAM_PROP = "Team";

    void Start()
    {
        if (!photonView.IsMine) return;

        voiceView = GetComponent<PhotonVoiceView>();
        primaryRecorder = PunVoiceClient.Instance.PrimaryRecorder;

        // 1. Check if we somehow beat the delay and are ALREADY in the room
        if (PunVoiceClient.Instance.Client.InRoom)
        {
            ApplyVoiceSettings();
        }
        else
        {
            // 2. THE EVENT LISTENER: Wait for the exact moment the connection state changes
            PunVoiceClient.Instance.Client.StateChanged += OnVoiceStateChanged;
        }

        //Player Freeze Logic

        PlayerController controller = GetComponent<PlayerController>();
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (controller != null)
        {
            if (sceneIndex == 1) // LOBBY
            {
                controller.SetState(PlayerController.PlayerState.InLobby);
            }
            else // GAME
            {
                controller.SetState(PlayerController.PlayerState.Playing);
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>(); // Or Rigidbody2D
        if (rb != null)
        {
            // If in lobby, freeze physics; if in game, unfreeze
            rb.isKinematic = (sceneIndex == 0);
        }
    }

    // This method is triggered AUTOMATICALLY by Photon whenever the connection shifts
    private void OnVoiceStateChanged(ClientState previousState, ClientState state)
    {
        // We only care about the moment we successfully JOIN the room
        if (state == ClientState.Joined)
        {
            // 3. Unsubscribe from the event to prevent memory leaks!
            PunVoiceClient.Instance.Client.StateChanged -= OnVoiceStateChanged;

            // 4. Now that we are safely inside, apply the settings
            ApplyVoiceSettings();
        }
    }

    // We moved the scene-check logic into its own clean method
    private void ApplyVoiceSettings()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex == 1) // LOBBY SCENE
        {
            Debug.Log("Spawned in Lobby. Activating Global Voice Chat.");
            SetGlobalChat();
        }
        else if (currentSceneIndex == 2) // GAME SCENE
        {
            Debug.Log("Spawned in Game. Activating Team Voice Chat.");

            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(TEAM_PROP))
            {
                int myTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties[TEAM_PROP];
                SetTeamChat(myTeam);
            }
            else
            {
                Debug.LogWarning("Player has no team assigned! Defaulting to Global.");
                SetGlobalChat();
            }
        }
    }

    public void SetGlobalChat()
    {
        if (!photonView.IsMine) return;

        primaryRecorder.InterestGroup = 0;
        PunVoiceClient.Instance.Client.OpChangeGroups(new byte[0], new byte[] { 0 });

        Debug.Log("🎙️ Switched to GLOBAL Chat");
    }

    public void SetTeamChat(int teamIndex)
    {
        if (!photonView.IsMine) return;

        byte targetGroup = (byte)teamIndex;
        primaryRecorder.InterestGroup = targetGroup;
        PunVoiceClient.Instance.Client.OpChangeGroups(new byte[0], new byte[] { targetGroup });

        Debug.Log("🎙️ Switched to TEAM Chat. Group: " + targetGroup);
    }
}