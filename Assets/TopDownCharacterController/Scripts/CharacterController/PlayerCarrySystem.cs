using UnityEngine;
using Photon.Pun;

public class PlayerCarrySystem : MonoBehaviourPun
{
    public enum CarryState { None, Carrying, Dragging }
    public CarryState CurrentState = CarryState.None;

    [Header("Settings")]
    [SerializeField] private float _interactRange = 2.0f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Aim Modifiers")]
    public float CarryRecoilMultiplier = 0.5f;
    public float DragRecoilMultiplier = 2.0f;

    private PhotonView _grabbedPlayerView;
    private Rigidbody2D _grabbedPlayerRb;

    private Collider2D _myCollider;
    private Collider2D _grabbedPlayerCollider;

    private void Awake()
    {
        _myCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        
        if (Input.GetKeyDown(KeyCode.E) && CurrentState == CarryState.None)
        {
            TryInteract(CarryState.Carrying);
        }
        
        else if (Input.GetKeyDown(KeyCode.F) && CurrentState == CarryState.None)
        {
            TryInteract(CarryState.Dragging);
        }
        
        else if (Input.GetKeyDown(KeyCode.Space) && CurrentState != CarryState.None)
        {
            ReleasePlayer();
        }

        
        if (CurrentState != CarryState.None && _grabbedPlayerView != null)
        {
            Vector3 offset = CurrentState == CarryState.Carrying ? (transform.up * -0.5f) : (transform.up * -1.2f);
            _grabbedPlayerView.transform.position = Vector3.Lerp(_grabbedPlayerView.transform.position, transform.position + offset, 15f * Time.deltaTime);
        }
        else if (CurrentState != CarryState.None && _grabbedPlayerView == null)
        {
            ReleasePlayer();
        }
    }

    private void TryInteract(CarryState modeToEnter)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _interactRange, _playerLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            PlayerRestraints restraints = hit.GetComponent<PlayerRestraints>();
            if (restraints != null && restraints.IsTiedUp)
            {
                PhotonView targetView = hit.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    targetView.RequestOwnership();

                    _grabbedPlayerView = targetView;
                    _grabbedPlayerRb = hit.GetComponent<Rigidbody2D>();

                    if (_grabbedPlayerRb != null)
                    {
                        _grabbedPlayerRb.bodyType = RigidbodyType2D.Kinematic;
                    }

                    _grabbedPlayerCollider = hit.GetComponent<Collider2D>();
                    if (_myCollider != null && _grabbedPlayerCollider != null)
                    {
                        Physics2D.IgnoreCollision(_myCollider, _grabbedPlayerCollider, true);
                    }

                    CurrentState = modeToEnter;
                    Debug.Log($"Successfully grabbed {targetView.Owner.NickName} in {modeToEnter} mode.");

                    photonView.RPC("RPC_SetCarryState", RpcTarget.Others, true, targetView.ViewID);
                    break;
                }
            }
        }
    }

    private void ReleasePlayer()
    {
        if (_grabbedPlayerRb != null)
        {
            _grabbedPlayerRb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (_myCollider != null && _grabbedPlayerCollider != null)
        {
            Physics2D.IgnoreCollision(_myCollider, _grabbedPlayerCollider, false);
        }
        _grabbedPlayerCollider = null;

        
        if (_grabbedPlayerView != null)
        {
            photonView.RPC("RPC_ForceDropPosition", RpcTarget.All, _grabbedPlayerView.ViewID, _grabbedPlayerView.transform.position);
        }
        

        CurrentState = CarryState.None;
        _grabbedPlayerView = null;
        Debug.Log("Dropped the body.");

        photonView.RPC("RPC_SetCarryState", RpcTarget.Others, false, 0);
    }

    [PunRPC]
    private void RPC_SetCarryState(bool isCarried, int viewID)
    {
        // Visual updates for other players seeing the carry happen can go here
    }

    [PunRPC]
    private void RPC_ForceDropPosition(int targetViewID, Vector3 dropPosition)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView != null)
        {
            
            targetView.transform.position = dropPosition;

            Rigidbody2D rb = targetView.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = dropPosition;
        }
    }
}