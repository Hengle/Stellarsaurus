﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public int playerNumber;


    public Sprite[] sprites;
    public float groundMovementSpeed;
    public float airMovementSpeed;
    public float jumpHeight;
    public float kyoteTime = 0.25f;
    public float jumpBufferTime = 0.25f;
    [Range(0,1.0f)]
    public float cutJumpHeight = 0.5f;

    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public LayerMask wallLayer;
    public Transform gunOrigin;
    public SpriteRenderer shadowSprite;

    private Animator animator;
    private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    
    private PlayerParams playerParams;
    
    private float kyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private float horizontal;
    private bool isHoldingJumpKey;



    // Get components and initialise stats from design master here
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();        
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[playerNumber - 1];
        playerParams = GameManager.instance.loader.saveObject.playerParams;
        groundMovementSpeed = playerParams.groundSpeed;
        airMovementSpeed = playerParams.airSpeed;
        jumpHeight = playerParams.jumpHeight;
        rigidbody.gravityScale = playerParams.gravityScale;
        kyoteTime = playerParams.kyoteTime;
        jumpBufferTime = playerParams.jumpBufferTime;
        cutJumpHeight = playerParams.cutJumpHeight;
    }

    private void FixedUpdate()
    {
        // Sprite & animation Update starts here
        animator.SetFloat("Speed", Mathf.Abs(rigidbody.velocity.x));
       
        if (gunOrigin.rotation.eulerAngles.z < -90 || gunOrigin.rotation.eulerAngles.z > 90)
        {
            spriteRenderer.flipX = true;
        }
        else 
        {
            spriteRenderer.flipX = false;
        }

        // ends here


        // Grounded & jump logic update starts here

        isGrounded = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y - 0.5f), 0.25f, groundLayer) || Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y - 0.5f), 0.25f, platformLayer);
        
        kyoteTimer -= Time.fixedDeltaTime;
        jumpBufferTimer -= Time.fixedDeltaTime;

        if (isGrounded)
        {
            kyoteTimer = kyoteTime;
        }

        BetterJump();

        if (kyoteTimer > 0 && jumpBufferTimer > 0)
        {
            Jump();
        }

        // ends here

        // movement update starts here

        rigidbody.velocity = new Vector2(horizontal * ((isGrounded)? groundMovementSpeed :airMovementSpeed) * Time.fixedDeltaTime , rigidbody.velocity.y);
    
    
        //ends here
    }

    public void Move (float value)
    {
        horizontal = value;
    }

    public void StartJump ()
    {
        isHoldingJumpKey = true;
        jumpBufferTimer = jumpBufferTime;
    }

    public void EndJump()
    {
        isHoldingJumpKey = false;
    }

    public void StartFall ()
    {
        gameObject.layer = 10;
    }

    public void EndFall()
    {
        gameObject.layer = 0;
    }

    void Jump ()
    {
        kyoteTimer = 0;
        jumpBufferTimer = 0;
        shadowSprite.gameObject.SetActive(false);
        float jumpVelocity = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rigidbody.gravityScale));
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, jumpVelocity);
    }

    void BetterJump ()
    {
        if (isHoldingJumpKey == false)
        {
            if (rigidbody.velocity.y > 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y * cutJumpHeight);
            }
        }
    }

    public void Knockback (Vector2 direction, float magnitude)
    {
        Vector2 reverseDirection = new Vector2(direction.x * -1, direction.y * -1);
        rigidbody.AddForce(reverseDirection * magnitude);
    }

}



[System.Serializable]
public class PlayerParams 
{
    public int startingHealth;
    public float groundSpeed;
    public float airSpeed;
    public float jumpHeight;
    public float gravityScale;
    public float kyoteTime;
    public float jumpBufferTime;
    public float cutJumpHeight;
}
