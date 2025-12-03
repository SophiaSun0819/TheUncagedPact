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
        CheckYButton();
    }

  

    // public void OnButtonClick()
    // {
    //     isShown = !isShown;           // 每按一次翻转状态
    //     // target.SetActive(isShown);    // 应用状态

    //     foreach (var obj in objs)
    // {
    //     if (obj != null)
    //         obj.SetActive(isShown);
    // }
    // }
    // 按 UI Button 调用的方法
    public void OnButtonClick()
    {
        ToggleObjects();
    }

    // 左手控制器 Y 键触发
    private void CheckYButton()
    {
        // OVRInput.Button.Three = 左手手柄 Y 键
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            ToggleObjects();
        }
    }

    // 切换物体显隐
    private void ToggleObjects()
    {
        isShown = !isShown;

        foreach (var obj in objs)
        {
            if (obj != null)
                obj.SetActive(isShown);
        }
    }
}
