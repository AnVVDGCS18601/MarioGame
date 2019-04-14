using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CharacterController2D : MonoBehaviour {

    [SerializeField] private float m_speed = 6f;
    [Range(0, 0.7f)] [SerializeField] private float m_SmoothTime = 0.3f;
    [SerializeField] private float m_JumpForce = 5f;
    [SerializeField] private float JumpTime;
    [SerializeField] private LayerMask m_WhatIsGround;

    [Header("Audio")]
    [SerializeField] private AudioClip JumpClip;
    [SerializeField] private AudioClip DeathClip;

    private float JumpTimeCounter;
    private float horizontalInput = 0f;
    private bool IsFacingRight;
    private bool IsGrounded;
    private bool IsJumping;


    AudioSource m_audioSource;
    Rigidbody2D m_rigidbody2D;
    Animator m_anim;
    Vector2 CurrentVelocity = Vector2.zero;
    
    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        Jump();
        Sprint();
        m_anim.speed = Mathf.Abs(horizontalInput);
        m_anim.SetFloat("IsRunning", Mathf.Abs(horizontalInput));
    }

    private void FixedUpdate()
    {
        Move(horizontalInput);
        Jump();
        HitEnemy();
    }

    private void Move(float move)
    {
        Vector2 targetVelocity = new Vector2(move * m_speed, m_rigidbody2D.velocity.y);

        //Add velocity to move player

        //m_rigidbody2D.velocity = new Vector2(move * m_speed, m_rigidbody2D.velocity.y);

        m_rigidbody2D.velocity = Vector2.SmoothDamp(m_rigidbody2D.velocity, targetVelocity, ref CurrentVelocity, m_SmoothTime);

        if (move > 0 && IsFacingRight)
        {
            Flip();
        }

        if(move < 0 && !IsFacingRight)
        {
            Flip();
        }
    }

    void Sprint()
    {

        if(Input.GetKey(KeyCode.Z))
        {
            horizontalInput *= 2;
        }
    }

    void Jump ()
    {
        if(IsGrounded == true && Input.GetKeyDown(KeyCode.X))
        {
            m_rigidbody2D.velocity = new Vector2(m_rigidbody2D.velocity.x, m_JumpForce);

            //m_rigidbody2D.AddForce(new Vector2(0f, m_JumpForce), ForceMode2D.Impulse);
            m_audioSource.clip = JumpClip;
            m_audioSource.Play();
            IsGrounded = false;
            IsJumping = true;
            JumpTimeCounter = JumpTime;
            m_anim.SetBool("IsJumping", true);
        }
        if(Input.GetKey(KeyCode.X) && IsJumping == true)
        {
            if(JumpTimeCounter > 0)
            {
                m_rigidbody2D.velocity = new Vector2(m_rigidbody2D.velocity.x, m_JumpForce);
                //m_rigidbody2D.AddForce(new Vector2(0f, m_JumpForce), ForceMode2D.Impulse);
                JumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                IsJumping = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.X))
        {
            IsJumping = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RaycastHit2D hit = Physics2D.Raycast(m_rigidbody2D.position, Vector2.down, 1f, m_WhatIsGround);

        if (hit.transform.tag == "Ground")
        {
            IsGrounded = true;
            m_anim.SetBool("IsJumping", false);

        }
        else
        {
            IsGrounded = false;
        }
    }
    public void Flip()
    {
        IsFacingRight = !IsFacingRight;

        //Flip the player, so player can rotate to another direction
        transform.Rotate(0f, 180f, 0f);
    }

    private void HitEnemy ()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down);

        if (hit != null && hit.collider != null && hit.distance < 0.9f && hit.collider.gameObject.tag == "Enemy")
        {
            IsGrounded = true;
            m_rigidbody2D.velocity = new Vector2(m_rigidbody2D.velocity.x, m_JumpForce / 2);
            hit.collider.gameObject.GetComponent<EnemyMovement>().Death();
        }
        if (hit != null && hit.collider != null && hit.distance < 0.2f && hit.collider.gameObject.tag != "Enemy")
        {
            IsGrounded = true;
        }
    }

    public void Death ()
    {
        m_rigidbody2D.isKinematic = true;
    }
}
