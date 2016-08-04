using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;

namespace LidarReader
{
    class TriggerSender
    {
        public AutoResetEvent autoEvent = new AutoResetEvent(false);
        public Queue triggersQ = new Queue();

        public TriggerSender()
        {
            Debug.Print("TriggerSender constructor");
        }

        public void SendTrigger()
        {
            while (true)
            {
                autoEvent.WaitOne();
                foreach (Object obj in triggersQ)
                    Debug.Print("From Q=" + triggersQ.Dequeue().ToString());
            }
        }
    }
}
