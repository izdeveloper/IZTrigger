using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace StatusLED
{
    public class StatusLED
    {
        private OutputPort _red ;
        private OutputPort _green ;
        private OutputPort _blue ;

        public StatusLED(Cpu.Pin red, Cpu.Pin green, Cpu.Pin blue)
        {
            _red = new OutputPort(red, false);
            _green = new OutputPort(green, false);
            _blue = new OutputPort(blue, false);
        }

        public void redLED ()
        {
            _red.Write(true);
            _green.Write(false);
            _blue.Write(false);
        }
        public void greenLED()
        {
            _red.Write(false);
            _green.Write(true);
            _blue.Write(false);
        }
        public void blueLED()
        {
            _red.Write(false);
            _green.Write(false);
            _blue.Write(true);
        }

        public void yellowLED()
        {
            _red.Write(true);
            _green.Write(true);
            _blue.Write(false);

        }

        public void cyanLED()
        {
            _red.Write(false);
            _green.Write(true);
            _blue.Write(true);

        }

        public void allLEDOff()
        {
            _red.Write(false);
            _green.Write(false);
            _blue.Write(false);
        }

        public void allLEDOn()
        {
            _red.Write(true);
            _green.Write(true);
            _blue.Write(true);
        }
    }
}
