using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Cần thiết cho Image component

public class AnimationItemCollect : MonoBehaviour
{
    // Các biến cũ
    public Canvas Canvas;
    public Vector3 sizeStart;
    public Vector3 sizeEnd;
    public float lifetime = 5f;
    
    // *** BIẾN MỚI: Object UI để hiển thị Sprite đã truyền vào ***
    [Header("Target UI")]
    [Tooltip("Image component trên UI object khác sẽ hiển thị Sprite")]
    public Image targetImageUI; 
    
    // ... (Các biến kiểm soát hiệu ứng khác giữ nguyên)
    [Header("Animation Settings")]
    public float durationGrow = 0.2f; 
    // ... (các biến khác) ...
    public float durationShrink = 0.2f; 
    public float overshootScale = 1.1f;
    private Vector3 sizeOvershoot;
    public static AnimationItemCollect Instance;

    void Awake()
    {
        Instance = this;
        Canvas = GetComponent<Canvas>();
        Canvas.enabled = false;
        sizeOvershoot = sizeEnd * overshootScale; 
    }
    
    // ********** HÀM GỌI HIỆU ỨNG VỚI THAM SỐ SPRITE **********
    public void TriggerAnimation(Sprite displaySprite)
    {
        // 1. Dừng Coroutine cũ và bắt đầu Coroutine mới
        StopAllCoroutines(); 
        StartCoroutine(AnimateCollect(displaySprite));
    }

    private IEnumerator AnimateCollect(Sprite spriteToDisplay)
    {
        // 2. GÁN SPRITE CHO TARGET IMAGE TRƯỚC KHI BẮT ĐẦU HIỆU ỨNG
        if (targetImageUI != null && spriteToDisplay != null)
        {
            targetImageUI.sprite = spriteToDisplay;
            // Tùy chọn: Đảm bảo Image component được bật
            targetImageUI.enabled = true; 
        }

        // --- Bắt đầu chuỗi animation cho Canvas (Giữ nguyên) ---
        Canvas.enabled = true;
        Canvas.transform.localScale = sizeStart;

        // ... (Các giai đoạn hiệu ứng giữ nguyên: PHÓNG TO -> ỔN ĐỊNH -> DỪNG LẠI) ...
        yield return StartCoroutine(ScaleObject(sizeStart, sizeOvershoot, durationGrow));
        yield return StartCoroutine(ScaleObject(sizeOvershoot, sizeEnd, durationGrow * 0.5f));
        yield return new WaitForSeconds(lifetime);

        // --- Giai đoạn 4: THU NHỎ ---
        yield return StartCoroutine(ScaleObject(sizeEnd, sizeStart, durationShrink));

        // --- KẾT THÚC ---
        Canvas.enabled = false;
        
        // Tắt Sprite trên targetImageUI khi animation kết thúc
        if (targetImageUI != null)
        {
            targetImageUI.enabled = false; 
            targetImageUI.sprite = null;
        }
    }

    private IEnumerator ScaleObject(Vector3 startScale, Vector3 endScale, float duration)
    {
        // ... (Hàm này giữ nguyên) ...
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTime;
            float t = elapsedTime / duration;
            Canvas.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        Canvas.transform.localScale = endScale;
    }
}