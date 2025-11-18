using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target theo dõi")]
    public PlayerController player; // Gán Player có script PlayerController
    public float PosZ = -10f;

    [Header("Hiệu ứng camera")]
    public float moveOffsetX = -4.5f;   // dịch sang trái khi mở balo
    public float smoothSpeed = 5f;      // tốc độ mượt

    private float currentOffsetX = 0f;

    void LateUpdate()
    {
        if (player.Backpack)
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
