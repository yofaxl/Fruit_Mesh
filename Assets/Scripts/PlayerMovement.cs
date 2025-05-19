using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    private float horizontal;
    private float speed = 6f;
    private float jumpingPower = 15f;
    private bool isFacingRight = true;
    private bool doubleJump;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(3f, 15f);

    private Vector2 rightParticlePos;
    private Vector2 leftParticlePos;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private ParticleSystem dustParticles;

    private void Start()
    {
        rightParticlePos = dustParticles.transform.localPosition;
        leftParticlePos = new Vector2(-rightParticlePos.x, rightParticlePos.y);
    }

    private void Update()
    {
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        animator.SetFloat("magnitude", rb.linearVelocity.magnitude);
        animator.SetBool("isWallSliding", isWallSliding);

        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                animator.SetTrigger("jump");
            }
            else if (!doubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
                animator.SetTrigger("doubleJump");
                doubleJump = true;
            }
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            FlipCheck(); // sürekli kontrol
        }

        UpdateParticleDirection(); // her karede particle yönünü güncelle

        // Dust particle play/stop logic
        if (Mathf.Abs(horizontal) > 0.1f && IsGrounded() && !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }
        else if ((Mathf.Abs(horizontal) < 0.1f || !IsGrounded()) && dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if ((isFacingRight && wallJumpingDirection < 0f) || (!isFacingRight && wallJumpingDirection > 0f))
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void FlipCheck()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void UpdateParticleDirection()
{
    if (isFacingRight)
    {
        // Sağda ise partikülleri karakterin arkasına doğru konumlandırıyoruz
        dustParticles.transform.localPosition = new Vector2(rightParticlePos.x, rightParticlePos.y);
        dustParticles.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    else
    {
        // Soldaysa da aynı şekilde partikülleri karakterin arkasına yerleştiriyoruz
        dustParticles.transform.localPosition = new Vector2(leftParticlePos.x, leftParticlePos.y);
        dustParticles.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    
    // Partikülün, ayakların arkasında kalması için pozisyonu güncelliyoruz
    // Bu durumda biraz daha geriye, örneğin X ekseninde -0.5f, Y ekseninde de -0.5f gibi bir offset ekliyoruz
    dustParticles.transform.position = new Vector3(transform.position.x - 0.5f * Mathf.Sign(transform.localScale.x), transform.position.y - 0.5f, transform.position.z);  
}

}
