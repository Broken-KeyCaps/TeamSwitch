using UnityEngine;
using Photon.Pun; // 1. Added Photon namespace

public class Health : MonoBehaviourPun // 2. Changed to MonoBehaviourPun
{
    public CharacterStats Character;

    [SerializeField] private float _health;

    private void Start()
    {
        _health = Character.MaxHealth;
    }

    // This is the public function your bullets will call
    public void TakeDamage(float damage)
    {
        // 3. Instead of just doing the math locally, we broadcast the damage to ALL players in the room
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    // 4. The [PunRPC] tag allows this function to be triggered over the internet
    [PunRPC]
    private void RPC_TakeDamage(float damage)
    {
        if (_health - damage <= 0f)
        {
            Die();
        }
        else
        {
            _health -= damage;
        }
    }

    private void Die()
    {
        // 5. Only the person who owns this player object is allowed to tell the server to destroy it
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}