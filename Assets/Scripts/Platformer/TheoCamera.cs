using UnityEngine;

public class TheoCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float damp = 8f;
    [SerializeField] Vector3 offset = new Vector3(2.5f, 1.2f, -10f);
    [SerializeField] float minX = -1f;
    [SerializeField] float maxX = 40f;
    [SerializeField] float minY = -1f;
    [SerializeField] float maxY = 10f;

    public void Bind(Transform follow)
    {
        target = follow;
    }

    public void ClampTo(float xMin, float xMax, float yMin, float yMax)
    {
        minX = xMin;
        maxX = xMax;
        minY = yMin;
        maxY = yMax;
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
        var goal = target.position + offset;
        goal.x = Mathf.Clamp(goal.x, minX, maxX);
        goal.y = Mathf.Clamp(goal.y, minY, maxY);
        goal.z = -10f;
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-damp * Time.deltaTime));
    }
}
