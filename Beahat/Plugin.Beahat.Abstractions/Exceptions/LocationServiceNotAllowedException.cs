using System;

namespace Plugin.Beahat.Abstractions
{
	/// <summary>
	/// 端末あるいはアプリに対して位置情報利用が許可されていないことを表す例外
	/// </summary>
	public class LocationServiceNotAllowedException : Exception
    {
		public LocationServiceNotAllowedException(string message) : base(message)
		{

		}

		public LocationServiceNotAllowedException(string message, Exception inner) : base(message, inner)
		{

		}
    }
}
