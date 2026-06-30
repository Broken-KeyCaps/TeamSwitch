using UnityEngine;
using Photon.Pun;

public class PlayerRestraints : MonoBehaviourPun
{
    public bool IsTiedUp = false;

    private PlayerController _playerController;
    private WeaponController _weaponController;

    [Header("Mash To Escape Settings")]
    public int ClicksRequiredToEscape = 15;
    private int _currentMashClicks = 0;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _weaponController = GetComponentInChildren<WeaponController>();
    }

    private void Update()
    {
        // Only allow the local player who is tied up to mash buttons
        if (photonView.IsMine && IsTiedUp)
        {
            // Mash Spacebar (or left click) to escape
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentMashClicks++;

                if (_currentMashClicks >= ClicksRequiredToEscape)
                {
                    // When free, tell the network to untie us!
                    photonView.RPC("RPC_EscapeTie", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_GetTiedUp()
    {
        IsTiedUp = true;
        _currentMashClicks = 0; // Reset escape progress

        Debug.Log(gameObject.name + " was hit by a Net Gun and is tied up!");

        // If this is MY player that got hit, disable my movement and shooting
        if (photonView.IsMine)
        {
            if (_playerController != null) _playerController.enabled = false;

            // Note: You might want to visually change the sprite here to look tied up!
        }
    }

    [PunRPC]
    public void RPC_EscapeTie()
    {
        IsTiedUp = false;
        Debug.Log(gameObject.name + " broke free from the net!");

        // If this is MY player, give me back my controls
        if (photonView.IsMine)
        {
            if (_playerController != null) _playerController.enabled = true;

            // Note: Revert your sprite back to normal here!
        }
    }
}