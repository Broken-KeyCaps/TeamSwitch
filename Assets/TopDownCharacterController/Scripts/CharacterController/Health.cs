using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Health : MonoBehaviourPun
{
    public CharacterStats Character;
    [SerializeField] private float _health;
    private bool _isDead = false;

    [Header("Overhead UI")]
    [SerializeField] private Slider _healthBarSlider;
    [SerializeField] private Text _healthText;

    private void Start()
    {
        _health = Character.MaxHealth;
        UpdateHealthUI(); 

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

        
        PlayerRestraints restraints = GetComponent<PlayerRestraints>();
        if (restraints != null && restraints.IsTiedUp)
        {
            damage *= 2f;
            Debug.Log("CRITICAL! " + gameObject.name + " is tied up and took DOUBLE damage: " + damage);
        }
        

        _health -= damage;
        UpdateHealthUI(); 

        Debug.Log(" " + gameObject.name + " took " + damage + " damage! Remaining Health: " + _health);

        if (_health <= 0f)
        {
            Debug.Log("dead " + gameObject.name + " HP reached 0! Triggering Death.");
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        
        if (_healthBarSlider != null)
        {
            _healthBarSlider.maxValue = Character.MaxHealth;
            _healthBarSlider.value = _health;
        }

        
        if (_healthText != null)
        {
            _healthText.text = Mathf.Ceil(_health).ToString() + " / " + Character.MaxHealth.ToString();
        }
    }

    private void Die()
    {
        _isDead = true;

        
        if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = false;
        if (GetComponent<WeaponController>() != null) GetComponent<WeaponController>().enabled = false;
        if (GetComponent<PlayerCarrySystem>() != null) GetComponent<PlayerCarrySystem>().enabled = false;

        Animator anim = GetComponent<PlayerController>()._animator;
        if (anim != null) anim.SetTrigger("Death");

        
        if (_healthBarSlider != null) _healthBarSlider.gameObject.SetActive(false);
        if (_healthText != null) _healthText.gameObject.SetActive(false);

        
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        
        yield return new WaitForSeconds(1.5f);

        
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}