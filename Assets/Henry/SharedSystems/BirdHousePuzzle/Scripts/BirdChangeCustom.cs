using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdChangeCustom : MonoBehaviour
{
    [Header("Fading Settings")]
    [Range(0f, 1f)]
    public float startAlpha = 0.1f;         // transparency at spawn
    public float fadeDuration = 0.5f;       // fade time

    [Header("Optional: assign bird renderers manually")]
    public Renderer[] renderers;

    [Header("Trigger Tag for Bottle Lid")]
    public string changeColorTag = "ChangeColorTrigger";

    // internal state
    private bool canFade = false;
    private bool hasFaded = false;

    private struct MatInfo
    {
        public Material mat;
        public Color fadedColor;
        public Color originalColor;
    }

    private List<MatInfo> mats = new List<MatInfo>();

    // common shader properties
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID     = Shader.PropertyToID("_Color");

    // ------------------------------------------
    // INIT — called by the spawner after Instantiate()
    // ------------------------------------------
    private ShaderWaterLevelController waterController;

    public void Init(ShaderWaterLevelController controller)
    {
        waterController = controller;

        if (waterController != null)
        {
            waterController.onWaterBottleComplete.AddListener(OnWaterComplete);
        }
        else
        {
            Debug.LogWarning("[BirdChangeColor] Init() was given a null controller!");
        }
    }

    // ------------------------------------------
    void Awake()
    {
        // Auto-grab all renderers in children (covers 3-part bird)
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }
    }

    void Start()
    {
        SetupFadedMaterials();
    }

    private void OnDestroy()
    {
        if (waterController != null)
        {
            waterController.onWaterBottleComplete.RemoveListener(OnWaterComplete);
        }
    }

    // ------------------------------------------
    // SETUP — prepares all materials for fading
    // ------------------------------------------
    private void SetupFadedMaterials()
    {
        mats.Clear();

        foreach (var r in renderers)
        {
            if (!r) continue;

            foreach (var m in r.materials)
            {
                if (!m) continue;

                // Determine which color property to use
                Color original;

                if (m.HasProperty(BaseColorID))
                    original = m.GetColor(BaseColorID);
                else if (m.HasProperty(ColorID))
                    original = m.GetColor(ColorID);
                else
                    continue; // no color to fade

                // Set faded alpha
                Color faded = original;
                faded.a = startAlpha;

                // Apply faded color to material
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

    // ------------------------------------------
    // WATER EVENT — called when bottle is full
    // ------------------------------------------
    private void OnWaterComplete()
    {
        canFade = true;
    }

    // ------------------------------------------
    // TRIGGER — start fade when hitting the lid
    // ------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!canFade || hasFaded) return;

        if (other.CompareTag(changeColorTag))
        {
            hasFaded = true;
            StartCoroutine(FadeToFullColor());
        }
    }

    // ------------------------------------------
    // FADING ROUTINE
    // ------------------------------------------
    private IEnumerator FadeToFullColor()
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
                else if (info.mat.HasProperty(ColorID))
                    info.mat.SetColor(ColorID, c);
            }

            yield return null;
        }

        // guarantee final exact color
        foreach (var info in mats)
        {
            if (info.mat.HasProperty(BaseColorID))
                info.mat.SetColor(BaseColorID, info.originalColor);
            else if (info.mat.HasProperty(ColorID))
                info.mat.SetColor(ColorID, info.originalColor);
        }
    }
}
