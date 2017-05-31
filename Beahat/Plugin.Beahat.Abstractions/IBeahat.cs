﻿using System;
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
		/// スキャンで検出されたiBeaconの一覧を取得します。
		/// 検出された各iBeaconについて、最も近接したときの情報を保持します。
		/// </summary>
		List<iBeacon> BeaconListFromClosestApproachedEvent { get; }

		/// <summary>
		/// スキャンで検出されたiBeaconの一覧を取得します。
		/// 検出された各iBeaconについて、最も直近に検出したときの情報を保持します。
		/// </summary>
		List<iBeacon> BeaconListFromLastApproachedEvent { get; }

		/// <summary>
		/// 端末がBluetoothに対応しているかどうかを取得します。
		/// </summary>
		/// <returns><c>true</c>対応<c>false</c>非対応</returns>
        bool SupportsBluetooth();

		/// <summary>
		/// 端末のBluetooth機能がオンにされているかどうかを取得します。
		/// 端末がBluetooth機能をオンにしていない場合、falseを返します。
        /// また、端末がBluetoothをサポートしていない場合もfalseを返します。
		/// </summary>
		/// <returns><c>true</c>オン<c>false</c>オフ</returns>
        bool IsReadyToUseBluetooth();

        /// <summary>
        /// iBeacon検知のために必要な位置情報の使用が許可されているかどうかを取得します。
        /// AndroidとUWPではiBeaconを検知するために位置情報利用の許可が必要ないため、常にtrueが返されます。
        /// </summary>
        /// <returns><c>true</c>許可されている<c>false</c>許可されていない</returns>
        bool CanUseLocationForDetectBeacons();

		/// <summary>
		/// 端末のBluetooth機能がオフにされている場合に、Bluetooth機能をオンにするためのダイアログを表示します。
		/// UWPには対応する機能がないため、何もしません。
		/// 端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
		/// </summary>
        void RequestToTurnOnBluetooth();

		/// <summary>
		/// 位置情報機能がオフにされている、あるいは許可されていない場合に、iBeaconを検知可能にするための位置情報の使用許可をユーザーに求めるダイアログを表示します。
		/// AndroidとUWPではiBeaconを検知するために位置情報利用の許可が必要ないため、何もしません。
		/// </summary>
		void RequestToAllowUsingLocationForDetectBeacons();

		/// <summary>
		/// 検知対象とするiBeaconと、そのiBeaconを検知したときに実行させる処理を追加します。
		/// 同じiBeaconに対してこのメソッドを続けて実行することで、同じiBeaconを検知したときの処理を複数設定することが可能です。
		/// </summary>
		/// <param name="uuid">検知対象とするiBeaconのUUID</param>
		/// <param name="major">検知対象とするiBeaconのMajor</param>
		/// <param name="minor">検知対象とするiBeaconのMinor</param>
		/// <param name="thresholdRssi">検知扱いとする下限RSSI</param>
		/// <param name="intervalMilliSec">次回の処理実行を待機させる時間（単位はミリ秒）</param>
		/// <param name="callback">iBeaconを検知したときに実行させる処理</param>
        void AddObservableBeaconWithCallback(Guid uuid,
                                             ushort major,
                                             ushort minor,
                                             short thresholdRssi,
                                             int intervalMilliSec,
                                             Action callback);

        /// <summary>
		/// 検知対象とするiBeaconと、そのiBeaconを検知したときに実行させる処理を追加します。
		/// 同じiBeaconに対してこのメソッドを続けて実行することで、同じiBeaconを検知したときの処理を複数設定することが可能です。
		/// </summary>
		/// <param name="uuid">検知対象とするiBeaconのUUID</param>
		/// <param name="major">検知対象とするiBeaconのMajor</param>
		/// <param name="thresholdRssi">検知扱いとする下限RSSI</param>
		/// <param name="intervalMilliSec">次回の処理実行を待機させる時間（単位はミリ秒）</param>
		/// <param name="callback">iBeaconを検知したときに実行させる処理</param>
        void AddObservableBeaconWithCallback(Guid uuid,
                                             ushort major,
                                             short thresholdRssi,
                                             int intervalMilliSec,
                                             Action callback);

        /// <summary>
		/// 検知対象とするiBeaconと、そのiBeaconを検知したときに実行させる処理を追加します。
		/// 同じiBeaconに対してこのメソッドを続けて実行することで、同じiBeaconを検知したときの処理を複数設定することが可能です。
		/// </summary>
		/// <param name="uuid">検知対象とするiBeaconのUUID</param>
		/// <param name="thresholdRssi">検知扱いとする下限RSSI</param>
		/// <param name="intervalMilliSec">次回の処理実行を待機させる時間（単位はミリ秒）</param>
		/// <param name="callback">iBeaconを検知したときに実行させる処理</param>
        void AddObservableBeaconWithCallback(Guid uuid,
                                             short thresholdRssi,
                                             int intervalMilliSec,
                                             Action callback);

        /// <summary>
        /// 検知対象とするiBeaconを追加します。
        /// 同じiBeaconに対してAddObservableBeaconWithCallbackメソッドを実行することで、
        /// 検出時に実行する処理を後から追加することが可能です。
        /// </summary>
        /// <param name="uuid">UUID.</param>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        void AddObservableBeacon(Guid uuid,
                                 ushort major,
                                 ushort minor);

        /// <summary>
        /// 検知対象とするiBeaconを追加します。
        /// 同じiBeaconに対してAddObservableBeaconWithCallbackメソッドを実行することで、
        /// 検出時に実行する処理を後から追加することが可能です。
        /// </summary>
        /// <param name="uuid">UUID.</param>
        /// <param name="major">Major.</param>
        void AddObservableBeacon(Guid uuid,
                                 ushort major);

        /// <summary>
        /// 検知対象とするiBeaconを追加します。
        /// 同じiBeaconに対してAddObservableBeaconWithCallbackメソッドを実行することで、
        /// 検出時に実行する処理を後から追加することが可能です。
        /// </summary>
        /// <param name="uuid">UUID.</param>
        void AddObservableBeacon(Guid uuid);

        /// <summary>
        /// AddEventで設定された検出対象iBeaconと実行処理を全て消去します。
        /// </summary>
        void ClearAllObservableBeacons();

        /// <summary>
        /// 検出されたiBeaconの一覧を初期化します。
        /// </summary>
        void InitializeDetectedBeaconList();

        /// <summary>
        /// iBeaconのスキャンを開始します。
        /// 端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
        /// 端末のBluetooth機能がオフにされている場合、例外BluetoothTurnedOffExceptionをthrowします。
        /// iOSのみ、位置情報サービスの使用が許可されていない場合、例外LocationServiceNotAllowedExceptionをthrowします。
        /// </summary>
        void StartScan();

		/// <summary>
		/// iBeaconのスキャンを停止します。
		/// </summary>
        void StopScan();
    }
}
