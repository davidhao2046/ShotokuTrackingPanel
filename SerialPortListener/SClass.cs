using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortListener
{
    public static class SClass
    {
        public static uint RotateLeft(this uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }
    }
}
