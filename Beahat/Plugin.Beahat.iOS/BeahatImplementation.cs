﻿using CoreBluetooth;
using CoreLocation;
using Foundation;
using Plugin.Beahat;
using Plugin.Beahat.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeahatImplementation))]
namespace Plugin.Beahat
{
    /// <summary>
    /// Implementation for Beahat
    /// </summary>
    public class BeahatImplementation : BindableBase, IBeahat
    {

        #region PROPERTIES

        private bool _isScanning = false;
        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        public List<iBeacon> BeaconListFromClosestApproachedEvent
        {
            get { return new List<iBeacon>(_detectedBeaconDictFromClosestApproachedInfo?.Values); }
		}

		public List<iBeacon> BeaconListFromLastApproachedEvent
		{
			get { return new List<iBeacon>(_detectedBeaconDictFromLastApproachedInfo?.Values); }
		}

        #endregion
        

        #region FIELDS

        private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
		private Dictionary<string, iBeacon> _detectedBeaconDictFromClosestApproachedInfo;
		private Dictionary<string, iBeacon> _detectedBeaconDictFromLastApproachedInfo;
        private CLLocationManager _locationManager;
        private CLLocationManager _locationAvailabilityChecker;
        private bool _canUseLocation;
        private CBCentralManager _bluetoothAvailabilityChecker;
        private CBCentralManagerState _bluetoothAvailability;

        #endregion
        

        #region CONSTRUCTOR

        public BeahatImplementation()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDictFromClosestApproachedInfo = new Dictionary<string, iBeacon>();
            _detectedBeaconDictFromLastApproachedInfo = new Dictionary<string, iBeacon>();
            _locationManager = new CLLocationManager();
            _locationAvailabilityChecker = new CLLocationManager();
            _canUseLocation = true;
            _bluetoothAvailabilityChecker = new CBCentralManager();
            _bluetoothAvailability = CBCentralManagerState.Unknown;

            _locationManager.DidRangeBeacons += didRangeBeacons;

            _locationAvailabilityChecker.RangingBeaconsDidFailForRegion += (s, e) =>
            {
                switch (e.Error.Code)
                {
                    case (long)CLError.Denied:
                    case (long)CLError.RangingFailure:
                        _canUseLocation = false;
                        return;
                    default:
                        return;
                }
            };

            _locationAvailabilityChecker.AuthorizationChanged += (s, e) =>
            {
                switch (e.Status)
                {
                    case CLAuthorizationStatus.AuthorizedAlways:
                    case CLAuthorizationStatus.AuthorizedWhenInUse:
                        // 位置情報の取得が常時許可されている
                        // 位置情報の取得がアプリ使用時のみ許可されている
                        _canUseLocation = true;
						return;
					case CLAuthorizationStatus.NotDetermined:
					case CLAuthorizationStatus.Denied:
					case CLAuthorizationStatus.Restricted:
                        // 位置情報の使用許可に関して、まだ選択がされていない
                        // 位置情報サービスが無効にされている
                        // 位置情報サービスを利用できない
                        _canUseLocation = false;
                        return;
                    default:
                        return;
                }
            };

            _bluetoothAvailabilityChecker.UpdatedState += (s, e) =>
            {
                _bluetoothAvailability = ((CBCentralManager)s).State;
            };
        }

        #endregion


        #region PUBLIC METHODS

        public bool SupportsBluetooth()
        {
            bool locationServiceSupported = CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion));
            bool bluetoothSupported = !(_bluetoothAvailability == CBCentralManagerState.Unsupported);

            return locationServiceSupported && bluetoothSupported;
        }


        public bool IsReadyToUseBluetooth()
        {
            if (!SupportsBluetooth())
            {
                return false;
            }

            return (_bluetoothAvailability == CBCentralManagerState.PoweredOn);
        }


        public bool CanUseLocationForDetectBeacons()
        {
            return _canUseLocation;
        }


        public void RequestToTurnOnBluetooth()
        {
            if (!SupportsBluetooth())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }

			new CBCentralManager(null, null, new CBCentralInitOptions() { ShowPowerAlert = true });
		}


		public void RequestToAllowUsingLocationForDetectBeacons()
		{
			new CLLocationManager().RequestWhenInUseAuthorization();
		}


        public void AddObservableBeaconWithCallback(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action func)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, minor, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddObservableBeaconWithCallback(Guid uuid, ushort major, short thresholdRssi, int intervalMilliSec, Action func)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddObservableBeaconWithCallback(Guid uuid, short thresholdRssi, int intervalMilliSec, Action func)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, func);

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddObservableBeacon(Guid uuid, ushort major, ushort minor)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, minor, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddObservableBeacon(Guid uuid, ushort major)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, major, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void AddObservableBeacon(Guid uuid)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }

            if (IsScanning)
            {
                var nsUuid = new NSUuid(uuid.ToString());
                var beaconRegion = new CLBeaconRegion(nsUuid, eventHolder.BeaconIdentifyStr);
                _locationManager.StartRangingBeacons(beaconRegion);
            }
        }


        public void ClearAllObservableBeacons()
        {
            _beaconEventHolderDict.Clear();
        }


        public void InitializeDetectedBeaconList()
		{
			_detectedBeaconDictFromClosestApproachedInfo.Clear();
			_detectedBeaconDictFromLastApproachedInfo.Clear();
        }


        public void StartScan()
        {
            if (!SupportsBluetooth())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }

            if (!IsReadyToUseBluetooth())
            {
                throw new BluetoothTurnedOffException("Bluetooth service on this device is turned off.");
            }
            
            if (!CanUseLocationForDetectBeacons())
            {
                throw new LocationServiceNotAllowedException("Location service is not allowed for this device or app.");
            }

            if (IsScanning)
            {
                return;
            }

            foreach (var eventHolder in _beaconEventHolderDict)
            {
                iBeacon targetBeacon = eventHolder.Value.ibeacon;

                var uuid = new NSUuid(targetBeacon.Uuid.ToString());
                ushort? major = targetBeacon.Major;
                ushort? minor = targetBeacon.Minor;

                CLBeaconRegion beaconRegion;

                if (major.HasValue && minor.HasValue)
                {
                    beaconRegion = new CLBeaconRegion(uuid, (ushort)major, (ushort)minor, eventHolder.Value.BeaconIdentifyStr);
                }
                else if (major.HasValue)
                {
                    beaconRegion = new CLBeaconRegion(uuid, (ushort)major, eventHolder.Value.BeaconIdentifyStr);
                }
                else
                {
                    beaconRegion = new CLBeaconRegion(uuid, eventHolder.Value.BeaconIdentifyStr);
                }

                _locationManager.StartRangingBeacons(beaconRegion);
            }

            IsScanning = true;
        }


        public void StopScan()
        {
            if (!IsScanning)
            {
                return;
            }

            foreach (var eventHolder in _beaconEventHolderDict)
            {
                iBeacon targetBeacon = eventHolder.Value.ibeacon;

                var uuid = new NSUuid(targetBeacon.Uuid.ToString());
                ushort? major = targetBeacon.Major;
                ushort? minor = targetBeacon.Minor;

                CLBeaconRegion beaconRegion;

                if (major.HasValue && minor.HasValue)
                {
                    beaconRegion = new CLBeaconRegion(uuid, (ushort)major, (ushort)minor, eventHolder.Value.BeaconIdentifyStr);
                }
                else if (major.HasValue)
                {
                    beaconRegion = new CLBeaconRegion(uuid, (ushort)major, eventHolder.Value.BeaconIdentifyStr);
                }
                else
                {
                    beaconRegion = new CLBeaconRegion(uuid, eventHolder.Value.BeaconIdentifyStr);
                }

                _locationManager.StopRangingBeacons(beaconRegion);
            }

            IsScanning = false;
        }

        #endregion
        

        #region PRIVATE METHODS

        private void didRangeBeacons(object s, CLRegionBeaconsRangedEventArgs e)
        {
            foreach (var detectedBeacon in e.Beacons)
            {
                string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(
					new Guid(detectedBeacon.ProximityUuid.AsString().ToUpper()),
                    detectedBeacon.Major.UInt16Value,
                    detectedBeacon.Minor.UInt16Value);

                string beaconIdentifierForNoMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(
                    new Guid(detectedBeacon.ProximityUuid.AsString().ToUpper()),
                    detectedBeacon.Major.UInt16Value, null);

                string beaconIdentifierForNoMajorMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(
                    new Guid(detectedBeacon.ProximityUuid.AsString().ToUpper()), null, null);

                if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier) &&
                    !_beaconEventHolderDict.ContainsKey(beaconIdentifierForNoMinor) &&
                    !_beaconEventHolderDict.ContainsKey(beaconIdentifierForNoMajorMinor))
                {
                    continue;
                }

                string[] beaconIdentifierArray = { beaconIdentifier,
                                                   beaconIdentifierForNoMinor,
                                                   beaconIdentifierForNoMajorMinor };

                foreach (var beaconId in beaconIdentifierArray)
				{
					iBeaconEventHolder eventHolder = null;
					if (_beaconEventHolderDict.ContainsKey(beaconId))
					{
						eventHolder = _beaconEventHolderDict[beaconId];
					}
					else
					{
						continue;
					}

					if (_detectedBeaconDictFromClosestApproachedInfo.ContainsKey(beaconIdentifier))
					{
						iBeacon detectedBeaconPrev = _detectedBeaconDictFromClosestApproachedInfo[beaconIdentifier];
						short? rssiPrev = detectedBeaconPrev.Rssi;

						if (rssiPrev == null || ((short)rssiPrev < detectedBeacon.Rssi))
						{
							eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
							if (detectedBeacon.Accuracy > 0.0)
							{
								eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
							}
							_detectedBeaconDictFromClosestApproachedInfo[beaconIdentifier] = eventHolder.ibeacon;
						}
					}
					else
					{
						eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
						if (detectedBeacon.Accuracy > 0.0)
						{
							eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
						}
						_detectedBeaconDictFromClosestApproachedInfo.Add(beaconIdentifier, eventHolder.ibeacon);
					}

                    if (_detectedBeaconDictFromLastApproachedInfo.ContainsKey(beaconIdentifier))
                    {
                        _detectedBeaconDictFromLastApproachedInfo[beaconIdentifier] = new iBeacon()
                        {
                            Uuid = new Guid(detectedBeacon.ProximityUuid.AsString()),
                            Major = detectedBeacon.Major.UInt16Value,
                            Minor = detectedBeacon.Minor.UInt16Value,
                            Rssi = (short)detectedBeacon.Rssi,
                            EstimatedDistanceMeter = detectedBeacon.Accuracy
                        };
                    }
                    else
                    {
                        _detectedBeaconDictFromLastApproachedInfo.Add(
                            beaconIdentifier,
                            new iBeacon()
							{
								Uuid = new Guid(detectedBeacon.ProximityUuid.AsString()),
								Major = detectedBeacon.Major.UInt16Value,
								Minor = detectedBeacon.Minor.UInt16Value,
								Rssi = (short)detectedBeacon.Rssi,
								EstimatedDistanceMeter = detectedBeacon.Accuracy
                            });
                    }

					foreach (iBeaconEventDetail eventDetail in eventHolder.EventList)
					{
						if (eventDetail.ThresholdRssi < detectedBeacon.Rssi &&
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
}