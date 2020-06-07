using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using HidSharp;
using Microsoft.Extensions.Logging;

namespace AuraSharp
{
    public class AuraUsbController:IAuraUsbController
    {
        private ILogger<AuraUsbController> _logger;
        private HidDevice _auraDevice;
        public AuraDeviceInfo AuraDeviceInfo { get; }

        public static UInt16 AsusVendorId { get; } = 0x0B05;
        public static byte MaxLeds { get; } = 120;

        private byte _messageHeader = 0xEC;
        private byte _messageLength = 65;
        private byte _ledsPerMessage = 20; // 60 bytes, 3 bytes per led [R,G,B]
            
        public AuraUsbController(ILogger<AuraUsbController> logger, int deviceIndex)
        {
            _logger = logger;
            
            var localDevices = DeviceList.Local;
            var auraDevices = localDevices.GetHidDevices(AsusVendorId)
                .Where(x => x.GetProductName() == "AURA LED Controller").ToList();

            if (auraDevices.Count < 1)
            {
                throw new DeviceNotFoundException("Unable to find LED Controller");
            }

            try
            {
                _auraDevice = auraDevices[deviceIndex];
            }
            catch
            {
                throw new DeviceNotFoundException($"Device index {deviceIndex} not found");
            }

            AuraDeviceInfo = GetDeviceInfo();
            SendInitMessage();
            _logger.LogInformation($"Initialised Aura device");
        }

        public AuraDeviceInfo GetDeviceInfo()
        {
            // Get device name
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x82;
            byte[] recieved = new byte[65];
            using (var deviceStream = _auraDevice.Open())
            {
                deviceStream.Write(message);
                deviceStream.Read(recieved);
            }

            string deviceName = "";
            if (recieved[1] == 0x02)
            {
                deviceName =  Encoding.Default.GetString(recieved.Skip(2).Take(16).ToArray());
            }
            else
            {
                throw new DeviceNotFoundException("Unable to determine device name");
            }
            return new AuraDeviceInfo(deviceName);
        }

        public void DirectControl(IList<LED> leds)
        {
            DirectControl(leds, false);
        }

        public void DirectControl(IList<LED> leds, bool resetAll)
        {
            if (leds.Count > MaxLeds)
            {
                throw new ArraySizeException($"You are trying to control too many LEDs. The limit is {MaxLeds}");
            }

            if (resetAll)
            {
                // turn off leds
                IList<LED> off = new List<LED>(MaxLeds);
                for(var i = 0; i<MaxLeds; i++)
                {
                    off.Insert(i, new LED(0,0,0));
                }
                SetLeds(off);
                SendApplyMessage();                
            }

            SetLeds(leds);
            SendApplyMessage();
        }

        private void SetLeds(IList<LED> leds)
        {
            int index = 0;
            
            while (index < leds.Count)
            {
                int remaining = leds.Count - index;
                if (remaining >= _ledsPerMessage)
                {
                    SendLedMessage(index, leds.Skip(index).Take(_ledsPerMessage));
                }
                else
                {
                    SendLedMessage(index, leds.Skip(index).Take(_ledsPerMessage-remaining));
                }

                index += _ledsPerMessage;
            }
        }

        private void SendLedMessage(int startLed, IEnumerable<LED> leds)
        {
            byte [] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x00;
            message[3] = (byte) startLed;
            message[4] = (byte) leds.Count();

            int index = 5;
            foreach (var led in leds)
            {
                byte[] rgb = led.ToByteArray();
                rgb.CopyTo(message, index);
                index += 3;
            }
            SendMessage(message);
        }

        private void SendInitMessage()
        {
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x35;
            message[5] = (byte) AuraEffect.DIRECT;
            using (var deviceStream = _auraDevice.Open())
            {
                deviceStream.Write(message, 0, _messageLength);
            }
            
            message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x84;
            message[4] = 0x02;
            SendMessage(message);;
            
            message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = 0x35;
            message[2] = 0x01;
            message[5] = (byte) AuraEffect.DIRECT;
            SendMessage(message);   
        }

        private void SendApplyMessage()
        {
            byte[] message = new byte[_messageLength];
            message[0] = _messageHeader;
            message[1] = (byte) AuraMode.DIRECT;
            message[2] = 0x80;
            SendMessage(message);
        }

        private void SendMessage(byte[] message)
        {
            using (var deviceStream = _auraDevice.Open())
            {
                deviceStream.Write(message,0, _messageLength);
            }
        }
        
    }
} 