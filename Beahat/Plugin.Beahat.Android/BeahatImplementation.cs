using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
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

        public List<iBeacon> DetectedBeaconList
        {
            get { return new List<iBeacon>(_scanCallback.DetectedBeaconDict.Values); }
        }

        #endregion

        
        #region FIELDS

        private BluetoothManager _btManager;
        private BluetoothAdapter _btAdapter;
        private BluetoothLeScanner _bleScanner;
        private BleScanCallback _scanCallback;

        #endregion
        

        #region CONSTRUCTOR

        public BeahatImplementation()
        {
            _btManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            _btAdapter = _btManager.Adapter;
            _scanCallback = new BleScanCallback();
            
            try
            {
                _bleScanner = _btAdapter.BluetoothLeScanner;
            }
            catch (Exception ex)
            {

            }
        }

        #endregion


        #region PUBLIC METHODS

        public bool BluetoothIsAvailableOnThisDevice()
        {
            return !(_btAdapter == null);
        }


        public bool BluetoothIsEnableOnThisDevice()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                return false;
            }

            return _btAdapter.IsEnabled;
        }


        public void RequestUserToTurnOnBluetooth()
        {
            if (!BluetoothIsAvailableOnThisDevice())
            {
                throw new BluetoothUnsupportedException("This device does not support Bluetooth.");
            }

            Intent btTurnOnIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            (Forms.Context as Activity).StartActivity(btTurnOnIntent);
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor, short thresholdRssi, int intervalMilliSec, Action function)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_scanCallback.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _scanCallback.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
            _scanCallback.BeaconEventHolderDict[eventHolder.BeaconIdentifyStr].AddEvent(thresholdRssi, intervalMilliSec, function);
        }


        public void AddEvent(Guid uuid, ushort major, ushort minor)
        {
            iBeaconEventHolder eventHolder = new iBeaconEventHolder(uuid, major, minor);

            if (!_scanCallback.BeaconEventHolderDict.ContainsKey(eventHolder.BeaconIdentifyStr))
            {
                _scanCallback.BeaconEventHolderDict.Add(eventHolder.BeaconIdentifyStr, eventHolder);
            }
        }


        public void ClearAllEvent()
        {
            _scanCallback.BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            _scanCallback.DetectedBeaconDict = new Dictionary<string, iBeacon>();
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

            if (_bleScanner == null)
            {
                _bleScanner = _btAdapter.BluetoothLeScanner;
            }

            _scanCallback.DetectedBeaconDict = new Dictionary<string, iBeacon>();
            _bleScanner.StartScan(_scanCallback);
            IsScanning = true;
        }


        public void StopScan()
        {
            if (!IsScanning)
            {
                return;
            }

            _bleScanner.StopScan(_scanCallback);
            IsScanning = false;
        }

        #endregion

    }


    class BleScanCallback : ScanCallback
    {

        #region PROPERTIES

        public Dictionary<string, iBeaconEventHolder> BeaconEventHolderDict { get; set; }
        public Dictionary<string, iBeacon> DetectedBeaconDict { get; set; }

        #endregion


        #region CONSTRUCTOR

        public BleScanCallback()
        {
            BeaconEventHolderDict = new Dictionary<string, iBeaconEventHolder>();
            DetectedBeaconDict = new Dictionary<string, iBeacon>();
        }

        #endregion


        #region OVERRIDE METHODS

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);

            if (iBeaconDroidUtility.IsIBeacon(result.ScanRecord))
            {
                Guid uuid = iBeaconDroidUtility.GetUuidFromRecord(result.ScanRecord);
                ushort major = iBeaconDroidUtility.GetMajorFromRecord(result.ScanRecord);
                ushort minor = iBeaconDroidUtility.GetMinorFromRecord(result.ScanRecord);

                string beaconIdentifier = iBeaconEventHolder.GenerateBeaconIdentifyStr(uuid, major, minor);

                if (!BeaconEventHolderDict.ContainsKey(beaconIdentifier))
                {
                    return;
                }

                iBeaconEventHolder eventHolder = BeaconEventHolderDict[beaconIdentifier];
                iBeacon holdBeacon = eventHolder.ibeacon;

                if (DetectedBeaconDict.ContainsKey(beaconIdentifier))
                {
                    iBeacon detectedBeaconPrev = DetectedBeaconDict[beaconIdentifier];
                    short? rssiPrev = detectedBeaconPrev.Rssi;

                    if (rssiPrev == null || ((short)rssiPrev < result.Rssi))
                    {
                        holdBeacon.Rssi = (short)result.Rssi;
                        holdBeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord);
                        holdBeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(holdBeacon.Rssi, holdBeacon.TxPower);
                        DetectedBeaconDict[beaconIdentifier] = holdBeacon;
                    }
                }
                else
                {
                    holdBeacon.Rssi = (short)result.Rssi;
                    holdBeacon.TxPower = iBeaconDroidUtility.GetTxPowerFromRecord(result.ScanRecord);
                    holdBeacon.EstimatedDistanceMeter = iBeaconDroidUtility.CalcDistanceMeterFromRssiAndTxPower(holdBeacon.Rssi, holdBeacon.TxPower);
                    DetectedBeaconDict.Add(beaconIdentifier, holdBeacon);
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

        #endregion

    }
}