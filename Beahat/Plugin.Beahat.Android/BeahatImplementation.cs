﻿using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Plugin.Beahat;
using Plugin.Beahat.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Android.OS;

[assembly: Dependency(typeof(BeahatImplementation))]
namespace Plugin.Beahat
{
	/// <summary>
	/// Implementation for Feature
	/// </summary>
	public class BeahatImplementation : BindableBase, IBeahat
	{

		#region PRERTIES

		private bool _isScanning = false;
		public bool IsScanning
		{
			get { return _isScanning; }
			set { SetProperty(ref _isScanning, value); }
		}

		public List<iBeacon> DetectedBeaconListFromClosestApproachedInfo
		{
			get
			{
				if (OS_VER < BuildVersionCodes.Lollipop)
				{
					return new List<iBeacon>(_scanCallbackOld.DetectedBeaconDictFromClosestApproachedInfo.Values);
				}
				else
				{
					return new List<iBeacon>(_scanCallbackNew.DetectedBeaconDictFromClosestApproachedInfo.Values);
				}
			}
		}

		public List<iBeacon> DetectedBeaconListFromLastApproachedInfo
        {
            get
			{
				if (OS_VER < BuildVersionCodes.Lollipop)
				{
					return new List<iBeacon>(_scanCallbackOld.DetectedBeaconDictFromLastApproachedInfo.Values);
				}
				else
				{
					return new List<iBeacon>(_scanCallbackNew.DetectedBeaconDictFromLastApproachedInfo.Values);
				}
            }
        }

		#endregion


		#region FIELDS

		private BluetoothManager _btManager;
		private BluetoothAdapter _btAdapter;
		private BluetoothLeScanner _bleScanner;
		private BleScanCallback _scanCallbackNew;
		private LeScanCallback _scanCallbackOld;

		private readonly BuildVersionCodes OS_VER;

		#endregion


		#region CONSTRUCTOR

		public BeahatImplementation()
		{
			OS_VER = Build.VERSION.SdkInt;

			_btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
			_btAdapter = _btManager.Adapter;

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				// 4.4以下はこちら
				_scanCallbackOld = new LeScanCallback();
			}
			else
			{
				// 5.0以上はこちら
				_scanCallbackNew = new BleScanCallback();
				_bleScanner = _btAdapter?.BluetoothLeScanner;
			}
		}

		#endregion


		#region PUBLIC METHODS

        public bool IsAvailableToUseBluetoothOnThisDevice()
		{
			return !(_btAdapter == null);
		}


        public bool IsEnableToUseBluetoothOnThisDevice()
		{
			if (!IsAvailableToUseBluetoothOnThisDevice())
			{
				return false;
			}

			return _btAdapter.IsEnabled;
		}


        public bool IsEnableToUseLocationServiceForDetectingBeacons()
        {
            return true;
        }


        public void RequestUserToAllowUsingLocationServiceForDetectingBeacons()
        {
            // Do nothing on Android.
        }


		public void RequestUserToTurnOnBluetooth()
		{
			if (!IsAvailableToUseBluetoothOnThisDevice())
			{
				throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
			}

			Intent btTurnOnIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
			(Forms.Context as Activity).StartActivity(btTurnOnIntent);
		}


		public void AddObservableBeaconWithCallback(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
		{
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
				_scanCallbackOld.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
			}
			else
			{
				if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
				_scanCallbackNew.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
			}
        }


        public void AddObservableBeaconWithCallback(Guid uuid, ushort major, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (OS_VER < BuildVersionCodes.Lollipop)
            {
                if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
                _scanCallbackOld.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
            }
            else
            {
                if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
                _scanCallbackNew.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
            }
        }


        public void AddObservableBeaconWithCallback(Guid uuid, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (OS_VER < BuildVersionCodes.Lollipop)
            {
                if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
                _scanCallbackOld.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
            }
            else
            {
                if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
                _scanCallbackNew.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
            }
        }


        public void AddObservableBeacon(Guid uuid, ushort major, ushort minor)
		{
			iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
				if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
			}
			else
			{
				if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
				{
					_scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
				}
			}
        }


        public void AddObservableBeacon(Guid uuid, ushort major)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (OS_VER < BuildVersionCodes.Lollipop)
            {
                if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
            }
            else
            {
                if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
            }
        }


        public void AddObservableBeacon(Guid uuid)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (OS_VER < BuildVersionCodes.Lollipop)
            {
                if (!_scanCallbackOld.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackOld.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
            }
            else
            {
                if (!_scanCallbackNew.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
                {
                    _scanCallbackNew.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
                }
            }
        }


        public void ClearAllObservableBeacons()
		{
			if (OS_VER < BuildVersionCodes.Lollipop)
			{
                _scanCallbackOld.BeaconEventHolderDict.Clear();
			}
			else
			{
                _scanCallbackNew.BeaconEventHolderDict.Clear();
			}
		}


		public void InitializeDetectedBeaconList()
		{
			if (OS_VER < BuildVersionCodes.Lollipop)
			{
                _scanCallbackOld.DetectedBeaconDictFromClosestApproachedInfo.Clear();
                _scanCallbackOld.DetectedBeaconDictFromLastApproachedInfo.Clear();
			}
			else
			{
				_scanCallbackNew.DetectedBeaconDictFromClosestApproachedInfo.Clear();
				_scanCallbackNew.DetectedBeaconDictFromLastApproachedInfo.Clear();
			}
        }


		public void StartScan()
		{
			if (!IsAvailableToUseBluetoothOnThisDevice())
			{
				throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
			}

			if (!IsEnableToUseBluetoothOnThisDevice())
			{
				throw new BluetoothTurnedOffException("Bluetooth service on this device is turned off.");
			}

			if (IsScanning)
			{
				return;
			}

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
                #pragma warning disable CS0618 // Type or member is obsolete
				_btAdapter.StartLeScan(_scanCallbackOld);
                #pragma warning restore CS0618 // Type or member is obsolete
			}
			else
			{
				if (_bleScanner == null)
				{
					_bleScanner = _btAdapter.BluetoothLeScanner;
				}
				_bleScanner.StartScan(_scanCallbackNew);
			}
			IsScanning = true;
		}


		public void StopScan()
		{
			if (!IsScanning)
			{
				return;
			}

			if (OS_VER < BuildVersionCodes.Lollipop)
			{
                #pragma warning disable CS0618 // Type or member is obsolete
				_btAdapter.StopLeScan(_scanCallbackOld);
                #pragma warning disable CS0618 // Type or member is obsolete
			}
			else
			{
				_bleScanner.StopScan(_scanCallbackNew);
			}

			IsScanning = false;
		}

		#endregion

	}


	#region CALLBACK FOR ANDROID VER 4.4

	class LeScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
	{

		#region PROPERTIES

		public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }
		public Dictionary<string, iBeacon> DetectedBeaconDictFromClosestApproachedInfo { get; set; }
        public Dictionary<string, iBeacon> DetectedBeaconDictFromLastApproachedInfo { get; set; }

		#endregion


		#region CONSTRUCTOR

		public LeScanCallback()
		{
			BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
			DetectedBeaconDictFromClosestApproachedInfo = new Dictionary<string, iBeacon>();
            DetectedBeaconDictFromLastApproachedInfo = new Dictionary<string, iBeacon>();
		}

        #endregion


        #region EVENT HANDLER

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
		{
			if (iBeaconDroidUtility.IsIBeacon(scanRecord))
			{
				Guid uuid = iBeaconDroidUtility.GetUuidFromRecord(scanRecord);
				ushort major = iBeaconDroidUtility.GetMajorFromRecord(scanRecord);
				ushort minor = iBeaconDroidUtility.GetMinorFromRecord(scanRecord);

				string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, minor);
				string beaconIdentifierForNoMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, null);
				string beaconIdentifierForNoMajorMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, null, null);

				if (!BeaconEventHolderDict.ContainsKey(beaconIdentifier) &&
					!BeaconEventHolderDict.ContainsKey(beaconIdentifierForNoMinor) &&
					!BeaconEventHolderDict.ContainsKey(beaconIdentifierForNoMajorMinor))
				{
					return;
				}

				string[] beaconIdentifierArray = { beaconIdentifier,
												   beaconIdentifierForNoMinor,
												   beaconIdentifierForNoMajorMinor };

                foreach (var beaconId in beaconIdentifierArray)
				{
					iBeaconEventHolder eventHolder = null;
					if (BeaconEventHolderDict.ContainsKey(beaconId))
					{
						eventHolder = BeaconEventHolderDict[beaconId];
					}
					else
					{
						continue;
					}

					if (DetectedBeaconDictFromClosestApproachedInfo.ContainsKey(beaconIdentifier))
					{
						iBeacon detectedBeaconPrev = DetectedBeaconDictFromClosestApproachedInfo[beaconIdentifier];
						short? rssiPrev = detectedBeaconPrev.Rssi;

						if (rssiPrev == null || ((short)rssiPrev < rssi))
						{
							eventHolder.ibeacon.Rssi = (short)rssi;
							eventHolder.ibeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord);
							eventHolder.ibeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(eventHolder.ibeacon.Rssi, eventHolder.ibeacon.TxPower);
							DetectedBeaconDictFromClosestApproachedInfo[beaconIdentifier] = eventHolder.ibeacon;
						}
					}
					else
					{
						eventHolder.ibeacon.Rssi = (short)rssi;
						eventHolder.ibeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord);
						eventHolder.ibeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(eventHolder.ibeacon.Rssi, eventHolder.ibeacon.TxPower);
						DetectedBeaconDictFromClosestApproachedInfo.Add(beaconIdentifier, eventHolder.ibeacon);
					}

                    if (DetectedBeaconDictFromLastApproachedInfo.ContainsKey(beaconIdentifier))
					{
						var thisBeacon = new iBeacon()
						{
							Uuid = uuid,
							Major = major,
							Minor = minor,
							Rssi = (short)rssi,
							TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord),
							EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(
                                (short)rssi,
                                iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord))
						};
                        DetectedBeaconDictFromLastApproachedInfo[beaconIdentifier] = thisBeacon;
                    }
                    else
					{
                        var thisBeacon = new iBeacon()
                        {
                            Uuid = uuid,
                            Major = major,
                            Minor = minor,
                            Rssi = (short)rssi,
							TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord),
							EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(
                                (short)rssi,
                                iBeaconDroidUtility.GetTxPowerFromRecord(scanRecord))
                        };
						DetectedBeaconDictFromLastApproachedInfo.Add(beaconIdentifier, thisBeacon);
                    }

					foreach (var eventDetail in eventHolder.EventList)
					{
						if (eventDetail.ThresholdRssi < rssi &&
							eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
						{
							eventDetail.LastTriggeredDateTime = DateTime.Now;
							eventDetail.Function();
						}
					}
                }
			}
		}

        #endregion

    }

	#endregion


	#region CALLBACK FOR ANDROID VER 5.0 AND OVER

	class BleScanCallback : ScanCallback
	{

		#region PROPERTIES

		public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }
		public Dictionary<string, iBeacon> DetectedBeaconDictFromClosestApproachedInfo { get; set; }
		public Dictionary<string, iBeacon> DetectedBeaconDictFromLastApproachedInfo { get; set; }

		#endregion


		#region CONSTRUCTOR

		public BleScanCallback()
		{
			BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
			DetectedBeaconDictFromClosestApproachedInfo = new Dictionary<string, iBeacon>();
            DetectedBeaconDictFromLastApproachedInfo = new Dictionary<string, iBeacon>();
		}

		#endregion


		#region EVENT HANDLER

		public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
		{
			base.OnScanResult(callbackType, result);

			if (iBeaconDroidUtility.IsIBeacon(result.ScanRecord))
			{
				Guid uuid = iBeaconDroidUtility.GetUuidFromRecord(result.ScanRecord);
				ushort major = iBeaconDroidUtility.GetMajorFromRecord(result.ScanRecord);
				ushort minor = iBeaconDroidUtility.GetMinorFromRecord(result.ScanRecord);

				string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, minor);
				string beaconIdentifierForNoMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, null);
                string beaconIdentifierForNoMajorMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, null, null);

				if (!BeaconEventHolderDict.ContainsKey(beaconIdentifier) &&
					!BeaconEventHolderDict.ContainsKey(beaconIdentifierForNoMinor) &&
					!BeaconEventHolderDict.ContainsKey(beaconIdentifierForNoMajorMinor))
				{
					return;
				}

				string[] beaconIdentifierArray = { beaconIdentifier,
												   beaconIdentifierForNoMinor,
												   beaconIdentifierForNoMajorMinor };

                foreach (var beaconId in beaconIdentifierArray)
                {
                    iBeaconEventHolder eventHolder = null;
                    if (BeaconEventHolderDict.ContainsKey(beaconId))
                    {
                        eventHolder = BeaconEventHolderDict[beaconId];
                    }
                    else
                    {
                        continue;
                    }

					iBeacon holdBeacon = eventHolder.ibeacon;

					if (DetectedBeaconDictFromClosestApproachedInfo.ContainsKey(beaconIdentifier))
					{
						iBeacon detectedBeaconPrev = DetectedBeaconDictFromClosestApproachedInfo[beaconIdentifier];
						short? rssiPrev = detectedBeaconPrev.Rssi;

						if (rssiPrev == null || ((short)rssiPrev < result.Rssi))
						{
							holdBeacon.Rssi = (short)result.Rssi;
							holdBeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord);
							holdBeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(holdBeacon.Rssi, holdBeacon.TxPower);
							DetectedBeaconDictFromClosestApproachedInfo[beaconIdentifier] = holdBeacon;
						}
					}
					else
					{
						holdBeacon.Rssi = (short)result.Rssi;
						holdBeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord);
						holdBeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(holdBeacon.Rssi, holdBeacon.TxPower);
						DetectedBeaconDictFromClosestApproachedInfo.Add(beaconIdentifier, holdBeacon);
					}

					if (DetectedBeaconDictFromLastApproachedInfo.ContainsKey(beaconIdentifier))
					{
						var thisBeacon = new iBeacon()
						{
							Uuid = uuid,
							Major = major,
							Minor = minor,
							Rssi = (short)result.Rssi,
							TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord),
							EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(
                                (short)result.Rssi,
                                iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord))
						};
						DetectedBeaconDictFromLastApproachedInfo[beaconIdentifier] = thisBeacon;
					}
					else
					{
						var thisBeacon = new iBeacon()
						{
							Uuid = uuid,
							Major = major,
							Minor = minor,
							Rssi = (short)result.Rssi,
							TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord),
							EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(
								(short)result.Rssi,
								iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord))
						};
						DetectedBeaconDictFromLastApproachedInfo.Add(beaconIdentifier, thisBeacon);
					}

					foreach (var eventDetail in eventHolder.EventList)
					{
						if (eventDetail.ThresholdRssi < result.Rssi &&
							eventDetail.LastTriggeredDateTime < DateTime.Now.AddMilliseconds(-1 * eventDetail.EventTriggerIntervalMilliSec))
						{
							eventDetail.LastTriggeredDateTime = DateTime.Now;
							eventDetail.Function();
						}
					}
                }
			}
		}

		#endregion

	}

	#endregion

}