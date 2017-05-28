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


        public iBeaconEventHolder(Guid uuid, ushort? major, ushort? minor)
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

        public static string GenerateBeaconIdentifyStr(Guid uuid, ushort? major, ushort? minor)
        {
            string majorStr, minorStr;
            majorStr = major.HasValue ? major.ToString() : "x";
            minorStr = minor.HasValue ? minor.ToString() : "x";

            return uuid.ToString().ToUpper() + "_" + majorStr + "_" + minorStr;
        }

        public static string GenerateBeaconIdentifyStr(iBeacon ibeacon)
        {
            return GenerateBeaconIdentifyStr(ibeacon.Uuid, ibeacon.Major, ibeacon.Minor);
        }
    }
}
