using UnityEngine;
using System.Collections;
using Oculus.Interaction;

/// <summary>
/// Cube Color Transfer System
/// When cube touches a wall, it attempts to paint the wall and checks if the color is correct
/// Integrated with EffectMeshWallInteraction for game logic validation
/// </summary>
public class CubeColorTransfer : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("Color of this cube")]
    public Color cubeColor = Color.red;

    [Header("Collision Settings")]
    [Tooltip("Require cube to be grabbed to paint walls")]
    public bool requireGrabbed = true;

    [Header("Visual Effects")]
    [Tooltip("Color transition speed (0 = instant change)")]
    public float transitionSpeed = 2f;

    [Header("Painting Settings")]
    [Tooltip("Allow painting even if color is wrong (for testing)")]
    public bool allowWrongColor = false;

    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool debugMode = true;

    private Renderer _cubeRenderer;
    private Grabbable _grabbable;
    private bool _isGrabbed = false;
    private EffectMeshWallInteraction _wallInteraction;

    private void Start()
    {
        // Get Renderer and set color
        _cubeRenderer = GetComponent<Renderer>();
        if (_cubeRenderer != null)
        {
            if (cubeColor == Color.clear || cubeColor.a == 0)
            {
                cubeColor = _cubeRenderer.material.color;
            }
            else
            {
                _cubeRenderer.material.color = cubeColor;
            }
        }

        // Get Grabbable component and subscribe to events
        _grabbable = GetComponent<Grabbable>();
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }

        // Find wall interaction system
        _wallInteraction = FindObjectOfType<EffectMeshWallInteraction>();
        if (_wallInteraction == null)
        {
            Debug.LogWarning("[CubeColorTransfer] EffectMeshWallInteraction not found in scene!");
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Initialized with color: {cubeColor}");
        }
    }

    /// <summary>
    /// Manually set grabbed state
    /// </summary>
    public void SetGrabbed(bool grabbed)
    {
        _isGrabbed = grabbed;
    }

    /// <summary>
    /// Manually set cube color
    /// </summary>
    public void SetCubeColor(Color newColor)
    {
        cubeColor = newColor;
        if (_cubeRenderer != null)
        {
            _cubeRenderer.material.color = newColor;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Color changed to: {newColor}");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_grabbable != null)
        {
            _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    /// <summary>
    /// Handle Grabbable pointer events
    /// </summary>
    private void HandlePointerEvent(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Select:
                _isGrabbed = true;
                if (debugMode)
                {
                    Debug.Log("[CubeColorTransfer] Cube grabbed");
                }
                break;
            case PointerEventType.Unselect:
            case PointerEventType.Cancel:
                _isGrabbed = false;
                if (debugMode)
                {
                    Debug.Log("[CubeColorTransfer] Cube released");
                }
                break;
        }
    }

    /// <summary>
    /// Physics collision
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (requireGrabbed && !_isGrabbed)
        {
            return;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Collision with: {collision.gameObject.name}");
        }

        // Try to paint the object
        TryPaintObject(collision.gameObject, collision.GetContact(0));
    }

    /// <summary>
    /// Trigger collision
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (requireGrabbed && !_isGrabbed)
        {
            return;
        }

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Trigger collision with: {other.gameObject.name}");
        }

        // Try to paint the object (no contact point available for trigger)
        TryPaintObject(other.gameObject);
    }

    /// <summary>
    /// Try to paint an object
    /// </summary>
    private void TryPaintObject(GameObject obj, ContactPoint? contact = null)
    {
        if (obj == null) return;

        string objName = obj.name.ToLower();

        // Check if object is a wall
        bool isWall = objName.Contains("wall") ||
                      objName.Contains("effect") ||
                      objName.Contains("mesh") ||
                      objName.Contains("anchor") ||
                      objName.Contains("plane");

        if (!isWall)
        {
            if (debugMode)
            {
                Debug.Log($"[CubeColorTransfer] {obj.name} is not a wall, skipping");
            }
            return;
        }

        // If wall interaction system exists, use it
        if (_wallInteraction != null)
        {
            PaintWallWithValidation(obj, contact);
        }
        else
        {
            // Fallback: directly paint without validation
            if (debugMode)
            {
                Debug.LogWarning("[CubeColorTransfer] No wall interaction system, painting directly");
            }
            DirectPaintWall(obj);
        }
    }

    /// <summary>
    /// Paint wall with game logic validation
    /// </summary>
    private void PaintWallWithValidation(GameObject wall, ContactPoint? contact)
    {
        // Determine wall direction
        EffectMeshWallInteraction.WallDirection wallDirection = DetermineWallDirection(wall, contact);

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Attempting to paint {wallDirection} wall with color {cubeColor}");
        }

        // Try to paint wall through wall interaction system
        bool success = _wallInteraction.TryPaintWall(wallDirection, cubeColor);

        if (success)
        {
            // Color is correct - paint the wall
            if (debugMode)
            {
                Debug.Log($"[CubeColorTransfer] SUCCESS! Correct color for {wallDirection} wall");
            }
            DirectPaintWall(wall);
        }
        else
        {
            // Color is wrong
            if (debugMode)
            {
                Debug.LogWarning($"[CubeColorTransfer] FAIL! Wrong color for {wallDirection} wall");
            }

            // If allow wrong color is enabled (for testing), still paint it
            if (allowWrongColor)
            {
                if (debugMode)
                {
                    Debug.Log("[CubeColorTransfer] Painting anyway (allowWrongColor is enabled)");
                }
                DirectPaintWall(wall);
            }
            // Otherwise, wall interaction system already showed error message to player
        }
    }

    /// <summary>
    /// Directly paint wall without validation (fallback or testing mode)
    /// </summary>
    private void DirectPaintWall(GameObject wall)
    {
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = wall.GetComponentInChildren<Renderer>();
        }

        if (renderer != null)
        {
            ChangeRendererColor(renderer, cubeColor);
        }
        else if (debugMode)
        {
            Debug.LogWarning($"[CubeColorTransfer] {wall.name} has no Renderer");
        }
    }

    /// <summary>
    /// Determine wall direction based on wall object
    /// </summary>
    private EffectMeshWallInteraction.WallDirection DetermineWallDirection(GameObject wall, ContactPoint? contact)
    {
        Vector3 normal;

        // If we have contact point, use its normal
        if (contact.HasValue)
        {
            normal = contact.Value.normal;
        }
        else
        {
            // Otherwise, calculate direction from player to wall
            Transform playerTransform = Camera.main?.transform;
            if (playerTransform == null)
            {
                Debug.LogWarning("[CubeColorTransfer] Camera.main not found, defaulting to North wall");
                return EffectMeshWallInteraction.WallDirection.North;
            }

            Vector3 directionToWall = (wall.transform.position - playerTransform.position).normalized;
            normal = -directionToWall; // Invert to get wall's outward normal
        }

        // Normalize the normal vector
        normal = normal.normalized;

        // Calculate dot products with cardinal directions
        float dotNorth = Vector3.Dot(normal, Vector3.forward);  // Z+
        float dotSouth = Vector3.Dot(normal, Vector3.back);     // Z-
        float dotEast = Vector3.Dot(normal, Vector3.right);     // X+
        float dotWest = Vector3.Dot(normal, Vector3.left);      // X-

        // Find maximum dot product
        float maxDot = Mathf.Max(dotNorth, dotSouth, dotEast, dotWest);

        // Determine direction
        EffectMeshWallInteraction.WallDirection direction;
        if (maxDot == dotNorth)
            direction = EffectMeshWallInteraction.WallDirection.North;
        else if (maxDot == dotSouth)
            direction = EffectMeshWallInteraction.WallDirection.South;
        else if (maxDot == dotEast)
            direction = EffectMeshWallInteraction.WallDirection.East;
        else
            direction = EffectMeshWallInteraction.WallDirection.West;

        if (debugMode)
        {
            Debug.Log($"[CubeColorTransfer] Wall direction determined: {direction} (normal: {normal})");
        }

        return direction;
    }

    /// <summary>
    /// Change renderer color with transition
    /// </summary>
    private void ChangeRendererColor(Renderer renderer, Color targetColor)
    {
        if (renderer == null || renderer.material == null) return;

        try
        {
            if (transitionSpeed > 0)
            {
                // Smooth transition
                StartCoroutine(SmoothColorTransition(renderer, targetColor));
            }
            else
            {
                // Instant change
                renderer.material.color = targetColor;
            }

            // Try to set other common color properties
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.SetColor("_Color", targetColor);
            }
            if (renderer.material.HasProperty("_BaseColor"))
            {
                renderer.material.SetColor("_BaseColor", targetColor);
            }

            if (debugMode)
            {
                Debug.Log($"[CubeColorTransfer] Changed {renderer.gameObject.name} color to {targetColor}");
            }
        }
        catch (System.Exception e)
        {
            if (debugMode)
            {
                Debug.LogError($"[CubeColorTransfer] Error changing color: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Smooth color transition coroutine
    /// </summary>
    private IEnumerator SmoothColorTransition(Renderer renderer, Color targetColor)
    {
        if (renderer == null || renderer.material == null) yield break;

        Color startColor = renderer.material.color;
        float elapsedTime = 0f;
        float duration = 1f / transitionSpeed;

        while (elapsedTime < duration)
        {
            if (renderer == null || renderer.material == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            renderer.material.color = currentColor;

            yield return null;
        }

        // Ensure final color is accurate
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = targetColor;
        }
    }

    /// <summary>
    /// Manual test: Paint all walls with this cube's color
    /// </summary>
    [ContextMenu("Test Paint All Walls")]
    private void TestPaintAllWalls()
    {
        if (_wallInteraction == null)
        {
            _wallInteraction = FindObjectOfType<EffectMeshWallInteraction>();
        }

        if (_wallInteraction == null)
        {
            Debug.LogError("[CubeColorTransfer] Cannot test: EffectMeshWallInteraction not found");
            return;
        }

        Debug.Log("[CubeColorTransfer] Testing painting all walls...");

        foreach (EffectMeshWallInteraction.WallDirection direction in System.Enum.GetValues(typeof(EffectMeshWallInteraction.WallDirection)))
        {
            bool success = _wallInteraction.TryPaintWall(direction, cubeColor);
            Debug.Log($"[CubeColorTransfer] {direction} wall: {(success ? "SUCCESS" : "FAIL")}");
        }
    }
}