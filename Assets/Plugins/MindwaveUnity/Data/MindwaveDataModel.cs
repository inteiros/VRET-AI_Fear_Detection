[System.Serializable]
public struct MindwaveDataModel
{

	#region Attributes

		public MindwaveDataESenseModel eSense;

		public MindwaveDataEegPowerModel eegPower;

		public int poorSignalLevel;
		public string status;

	#endregion


	#region Accessors
		public bool NoSignal
		{
			get { return (poorSignalLevel >= MindwaveHelper.NO_SIGNAL_LEVEL); }
		}

	#endregion

}