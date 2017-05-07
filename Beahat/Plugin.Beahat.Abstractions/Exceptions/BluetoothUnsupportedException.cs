using System;

namespace Plugin.Beahat.Abstractions
{
	/// <summary>
	/// 端末がBluetoothに対応していないことを表す例外
	/// </summary>
    public class BluetoothUnsupportedException : Exception
    {
        public BluetoothUnsupportedException(string message) : base(message)
        {
            
        }

        public BluetoothUnsupportedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
