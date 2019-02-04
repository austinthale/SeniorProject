using UnityEngine;

namespace FuzzyTools
{
	public static class FuzzyWait
	{
		private static readonly WaitForFixedUpdate _forFixedUpdate = new WaitForFixedUpdate();
		private static readonly WaitForEndOfFrame _forEndOfFrame = new WaitForEndOfFrame();
		private static readonly WaitForSeconds _forPointOneSecond = new WaitForSeconds(.1f);
		private static readonly WaitForSeconds _forHalfASecond = new WaitForSeconds(.5f);
		private static readonly WaitForSeconds _forOneSecond = new WaitForSeconds(1);
		private static readonly WaitForSeconds _forTwoSeconds = new WaitForSeconds(2);
		private static readonly WaitForSeconds _forFiveSeconds = new WaitForSeconds(5);
		private static readonly WaitForSeconds _forTenSeconds = new WaitForSeconds(10);
		private static readonly WaitForSeconds _forOneMinute = new WaitForSeconds(60);

		private static readonly WaitForSeconds[] _for5To20Seconds =
		{
			new WaitForSeconds(5),
			new WaitForSeconds(6),
			new WaitForSeconds(7),
			new WaitForSeconds(8),
			new WaitForSeconds(9),
			new WaitForSeconds(10),
			new WaitForSeconds(11),
			new WaitForSeconds(12),
			new WaitForSeconds(13),
			new WaitForSeconds(14),
			new WaitForSeconds(15),
			new WaitForSeconds(16),
			new WaitForSeconds(17),
			new WaitForSeconds(18),
			new WaitForSeconds(19),
			new WaitForSeconds(20)
		};


		public static WaitForFixedUpdate ForFixedUpdate()
		{
			return _forFixedUpdate;
		}

		public static WaitForEndOfFrame ForEndOfFrame()
		{
			return _forEndOfFrame;
		}

		public static WaitForSeconds ForPointOneSecond()
		{
			return _forPointOneSecond;
		}

		public static WaitForSeconds ForHalfASecond()
		{
			return _forHalfASecond;
		}

		public static WaitForSeconds ForOneSecond()
		{
			return _forOneSecond;
		}

		public static WaitForSeconds ForTwoSeconds()
		{
			return _forTwoSeconds;
		}

		public static WaitForSeconds ForFiveSeconds()
		{
			return _forFiveSeconds;
		}

		public static WaitForSeconds ForTenSeconds()
		{
			return _forTenSeconds;
		}

		public static WaitForSeconds ForOneMinute()
		{
			return _forOneMinute;
		}

		public static WaitForSeconds ForRandom5To20Seconds()
		{
			var random = Random.Range(0, _for5To20Seconds.Length);
			return _for5To20Seconds[random];
		}
	}
}