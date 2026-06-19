using UnityEngine;
using Photon.Pun;

public class PlayerCameraSetup : MonoBehaviourPun
{
    void Start()
    {
        // We only want the camera to follow OUR player, not the enemies popping in.
        if (photonView.IsMine)
        {
            // Find the Main Camera in the scene and get its FollowCamera script
            FollowCamera cam = Camera.main.GetComponent<FollowCamera>();

            if (cam != null)
            {
                // Assign THIS player as the target!
                cam.target = this.gameObject;
            }
            else
            {
                Debug.LogError("FollowCamera script not found on the Main Camera!");
            }
        }
    }
}