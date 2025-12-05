using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdChangeCustom : MonoBehaviour
{
    [Header("Fading Settings")]
    [Range(0f, 1f)]
    public float startAlpha = 0.1f;
    public float fadeDuration = 0.5f;

    [Header("Optional Renderers")]
    public Renderer[] renderers;

    [Header("Trigger Tag")]
    public string changeColorTag = "ChangeColorTrigger";

    bool canFade = false;
    bool hasFaded = false;

    struct MatInfo
    {
        public Material mat;
        public Color fadedColor;
        public Color originalColor;
    }

    List<MatInfo> mats = new List<MatInfo>();
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int ColorID     = Shader.PropertyToID("_Color");

    ShaderWaterLevelController waterController;

    // Called by spawner
    public void Init(ShaderWaterLevelController controller)
    {
        waterController = controller;

        if (controller != null)
        {
            controller.onWaterBottleComplete.AddListener(OnWaterComplete);

            // If water puzzle was already complete before bird spawned:
            if (controller.IsComplete)
                OnWaterComplete();
        }
    }

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        SetupFadedMaterials();
    }

    void OnDestroy()
    {
        if (waterController != null)
            waterController.onWaterBottleComplete.RemoveListener(OnWaterComplete);
    }

    void SetupFadedMaterials()
    {
        mats.Clear();

        foreach (var r in renderers)
        {
            if (!r) continue;

            foreach (var m in r.materials)
            {
                if (!m) continue;

                Color original;

                if (m.HasProperty(BaseColorID))
                    original = m.GetColor(BaseColorID);
                else if (m.HasProperty(ColorID))
                    original = m.GetColor(ColorID);
                else
                    continue;

                Color faded = original;
                faded.a = startAlpha;

                if (m.HasProperty(BaseColorID))
                    m.SetColor(BaseColorID, faded);
                else
                    m.SetColor(ColorID, faded);

                mats.Add(new MatInfo
                {
                    mat = m,
                    fadedColor = faded,
                    originalColor = original
                });
            }
        }
    }

    void OnWaterComplete()
    {
        canFade = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!canFade || hasFaded) return;
        if (!other.CompareTag(changeColorTag)) return;

        hasFaded = true;
        StartCoroutine(FadeToFullColor());
    }

    IEnumerator FadeToFullColor()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            foreach (var info in mats)
            {
                Color c = Color.Lerp(info.fadedColor, info.originalColor, lerp);

                if (info.mat.HasProperty(BaseColorID))
                    info.mat.SetColor(BaseColorID, c);
                else
                    info.mat.SetColor(ColorID, c);
            }

            yield return null;
        }

        foreach (var info in mats)
        {
            if (info.mat.HasProperty(BaseColorID))
                info.mat.SetColor(BaseColorID, info.originalColor);
            else
                info.mat.SetColor(ColorID, info.originalColor);
        }
    }
}
