using System.Collections.Generic;

namespace AuraSharp
{
    public interface IAuraUsbController
    {
        void DirectControl(IList<LED> leds);
        void DirectControl(IList<LED> leds, bool resetAll);
    }
}