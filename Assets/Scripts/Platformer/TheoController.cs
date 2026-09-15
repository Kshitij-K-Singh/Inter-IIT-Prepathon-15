using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TheoController : MonoBehaviour
{
    public static TheoController Instance { get; private set; }

    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 11.5f;
    [SerializeField] float gravityScale = 4f;
    [SerializeField] float fallGravityMultiplier = 1.55f;
    [SerializeField] float dashSpeed = 12f;
    [SerializeField] float dashSeconds = 0.16f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.12f;
    [SerializeField] LayerMask groundMask = 1 << 6;
    [SerializeField] Animator animator;

    Rigidbody2D rb;
    SpriteRenderer sprite;
    bool frozen;
    bool grounded;
    int airJumps;
    float dashLeft;
    float coyote;
    float delayToIdle;
    int facing = 1;
    int currentAttack;
    float timeSinceAttack = 1f;
    float pendingHit = -1f;
    bool wasGrounded = true;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = gravityScale;
        rb.linearDamping = 0f;
        if (groundCheck == null)
        {
            var sensor = transform.Find("GroundSensor");
            groundCheck = sensor != null ? sensor : CreateGroundCheck();
        }
    }

    Transform CreateGroundCheck()
    {
        var check = new GameObject("GroundCheck");
        check.transform.SetParent(transform, false);
        check.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        return check.transform;
    }

    public void Freeze()
    {
        frozen = true;
        GameAudio.Instance?.SetWalking(false);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    public void Unfreeze()
    {
        frozen = false;
        if (rb != null) rb.simulated = true;
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        dashLeft = 0f;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = !frozen;
        }
    }

    void Update()
    {
        if (frozen || (GameManager.Instance != null && (GameManager.Instance.IsPaused || !GameManager.Instance.IsPlaying)))
        {
            GameAudio.Instance?.SetWalking(false);
            return;
        }

        var keyboard = Keyboard.current;

        grounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
        if (grounded && !wasGrounded) GameAudio.Instance?.PlayLand();
        wasGrounded = grounded;
        if (grounded) coyote = 0.1f;
        else coyote -= Time.deltaTime;

        if (grounded) airJumps = RuleBook.CanFly() ? 1 : 0;

        float x = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        }

        if (Mathf.Abs(x) > 0.01f)
        {
            facing = x < 0f ? -1 : 1;
            if (sprite != null) sprite.flipX = x < 0f;
        }

        bool jumpPressed = keyboard != null && (keyboard.spaceKey.wasPressedThisFrame
                           || keyboard.wKey.wasPressedThisFrame
                           || keyboard.upArrowKey.wasPressedThisFrame);
        if (jumpPressed) TryJump();

        bool dashPressed = keyboard != null && (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame);
        if (dashPressed) TryDash(x);

        timeSinceAttack += Time.deltaTime;
        bool attackPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                             || (keyboard != null && (keyboard.kKey.wasPressedThisFrame || keyboard.xKey.wasPressedThisFrame));
        if (attackPressed) TryAttack();

        if (pendingHit >= 0f)
        {
            pendingHit -= Time.deltaTime;
            if (pendingHit <= 0f)
            {
                pendingHit = -1f;
                DealAttackHit();
            }
        }

        bool walking = grounded && Mathf.Abs(x) > 0.2f && dashLeft <= 0f && !frozen;
        GameAudio.Instance?.SetWalking(walking);

        UpdateAnimator(x);
    }

    void UpdateAnimator(float x)
    {
        if (animator == null) return;
        animator.SetBool("Grounded", grounded);
        animator.SetFloat("AirSpeedY", rb != null ? rb.linearVelocity.y : 0f);
        if (Mathf.Abs(x) > 0.01f)
        {
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        }
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0f) animator.SetInteger("AnimState", 0);
        }
    }

    void FixedUpdate()
    {
        if (frozen || rb == null || !rb.simulated) return;

        if (dashLeft > 0f)
        {
            dashLeft -= Time.fixedDeltaTime;
            rb.gravityScale = gravityScale;
            return;
        }

        rb.gravityScale = rb.linearVelocity.y < 0f
            ? gravityScale * fallGravityMultiplier
            : gravityScale;

        var keyboard = Keyboard.current;
        float x = 0f;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        }

        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
    }

    void TryJump()
    {
        if (coyote > 0f)
        {
            coyote = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (animator != null) animator.SetTrigger("Jump");
            GameAudio.Instance?.PlayJump();
            return;
        }

        if (RuleBook.CanFly() && airJumps > 0)
        {
            airJumps--;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.92f);
            if (animator != null) animator.SetTrigger("Jump");
            GameAudio.Instance?.PlayJump();
        }
    }

    void TryDash(float x)
    {
        if (!RuleBook.CanDash()) return;
        if (dashLeft > 0f) return;
        float dir = Mathf.Abs(x) > 0.1f ? Mathf.Sign(x) : facing;
        dashLeft = dashSeconds;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0.2f * jumpForce);
        if (animator != null) animator.SetTrigger("Roll");
        GameAudio.Instance?.PlayDash();
    }

    void TryAttack()
    {
        if (dashLeft > 0f) return;
        if (timeSinceAttack < 0.22f) return;
        if (timeSinceAttack > 1f) currentAttack = 0;
        currentAttack++;
        if (currentAttack > 3) currentAttack = 1;
        timeSinceAttack = 0f;
        pendingHit = 0.12f;
        if (animator != null) animator.SetTrigger("Attack" + currentAttack);
        GameAudio.Instance?.PlaySlice();
    }

    void DealAttackHit()
    {
        DemonPillar best = null;
        float bestDist = 2.6f;
        var pillars = FindObjectsByType<DemonPillar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < pillars.Length; i++)
        {
            var pillar = pillars[i];
            if (pillar == null || !pillar.gameObject.activeInHierarchy) continue;
            Vector2 to = (Vector2)pillar.transform.position - (Vector2)transform.position;
            float dx = Mathf.Abs(to.x);
            float dy = Mathf.Abs(to.y);
            if (dx < bestDist && dy < 4f)
            {
                bestDist = dx;
                best = pillar;
            }
        }

        if (best != null)
            best.TakeHit(1);
    }
}
