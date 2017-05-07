using System;
using System.Collections.Generic;

namespace Plugin.Beahat.Abstractions
{
    public class iBeaconEventHolder
    {
        public iBeacon ibeacon;
        public List<iBeaconEventDetail> EventList { get; private set; }
        public string BeaconIdentifyStr
        {
            get { return GenerateBeaconIdentifyStr(ibeacon.Uuid, ibeacon.Major, ibeacon.Minor); }
        }


        public iBeaconEventHolder(Guid uuid, ushort major, ushort minor)
        {
            ibeacon = new iBeacon()
            {
                Uuid = uuid,
                Major = major,
                Minor = minor
            };
            EventList = new List<iBeaconEventDetail>();
        }

        public void AddEvent(short thresholdRssi, int eventTriggerIntervalMilliSec, Action function)
        {
            var eventDetail = new iBeaconEventDetail(thresholdRssi, eventTriggerIntervalMilliSec, function);
            EventList.Add(eventDetail);
        }

        public static string GenerateBeaconIdentifyStr(Guid uuid, ushort major, ushort minor)
        {
            return uuid.ToString().ToUpper() + "_" + major.ToString() + "_" + minor.ToString();
        }

        public static string GenerateBeaconIdentifyStr(iBeacon ibeacon)
        {
            return ibeacon.Uuid.ToString().ToUpper() + "_" + ibeacon.Major.ToString() + "_" + ibeacon.Minor.ToString();
        }
    }
}
