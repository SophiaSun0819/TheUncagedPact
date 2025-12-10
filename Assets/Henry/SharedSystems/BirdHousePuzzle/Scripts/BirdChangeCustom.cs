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

    // assigned by spawner (optional, no longer required for trigger check)
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

    // allow other scripts (BirdPickUp) to check if bird is fully colored
    public bool IsFullyColored => hasFaded;

    // ------------------------------------------------------------------
    // INIT — called by spawn button
    // ------------------------------------------------------------------
    public void Init(ShaderWaterLevelController controller)
    {
        waterLevelController = controller;

        if (controller != null)
        {
            controller.onWaterBottleComplete.AddListener(OnWaterComplete);

            // If water was already done before bird spawned
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
        Debug.Log("[BirdChangeCustom] Water complete → bird can fade now.");
    }

    // ------------------------------------------------------------------
    // TRIGGER → bird touching bottle lip
    // ------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // ✅ Use TAG, like the old working version
        if (!other.CompareTag(changeColorTag))
            return;

        Debug.Log("[BirdChangeCustom] Entered ChangeColorTrigger.");

        // If water not ready yet → play 'too low' VO once and bail
        if (!canFade)
        {
            if (!playedTooLowOnce && waterLevelController != null)
            {
                Debug.Log("[BirdChangeCustom] Water too low → playing hint VO.");
                waterLevelController.TryPlayTooLowVO();
                playedTooLowOnce = true;
            }
            return;
        }

        // Water is ready but bird has already faded once
        if (hasFaded)
            return;

        hasFaded = true;
        StartCoroutine(FadeToFullColor());
    }

    // ------------------------------------------------------------------
    // FADING ROUTINE → Transition from faded → fully opaque,
    // then play "after water" VO once.
    // ------------------------------------------------------------------
    private IEnumerator FadeToFullColor()
    {
        Debug.Log("[BirdChangeCustom] Starting fade to full color.");

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

                bool stillTransparent = c.a < 1f;
                SetMaterialToTransparent(info.mat, stillTransparent);
            }

            yield return null;
        }

        // ensure final colors + opaque mode
        foreach (var info in mats)
        {
            if (info.mat.HasProperty(BaseColorID))
                info.mat.SetColor(BaseColorID, info.originalColor);
            else if (info.mat.HasProperty(ColorID))
                info.mat.SetColor(ColorID, info.originalColor);

            SetMaterialToTransparent(info.mat, false);
        }

        Debug.Log("[BirdChangeCustom] Fade complete.");

        // 🔊 AFTER-WATER VO: "It looks better now… maybe I can repair its home."
        if (SoundPuzzleVOController.Instance != null)
        {
            SoundPuzzleVOController.Instance.CueAfterWaterHomeHint();
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
