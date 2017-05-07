using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.Beahat.Abstractions
{
  /// <summary>
  /// Interface for Beahat
  /// </summary>
  public interface IBeahat : INotifyPropertyChanged
    {
		/// <summary>
		/// BeahatがiBeaconをスキャンしているかどうかを取得します。
		/// </summary>
		/// <value><c>true</c>スキャン実行中<c>false</c>スキャン停止中</value>
        bool IsScanning { get; }

		/// <summary>
		/// 直近実行した、あるいは現在実行中のスキャンで検出されたiBeaconの一覧を取得します。
		/// StartScanが実行されたときにリセットされます。
		/// </summary>
		/// <value>The detected beacon list.</value>
        List<iBeacon> DetectedBeaconList { get; }

		/// <summary>
		/// 端末がBluetoothに対応しているかどうかを取得します。
		/// </summary>
		/// <returns><c>true</c>対応<c>false</c>非対応</returns>
        bool BluetoothIsAvailableOnThisDevice();

		/// <summary>
		/// 端末のBluetooth機能がオンにされているかどうかを取得します。
		/// 端末がBluetooth機能をオンにしていない場合、falseを返します。
        /// また、端末がBluetoothをサポートしていない場合もfalseを返します。
		/// </summary>
		/// <returns><c>true</c>オン<c>false</c>オフ</returns>
        bool BluetoothIsEnableOnThisDevice();

		/// <summary>
		/// 端末のBluetooth機能をオンにするためのダイアログを表示します。
		/// UWPには対応する機能がないため、何もしません。
		/// 端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
		/// </summary>
        void RequestUserToTurnOnBluetooth();

		/// <summary>
		/// 検知対象とするiBeaconと、そのiBeaconを検知したときに実行させる処理を追加します。
		/// 同じUUID、Major、Minorに対してこのメソッドを続けて実行することで、同じiBeaconを検知したときの処理を複数設定することが可能です。
		/// </summary>
		/// <param name="uuid">検知対象とするiBeaconのUUID</param>
		/// <param name="major">検知対象とするiBeaconのMajor</param>
		/// <param name="minor">検知対象とするiBeaconのMinor</param>
		/// <param name="thresholdRssi">検知扱いとする下限RSSI</param>
		/// <param name="intervalMilliSec">次回の処理実行を待機させる時間（単位はミリ秒）</param>
		/// <param name="function">iBeaconを検知したときに実行させる処理</param>
        void AddEvent(Guid uuid,
                      ushort major,
                      ushort minor,
                      short thresholdRssi,
                      int intervalMilliSec,
                      Action function);

		/// <summary>
		/// 検知対象とするiBeaconを追加します。
		/// iBeaconの追加後に同じUUID、Major、Minorに対してAddEventメソッドを実行することで、同じiBeaconに対して検出時の処理を追加することが可能です。
		/// </summary>
		/// <param name="uuid">UUID.</param>
		/// <param name="major">Major.</param>
		/// <param name="minor">Minor.</param>
        void AddEvent(Guid uuid,
                      ushort major,
                      ushort minor);

		/// <summary>
		/// AddEventで設定された処理を全て消去します。
		/// </summary>
        void ClearAllEvent();

		/// <summary>
		/// iBeaconのスキャンを開始します。
		/// 端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
		/// 端末のBluetooth機能がオフにされている場合、例外BluetoothTurnedOffExceptionをthrowします。
		/// スキャンの開始時、DetectedBeaconListをリセットします。
		/// </summary>
        void StartScan();

		/// <summary>
		/// iBeaconのスキャンを停止します。
		/// </summary>
        void StopScan();
    }
}
