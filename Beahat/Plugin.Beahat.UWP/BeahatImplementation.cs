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

        public List<iBeacon> DetectedBeaconList
        {
            get { return new List<iBeacon>(_detectedBeaconDict.Values); }
        }

        #endregion
        

        #region FIELDS

        private Dictionary<string, iBeaconEventHolder> _beaconEventHolderDict;
        private Dictionary<string, iBeacon> _detectedBeaconDict;
        private BluetoothLEAdvertisementWatcher _bleAdvWatcher, _bleAvailabilityChecker;

        #endregion
        

        #region CONSTRUCTOR

        public BeahatImplementation()
        {
            _beaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _detectedBeaconDict = new Dictionary<string, iBeacon>();
            _bleAdvWatcher = new BluetoothLEAdvertisementWatcher();
            _bleAvailabilityChecker = new BluetoothLEAdvertisementWatcher();

            _bleAdvWatcher.Received += bleAdvWatcherReceived;
            _bleAvailabilityChecker.Received += dummyBleAdvWatcherReceived;
        }

        #endregion
        

        #region PUBLIC METHODS

        public bool BluetoothIsAvailableOnThisDevice()
        {
            return true;
        }


        public bool BluetoothIsEnableOnThisDevice()
        {
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


        public void RequestUserToTurnOnBluetooth()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _beaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_beaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _beaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
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

            if (!BluetoothIsEnableOnThisDevice())
            {
                throw new BluetoothTurnedOffException("Bluetooth service on this device is turned off.");
            }

            if (IsScanning)
            {
                return;
            }

            _detectedBeaconDict = new Dictionary<string, iBeacon>();
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

            if (!_beaconEventHolderDict.ContainsKey(beaconIdentifier))
            {
                return;
            }

            iBeaconEventHolder eventHolder = _beaconEventHolderDict[beaconIdentifier];

            if (_detectedBeaconDict.ContainsKey(beaconIdentifier))
            {
                iBeacon detectedBeaconPrev = _detectedBeaconDict[beaconIdentifier];
                short? rssiPrev = detectedBeaconPrev.Rssi;

                if (rssiPrev == null || ((short)rssiPrev < detectedBeacon.Rssi))
                {
                    eventHolder.ibeacon.Rssi = detectedBeacon.Rssi;
                    eventHolder.ibeacon.TxPower = detectedBeacon.TxPower;
                    eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.EstimatedDistanceMeter;
                    _detectedBeaconDict[beaconIdentifier] = eventHolder.ibeacon;
                }
            }
            else
            {
                eventHolder.ibeacon.Rssi = detectedBeacon.Rssi;
                eventHolder.ibeacon.TxPower = detectedBeacon.TxPower;
                eventHolder.ibeacon.EstimatedDistanceMeter = detectedBeacon.EstimatedDistanceMeter;    
                _detectedBeaconDict.Add(beaconIdentifier, eventHolder.ibeacon);
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


        private void dummyBleAdvWatcherReceived(BluetoothLEAdvertisementWatcher s, BluetoothLEAdvertisementReceivedEventArgs e)
        {
			
        }

        #endregion

    }
}