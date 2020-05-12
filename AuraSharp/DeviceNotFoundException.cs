using System;

namespace AuraSharp
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException()
        {
        }

        public DeviceNotFoundException(string exception):base(exception)
        {
        }
        
        public DeviceNotFoundException(string exception, Exception inner):base(exception, inner)
        {
        }
    }
}