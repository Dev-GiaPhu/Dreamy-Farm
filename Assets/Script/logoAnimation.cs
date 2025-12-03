using UnityEngine;

public class logoAnimator : MonoBehaviour
{
    // Tốc độ di chuyển và xoay
    [Tooltip("Tốc độ chuyển động dao động và xoay.")]
    public float animationSpeed = 1f;

    // Khoảng cách tối đa di chuyển lên/xuống (theo trục Y)
    [Tooltip("Khoảng cách tối đa di chuyển lên/xuống (theo trục Y).")]
    public float moveRangeY = 0.05f;

    // Góc xoay tối đa theo trục Z
    [Tooltip("Góc xoay tối đa theo trục Z.")]
    public float rotateRangeZ = 1f;

    // Vị trí và góc quay ban đầu
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // Lưu lại vị trí và góc quay ban đầu của logo
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // Sử dụng Time.time để tạo ra giá trị dao động dựa trên hàm Sin
        float time = Time.time * animationSpeed;

        // 1. Chuyển động dao động theo trục Y
        // Hàm Sin(time) sẽ tạo ra giá trị từ -1 đến 1.
        float offsetY = Mathf.Sin(time) * moveRangeY;
        transform.localPosition = initialPosition + new Vector3(0f, offsetY, 0f);

        // 2. Chuyển động xoay nhẹ theo trục Z
        // Sử dụng một hàm Sin khác (hoặc cos) để xoay
        float rotationZ = Mathf.Sin(time * 0.5f) * rotateRangeZ;
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, rotationZ);
    }
}