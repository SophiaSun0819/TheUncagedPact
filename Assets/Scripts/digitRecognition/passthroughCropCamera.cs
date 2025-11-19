using UnityEngine;
using PassthroughCameraSamples;
using TMPro;
using UnityEngine.Events;
public class passthroughCropCamera : MonoBehaviour
{

    public float cropPercent;
    public WebCamTextureManager webcamManager;
    public Renderer quadRenderer;
    // public Renderer quadRenderer2; //show what picture be taken
    public float quadDistance = 1;
    private Texture2D picture;
    private RenderTexture webcamRenderTexture;
    public DigitRecognition digitRecognition;
    public TextMeshPro tmp; // for webcam recognition debug

     public UnityEvent<int> setPswDigit; //set one digit psw
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!webcamManager.WebCamTexture)
        {
            return;
        }
        PlaceQuad();
        TakePicture();
        int result = digitRecognition.RunAI(picture);
        tmp.text = "prediction: " + result; // get result digit
        if (OVRInput.GetDown(OVRInput.Button.One)) //when press A ,set one digit
        {
            setPswDigit?.Invoke(result);
        }
    }
    public void TakePicture()
    {
        int sourceWidth = webcamManager.WebCamTexture.width;
        int sourceHeight = webcamManager.WebCamTexture.height;

        int cropWidth = (int)(sourceWidth * cropPercent);

        // Calculate crop position (centered)
        int startX = (sourceWidth - cropWidth) / 2;
        int startY = (sourceHeight - cropWidth) / 2;

        // Update the RenderTexture to match the webcam feed
        if (webcamRenderTexture == null)
        {
            webcamRenderTexture = new RenderTexture(sourceWidth, sourceHeight, 0);
        }

        // Blit (copy) webcam texture into the RenderTexture
        Graphics.Blit(webcamManager.WebCamTexture, webcamRenderTexture);

        // Create texture for the picture if needed
        if (picture == null || picture.width != cropWidth || picture.height != cropWidth)
        {
            picture = new Texture2D(cropWidth, cropWidth, TextureFormat.RGBA32, false);
        }

        // Read the pixels from the RenderTexture
        RenderTexture.active = webcamRenderTexture;

        // Note: Y axis is flipped in ReadPixels
        picture.ReadPixels(new Rect(startX, sourceHeight - startY - cropWidth, cropWidth, cropWidth), 0, 0);
        picture.Apply();
        // quadRenderer2.material.mainTexture = picture;
    }
public void PlaceQuad()
{
    Transform quadTransform = quadRenderer.transform;

    Pose cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
    Vector2Int resolution = PassthroughCameraUtils.GetCameraIntrinsics(PassthroughCameraEye.Left).Resolution;

    int width = (int)(resolution.x * cropPercent);

    quadTransform.position = cameraPose.position + cameraPose.forward * quadDistance;
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
    float quadScale = 2 * quadDistance * Mathf.Tan(horizontalFov * Mathf.Deg2Rad / 2);

    quadTransform.localScale = new Vector3(quadScale, quadScale, 1);
}


}
