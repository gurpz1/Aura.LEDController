using System;

namespace AuraSharp
{
    public class ArraySizeException:Exception
    {
        public ArraySizeException()
        {
        }
        
        public ArraySizeException(string message):base(message)
        {
        }
        
        public ArraySizeException(string message, Exception inner):base(message, inner)
        {
        }
    }
}