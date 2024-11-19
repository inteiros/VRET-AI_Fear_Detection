#region Headers

using UnityEngine;
using System.IO;

#endregion

[AddComponentMenu("Scripts/MindwaveUnity/Mindwave UI")]
public class MindwaveUI : MonoBehaviour
{

    #region Attributes

    private const int LABEL_WIDTH = 200;
    private const int VALUE_MIN_WIDTH = 100;
    private int sessionNumber = 1;
    private string filePath;

    [Header("References")]

    [SerializeField]
    private MindwaveController m_Controller = null;

    [SerializeField]
    private MindwaveCalibrator m_Calibrator = null;


    private MindwaveDataModel m_MindwaveData;
    private int m_EEGValue = 0;
    private int m_BlinkStrength = 0;

    #endregion


    #region Engine Methods

    private void Awake()
    {
        if (m_Controller == null)
        {
            m_Controller = GetComponent<MindwaveController>();
        }

        if (m_Calibrator == null)
        {
            m_Calibrator = GetComponent<MindwaveCalibrator>();
        }

        BindMindwaveControllerEvents();
        InitializeCSV();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            DrawControllerGUI();
            DrawCalibratorGUI();
        }
        GUILayout.EndHorizontal();
    }

    #endregion


    #region Public Methods

    public void OnUpdateMindwaveData(MindwaveDataModel _Data)
    {
        m_MindwaveData = _Data;
        SaveDataToCSV();
    }

    public void OnUpdateRawEEG(int _EEGValue)
    {
        m_EEGValue = _EEGValue;
    }

    public void OnUpdateBlink(int _BlinkStrength)
    {
        m_BlinkStrength = _BlinkStrength;
    }

    #endregion


    #region Private Methods

    private void BindMindwaveControllerEvents()
    {
        m_Controller.OnUpdateMindwaveData += OnUpdateMindwaveData;
        m_Controller.OnUpdateRawEEG += OnUpdateRawEEG;
        m_Controller.OnUpdateBlink += OnUpdateBlink;
    }

    private void InitializeCSV()
    {
        // Define o caminho do arquivo na pasta Assets
        while (File.Exists(GetFilePath(sessionNumber)))
        {
            sessionNumber++;
        }

        filePath = GetFilePath(sessionNumber);
        Debug.Log($"Salvando dados em: {filePath}");

        // Cabeçalhos do CSV
        File.WriteAllText(filePath, "Delta,Theta,LowAlpha,HighAlpha,LowBeta,HighBeta,LowGamma,HighGamma,Attention,Meditation,EEGValue,BlinkStrength\n");
    }

    private string GetFilePath(int session)
    {
        return Path.Combine(Application.dataPath, $"mindwave_session{session}.csv");
    }

    private void SaveDataToCSV()
    {
        if (m_MindwaveData.Equals(default(MindwaveDataModel))) return;

        string dataLine = $"{m_MindwaveData.eegPower.delta}," +
                          $"{m_MindwaveData.eegPower.theta}," +
                          $"{m_MindwaveData.eegPower.lowAlpha}," +
                          $"{m_MindwaveData.eegPower.highAlpha}," +
                          $"{m_MindwaveData.eegPower.lowBeta}," +
                          $"{m_MindwaveData.eegPower.highBeta}," +
                          $"{m_MindwaveData.eegPower.lowGamma}," +
                          $"{m_MindwaveData.eegPower.highGamma}," +
                          $"{m_MindwaveData.eSense.attention}," +
                          $"{m_MindwaveData.eSense.meditation}," +
                          $"{m_EEGValue}," +
                          $"{m_BlinkStrength}\n";

        File.AppendAllText(filePath, dataLine);
    }

    private void DrawControllerGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Box("CONTROLLER");
            DrawSpace();

            if (m_Controller.IsConnecting)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                {
                    GUILayout.Label("Trying to connect to Mindwave...");
                    GUILayout.Label("Timeouts in " + Mathf.CeilToInt(m_Controller.ConnectionTimeoutDelay - m_Controller.TimeoutTimer) + "s");
                }
                GUILayout.EndVertical();
            }

            else if (m_Controller.IsConnected)
            {
                DrawConnectedGUI();
            }

            else
            {
                DrawDisconnectedGUI();
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawCalibratorGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Box("CALIBRATOR");
            DrawSpace();

            if (m_Controller != null && m_Controller.IsConnected)
            {
                GUILayout.Box("Data");
                DrawData("Nb. data collected", m_Calibrator.DataCount);
                DrawSpace();

                GUILayout.Box("Ratios");
                DrawData("Delta", m_Calibrator.EvaluateRatio(Brainwave.Delta, m_MindwaveData.eegPower.delta));
                DrawData("Theta", m_Calibrator.EvaluateRatio(Brainwave.Theta, m_MindwaveData.eegPower.theta));
                DrawData("Low Alpha", m_Calibrator.EvaluateRatio(Brainwave.LowAlpha, m_MindwaveData.eegPower.lowAlpha));
                DrawData("High Alpha", m_Calibrator.EvaluateRatio(Brainwave.HighAlpha, m_MindwaveData.eegPower.highAlpha));
                DrawData("Low Beta", m_Calibrator.EvaluateRatio(Brainwave.LowBeta, m_MindwaveData.eegPower.lowBeta));
                DrawData("High Beta", m_Calibrator.EvaluateRatio(Brainwave.HighBeta, m_MindwaveData.eegPower.highBeta));
                DrawData("Low Gamma", m_Calibrator.EvaluateRatio(Brainwave.LowGamma, m_MindwaveData.eegPower.lowGamma));
                DrawData("High Gamma", m_Calibrator.EvaluateRatio(Brainwave.HighGamma, m_MindwaveData.eegPower.highGamma));
            }

            else
            {
                GUILayout.Box("Not connected");
            }
        }
        GUILayout.EndVertical();
    }

    private void DrawConnectedGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Box("Signal");
            DrawData("Status", m_MindwaveData.status);
            DrawData("Poor signal level", m_MindwaveData.poorSignalLevel);
            DrawSpace();

            GUILayout.Box("Senses");
            DrawData("Attention", m_MindwaveData.eSense.attention);
            DrawData("Meditation", m_MindwaveData.eSense.meditation);
            DrawSpace();

            GUILayout.Box("Brain Waves");
            DrawData("Delta", m_MindwaveData.eegPower.delta);
            DrawData("Theta", m_MindwaveData.eegPower.theta);
            DrawData("Low Alpha", m_MindwaveData.eegPower.lowAlpha);
            DrawData("High Alpha", m_MindwaveData.eegPower.highAlpha);
            DrawData("Low Beta", m_MindwaveData.eegPower.lowBeta);
            DrawData("High Beta", m_MindwaveData.eegPower.highBeta);
            DrawData("Low Gamma", m_MindwaveData.eegPower.lowGamma);
            DrawData("High Gamma", m_MindwaveData.eegPower.highGamma);
            DrawSpace();

            GUILayout.Box("Others");
            DrawData("Blink strength", m_BlinkStrength);
            DrawData("Raw EEG", m_EEGValue);
        }
        GUILayout.EndVertical();

        if (GUILayout.Button("Disconnect from Mindwave"))
        {
            m_Controller.Disconnect();
        }
    }

    private void DrawData(string _Label, string _Value)
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(_Label, GUILayout.Width(LABEL_WIDTH));
            GUILayout.Label(_Value, GUILayout.MinWidth(VALUE_MIN_WIDTH));
        }
        GUILayout.EndHorizontal();
    }

    private void DrawData(string _Label, int _Value)
    {
        DrawData(_Label, _Value.ToString());
    }

    private void DrawData(string _Label, float _Value)
    {
        DrawData(_Label, _Value.ToString());
    }

    private void DrawSpace()
    {
        GUILayout.Label("");
    }

    public int GetEEGValue()
    {
        return m_EEGValue;
    }

    public int GetBlinkStrength()
    {
        return m_BlinkStrength;
    }

    private void DrawDisconnectedGUI()
    {
        GUILayout.Box("Not connected");

        if (m_Controller != null)
        {
            if (GUILayout.Button("Connect to Mindwave") || Input.GetKeyDown(KeyCode.P))
            {
                m_Controller.Connect();
            }
        }

        else
        {
            GUILayout.Box("No MindwaveController");
        }
    }

    #endregion

}
