using UnityEngine;

public class BirdChangeColor : MonoBehaviour
{
    [Header("Set Bird Color")]
    public Color birdColor = Color.blue;   // Inspector 可设置的颜色

    private Renderer birdRenderer;

     [Header("Link to Water Level Controller (Task Event Source)")]
    public ShaderWaterLevelController waterLevelController;  // ← 新增：事件来源

    [Header("Change Color trigger")]
    public GameObject changeColorTrigger;
    private bool changeColor=false; // only finish eyebolls and can detect the tregger collider and change color

    void Start()
    {
        // 获取自身的 Renderer
        birdRenderer = GetComponent<Renderer>();

        
        if (waterLevelController != null)
        {
            waterLevelController.onWaterBottleComplete.AddListener(setChangeColorTrigger);
        }
    }

   
    void setChangeColorTrigger()
    {
        changeColor=true;
    }
     void OnTriggerEnter(Collider collision)
    {
        Debug.Log(" change color trigger enter");
        if (changeColor == false)
        {
            Debug.Log("change color false");
            return;
        }
        if (collision.gameObject.CompareTag("ChangeColorTrigger"))
        {
            if (birdRenderer != null)
        {
            birdRenderer.material.color = birdColor;
        }
        }
    }
}
