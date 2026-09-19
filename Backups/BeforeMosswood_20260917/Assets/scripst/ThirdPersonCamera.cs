using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;

    [Header("Camera Collision")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionLayers = ~0;

    [Header("Shake")]
    public float shakeDecay = 5f;

    private float rotationY;
    private float rotationX;
    private float currentShakeAmount;
    private float desiredDistance;
    private float currentDistance;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        desiredDistance = offset.magnitude;
        currentDistance = desiredDistance;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (GameUIManager.Instance != null && 
           (GameUIManager.Instance.CurrentState == GameUIManager.GameState.MainMenu || 
            GameUIManager.Instance.CurrentState == GameUIManager.GameState.GameOver))
        {
            return;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        }

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 desiredPosition = target.position + rotation * offset;

        Vector3 direction = (desiredPosition - target.position).normalized;
        float distance = offset.magnitude;
        RaycastHit hit;

        if (Physics.SphereCast(target.position + Vector3.up * 1.5f, collisionRadius, direction, out hit, distance, collisionLayers))
        {
            currentDistance = Mathf.Lerp(currentDistance, hit.distance - 0.1f, Time.deltaTime * 10f);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * 5f);
        }

        Vector3 finalOffset = offset.normalized * currentDistance;
        transform.position = target.position + rotation * finalOffset;

        transform.LookAt(target.position + Vector3.up * 1.5f);

        if (currentShakeAmount > 0)
        {
            transform.position += Random.insideUnitSphere * currentShakeAmount;
            currentShakeAmount = Mathf.Lerp(currentShakeAmount, 0, Time.deltaTime * shakeDecay);
        }
    }

    public void Shake(float amount)
    {
        currentShakeAmount = Mathf.Max(currentShakeAmount, amount);
    }
}
