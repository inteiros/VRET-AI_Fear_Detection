[System.Serializable]
public struct MindwaveDataESenseModel
{

	#region Attributes

		public int attention;
		public int meditation;

	#endregion


	#region Accessors

		public float AttentionRatio
		{
			get { return MindwaveHelper.GetSenseRatio(attention); }
		}

		public float MeditationRatio
		{
			get { return MindwaveHelper.GetSenseRatio(meditation); }
		}

	#endregion

}