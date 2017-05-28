using Plugin.Beahat;
using Plugin.Beahat.Abstractions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Xamarin.Forms;

[assembly: Dependency(typeof(BeahatImplementation))]
namespace Plugin.Beahat
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class BeahatImplementation : BindableBase, IBeahat
    {

        #region PROPERTIES

        private bool _isScanning = false;
        public bool IsScanning
        {
            get { return _isScanning; }
            private set { SetProperty(ref _isScanning, value); }
        }

        public List<iBeacon> DetectedBeaconListFromClosestApproachedInfo
        {
            get { return new List<iBeacon>(_detectedBeaconDictFromClosestApproachedInfo.Values); }
		}

		public List<iBeacon> DetectedBeaconListFromLastApproachedInfo
		{
			get { return new List<iBeacon>(_detectedBeaconDictFromLastApproachedInfo.Values); }
		}

        #endregion
        

        #region FIELDS

        private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
		private Dictionary<string, iBeacon> _detectedBeaconDictFromClosestApproachedInfo;
		private Dictionary<string, iBeacon> _detectedBeaconDictFromLastApproachedInfo;
        private BluetoothLEAdvertisementWatcher _bleAdvWatcher, _bleAvailabilityChecker;

        #endregion
        

        #region CONSTRUCTOR

        public BeahatImplementation()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDictFromClosestApproachedInfo = new Dictionary<string, iBeacon>();
            _detectedBeaconDictFromLastApproachedInfo = new Dictionary<string, iBeacon>();
            _bleAdvWatcher = new BluetoothLEAdvertisementWatcher();
            _bleAvailabilityChecker = new BluetoothLEAdvertisementWatcher();

            _bleAdvWatcher.Received += bleAdvWatcherReceived;
            _bleAvailabilityChecker.Received += dummyBleAdvWatcherReceived;
        }

        #endregion
        

        #region PUBLIC METHODS

        public bool IsAvailableToUseBluetoothOnThisDevice()
        {
            return true;
        }


        public bool IsEnableToUseBluetoothOnThisDevice()
		{
			if (!IsAvailableToUseBluetoothOnThisDevice())
			{
				return false;
			}

            _bleAvailabilityChecker.Start();
            
            var task = Task.Delay(50);
            task.Wait();

            if (_bleAvailabilityChecker.Status == BluetoothLEAdvertisementWatcherStatus.Started)
            {
                _bleAvailabilityChecker.Stop();
                return true;
            }
            else
            {
                _bleAvailabilityChecker.Stop();
                return false;
            }
        }


        public bool IsEnableToUseLocationServiceForDetectingBeacons()
        {
            return true;
		}


		public void RequestUserToAllowUsingLocationServiceForDetectingBeacons()
		{
			// Do nothing on UWP.
		}


        public void RequestUserToTurnOnBluetooth()
        {
            if (!IsAvailableToUseBluetoothOnThisDevice())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }
        }


        public void AddObservableBeaconWithCallback(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddObservableBeaconWithCallback(Guid uuid, ushort major, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddObservableBeaconWithCallback(Guid uuid, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddObservableBeacon(Guid uuid, ushort major, ushort minor)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
        }


        public void AddObservableBeacon(Guid uuid, ushort major)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
        }


        public void AddObservableBeacon(Guid uuid)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, null, null);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
        }


        public void ClearAllObservableBeacons()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
		}


		public void InitializeDetectedBeaconList()
		{
			_detectedBeaconDictFromClosestApproachedInfo.Clear();
			_detectedBeaconDictFromLastApproachedInfo.Clear();
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

            _bleAdvWatcher.Start();
            IsScanning = true;
        }


        public void StopScan()
        {
            if (!IsScanning)
            {
                return;
            }
            _bleAdvWatcher.Stop();
            IsScanning = false;
        }

        #endregion


        #region PRIVATE METHODS

        private void bleAdvWatcherReceived(BluetoothLEAdvertisementWatcher s, BluetoothLEAdvertisementReceivedEventArgs e)
        {
            iBeacon detectedBeacon = iBeaconUwpUtility.ConvertReceivedDataToBeacon(e);
            if (detectedBeacon == null || detectedBeacon.Rssi == null)
            {
                return;
            }

            string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(detectedBeacon.Uuid, detectedBeacon.Major, detectedBeacon.Minor);
            string beaconIdentifierForNoMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(detectedBeacon.Uuid, detectedBeacon.Major, null);
            string beaconIdentifierForNoMajorMinor = iBeaconEventHolder.GenerateBeaconIdentifyStr(detectedBeacon.Uuid, null, null);

            if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier) &&
                !_beaconEventHolderDict.ContainsKey(beaconIdentifierForNoMinor) &&
                !_beaconEventHolderDict.ContainsKey(beaconIdentifierForNoMajorMinor))
            {
                return;
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
                        eventHolder.ibeacon.Rssi = detectedBeacon.Rssi;
                        eventHolder.ibeacon.TxPower = detectedBeacon.TxPower;
                        eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.EstimatedDistanceMeter;
                        _detectedBeaconDictFromClosestApproachedInfo[beaconIdentifier] = eventHolder.ibeacon;
                    }
                }
                else
                {
                    eventHolder.ibeacon.Rssi = detectedBeacon.Rssi;
                    eventHolder.ibeacon.TxPower = detectedBeacon.TxPower;
                    eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.EstimatedDistanceMeter;
                    _detectedBeaconDictFromClosestApproachedInfo.Add(beaconIdentifier, eventHolder.ibeacon);
                }

                if (_detectedBeaconDictFromLastApproachedInfo.ContainsKey(beaconIdentifier))
				{
                    _detectedBeaconDictFromLastApproachedInfo[beaconIdentifier] = detectedBeacon;
                }
                else
                {
                    _detectedBeaconDictFromLastApproachedInfo.Add(beaconIdentifier, detectedBeacon);
                }

                foreach (var eventDetail in eventHolder.EventList)
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


        private void dummyBleAdvWatcherReceived(BluetoothLEAdvertisementWatcher s, BluetoothLEAdvertisementReceivedEventArgs e)
        {
			
        }

        #endregion

    }
}