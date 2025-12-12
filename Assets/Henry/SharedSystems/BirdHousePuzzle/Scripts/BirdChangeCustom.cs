using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdChangeCustom : MonoBehaviour
{
    [Header("Fading Settings")]
    [Range(0f, 1f)] public float startAlpha = 0.1f;
    public float fadeDuration = 0.5f;

    [Header("Renderer Setup (auto-filled if empty)")]
    public Renderer[] renderers;

    [Header("Trigger Tag for Bottle Lip")]
    public string changeColorTag = "ChangeColorTrigger"; // MUST match your trigger’s Tag exactly

    [Header("Water Logic")]
    public ShaderWaterLevelController waterLevelController;

    private bool canFade = false;
    private bool hasFaded = false;
    private bool playedTooLowOnce = false;

    private struct MatInfo
    {
        public Material mat;
        public Color fadedColor;
        public Color originalColor;
    }

    private readonly List<MatInfo> mats = new List<MatInfo>();

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID     = Shader.PropertyToID("_Color");

    public bool IsFullyColored => hasFaded;

    public void Init(ShaderWaterLevelController controller)
    {
        waterLevelController = controller;

        if (waterLevelController == null)
        {
            Debug.LogWarning("[BirdChangeCustom] Init() called with NULL water controller.");
            return;
        }

        // ✅ listen for READY (threshold reached)
        waterLevelController.onWaterReady.RemoveListener(OnWaterReady);
        waterLevelController.onWaterReady.AddListener(OnWaterReady);

        // (optional) still listen for FULL if you want
        waterLevelController.onWaterBottleComplete.RemoveListener(OnWaterReady);
        waterLevelController.onWaterBottleComplete.AddListener(OnWaterReady);

        // handle already-ready
        if (waterLevelController.waterReady || waterLevelController.waterBottleComplete)
            OnWaterReady();
    }

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    private void Start()
    {
        SetupFadedMaterials();
    }

    private void OnDestroy()
    {
        if (waterLevelController != null)
        {
            waterLevelController.onWaterReady.RemoveListener(OnWaterReady);
            waterLevelController.onWaterBottleComplete.RemoveListener(OnWaterReady);
        }
    }

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
                if (m.HasProperty(BaseColorID)) original = m.GetColor(BaseColorID);
                else if (m.HasProperty(ColorID)) original = m.GetColor(ColorID);
                else continue;

                Color faded = original;
                faded.a = startAlpha;

                if (m.HasProperty(BaseColorID)) m.SetColor(BaseColorID, faded);
                else m.SetColor(ColorID, faded);

                SetMaterialToTransparent(m, true);

                mats.Add(new MatInfo { mat = m, fadedColor = faded, originalColor = original });
            }
        }
    }

    private void OnWaterReady()
    {
        canFade = true;
        playedTooLowOnce = false; // ✅ IMPORTANT: lets it react again
        Debug.Log("[BirdChangeCustom] Water READY → bird can fade now.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(changeColorTag))
            return;

        Debug.Log("[BirdChangeCustom] Entered ChangeColorTrigger.");

        if (!canFade)
        {
            if (!playedTooLowOnce && waterLevelController != null)
            {
                waterLevelController.TryPlayTooLowVO();
                playedTooLowOnce = true;
            }
            return;
        }

        if (hasFaded) return;

        hasFaded = true;
        StartCoroutine(FadeToFullColor());
    }

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

                if (info.mat.HasProperty(BaseColorID)) info.mat.SetColor(BaseColorID, c);
                else if (info.mat.HasProperty(ColorID)) info.mat.SetColor(ColorID, c);

                SetMaterialToTransparent(info.mat, c.a < 1f);
            }

            yield return null;
        }

        foreach (var info in mats)
        {
            if (info.mat.HasProperty(BaseColorID)) info.mat.SetColor(BaseColorID, info.originalColor);
            else if (info.mat.HasProperty(ColorID)) info.mat.SetColor(ColorID, info.originalColor);

            SetMaterialToTransparent(info.mat, false);
        }

        if (SoundPuzzleVOController.Instance != null)
            SoundPuzzleVOController.Instance.CueAfterWaterHomeHint();
    }

    private void SetMaterialToTransparent(Material mat, bool transparent)
    {
        if (transparent)
        {
            mat.SetFloat("_Surface", 1);
            mat.SetFloat("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            mat.SetFloat("_Surface", 0);
            mat.SetFloat("_ZWrite", 1);
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
}
