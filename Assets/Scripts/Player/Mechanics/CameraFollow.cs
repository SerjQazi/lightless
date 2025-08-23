using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float minXPos;
    [SerializeField] private float maxXPos;
    [SerializeField] private float minYPos;
    [SerializeField] private float maxYPos;

    [SerializeField] private Transform target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!target)
            Debug.LogError("Target not set for CameraFollow script. Please assign a target in the inspector.");
    }

    // Update is called once per frame
    void Update()
    {
        if (!target) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(target.position.x, minXPos, maxXPos);
        pos.y = Mathf.Clamp(target.position.y, minYPos, maxYPos);
        transform.position = pos;
    }
}