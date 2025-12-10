using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdChangeCustom : MonoBehaviour
{
    [Header("Fading Settings")]
    [Range(0f, 1f)]
    public float startAlpha = 0.1f;        // transparency at spawn
    public float fadeDuration = 0.5f;      // fade time

    [Header("Renderer Setup (auto-filled if empty)")]
    public Renderer[] renderers;

    [Header("Trigger Tag for Bottle Lip")]
    public string changeColorTag = "ChangeColorTrigger";

    [Header("Water Logic")]
    public ShaderWaterLevelController waterLevelController;

    // assigned by spawner
    [HideInInspector] 
    public GameObject changeColorTrigger;

    // fading control
    private bool canFade = false;
    private bool hasFaded = false;

    // too-low VO limiter
    private bool playedTooLowOnce = false;

    // material cache
    private struct MatInfo
    {
        public Material mat;
        public Color fadedColor;
        public Color originalColor;
    }

    private List<MatInfo> mats = new List<MatInfo>();

    // shader color properties
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID     = Shader.PropertyToID("_Color");

    // ------------------------------------------------------------------
    // INIT — called by spawn button
    // ------------------------------------------------------------------
    public void Init(ShaderWaterLevelController controller)
    {
        waterLevelController = controller;

        if (controller != null)
        {
            controller.onWaterBottleComplete.AddListener(OnWaterComplete);

            // water already complete when bird spawns
            if (controller.waterBottleComplete)
                OnWaterComplete();
        }
        else
        {
            Debug.LogWarning("[BirdChangeCustom] Init() called with NULL water controller.");
        }
    }

    private void Awake()
    {
        // Auto-fill renderers
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }
    }

    private void Start()
    {
        SetupFadedMaterials();
    }

    private void OnDestroy()
    {
        if (waterLevelController != null)
        {
            waterLevelController.onWaterBottleComplete.RemoveListener(OnWaterComplete);
        }
    }

    // ------------------------------------------------------------------
    // PREPARE MATERIALS (set bird faded at start)
    // ------------------------------------------------------------------
    private void SetupFadedMaterials()
    {
        mats.Clear();

        foreach (var r in renderers)
        {
            if (!r) continue;

            foreach (var m in r.materials)   // uses duplicated instance; safe to edit
            {
                if (!m) continue;

                Color original;

                if (m.HasProperty(BaseColorID))
                    original = m.GetColor(BaseColorID);
                else if (m.HasProperty(ColorID))
                    original = m.GetColor(ColorID);
                else
                    continue;

                // faded version of the color
                Color faded = original;
                faded.a = startAlpha;

                // apply faded material
                if (m.HasProperty(BaseColorID))
                    m.SetColor(BaseColorID, faded);
                else
                    m.SetColor(ColorID, faded);

                // force transparent mode
                SetMaterialToTransparent(m, true);

                mats.Add(new MatInfo
                {
                    mat = m,
                    fadedColor = faded,
                    originalColor = original
                });
            }
        }
    }

    // ------------------------------------------------------------------
    // WATER LEVEL FINISHED → allow fade
    // ------------------------------------------------------------------
    private void OnWaterComplete()
    {
        canFade = true;
        playedTooLowOnce = false;
    }

    // ------------------------------------------------------------------
    // TRIGGER → Fade when bird touches pitcher rim
    // ------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (changeColorTrigger != null && other.gameObject == changeColorTrigger)
        {
            // too-low VO logic
            if (!playedTooLowOnce && waterLevelController != null && !canFade)
            {
                waterLevelController.TryPlayTooLowVO();
                playedTooLowOnce = true;
            }

            // cannot fade until water system completes
            if (!canFade || hasFaded)
                return;

            hasFaded = true;
            StartCoroutine(FadeToFullColor());
        }
    }

    // ------------------------------------------------------------------
    // FADING ROUTINE → Transition from faded → fully opaque
    // ------------------------------------------------------------------
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

                // adjust transparency as alpha changes
                bool stillTransparent = c.a < 1f;
                SetMaterialToTransparent(info.mat, stillTransparent);
            }

            yield return null;
        }

        // ensure final colors + full opacity set
        foreach (var info in mats)
        {
            if (info.mat.HasProperty(BaseColorID))
                info.mat.SetColor(BaseColorID, info.originalColor);
            else
                info.mat.SetColor(ColorID, info.originalColor);

            // set opaque mode
            SetMaterialToTransparent(info.mat, false);
        }
    }

    // ------------------------------------------------------------------
    // Switch URP Lit material between transparent and opaque
    // ------------------------------------------------------------------
    private void SetMaterialToTransparent(Material mat, bool transparent)
    {
        if (transparent)
        {
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            mat.SetFloat("_Surface", 0); // Opaque
            mat.SetFloat("_ZWrite", 1);
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
}
