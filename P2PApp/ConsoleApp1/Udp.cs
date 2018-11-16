using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P2PApp
{
    public class Peer
    {
        public string Ip { get; set; }
        public int TTL { get; set; }

        public Peer (string ip, int ttl)
        {
            Ip = ip;
            TTL = ttl;
        }
    }

    public class Udp
    {
        private const int PORT = 15000;
        private const int TIME_OUT = 5;
        private const string SPLIT_STRING = "<split>";

        private UdpClient _udp;
        private IAsyncResult _ar;

        private Thread _workThread;
        private Thread _broadcastThread;
        private object _lockObject = new object();

        private bool _showAlerts;

        private ArrayList _clientList;

        private Tcp _tcp;

        public Udp()
        {
            _tcp = new Tcp();
            try
            {
                _udp = new UdpClient(PORT);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"[ERROR] Port number {PORT} is already used.");
                Application.Exit();
            }
            _clientList = new ArrayList();
#if DEBUG
            _showAlerts = true;
#endif
        }


        public void Start()
        {
            if (_workThread != null)
            {
                throw new Exception("listener thread is already running");
            }
            _workThread = new Thread(new ThreadStart(StartListening));
            _workThread.Start();
            Console.WriteLine("Listener Started.");
        }

        public void StartBroadcaster()
        {
            if (_broadcastThread != null)
            {
                throw new Exception("broadcast thread is already running");
            }
            _broadcastThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Send("check");
                    Thread.Sleep(2000);
                }
            }));
            _broadcastThread.Start();
            Console.WriteLine("Boradcaster Started.");
        }

        private void StartListening()
        {
            _ar = _udp.BeginReceive(Receive, new object());
        }
        
        public void ToggleAlert()
        {
            _showAlerts = !_showAlerts;
            Console.WriteLine($"[Setting]Show alert: {_showAlerts}");
        }

        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, PORT);
            byte[] bytes = _udp.EndReceive(ar, ref ip);
            var message = Encoding.UTF8.GetString(bytes).Split(new string[] { SPLIT_STRING }, StringSplitOptions.RemoveEmptyEntries);
            switch (message[0])
            {
                case "request-ftp":
                    Console.WriteLine($"[Receive] {ip.Address.ToString()} sent request. File: {message[1]}({long.Parse(message[2]).ToAutoByte()})");
                    Console.WriteLine("Accept the request? Y/N");
                    ConsoleKeyInfo cki = Console.ReadKey();
                    if (cki.KeyChar == 'Y' || cki.KeyChar == 'y')
                    {
                        try
                        {
                            _tcp.StartReceiving(ip.Address.ToString(), long.Parse(message[2]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("You declined accept.");
                    }
                    break;
                case "check":
                default:
                    if (_showAlerts)
                        Console.WriteLine($"[Receive] {ip.Address.ToString()}: {message[0]}");
                    break;
            }
            UpdateList(ip.Address.ToString());
            StartListening();
        }

        private void UpdateList(string ip)
        {
            lock(_lockObject)
            {
                foreach (Peer peer in _clientList)
                {
                    if (peer.Ip.Equals(ip))
                    {
                        peer.TTL = TIME_OUT;
                        return;
                    }
                }
                _clientList.Add(new Peer(ip, TIME_OUT));
            }
        }

        public bool ShowList()
        {
            if (_clientList.Count < 1)
            {
                Console.WriteLine("No avaliable client.");
                return false;
            }
            Console.WriteLine("-------- CLIENT LIST --------");
            Console.WriteLine($"PORT: {PORT}, TTL: {TIME_OUT}");
            foreach (Peer peer in _clientList)
            {
                string isAlive;
                if (peer.TTL > 0)
                    isAlive = "Alive";
                else
                    isAlive = "Dead";
                Console.WriteLine($"{peer.Ip} - State: {isAlive}, Time-To-Live: {peer.TTL}");
            }
            return true;
        }

        private void Send(string message)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), PORT);
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
            if (_showAlerts)
                Console.WriteLine("[Boradcast] {0} ", message);
            lock (_lockObject)
            {
                foreach (Peer peer in _clientList)
                {
                    if (peer.TTL > 0)
                        peer.TTL -= 1;
                }
            }
        }

        public void SendRequest()
        {
            try
            {
                Console.WriteLine("Insert client's IP");
                if (!ShowList())
                    return;
                Console.Write("destination: ");
                string clientIp = Console.ReadLine();
#warning 개발 필요 - 유효성 검사
                Console.WriteLine("Select file.");
                FileInfo fileinfo = null;
                OpenFileDialog fileDialog = new OpenFileDialog();
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileinfo = new FileInfo(fileDialog.FileName);
                }
                else
                {
                    Console.WriteLine("You canceled request.");
                    return;
                }
                string message = $"request-ftp{SPLIT_STRING}{fileinfo.FullName}{SPLIT_STRING}{fileinfo.Length}";

                UdpClient client = new UdpClient();
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(clientIp), PORT);
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                client.Send(bytes, bytes.Length, ip);
                client.Close();

                _tcp.StartSend(fileinfo.Name, fileinfo.DirectoryName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Stop()
        {
            if (_broadcastThread == null)
                throw new Exception("boradcast thread is not running");
            else
                _broadcastThread.Abort();

            if (_workThread == null)
                throw new Exception("listener thread is not running");
            else
                _workThread.Abort();
        }
    }
}
