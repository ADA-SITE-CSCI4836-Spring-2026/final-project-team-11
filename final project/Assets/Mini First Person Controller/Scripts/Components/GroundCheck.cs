using UnityEngine;

[DefaultExecutionOrder(-20)]
public class GroundCheck : MonoBehaviour
{
    [Tooltip("How far below the feet we look for ground.")]
    public float distanceThreshold = 0.15f;

    [Tooltip("Whether this transform is grounded now.")]
    public bool isGrounded;

    /// <summary>
    /// Called when the ground is touched again.
    /// </summary>
    public event System.Action Grounded;

    Rigidbody body;
    CapsuleCollider capsule;

    const float SkinPadding = 0.08f;

    void Awake()
    {
        body = GetComponentInParent<Rigidbody>();
        capsule = GetComponentInParent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        bool isGroundedNow = EvaluateGrounded();

        if (isGroundedNow && !isGrounded)
            Grounded?.Invoke();

        isGrounded = isGroundedNow;
    }

    void Start()
    {
        if (body)
            isGrounded = EvaluateGrounded();
    }

    bool EvaluateGrounded()
    {
        if (!body)
            return false;

        Vector3 foot = FeetOrigin();
        float castDistance = distanceThreshold + SkinPadding;
        int mask = Physics.DefaultRaycastLayers;

        if (Physics.Raycast(foot, Vector3.down, out RaycastHit hit, castDistance, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody == body)
                return false;
            return true;
        }

        return false;
    }

    Vector3 FeetOrigin()
    {
        if (capsule)
            return CapsuleBottomWorld(capsule) + Vector3.up * SkinPadding;

        return transform.position + Vector3.up * SkinPadding;
    }

    static Vector3 CapsuleBottomWorld(CapsuleCollider cap)
    {
        Transform t = cap.transform;
        Vector3 center = t.TransformPoint(cap.center);
        Vector3 axis = cap.direction == 0 ? t.right : cap.direction == 1 ? t.up : t.forward;
        float half = Mathf.Max(0f, cap.height * 0.5f - cap.radius);
        return center - axis * half;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        Vector3 foot = FeetOrigin();
        float castDistance = distanceThreshold + SkinPadding;
        Debug.DrawLine(foot, foot + Vector3.down * castDistance, isGrounded ? Color.green : Color.red);
    }
}
