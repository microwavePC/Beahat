using CoreBluetooth;
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

        public List<iBeacon> DetectedBeaconList
        {
            get { return new List<iBeacon>(_detectedBeaconDict?.Values); }
        }

        #endregion
        

        #region FIELDS

        private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
        private Dictionary<string, iBeacon> _detectedBeaconDict;
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
            _detectedBeaconDict = new Dictionary<string, iBeacon>();
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
                        _canUseLocation = true;
                        return;
                    case CLAuthorizationStatus.Denied:
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

        public bool BluetoothIsAvailableOnThisDevice()
        {
            bool locationServiceSupported = CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion));
            bool bluetoothSupported = !(_bluetoothAvailability == CBCentralManagerState.Unsupported);

            return locationServiceSupported && bluetoothSupported;
        }


        public bool BluetoothIsEnableOnThisDevice()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                return false;
            }

            return _canUseLocation && (_bluetoothAvailability == CBCentralManagerState.PoweredOn);
        }


        public void RequestUserToTurnOnBluetooth()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }

            new CLLocationManager().RequestWhenInUseAuthorization();
            new CBCentralManager(null, null, new CBCentralInitOptions() { ShowPowerAlert = true });
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action func)
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


        public void AddEvent(Guid uuid, ushort major, ushort minor)
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


        public void ClearAllEvent()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDict = new Dictionary<string, iBeacon>();
        }


        public void StartScan()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }

            if (!CLLocationManager.RegionMonitoringEnabled)
            {
                throw new BluetoothTurnedOffException("Bluetooth service on this device is turned off.");
            }

            if (IsScanning)
            {
                return;
            }

            _detectedBeaconDict = new Dictionary<string, iBeacon>();

            foreach (var eventHolder in _beaconEventHolderDict)
            {
                var uuid = new NSUuid(eventHolder.Value.ibeacon.Uuid.ToString());
                var beaconRegion = new CLBeaconRegion(uuid,
                                                      eventHolder.Value.ibeacon.Major,
                                                      eventHolder.Value.ibeacon.Minor,
                                                      eventHolder.Value.BeaconIdentifyStr);

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
                var uuid = new NSUuid(eventHolder.Value.ibeacon.Uuid.ToString());
                var beaconRegion = new CLBeaconRegion(uuid,
                                                      eventHolder.Value.ibeacon.Major,
                                                      eventHolder.Value.ibeacon.Minor,
                                                      eventHolder.Value.BeaconIdentifyStr);

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

                if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier))
                {
                    continue;
                }

                iBeaconEventHolder eventHolder = _beaconEventHolderDict[beaconIdentifier];
                
                if (_detectedBeaconDict.ContainsKey(beaconIdentifier))
                {
                    iBeacon detectedBeaconPrev = _detectedBeaconDict[beaconIdentifier];
                    short? rssiPrev = detectedBeaconPrev.Rssi;

                    if (rssiPrev == null || ((short)rssiPrev < detectedBeacon.Rssi))
                    {
                        eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
                        if (detectedBeacon.Accuracy > 0.0)
                        {
                            eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
                        }
                        _detectedBeaconDict[beaconIdentifier] = eventHolder.ibeacon;
                    }
                }
                else
                {
                    eventHolder.ibeacon.Rssi = (short)detectedBeacon.Rssi;
                    if (detectedBeacon.Accuracy > 0.0)
                    {
                        eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.Accuracy;
                    }
                    _detectedBeaconDict.Add(beaconIdentifier, eventHolder.ibeacon);
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

        #endregion

    }
}