using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float stopDistance = 1.5f;
    public float rotationSpeed = 5f;
    public float eyeHeight = 1.5f;

    [Header("Patrol (A ↔ B)")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentTarget;

    private Animator anim;

    [Header("Game Over")]
    public GameTimer gameTimer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        currentTarget = pointA;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        bool canSee = CanSeePlayer();
        bool isMoving = false;

        if (canSee)
        {
            // 🔴 ПРЕСЛЕДОВАНИЕ
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;

            Rotate(dir);

            float dist = Vector3.Distance(transform.position, player.position);

            if (dist > stopDistance)
            {
                Move(dir.normalized);
                isMoving = true;
            }
        }
        else
        {
            // 🟢 ПАТРУЛЬ A ↔ B
            if (currentTarget == null) return;

            Vector3 dir = (currentTarget.position - transform.position);
            dir.y = 0f;

            Rotate(dir);

            if (dir.magnitude < 0.5f)
            {
                // переключаем точку
                currentTarget = (currentTarget == pointA) ? pointB : pointA;
            }
            else
            {
                Move(dir.normalized);
                isMoving = true;
            }
        }

        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
        }
    }

    void Move(Vector3 dir)
    {
        transform.position += dir * speed * Time.fixedDeltaTime;
    }

    void Rotate(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            if (gameTimer != null)
            {
                gameTimer.SendMessage("TriggerGameOver", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    bool CanSeePlayer()
{
    if (player == null) return false;

    float viewDistance = 10f;   // 👈 насколько далеко видит
    float viewAngle = 60f;      // 👈 угол зрения

    Vector3 origin = transform.position + Vector3.up * eyeHeight;
    Vector3 target = player.position + Vector3.up * 1f;

    Vector3 dirToPlayer = target - origin;
    float distance = dirToPlayer.magnitude;

    // ❌ слишком далеко → не видит
    if (distance > viewDistance)
        return false;

    // ❌ вне угла зрения → не видит
    float angle = Vector3.Angle(transform.forward, dirToPlayer);
    if (angle > viewAngle)
        return false;

    // 🔍 проверка стен
    if (Physics.Raycast(origin, dirToPlayer.normalized, out RaycastHit hit, distance))
    {
        if (hit.transform.root.CompareTag("Player"))
        {
            return true;
        }
    }

    return false;
}
}