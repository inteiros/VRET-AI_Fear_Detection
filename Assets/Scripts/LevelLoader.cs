using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Barracuda;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private float levelTime = 120f;

    [Header("Mindwave References")]
    private MindwaveController mindwaveController;
    private MindwaveCalibrator mindwaveCalibrator;

    private MindwaveDataModel mindwaveData;
    private bool fearDetected = false;
    private bool calmState = false;
    public static bool IsMindwaveManagerInitialized = false;
    private bool isAutoLoadRunning = false;


    [Header("ONNX Model")]
    [SerializeField] private NNModel onnxModel;
    private IWorker worker;

    [Header("Mindwave UI Reference")]
    private MindwaveUI mindwaveUI;

    [Header("Audio Player")]
    private AudioPlayer audioPlayer;
    void Awake()
    {
        LevelLoader[] instances = FindObjectsByType<LevelLoader>(FindObjectsSortMode.None);
        if (instances.Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        if (mindwaveController == null)
        {
            mindwaveController = FindFirstObjectByType<MindwaveController>();
        }

        if (MindwaveManager.Instance != null)
        {
            mindwaveController = MindwaveManager.Instance.Controller;
            mindwaveCalibrator = MindwaveManager.Instance.Calibrator;
            mindwaveUI = FindFirstObjectByType<MindwaveUI>();
        }

        mindwaveController.OnUpdateMindwaveData += OnUpdateMindwaveData;
        DontDestroyOnLoad(this.gameObject);

        if (worker == null)
        {
            var model = ModelLoader.Load(onnxModel);
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
        }

        audioPlayer = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioPlayer>();

        if (SceneManager.GetActiveScene().buildIndex == 5)
        {
            PlayChillMusic();
            StartCoroutine(CheckCalmState());
        }

        StartCoroutine(AutoLoadNextScene());
    }

    private void OnUpdateMindwaveData(MindwaveDataModel data)
    {
        mindwaveData = data;
        if (mindwaveController.IsConnected)
        {
            RunNeuralNetworkInference();
        }
    }

    private float Normalize(float value, float mean, float std)
    {
        return (value - mean) / std;
    }

    private void RunNeuralNetworkInference()
    {
        Tensor inputTensor = null;
        Tensor outputTensor = null;

        // Parametros de normalizacao extraidos do StandardScaler no python
        float[] means = {
        606613.29f, 156660.35f, 37944.38f, 35300.97f,
        29381.38f, 23087.31f, 13879.81f, 8614.79f,
        46.53f, 54.74f, 40.52f, 53.71f
    };

        float[] stds = {
        689843.11f, 252119.97f, 75617.15f, 82538.61f,
        52459.55f, 37342.76f, 21381.26f, 13010.53f,
        23.45f, 17.70f, 218.40f, 20.52f
    };

        try
        {
            float[] normalizedInput = new float[]
            {
                Normalize(mindwaveData.eegPower.delta, means[0], stds[0]),
                Normalize(mindwaveData.eegPower.theta, means[1], stds[1]),
                Normalize(mindwaveData.eegPower.lowAlpha, means[2], stds[2]),
                Normalize(mindwaveData.eegPower.highAlpha, means[3], stds[3]),
                Normalize(mindwaveData.eegPower.lowBeta, means[4], stds[4]),
                Normalize(mindwaveData.eegPower.highBeta, means[5], stds[5]),
                Normalize(mindwaveData.eegPower.lowGamma, means[6], stds[6]),
                Normalize(mindwaveData.eegPower.highGamma, means[7], stds[7]),
                Normalize(mindwaveData.eSense.attention, means[8], stds[8]),
                Normalize(mindwaveData.eSense.meditation, means[9], stds[9]),
                Normalize(mindwaveUI.GetEEGValue(), means[10], stds[10]),
                Normalize(mindwaveUI.GetBlinkStrength(), means[11], stds[11])
            };

            inputTensor = new Tensor(new int[] { 1, 12, 1, 1 }, normalizedInput);

            worker.Execute(inputTensor);
            outputTensor = worker.PeekOutput();

            // Obter a predicao (0 = chill, 1 = fear)
            float prediction = outputTensor[0];
            fearDetected = prediction > 0.5f;
            calmState = prediction < 0.25f;

            Debug.Log($"Fear level: {prediction}");
            Debug.Log($"Fear Detected: {fearDetected}");
        }
        finally
        {
            if (inputTensor != null) inputTensor.Dispose();
            if (outputTensor != null) outputTensor.Dispose();
        }
    }


    void OnDestroy()
    {
        if (worker != null)
        {
            worker.Dispose();
            worker = null;
        }
    }

    private IEnumerator CheckCalmState()
    {
        while (true)
        {
            if (calmState) 
            {
                LoadScene(1);
                StartCoroutine(AutoLoadNextScene());
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void PlayChillMusic()
    {
        if (audioPlayer != null)
        {
            audioPlayer.PlayChill();
        }
    }

    private IEnumerator AutoLoadNextScene()
    {
        float timer = 0f;

        while (timer < levelTime)
        {
            if (fearDetected)
            {
                LoadScene(5);
                isAutoLoadRunning = false;
                isCheckingCalmState = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        LoadNextScene();
        isAutoLoadRunning = false;
    }

    private bool isCheckingCalmState = false;

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 5 && !isAutoLoadRunning)
        {
            StartCoroutine(AutoLoadNextScene()); 
            isAutoLoadRunning = true; 
        }

        if (SceneManager.GetActiveScene().buildIndex == 5 && !isCheckingCalmState)
        {
            PlayChillMusic();
            StartCoroutine(CheckCalmState()); 
            isCheckingCalmState = true;
        }
    }


    private void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex > 4) nextSceneIndex = 1;
        StartCoroutine(LoadLevel(nextSceneIndex));
    }
     
    private void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadLevel(sceneIndex));
    }

    private IEnumerator LoadLevel(int sceneIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneIndex);
        transition.SetTrigger("Reset");
    }

}
