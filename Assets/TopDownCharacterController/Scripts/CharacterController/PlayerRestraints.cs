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
        
        if (photonView.IsMine && IsTiedUp)
        {
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _currentMashClicks++;

                if (_currentMashClicks >= ClicksRequiredToEscape)
                {
                   
                    photonView.RPC("RPC_EscapeTie", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_GetTiedUp()
    {
        IsTiedUp = true;
        _currentMashClicks = 0; 

        Debug.Log(gameObject.name + " was hit by a Net Gun and is tied up!");

        
        if (photonView.IsMine)
        {
            if (_playerController != null) _playerController.enabled = false;

            // Note: Change sprites to look tied up
        }
    }

    [PunRPC]
    public void RPC_EscapeTie()
    {
        IsTiedUp = false;
        Debug.Log(gameObject.name + " broke free from the net!");

        
        if (photonView.IsMine)
        {
            if (_playerController != null) _playerController.enabled = true;

            // Note: Change to normal sprite
        }
    }
}