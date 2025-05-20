using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    private float smoothTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    
    [Header("Advanced Settings")]
    private float lookAheadFactor = 0.5f;
    private float lookAheadReturnSpeed = 0.5f;
    private float lookAheadMoveThreshold = 0.1f;
    private float verticalSmoothTime = 0.1f;
    
    private Transform target;
    private Vector3 lastTargetPosition;
    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirX;
    private bool lookAheadStopped;
    private float verticalSmoothVelocity;
    private float lastTargetY;

    private void Start()
    {
        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            lastTargetPosition = target.position;
            lastTargetY = target.position.y;
        }
        else
        {
            Debug.LogWarning("CameraFollow: Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Eğer target yoksa tekrar bulmayı dene
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                lastTargetPosition = target.position;
                lastTargetY = target.position.y;
            }
            return;
        }

        // Yatay hareket ve look ahead hesaplaması
        float xMoveDelta = (target.position - lastTargetPosition).x;
        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget)
        {
            lookAheadDirX = Mathf.Sign(xMoveDelta);
            lookAheadStopped = false;
        }
        else
        {
            if (!lookAheadStopped)
            {
                lookAheadStopped = true;
                targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadFactor);
            }
        }

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref lookAheadDirX, lookAheadReturnSpeed);

        // Dikey hareket için yumuşak geçiş
        float targetY = target.position.y;
        float smoothedY = Mathf.SmoothDamp(lastTargetY, targetY, ref verticalSmoothVelocity, verticalSmoothTime);

        // Hedef pozisyonu hesapla
        Vector3 targetPosition = new Vector3(
            target.position.x + currentLookAheadX,
            smoothedY,
            target.position.z
        ) + offset;

        // Kamera pozisyonunu yumuşak bir şekilde güncelle
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Son pozisyonları kaydet
        lastTargetPosition = target.position;
        lastTargetY = target.position.y;
    }
    
} 