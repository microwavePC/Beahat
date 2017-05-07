using System;

namespace Plugin.Beahat.Abstractions
{
	/// <summary>
	/// 端末のBluetooth機能がオフにされていることを表す例外
	/// </summary>
    public class BluetoothTurnedOffException : Exception
    {
        public BluetoothTurnedOffException(string message) : base(message)
        {

        }

        public BluetoothTurnedOffException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
