using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float stopDistance = 1.5f;
    public float rotationSpeed = 5f;
    public float eyeHeight = 1.5f;

    private Animator anim;

    [Header("Game Over")]
    public GameTimer gameTimer;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        bool canSee = CanSeePlayer();
        bool isMoving = false;

        if (canSee)
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;

            // Поворот к игроку
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.fixedDeltaTime
                );
            }

            float dist = Vector3.Distance(transform.position, player.position);

            // Движение
            if (dist > stopDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.fixedDeltaTime
                );

                isMoving = true;
            }
        }

        // Анимация
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            Debug.Log("ENEMY HIT → GAME OVER");

            if (gameTimer != null)
            {
                gameTimer.SendMessage("TriggerGameOver", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f;

        Vector3 dir = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, distance))
        {
            if (hit.transform.root.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}