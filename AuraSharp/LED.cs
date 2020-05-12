﻿namespace AuraSharp
{
    public class LED
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public LED(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
        public byte[] ToByteArray()
        {
            return new [] {R, G, B};
        }
    }
}