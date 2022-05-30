using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles Player's animations, collider, sounds and input
/// </summary>
public class PlayerController : MonoBehaviour
{
    public ParticleSystem exploisionParticle;
    public Animator playerAnimator;
    public AudioSource playerAudio;
    public AudioClip crashSound;
    public ParticleSystem dirtParticle;
    public bool onGround = true;
    public bool flying = false;
    [SerializeField] private ParticleSystem crouchDirtParticle;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private GameManager gameManager;
    private Rigidbody playerRb;
    private BoxCollider playerCollider;
    private float jumpForce = 900.0f;
    private bool crouching = false;
    private Vector3 crouchColliderCenter = new Vector3(0.03f, 1.22f, 0.6f);
    private Vector3 crouchColliderSize = new Vector3(1.5f, 2.5f, 1.15f);
    private Vector3 defaultColliderCenter = new Vector3(0.02f, 1.42f, 0.02f);
    private Vector3 defaultColliderSize = new Vector3(1.45f, 2.89f, 0.86f);

    void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        playerCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !GameManager.gameStarted && !GameManager.gameOver)
        {
            gameManager.OnGameStarted();
        }

        if (GameManager.gameStarted && !GameManager.gameOver && onGround)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
            if (Input.GetKeyDown(KeyCode.C) && !crouching)
            {
                StartCoroutine(Crouch());
            }
        }
    }

    void FixedUpdate()
    {
        // Jumping using force
        if (!onGround && !flying)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            flying = true;
        }
    }

    private void Jump()
    {
        onGround = false;
        playerAnimator.SetTrigger("Jump_trig");
        dirtParticle.Stop();
        if (crouching)
        {
            StopCrouhing();
        }
        playerAudio.PlayOneShot(jumpSound, 0.6f);
    }

    private IEnumerator Crouch()
    {
        StartCrouching();
        yield return new WaitForSeconds(1.0f);
        StopCrouhing();
    }

    private void StartCrouching()
    {
        dirtParticle.Stop();
        crouchDirtParticle.Play();
        playerCollider.center = crouchColliderCenter;
        playerCollider.size = crouchColliderSize;
        playerAnimator.SetBool("Crouch_b", true);
        crouching = true;
    }

    public void StopCrouhing()
    {
        crouchDirtParticle.Stop();
        if (onGround && !GameManager.gameOver)
        {
            dirtParticle.Play();
        }
        playerCollider.center = defaultColliderCenter;
        playerCollider.size = defaultColliderSize;
        playerAnimator.SetBool("Crouch_b", false);
        crouching = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            flying = false;
            onGround = true;
            if (!GameManager.gameOver && GameManager.gameStarted)
            {
                dirtParticle.Play();
            }
        }
        else if (collision.gameObject.CompareTag("Obstacle") && !GameManager.gameOver)
        {
            gameManager.OnGameOver();
        }
    }
}
