using UnityEngine;

[ExecuteAlways]
public class RadialBlurController : MonoBehaviour
{
    public Material blurMat;
    [Range(0f, 1f)] public float focusRadius = 0.2f;
    [Range(0f, 2f)] public float blurStrength = 0.5f;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (blurMat == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        // Tâm focus luôn ở giữa camera (trung tâm màn hình)
        blurMat.SetVector("_FocusPos", new Vector4(0.5f, 0.5f, 0, 0));
        blurMat.SetFloat("_FocusRadius", focusRadius);
        blurMat.SetFloat("_BlurStrength", blurStrength);

        Graphics.Blit(src, dest, blurMat);
    }
}
