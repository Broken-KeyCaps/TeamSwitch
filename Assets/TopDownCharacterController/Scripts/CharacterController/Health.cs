using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Health : MonoBehaviourPun
{
    public CharacterStats Character;
    [SerializeField] private float _health;
    private bool _isDead = false;

    private void Start()
    {
        _health = Character.MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    private void RPC_TakeDamage(float damage)
    {
        if (_isDead) return;

        _health -= damage;

        
        Debug.Log(" " + gameObject.name + " took " + damage + " damage! Remaining Health: " + _health);

        if (_health <= 0f)
        {
            Debug.Log("dead " + gameObject.name + " HP reached 0! Triggering Death.");
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;

        
        GetComponent<PlayerController>().enabled = false;
        GetComponent<WeaponController>().enabled = false;

        
        Animator anim = GetComponent<PlayerController>()._animator;
        if (anim != null) anim.SetTrigger("Death");

        if (photonView.IsMine)
        {
            
            StartCoroutine(DestroyAfterDeath());
        }
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1.5f);
        PhotonNetwork.Destroy(gameObject);
    }
}