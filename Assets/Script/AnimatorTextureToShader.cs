using UnityEngine;
using UnityEngine.UI;

public class AnimatorTextureToShader : MonoBehaviour
{
    private const string TexturePropertyName = "_MainTex"; // Phải khớp với tên trong Shader
    private Image uiImage;
    private Material material;

    void Start()
    {
        uiImage = GetComponent<Image>();
        if (uiImage == null) return;
        
        // Lấy Material đang dùng. 
        // **QUAN TRỌNG:** Đây phải là Material đang dùng Shader của bạn.
        material = uiImage.material; 
        if (material == null) return;
    }

    void Update()
    {
        // 1. Kiểm tra nếu có Sprite và Material
        Sprite currentSprite = uiImage.sprite;
        if (material != null && currentSprite != null)
        {
            // 2. Lấy Texture2D từ Sprite hiện tại
            Texture2D currentTexture = currentSprite.texture;

            // 3. **BUỘC** Material cập nhật Texture
            material.SetTexture(TexturePropertyName, currentTexture);
            
            // **QUAN TRỌNG:** Nếu Sprite không phải là full-size của Texture
            // (ví dụ: là một phần trong Sprite Sheet), bạn phải truyền thêm 
            // thông tin UV/Offset để Shader biết lấy phần nào. 
            // Nếu là Sprite full-size thì bước này có thể bỏ qua.
        }
    }
}