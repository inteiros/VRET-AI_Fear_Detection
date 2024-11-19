#region Headers

	using UnityEngine;

#endregion


[AddComponentMenu("Scripts/MindwaveUnity/Mindwave Manager")]
[RequireComponent(typeof(MindwaveController))]
[RequireComponent(typeof(MindwaveCalibrator))]
public class MindwaveManager : MuffinTools.MonoSingleton<MindwaveManager>
{

	#region Attributes

		private MindwaveController m_Controller = null;
		private MindwaveCalibrator m_Calibrator = null;

	#endregion

	
	#region Protected Methods

		protected override void OnInstanceInit()
		{
			base.OnInstanceInit();

			DontDestroyOnLoad(gameObject);

			m_Controller = GetComponent<MindwaveController>();
			m_Calibrator = GetComponent<MindwaveCalibrator>();
		}

	#endregion

	
	#region Accessors

		public MindwaveController Controller
		{
			get { return m_Controller; }
		}

		public MindwaveCalibrator Calibrator
		{
			get { return m_Calibrator; }
		}

	#endregion

}