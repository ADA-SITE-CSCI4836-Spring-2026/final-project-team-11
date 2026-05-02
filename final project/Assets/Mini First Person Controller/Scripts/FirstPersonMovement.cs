using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5f;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9f;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Ground")]
    [SerializeField] GroundCheck groundCheck;

    [Range(0f, 1f)]
    public float airControl = 0.35f;

    Rigidbody rb;
    Jump jump;

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        jump = GetComponent<Jump>();

        if (!groundCheck)
            groundCheck = GetComponentInChildren<GroundCheck>();

        // 🔥 ВАЖНО
        rb.freezeRotation = true; // убирает кручение
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        IsRunning = canRun && Input.GetKey(runningKey);

        float currentSpeed = IsRunning ? runSpeed : speed;

        if (speedOverrides.Count > 0)
            currentSpeed = speedOverrides[speedOverrides.Count - 1]();

        bool grounded = groundCheck && groundCheck.isGrounded;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.forward * v + transform.right * h).normalized;

        // 🔥 ГЛАВНЫЙ ФИКС
        Vector3 velocity = rb.velocity;

        float control = grounded ? 1f : airControl;

        velocity.x = move.x * currentSpeed * control;
        velocity.z = move.z * currentSpeed * control;

        // ❌ УБРАЛИ РУЧНОЙ Y
        // Unity сама управляет гравитацией

        rb.velocity = velocity;
    }
}