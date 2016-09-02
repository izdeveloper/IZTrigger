using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace Laser
{
    public class Laser
    {
        private OutputPort _laserPin;
        private bool _laserStatus;
        private Timer _switchOffTimer;
        TimerCallback _laserSwitchOff;
        int _timerToSwitchOff = 10 * 60 * 1000; // 10 minutes

        public Laser(Cpu.Pin laser)
        {
            _laserPin = new OutputPort(laser, false);
            _laserStatus = false;
            _laserSwitchOff = null;
            _switchOffTimer = null;
        }

        public bool getStatus()
        {
            return _laserStatus;
        }

        public void laserOn()
        {
            if (_laserStatus)
            {
                if (_switchOffTimer != null)
                    _switchOffTimer.Dispose();
            }
            else 
            {
                _laserPin.Write(true);
                _laserStatus = true;
            }
            _laserSwitchOff = new TimerCallback(this.laserOff);
            _switchOffTimer = new Timer(_laserSwitchOff, null, _timerToSwitchOff, Timeout.Infinite);
        }

        public void laserOff(Object stateInfo)
        {
            if (_switchOffTimer != null)
                _switchOffTimer.Dispose();
            _laserPin.Write(false);
            _laserStatus = false;
        }
    }
}
