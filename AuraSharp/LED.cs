namespace AuraSharp
{
    public class LED
    {
        public byte R { get; set; } = 0x00;
        public byte G { get; set; }= 0x00;
        public byte B { get; set; }= 0x00;

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