﻿using Plugin.Beahat.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BeahatTutorial.ViewModels
{
    public class MainPageViewModel : BindableBase, INavigationAware
    {

        #region PROPERTIES

        private string _uuidStr = "FDA50693-A4E2-4FB1-AFCF-C6EB07647825";
        public string UuidStr
        {
            get { return _uuidStr; }
            set { SetProperty(ref _uuidStr, value); }
        }

        private string _majorStr = "10";
        public string MajorStr
        {
            get { return _majorStr; }
            set { SetProperty(ref _majorStr, value); }
        }

        private string _minorStr = "7";
        public string MinorStr
        {
            get { return _minorStr; }
            set { SetProperty(ref _minorStr, value); }
        }

        private short _thresholdRssi = -65;
        public short ThresholdRssi
        {
            get { return _thresholdRssi; }
            set { SetProperty(ref _thresholdRssi, value); }
        }

        private int _timeIntervalSec = 30;
        public int TimeIntervalSec
        {
            get { return _timeIntervalSec; }
            set { SetProperty(ref _timeIntervalSec, value); }
        }

        private int _eventIndex;
        public int EventIndex
        {
            get { return _eventIndex; }
            set { SetProperty(ref _eventIndex, value); }
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get { return _isScanning; }
            set { SetProperty(ref _isScanning, value); }
        }

        #endregion


        #region FIELDS

        private readonly IBeahat _beahat;
        private readonly IPageDialogService _pageDialogService;

        #endregion


        #region COMMANDS

        public ICommand AddEventCommand { get; }
        public ICommand ClearAllEventCommand { get; }
        public ICommand StartScanCommand { get; }
        public ICommand StopScanCommand { get; }

        #endregion


        #region CONSTRUCTOR

        public MainPageViewModel(IBeahat beahat,
                                 IPageDialogService pageDialogService)
        {
            _beahat = beahat;
            //_beahat = Beahat.Current;
            _pageDialogService = pageDialogService;

            _beahat.PropertyChanged += servicePropertyChanged;

            AddEventCommand = new DelegateCommand(addEvent);
            ClearAllEventCommand = new DelegateCommand(clearAllEvent);
            StartScanCommand = new DelegateCommand(async () => await startScanAsync());
            StopScanCommand = new DelegateCommand(stopScan);
        }

        #endregion


        #region PUBLIC METHODS

        public void OnNavigatedFrom(NavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(NavigationParameters parameters)
		{
            
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {

        }

        #endregion


        #region PRIVATE METHODS

        private void addEvent()
        {
            string errorMessage = string.Empty;

            if (!isValidUuid())
            {
                errorMessage += "Invalid UUID format." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(MajorStr) && !isValidMajor())
            {
                errorMessage += "MAJOR value should be integral number ranging from 1 and 65535." + Environment.NewLine;
            }
            if (!string.IsNullOrEmpty(MinorStr) && !isValidMinor())
            {
                errorMessage += "MINOR value should be integral number ranging from 1 and 65535." + Environment.NewLine;
            }
            if (string.IsNullOrEmpty(MajorStr) && !string.IsNullOrEmpty(MinorStr))
            {
                errorMessage += "Cannot designate MINOR value only, except MAJOR value designating." + Environment.NewLine;
            }

            if (errorMessage != string.Empty)
            {
                _pageDialogService.DisplayAlertAsync("ERROR",
                                                     errorMessage,
                                                     "OK");
                return;
            }

            Action targetEvent = null;
            switch (EventIndex)
            {
                case 0:
                    targetEvent = showAlert;
                    break;
                case 1:
                    targetEvent = null;
                    break;
                case 2:
                    targetEvent = vibrate;
                    break;
                default:
                    return;
            }

            if (string.IsNullOrEmpty(MajorStr) && string.IsNullOrEmpty(MinorStr))
            {
                if (targetEvent == null)
				{
					_beahat.AddObservableBeacon(new Guid(UuidStr));
                }
                else
				{
					_beahat.AddObservableBeaconWithCallback(new Guid(UuidStr),
									 ThresholdRssi,
									 TimeIntervalSec * 1000,
									 targetEvent);
                }
            }
            else if (string.IsNullOrEmpty(MinorStr))
            {
                if (targetEvent == null)
                {
                    _beahat.AddObservableBeacon(new Guid(UuidStr),
                                     ushort.Parse(MajorStr));
                }
                else
                {
                    _beahat.AddObservableBeaconWithCallback(new Guid(UuidStr),
                                     ushort.Parse(MajorStr),
                                     ThresholdRssi,
                                     TimeIntervalSec * 1000,
                                     targetEvent);
                }
            }
            else
            {
                if (targetEvent == null)
                {
                    _beahat.AddObservableBeacon(new Guid(UuidStr),
                                     ushort.Parse(MajorStr),
                                     ushort.Parse(MinorStr));
                }
                else
                {
                    _beahat.AddObservableBeaconWithCallback(new Guid(UuidStr),
                                     ushort.Parse(MajorStr),
                                     ushort.Parse(MinorStr),
                                     ThresholdRssi,
                                     TimeIntervalSec * 1000,
                                     targetEvent);
                }
            }

            _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                 "Added an event.",
                                                 "OK");
        }


        private void clearAllEvent()
        {
            _beahat.ClearAllObservableBeacons();
            _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                 "Deleted all event.",
                                                 "OK");
        }


        private async Task startScanAsync()
        {
            if (!_beahat.SupportsBluetooth())
            {
                await _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                           "This device does not support Bluetooth." + Environment.NewLine +
                                                           "Cannot start scanning on this device.",
                                                           "OK");
                return;
            }
            
            if (!_beahat.IsReadyToUseBluetooth())
            {
                await _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                           "Bluetooth is turned off on this device." + Environment.NewLine +
                                                           "To start scanning, turn on Bluetooth.",
                                                           "OK");
                _beahat.RequestToTurnOnBluetooth();
                return;
            }
            
            if (!_beahat.CanUseLocationForDetectBeacons())
            {
				await _pageDialogService.DisplayAlertAsync("BeahatTutorial",
														   "Location service is not enabled." + Environment.NewLine +
														   "To start scanning, switch the permission setting.",
														   "OK");
				_beahat.RequestToAllowUsingLocationForDetectBeacons();
                return;
            }

            _beahat.InitializeDetectedBeaconList();

            try
			{
				_beahat.StartScan();
            }
            catch(BluetoothUnsupportedException ex1)
			{
				Debug.WriteLine(ex1.Message);
				await _pageDialogService.DisplayAlertAsync("ERROR",
														   "Cannot start scanning because this device does not support Bluetooth.",
														   "OK");
            }
            catch(BluetoothTurnedOffException ex2)
            {
                Debug.WriteLine(ex2.Message);
				await _pageDialogService.DisplayAlertAsync("ERROR",
														   "Cannot start scanning because Bluetooth is turned off." + Environment.NewLine +
														   "Please turn on it to start scanning.",
														   "OK");
				_beahat.RequestToTurnOnBluetooth();
            }
            catch(LocationServiceNotAllowedException ex3)
			{
				Debug.WriteLine(ex3.Message);
				await _pageDialogService.DisplayAlertAsync("ERROR",
														   "Location service is not enabled." + Environment.NewLine +
														   "Please turn it enable to start scanning.",
														   "OK");
				_beahat.RequestToAllowUsingLocationForDetectBeacons();
            }
        }


        private void stopScan()
        {
            _beahat.StopScan();

            string message = "Finished scanning." + Environment.NewLine;
            if (_beahat.BeaconListFromClosestApproachedEvent.Count == 0)
            {
                message += "iBeacon not found.";
            }
            else
            {
                message += "Found iBeacons below:" + Environment.NewLine;
                message += "-------------------------------" + Environment.NewLine;

                int beaconCount = 0;
                foreach (var beacon in _beahat.BeaconListFromClosestApproachedEvent)
                {
                    beaconCount++;
                    message += "//////// " + beaconCount + " ////////" + Environment.NewLine;
                    message += "UUID : " + beacon.Uuid.ToString().ToUpper() + Environment.NewLine;
                    message += "MAJOR : " + beacon.Major + Environment.NewLine;
                    message += "MINOR : " + beacon.Minor + Environment.NewLine;
                    message += "Max RSSI : " + beacon.Rssi + Environment.NewLine;
                    message += "Distance : " + beacon.EstimatedDistanceMeter + "m";
                }
            }

            _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                 message,
                                                 "OK");
        }


        private bool isValidUuid()
        {
            Guid uuid;
            bool result = Guid.TryParse(UuidStr, out uuid);
            return result;
        }


        private bool isValidMajor()
        {
            ushort major;
            bool result = ushort.TryParse(MajorStr, out major);
            return result;
        }


        private bool isValidMinor()
        {
            ushort minor;
            bool result = ushort.TryParse(MinorStr, out minor);

            return result;
        }

        #endregion


        #region EVENTS TRIGGERED BY Beahat

        private void showAlert()
        {
            _pageDialogService.DisplayAlertAsync("BeahatTutorial",
                                                 "This event is triggered by service!",
                                                 "OK");
        }


        private void vibrate()
        {
            /*
            var vibrator = CrossVibrate.Current;
            vibrator.Vibration(1000);
            */
        }


        private void servicePropertyChanged(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning")
            {
                IsScanning = _beahat.IsScanning;
            }
        }

        #endregion

    }
}
