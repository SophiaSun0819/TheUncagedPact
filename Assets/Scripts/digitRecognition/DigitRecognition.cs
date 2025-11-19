using UnityEngine;
using Unity.InferenceEngine;
using TMPro;
public class DigitRecognition : MonoBehaviour
{
    public float threshold=0.9f; //model recognization error, avoid cannot detect the number, not used currently
    public Texture2D textPicture;
    public ModelAsset modelAsset;
    public Worker worker;
    public float[] results;
    
    public TextMeshPro digitText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Model model = ModelLoader.Load(modelAsset);
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(model);
        FunctionalTensor[] outputs = Functional.Forward(model, inputs);
        FunctionalTensor softmax = Functional.Softmax(outputs[0]);
        

        Model runTimeModel = graph.Compile(softmax);
        worker = new Worker(runTimeModel, BackendType.GPUCompute);
        // Debug.Log(RunAI(textPicture));
        digitText.text = RunAI(textPicture).ToString();
    }

    public int RunAI(Texture2D picture)
    {
        using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, 28, 28, 1);
        worker.Schedule(inputTensor);
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();
       return  GetMaxIndex(results);
    }
    void OnDisable()
    {
        // Tell the GPU we're finished with the memory the engine used
        worker.Dispose();
    }
    
    public int GetMaxIndex(float[] array)
    {
        int maxIndex = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > array[maxIndex])
            {
                maxIndex = i;
            }


        }
        if (array[maxIndex] > threshold)
        {
            return maxIndex;
        }
        else
        {
            return -1; //default index
        }

        // return maxIndex;
        
    }
}
