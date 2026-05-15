using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // 1. เพิ่ม Namespace ของ New Input System

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))] // บังคับว่าต้องมี Animator แปะอยู่ด้วย
public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference dashAction;

    [Header("Movement")]
    public float moveSpeed = 8f;
    private float horizontalInput;
    private bool isFacingRight = true;

    [Header("Jump")]
    public float jumpForce = 16f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Dash")]
    public float dashSpeed = 24f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing;
    private bool canDash = true;

    // Components
    private Rigidbody2D rb;
    private Animator anim; // 2. ประกาศตัวแปร Animator

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // ดึง Component มาใช้งาน
    }

    // 3. กฎของ New Input System (แบบ Reference) ต้องสั่ง Enable/Disable ด้วย
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();
    }

    void Update()
    {
        if (isDashing) return;

        // --- ระบบเคลื่อนที่แบบใหม่ ---
        // อ่านค่า Move เป็น Vector2 แล้วดึงเฉพาะแกน X
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        horizontalInput = moveInput.x;

        // 4. ส่งค่าความเร็วไปให้ Animator เช็ค (ใช้ Mathf.Abs เพื่อแปลงค่าลบเวลาเดินซ้ายให้เป็นบวกเสมอ)
        anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // --- ระบบกระโดดแบบใหม่ ---
        // WasPressedThisFrame() ทำหน้าที่เหมือน GetButtonDown() ในระบบเก่า
        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // --- ระบบ Dash แบบใหม่ ---
        if (dashAction.action.WasPressedThisFrame() && canDash)
        {
            StartCoroutine(Dash());
        }

        FlipCheck();
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void FlipCheck()
    {
        if (isFacingRight && horizontalInput < 0f || !isFacingRight && horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}