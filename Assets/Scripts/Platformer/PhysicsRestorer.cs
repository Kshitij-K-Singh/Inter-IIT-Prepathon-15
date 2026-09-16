using UnityEngine;

public class PhysicsRestorer : MonoBehaviour
{
    Vector3 localPos;
    Quaternion localRot;
    bool captured;

    void Awake()
    {
        Capture();
    }

    public void Capture()
    {
        localPos = transform.localPosition;
        localRot = transform.localRotation;
        captured = true;
    }

    public void Restore()
    {
        if (!captured) Capture();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        transform.localPosition = localPos;
        transform.localRotation = localRot;

        if (rb != null)
        {
            rb.position = transform.position;
            rb.rotation = transform.eulerAngles.z;
            rb.WakeUp();
            rb.simulated = true;
        }
    }
}
