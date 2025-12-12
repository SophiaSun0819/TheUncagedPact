using UnityEngine;
using PassthroughCameraSamples;
using TMPro;
using UnityEngine.Events;

public class passthroughCropCamera : MonoBehaviour
{
    public float cropPercent;
    public WebCamTextureManager webcamManager;
    public Renderer quadRenderer;
    public float quadDistance = 1;

    [Header("UI 切換設定")]
    [Tooltip("UI 在攝影機後方的距離（負數）")]
    public float backDistance = -2f;

    [Tooltip("按此鍵切換 UI 位置")]
    public OVRInput.Button toggleUIButton = OVRInput.Button.Four; // Y 鍵

    private Texture2D picture;
    private RenderTexture webcamRenderTexture;
    public DigitRecognition digitRecognition;
    public TextMeshPro tmp;
    public UnityEvent<int> setPswDigit;

    [Header("調試")]
    public bool debugMode = true;

    // 新增：控制 UI 是否在前方
    private bool isUIInFront = true;

    void Update()
    {
        if (!webcamManager.WebCamTexture)
        {
            return;
        }

        // 新增：Y 鍵切換 UI 位置
        if (OVRInput.GetDown(toggleUIButton))
        {
            ToggleUIPosition();
        }

        PlaceQuad();
        TakePicture();
        int result = digitRecognition.RunAI(picture);
        tmp.text = "prediction: " + result;

        if (OVRInput.GetDown(OVRInput.Button.One)) // A 鍵
        {
            setPswDigit?.Invoke(result);

            if (debugMode)
            {
                Debug.Log($"[passthroughCropCamera] 識別結果: {result}");
            }
        }
    }

    /// <summary>
    /// 切換 UI 位置（前方 <-> 後方）
    /// </summary>
    private void ToggleUIPosition()
    {
        isUIInFront = !isUIInFront;

        if (debugMode)
        {
            Debug.Log($"[passthroughCropCamera] UI 切換到攝影機{(isUIInFront ? "前方" : "後方")}");
        }
    }

    public void TakePicture()
    {
        int sourceWidth = webcamManager.WebCamTexture.width;
        int sourceHeight = webcamManager.WebCamTexture.height;

        int cropWidth = (int)(sourceWidth * cropPercent);

        int startX = (sourceWidth - cropWidth) / 2;
        int startY = (sourceHeight - cropWidth) / 2;

        if (webcamRenderTexture == null)
        {
            webcamRenderTexture = new RenderTexture(sourceWidth, sourceHeight, 0);
        }

        Graphics.Blit(webcamManager.WebCamTexture, webcamRenderTexture);

        if (picture == null || picture.width != cropWidth || picture.height != cropWidth)
        {
            picture = new Texture2D(cropWidth, cropWidth, TextureFormat.RGBA32, false);
        }

        RenderTexture.active = webcamRenderTexture;
        picture.ReadPixels(new Rect(startX, sourceHeight - startY - cropWidth, cropWidth, cropWidth), 0, 0);
        picture.Apply();
    }

    public void PlaceQuad()
    {
        Transform quadTransform = quadRenderer.transform;

        Pose cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
        Vector2Int resolution = PassthroughCameraUtils.GetCameraIntrinsics(PassthroughCameraEye.Left).Resolution;

        int width = (int)(resolution.x * cropPercent);

        // 修改：根據 isUIInFront 決定距離
        float currentDistance = isUIInFront ? quadDistance : backDistance;

        quadTransform.position = cameraPose.position + cameraPose.forward * currentDistance;
        quadTransform.rotation = cameraPose.rotation;

        Ray leftSide = PassthroughCameraUtils.ScreenPointToRayInCamera(
            PassthroughCameraEye.Left,
            new Vector2Int((resolution.x - width) / 2, resolution.y / 2)
        );
        Ray rightSide = PassthroughCameraUtils.ScreenPointToRayInCamera(
            PassthroughCameraEye.Left,
            new Vector2Int((resolution.x + width) / 2, resolution.y / 2)
        );

        float horizontalFov = Vector3.Angle(leftSide.direction, rightSide.direction);
        float quadScale = 2 * Mathf.Abs(currentDistance) * Mathf.Tan(horizontalFov * Mathf.Deg2Rad / 2);

        quadTransform.localScale = new Vector3(quadScale, quadScale, 1);
    }

    /// <summary>
    /// 公開方法：將 UI 移到前方
    /// </summary>
    public void MoveUIToFront()
    {
        if (!isUIInFront)
        {
            isUIInFront = true;

            if (debugMode)
            {
                Debug.Log("[passthroughCropCamera] UI 強制移到前方");
            }
        }
    }

    /// <summary>
    /// 公開方法：將 UI 移到後方
    /// </summary>
    public void MoveUIToBack()
    {
        if (isUIInFront)
        {
            isUIInFront = false;

            if (debugMode)
            {
                Debug.Log("[passthroughCropCamera] UI 強制移到後方");
            }
        }
    }
}