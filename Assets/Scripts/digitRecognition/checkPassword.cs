using TMPro;
using UnityEngine;

public class checkPassword : MonoBehaviour
{
    public string correctPsw="1234";
    public TextMeshPro[] pswTexts = new TextMeshPro[4]; // 4 digits text psw
    public TextMeshPro pswResult; // correct or incorrect
    public passthroughCropCamera sender; // send the digit psw
    private int writeIndex;

    void OnEnable()
    {
        if (sender != null && sender.setPswDigit != null)
        {
            sender.setPswDigit.AddListener(setPsw);
        }
    }
    void Onsable()
    {
         if (sender != null && sender.setPswDigit != null)
            sender.setPswDigit.RemoveListener(setPsw);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            checkPsw();
        }
    }
    void setPsw( int digit)
    {
        digit = Mathf.Clamp(digit, 0, 9);
        pswTexts[writeIndex].text = digit.ToString();
        writeIndex = Mathf.Min(writeIndex + 1, pswTexts.Length);
    }
    void checkPsw()
    {
        string inputPsw = "";
        for (int i = 0; i < pswTexts.Length; i++)
        {
            // inputPsw += pswTexts[i].text;
            inputPsw += string.IsNullOrEmpty(pswTexts[i].text) ? "" : pswTexts[i].text;
        }
        if (inputPsw == correctPsw)
        {
            pswResult.text = "correct";
        }
        else
        {
            pswResult.text = "incorrect";
             ClearAll();
        }
    }
    void ClearAll()
    {
        for (int i = 0; i < pswTexts.Length; i++)
        {
            pswTexts[i].text = "-";
        }
        writeIndex = 0;
    }
}
