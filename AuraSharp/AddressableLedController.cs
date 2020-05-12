using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using Microsoft.Extensions.Logging;

namespace AuraSharp
{
    public class AddressableLedController:IAddressableLedController
    {
        private ILogger<AddressableLedController> _logger;
        private IList<HidDevice> _auraDevices;

        public static UInt16 AsusVendorId { get; } = 0x0B05;
        public static byte MaxLeds { get; } = 120;

        private byte _messageHeader = 0xEC;
        private byte _messageLength = 65;
        private byte _ledsPerMessage = 20; // 60 bytes, 3 bytes per led [R,G,B]
            
        public AddressableLedController(ILogger<AddressableLedController> logger)
        {
            _logger = logger;
            
            var localDevices = DeviceList.Local;
            _auraDevices = localDevices.GetHidDevices(0x0B05)
                .Where(x => x.GetProductName() == "AURA LED Controller").ToList();

            if (_auraDevices.Count < 1)
            {
                throw new DeviceNotFoundException("Unable to find LED Controller");
            }
            _logger.LogInformation($"Number found: {_auraDevices.Count}");
        }

        public void SetLeds(List<LED> leds, int deviceIndex)
        {
            if (leds.Count > MaxLeds)
            {
                throw new ArraySizeException("You are trying to control too many LEDs");
            }
            SendInitMessage(deviceIndex);
            int index = 0;
            while (index < leds.Count)
            {
                int remaining = leds.Count - index; 
                if(remaining > _ledsPerMessage)
                {
                    SendLedMessage(index, leds.GetRange(index, _ledsPerMessage), deviceIndex);
                }
                else
                {
                    SendLedMessage(index, leds.GetRange(index, _ledsPerMessage-remaining), deviceIndex);
                }
                index += _ledsPerMessage;
            }
            SendCompleteMessage(deviceIndex);
        }
        #region Send data
        private void SendLedMessage(int startLed, List<LED> leds, int deviceIndex)
        {
            byte[] message = new byte[_messageLength];
            Array.Fill(message, Byte.MinValue);
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] =  (byte) deviceIndex;
            message[3] = (byte) startLed;
            message[4] = (byte) (_ledsPerMessage - startLed);

            int index = 5;
            foreach (var led in leds)
            {
                byte[] rgb = led.ToByteArray();
                rgb.CopyTo(message,index);
                index += 3;
            }
            SendMessage(message, deviceIndex);
        }

        private void SendCompleteMessage(int deviceIndex)
        {
            byte[] message = new byte[_messageLength];
            Array.Fill(message, Byte.MinValue);
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x80;
            SendMessage(message, deviceIndex);
        }
        
        private void SendInitMessage(int deviceIndex)
        {
            byte[] message = new byte[_messageLength];
            Array.Fill(message, Byte.MinValue);
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.EFFECT;
            message[4] = 255;
            SendMessage(message, deviceIndex);
        }
        private void SendMessage(byte[] message, int deviceIndex)
        {
            if (message.Length > 65)
            {
                throw new ArraySizeException("Byte array is too big to send to LED Controller");
            }

            using (var deviceStream = _auraDevices[deviceIndex].Open())
            {
                _logger.LogTrace($"Device is writeable? {deviceStream.CanWrite}");
                deviceStream.Write(message, 0, _messageLength);
            }
        }
        #endregion
    }
}