using System.Collections;
using UnityEngine;

public class ButtonPressLerp : MonoBehaviour
{
    [Header("Moving Part")]
    public Transform buttonCap;               // assign the red part here

    [Header("Movement")]
    public Vector3 pressedLocalOffset = new Vector3(0f, -0.02f, 0f); // how far it goes down (local)
    public float pressDuration = 0.08f;      // time to go down
    public float returnDuration = 0.15f;     // time to go back up

    [Header("Trigger")]
    public string triggerTag = "Controller";     // tag of player/controller collider

    // internal
    Vector3 _startLocalPos;
    bool _isAnimating;

    void Awake()
    {
        if (buttonCap == null)
        {
            Debug.LogError("ButtonPressLerp: buttonCap is not assigned!");
            enabled = false;
            return;
        }

        _startLocalPos = buttonCap.localPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isAnimating) return;
        if (!other.CompareTag(triggerTag)) return;

        StartCoroutine(PressRoutine());
    }

    IEnumerator PressRoutine()
    {
        _isAnimating = true;

        // 1) go down
        Vector3 pressedPos = _startLocalPos + pressedLocalOffset;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / pressDuration;
            buttonCap.localPosition = Vector3.Lerp(_startLocalPos, pressedPos, t);
            yield return null;
        }

        // 2) small pause (optional)
        yield return new WaitForSeconds(0.05f);

        // 3) go back up
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / returnDuration;
            buttonCap.localPosition = Vector3.Lerp(pressedPos, _startLocalPos, t);
            yield return null;
        }

        _isAnimating = false;

        // 4) call your old logic here if needed:
        // e.g. spawn bird, play VFX, etc.
        // MyBirdPuzzleManager.Instance.OnButtonPressed();
    }
}
