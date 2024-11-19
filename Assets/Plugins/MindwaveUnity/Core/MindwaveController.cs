#region Headers

	using UnityEngine;

	using System.Net.Sockets;
	using System.IO;
	using System.Text;
	using System.Collections;

	using Jayrock.Json;
	using Jayrock.Json.Conversion;

#endregion

[AddComponentMenu("Scripts/MindwaveUnity/Mindwave Controller")]
public class MindwaveController : MonoBehaviour
{

	#region Attributes
		private const string DEFAULT_TCP_HOSTNAME = "127.0.0.1";
		private const int DEFAULT_TCP_PORT = 13854;

		private const int BUFFER_LENGTH = 1024;

		private const string POOR_SIGNAL_LABEL = "poorSignalLevel";
		private const string RAW_EEG_LABEL = "rawEeg";
		private const string BLINK_STRENGTH_LABEL = "blinkStrength";

		public delegate void VoidDelegate();
		public delegate void MindwaveDataDelegate(MindwaveDataModel _Data);
		public delegate void IntValueDelegate(int _Value);

		public event VoidDelegate OnConnectMindwave;
		public event VoidDelegate OnDisconnectMindwave;
		public event VoidDelegate OnConnectionTimeout;
		public event MindwaveDataDelegate OnUpdateMindwaveData;
		public event IntValueDelegate OnUpdateRawEEG;
		public event IntValueDelegate OnUpdateBlink;

		[Header("Connection settings")]

		[SerializeField, Tooltip("By default: 127.0.0.1")]
		private string m_TcpHostname = DEFAULT_TCP_HOSTNAME;

		[SerializeField, Tooltip("By default: 13854")]
		private int m_TcpPort = DEFAULT_TCP_PORT;

		[Header("Data stream settings")]

		[SerializeField, Tooltip("If it's set to false, you must connect to Mindwave manually using Connect(). Else, automatically try to connect to Mindwave at game start.")]
		private bool m_TryConnectAtStart = false;

		[SerializeField, Range(0.0f, 30.0f), Tooltip("Defines the timing before a connection try timeouts.")]
		private float m_ConnectionTimeout = 10.0f;

		[SerializeField, Range(0.0f, 1.0f), Tooltip("Defines the interval between each Mindwave call.")]
		private float m_UpdateStreamRate = 0.02f;

		[Header("Debug settings")]

		[SerializeField]
		private bool m_ShowDataPackets = true;

		[SerializeField]
		private bool m_ShowStreamErrors = false;

		private TcpClient m_TcpClient = null;
		private Stream m_DataStream = null;
		private byte[] m_Buffer = { };

		private Coroutine m_StreamRoutine = null;
		private bool m_ConnectedFlag = false;
		private bool m_PendingConnection = false;

		private float m_TimeoutTimer = 0.0f;

	#endregion

	
	#region Engine Methods

		private void OnEnable()
		{
			if(m_TryConnectAtStart)
			{
				ConnectToMindwave();
			}
		}

		private void OnDisable()
		{
			DisconnectFromMindwave();
		}

		private void Update()
		{
			UpdateTimeoutTimer(Time.deltaTime);
		}

	#endregion

	
	#region Public Methods

		public void Connect()
		{
			ConnectToMindwave();
		}

		public void Disconnect()
		{
			DisconnectFromMindwave();
		}

	#endregion


	#region Protected Methods
	#endregion

	
	#region Private Methods
		private void ConnectToMindwave()
		{
			if(m_StreamRoutine == null)
			{
				m_TcpClient = new TcpClient(m_TcpHostname, m_TcpPort);
				m_DataStream = m_TcpClient.GetStream();

				InitBuffer();

				m_StreamRoutine = StartCoroutine(ParseData(m_UpdateStreamRate));

				PendingConnection = true;
			}
		}

		private void InitBuffer()
		{
			m_Buffer = new byte[BUFFER_LENGTH];
			byte[] writeBuffer = Encoding.ASCII.GetBytes(@"{""enableRawOutput"": true, ""format"": ""Json""}");
			m_DataStream.Write(writeBuffer, 0, writeBuffer.Length);
		}

		private void DisconnectFromMindwave()
		{
			if(m_StreamRoutine != null)
			{
				StopCoroutine(m_StreamRoutine);
				m_StreamRoutine = null;
			}

			PendingConnection = false;
			ConnectedFlag = false;

			if(m_DataStream != null)
			{
				m_DataStream.Close();
			}
		}

		private IEnumerator ParseData(float _UpdateRate)
		{
			if(m_DataStream.CanRead)
			{
				int streamBytes = m_DataStream.Read(m_Buffer, 0, m_Buffer.Length);
				string[] packets = Encoding.ASCII.GetString(m_Buffer, 0, streamBytes).Split('\r');

				if(m_ShowDataPackets)
				{
					Debug.Log(Encoding.ASCII.GetString(m_Buffer, 0, streamBytes));
				}

				foreach(string packet in packets)
				{
					if(string.IsNullOrEmpty(packet))
					{
						continue;
					}

					try
					{
						IDictionary data = (IDictionary)JsonConvert.Import(typeof(IDictionary), packet);

						if(data.Contains(POOR_SIGNAL_LABEL))
						{
							MindwaveDataModel model = JsonUtility.FromJson<MindwaveDataModel>(packet);

							if(model.NoSignal)
							{
								ConnectedFlag = false;
							}

							else
							{
								ConnectedFlag = true;
								PendingConnection = false;

								if (OnUpdateMindwaveData != null)
								{
									OnUpdateMindwaveData(model);
								}
							}
						}
						
						else if (data.Contains(RAW_EEG_LABEL))
						{
							if (OnUpdateRawEEG != null)
							{
								OnUpdateRawEEG(int.Parse(data[RAW_EEG_LABEL].ToString()));
							}
						}

						else if (data.Contains(BLINK_STRENGTH_LABEL))
						{
							if (OnUpdateBlink != null)
							{
								OnUpdateBlink(int.Parse(data[BLINK_STRENGTH_LABEL].ToString()));
							}
						}
					}

					catch(IOException _JsonException)
					{
						if(m_ShowStreamErrors)
						{
							Debug.LogWarning("MindwaveBinding stream Error: " + _JsonException.ToString());
						}
					}

					catch (JsonException _JsonException)
					{
						if (m_ShowStreamErrors)
						{
							Debug.LogWarning("MindwaveBinding stream Error: " + _JsonException.ToString());
						}
					}

					catch(System.Exception _Exception)
					{
						Debug.LogError("MindwaveBinding error: " + _Exception.ToString());
					}
				}
			}

			else
			{
				ConnectedFlag = false;
			}

			yield return new WaitForSeconds(_UpdateRate);

			m_StreamRoutine = StartCoroutine(ParseData(_UpdateRate));
		}
		private void UpdateTimeoutTimer(float _DeltaTime)
		{
			if(m_PendingConnection)
			{
				m_TimeoutTimer += _DeltaTime;
				if(m_TimeoutTimer >= m_ConnectionTimeout)
				{
					if(OnConnectionTimeout != null)
					{
						OnConnectionTimeout();
					}
					Disconnect();
				}
			}
		}

	#endregion

	
	#region Accessors

		private bool ConnectedFlag
		{
			get { return m_ConnectedFlag; }
			set
			{
				if(m_ConnectedFlag != value)
				{
					m_ConnectedFlag = value;
					if(m_ConnectedFlag)
					{
						if (OnConnectMindwave != null)
						{
							OnConnectMindwave();
						}
					}
					else
					{
						if (OnDisconnectMindwave != null)
						{
							OnDisconnectMindwave();
						}
					}
				}
			}
		}

		private bool PendingConnection
		{
			get { return m_PendingConnection; }
			set
			{
				if(m_PendingConnection != value)
				{
					m_PendingConnection = value;
					if(m_PendingConnection)
					{
						m_TimeoutTimer = 0.0f;
					}
				}
			}
		}

		public bool IsConnecting
		{
			get { return m_PendingConnection; }
		}

		public bool IsConnected
		{
			get { return m_ConnectedFlag; }
		}

		public float TimeoutTimer
		{
			get { return m_TimeoutTimer; }
		}

		public float ConnectionTimeoutDelay
		{
			get { return m_ConnectionTimeout; }
		}

	#endregion

}