
public static class MindwaveHelper
{

	#region Attributes

		public const int SENSE_MAX = 100;

		public const int BLINK_MAX = 200;

		public const int NO_SIGNAL_LEVEL = 200;

	#endregion


	#region Public Methods

		public static float GetSenseRatio(int _SenseValue)
		{
			return (_SenseValue / SENSE_MAX);
		}
		
		public static float GetBlinkRatio(int _BlinkValue)
		{
			return (_BlinkValue / BLINK_MAX);
		}

	#endregion

}