using UnityEngine;
using System.Collections.Generic;
public class prefabsController : MonoBehaviour
{
    [Header("digital recognization object")]
    public List<GameObject> objs;
    private bool isShown = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
  

    public void OnButtonClick()
    {
        isShown = !isShown;           // 每按一次翻转状态
        // target.SetActive(isShown);    // 应用状态

        foreach (var obj in objs)
    {
        if (obj != null)
            obj.SetActive(isShown);
    }
    }
}
