using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target theo dõi")]
    public PlayerController player;
    public float PosZ = -10f;

    [Header("Hiệu ứng camera")]
    public float moveOffsetX = -4.5f;
    public float smoothSpeed = 5f;

    private float currentOffsetX = 0f;

    void LateUpdate()
    {
        if (player.OpenPackBack)
        {
            currentOffsetX = Mathf.Lerp(currentOffsetX, moveOffsetX, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentOffsetX = Mathf.Lerp(currentOffsetX, 0f, Time.deltaTime * smoothSpeed);
        }

        transform.position = new Vector3(player.transform.position.x + currentOffsetX, player.transform.position.y, PosZ);

    }
}
