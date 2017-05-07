using Plugin.Beahat.Abstractions;
using System;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;

namespace Plugin.Beahat
{
	/// <summary>
	/// BluetoothでiBeacon用のデータを扱うためのユーティリティ（UWP用）
	/// </summary>
    public static class iBeaconUwpUtility
    {
        private const int MINIMUM_LENGTH_BYTES = 25;
        private const int ADJUSTED_LENGTH_BYTES = -2;

		/// <summary>
		/// Bluetoothの受信データをiBeacon情報に変換します。
		/// 受信データがiBeaconのものでなかった場合、nullを返します。
		/// </summary>
		/// <returns>受信データから生成したiBeacon情報</returns>
		/// <param name="args">Bluetoothの受信データ</param>
        public static iBeacon ConvertReceivedDataToBeacon(BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //出力されているbyteデータから各値を抽出する
            IList<BluetoothLEManufacturerData> manufacturerSections = args.Advertisement.ManufacturerData;

            Guid uuid;
            ushort? major;
            ushort? minor;
            short? rssi;
            short? txPower;

            if (manufacturerSections.Count > 0)
            {
                BluetoothLEManufacturerData manufacturerData = manufacturerSections[0];
                var data = new byte[manufacturerData.Data.Length];

                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }

                //長さをチェック
                if (data == null || data.Length < MINIMUM_LENGTH_BYTES + ADJUSTED_LENGTH_BYTES)
                {
                    return null;
                }

                //イベントから取得
                rssi = args.RawSignalStrengthInDBm;

                //バイトデータから抽出
                //公式での出力値（Windowsでは2byteずれているので補正が必要）
                // Byte(s)  WinByte(s) Name
                // --------------------------
                // 0-1      none       Manufacturer ID (16-bit unsigned integer, big endian)
                // 2-3      0-1        Beacon code (two 8-bit unsigned integers, but can be considered as one 16-bit unsigned integer in little endian)
                // 4-19     2-17       ID1 (UUID)
                // 20-21    18-19      ID2 (16-bit unsigned integer, big endian)
                // 22-23    20-21      ID3 (16-bit unsigned integer, big endian)
                // 24       22         Measured Power (signed 8-bit integer)
                // 25       23         Reserved for use by the manufacturer to implement special features (optional)

                //BigEndianの値を取得

                string uuidStr = BitConverter.ToString(data, 2, 4).Replace("-", string.Empty) + "-" +
                                 BitConverter.ToString(data, 6, 2).Replace("-", string.Empty) + "-" +
                                 BitConverter.ToString(data, 8, 2).Replace("-", string.Empty) + "-" +
                                 BitConverter.ToString(data, 10, 2).Replace("-", string.Empty) + "-" +
                                 BitConverter.ToString(data, 12, 6).Replace("-", string.Empty);
                uuid = new Guid(uuidStr);
                txPower = Convert.ToSByte(BitConverter.ToString(data, 24 + ADJUSTED_LENGTH_BYTES, 1), 16); // Byte 22

                //.NET FramewarkのEndianはCPUに依存するらしい
                if (BitConverter.IsLittleEndian)
                {
                    //LittleEndianの値を取得
                    byte[] revData;

                    revData = new byte[] { data[20 + ADJUSTED_LENGTH_BYTES], data[21 + ADJUSTED_LENGTH_BYTES] };// Bytes 18-19
                    Array.Reverse(revData);
                    major = BitConverter.ToUInt16(revData, 0);

                    revData = new byte[] { data[22 + ADJUSTED_LENGTH_BYTES], data[23 + ADJUSTED_LENGTH_BYTES] };// Bytes 20-21
                    Array.Reverse(revData);
                    minor = BitConverter.ToUInt16(revData, 0);
                }
                else
                {
                    //BigEndianの値を取得
                    major = BitConverter.ToUInt16(data, 20 + ADJUSTED_LENGTH_BYTES); // Bytes 18-19
                    minor = BitConverter.ToUInt16(data, 22 + ADJUSTED_LENGTH_BYTES); // Bytes 20-21
                }

                if (uuid != null && major != null && minor != null)
                {
                    var ibeacon = new iBeacon()
                    {
                        Uuid = uuid,
                        Major = (ushort)major,
                        Minor = (ushort)minor,
                        Rssi = rssi,
                        TxPower = txPower,
                        EstimatedDistanceMeter = CalcDistanceMeterFromRssiAndTxPower(rssi, txPower)
                    };

                    return ibeacon;
                }
            }

            return null;
        }

		/// <summary>
		/// RSSIとTx Powerから、iBeaconまでの推定距離（単位はメートル）を計算します。
		/// RSSIとTx Powerのどちらかがnullの場合、nullを返します。
		/// </summary>
		/// <returns>推定距離（メートル）</returns>
		/// <param name="rssi">RSSI</param>
		/// <param name="txPower">Tx Power</param>
        public static double? CalcDistanceMeterFromRssiAndTxPower(short? rssi, short? txPower)
        {
            if (rssi == null || txPower == null)
            {
                return null;
            }

            double distanceMeter = Math.Pow(10.0, ((double)txPower - (double)rssi) / 20.0);
            return distanceMeter;
        }

    }
}
