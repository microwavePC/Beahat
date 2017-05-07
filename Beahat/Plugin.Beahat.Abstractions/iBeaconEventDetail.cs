using System;

namespace Plugin.Beahat.Abstractions
{
    public class iBeaconEventDetail
    {
        public short ThresholdRssi { get; private set; }
        public int EventTriggerIntervalMilliSec { get; private set; }
        public DateTime LastTriggeredDateTime { get; set; }
        public Action Function { get; private set; }

        public iBeaconEventDetail(short thresholdRssi, int eventTriggerIntervalMilliSec, Action function)
        {
            this.ThresholdRssi = thresholdRssi;
            this.EventTriggerIntervalMilliSec = eventTriggerIntervalMilliSec;
            this.Function = function;
        }
    }
}
