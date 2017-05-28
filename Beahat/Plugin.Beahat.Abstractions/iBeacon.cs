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
        public ushort? Major { get; set; } = null;

		/// <summary>
		/// iBeaconのMINOR値
		/// </summary>
		public ushort? Minor { get; set; } = null;

        /// <summary>
        /// 測定された電波強度（RSSI）
        /// </summary>
        public short? Rssi { get; set; } = null;

        /// <summary>
        /// 発信電波強度（TxPower）
        /// iOSの場合は検知不可。
        /// </summary>
        public short? TxPower { get; set; } = null;

        /// <summary>
        /// iBeaconと端末の推定距離（単位はメートル）。
        /// </summary>
        public double? EstimatedDistanceMeter { get; set; } = null;

        #endregion

    }
}
