using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2PApp
{
    public class StartUp
    {
        private Udp _udp;
        public StartUp(string[] args)
        {
            // TODO
            _udp = new Udp();

            // Search firewall rights
        }

        public void Run()
        {
            _udp.Start();
            _udp.StartBroadcaster();

            ConsoleKeyInfo cki;
            do
            {
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey(true);
                    switch (cki.KeyChar)
                    {
                        case 't':
                            _udp.ToggleAlert();
                            break;
                        case 'l':
                            _udp.ShowList();
                            break;
                        case 'r':
                            _udp.SendRequest();
                            break;
                        case 'x':
                            _udp.Stop();
                            return;
                    }
                }
                Thread.Sleep(10);
            } while (true);
        }
    }
}
