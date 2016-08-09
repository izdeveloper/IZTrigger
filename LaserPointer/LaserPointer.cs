using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace LaserPointer
{
    public class LaserPointer
    {
        private OutputPort _laserpointer;
        private bool _status;
        public LaserPointer(Cpu.Pin laser)
        {
            _laserpointer = new OutputPort(laser, false);
            _status = false;
        }

        public void turnLaserPointerOn()
        {
            _laserpointer.Write(true);
            _status = true;
        }

        public void turnLaserPointerOff()
        {
            _laserpointer.Write(false);
            _status = false;
        }

        public bool getStatus()
        {
            return _status;
        }
    }
}
