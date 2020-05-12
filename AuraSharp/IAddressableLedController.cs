using System.Collections.Generic;

namespace AuraSharp
{
    public interface IAddressableLedController
    {
        public void SetLeds(List<LED> leds, int deviceIndex);
    }
}