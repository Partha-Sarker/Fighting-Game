﻿using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    private AudioManager audioManager;
    private Shake camShake;
    public Manager manager;
    private PlayerMovement playerMovement;
    public Collider shieldCollider;
    public int maxHealth = 100;
    public RectTransform myHealth;
    public RectTransform myHealthHolder;
    private Animator animator;
    private Animator shootAnimator;
    private NetworkAnimator networkAnimator;
    public Transform hitHolder, blockHolder;
    public GameObject fistHitParticle, swordHitParticle;
    public float shakeDelay = .1f;
    public int currentHealth;
    private Vector3 initialPos;
    private Vector3 initialRot;
    [SerializeField]
    private MeshRenderer shield;

    [SerializeField]
    private Transform fireballPos;
    [SerializeField]
    private GameObject fireBall;
    [SerializeField]
    private int shootForce = 5;

    [SyncVar]
    private Vector3 scale;
    [SyncVar]
    private bool isDead, isWinner;

    private void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        audioManager = GetComponent<AudioManager>();
        camShake = FindObjectOfType<Shake>();
        manager = FindObjectOfType<Manager>();
        playerMovement = GetComponent<PlayerMovement>();
        shootAnimator = manager.shootAnimator;

        initialPos = transform.position;
        initialRot = transform.eulerAngles;

        SetupHealthBar();

        ResetAll();

    }

    private void SetupHealthBar()
    {
        if (isLocalPlayer)
        {
            myHealth = GameObject.Find("local player health").GetComponent<RectTransform>();
            myHealthHolder = GameObject.Find("local player health holder").GetComponent<RectTransform>();
        }
        else
        {
            myHealth = GameObject.Find("oponent health").GetComponent<RectTransform>();
            myHealthHolder = GameObject.Find("oponent health holder").GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && isLocalPlayer)
            TakeDamage(40, "Sword");

        if (Input.GetKeyDown(KeyCode.V) && isLocalPlayer)
            Win();
    }

    public void TakeDamage(int damage, string type)
    {
        if (isDead)
            return;

        camShake.ShakeCam(shakeDelay);

        if (type == "Sword")
        {
            //blood particle
            audioManager.PlaySFX("Sword Hit");
            GameObject particle = Instantiate(swordHitParticle, hitHolder);
            Destroy(particle, 1f);
        }
        else if(type == "Fist")
        {
            //hit effect
            audioManager.PlaySFX("Fist Hit");
            GameObject particle = Instantiate(fistHitParticle, hitHolder);
            Destroy(particle, 1f);
        }

        audioManager.PlaySFX("Pain");

        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;
        SetHealthBar(currentHealth);

        if (isLocalPlayer)
        {
            if (currentHealth == 0)
                Die();
            animator.SetInteger("HitNO", Random.Range(1, 4));
            animator.SetTrigger("GotHit");
        }
        if (currentHealth == 0 && !isLocalPlayer)
        {
            manager.localPlayer.GetComponent<Player>().Win();            
        }
    }

    public void BlockAttack(string type)
    {
        animator.SetTrigger("Blocked");
    }

    private void SetHealthBar(int amount)
    {
        scale.x = (float)amount / (float)maxHealth;
        myHealth.localScale = scale;
    }

    public void DisableShield()
    {
        shieldCollider.enabled = false;
    }

    public void EnableShield()
    {
        shieldCollider.enabled = true;
    }

    public void ResetAll()
    {
        //if (manager != null)
        //    manager.disableControl = false;

        manager.disableControl = false;
        //playerMovement.isGrounded = true;
        isDead = false;
        isWinner = false;
        Attack.isWinner = false;
        animator.SetBool("Dead", false);
        animator.SetBool("Win", false);
        animator.SetBool("Armed", false);
        scale = Vector3.one;
        myHealthHolder.localScale = scale;
        currentHealth = maxHealth;
        SetHealthBar(maxHealth);
        transform.position = initialPos;
        transform.eulerAngles = initialRot;
        shield.enabled = true;
        GetComponent<PlayerMovement>().isGrounded = true;
        if (isLocalPlayer)
        {
            shootAnimator.Play("Shoot Recovering");
        }
    }

    public void Die()
    {
        isDead = true;
        animator.SetBool("Dead", true);
        //manager = FindObjectOfType<Manager>();
        manager.reMatchButton.gameObject.SetActive(true);
        manager.disableControl = true;
        audioManager.PlaySFX("Defeat");
        print("I am dead :(");
    }

    public void Win()
    {
        isWinner = true;
        animator.SetBool("Win", true);
        audioManager.PlaySFX("Victory");
        //manager = FindObjectOfType<Manager>();
        manager.disableControl = true;
        print("I win :D");
    }

    private void OnDisable()
    {
        scale = Vector3.zero;
        if(myHealthHolder != null)
            myHealthHolder.localScale = scale;
    }

    public void HideShield()
    {
        shield.enabled = false;
    }

    public void ShootFireball()
    {
        GameObject tempFireBall = Instantiate(fireBall, fireballPos);
        tempFireBall.GetComponent<Rigidbody>().AddForce(fireballPos.forward * shootForce, ForceMode.Impulse);
    }

}
