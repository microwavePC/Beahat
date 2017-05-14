using System;

namespace Plugin.Beahat.Abstractions
{
	public class iBeacon
	{

		#region PROPERTIES

		/// <summary>
		/// iBeaconのUUID
		/// </summary>
		public Guid Uuid { get; set; }

		/// <summary>
		/// iBeaconのMAJOR値
		/// </summary>
		public ushort Major { get; set; }

		/// <summary>
		/// iBeaconのMINOR値
		/// </summary>
		public ushort Minor { get; set; }

		/// <summary>
		/// 測定された電波強度（RSSI）
		/// </summary>
		private short? _rssi = null;
		public short? Rssi
		{
			get { return _rssi; }
			set
			{
				_rssi = value;
			}
		}

		/// <summary>
		/// 発信電波強度（TxPower）
		/// iOSの場合は検知不可。
		/// </summary>
		private short? _txPower = null;
		public short? TxPower
		{
			get { return _txPower; }
			set
			{
				_txPower = value;
			}
		}

		/// <summary>
		/// iBeaconと端末の推定距離（単位はメートル）。
		/// </summary>
		private double? _estimatedDistanceMeter = null;
		public double? EstimatedDistanceMeter
		{
			get { return _estimatedDistanceMeter; }
			set
			{
				_estimatedDistanceMeter = value;
			}
		}

		#endregion

	}
}
