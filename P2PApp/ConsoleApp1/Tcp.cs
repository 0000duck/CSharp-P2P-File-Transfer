using System;
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
    public class Tcp
    {
        private const int PORT = 15001;
        private const long TIME_OUT_MS = 5000;  // 5s
        private const int BUFFER_SIZE = 1024;

        public Tcp()
        {
            
        }
        
        [STAThread]
        public void StartReceiving(string ipAddress, long fileSize)
        {
            BinaryWriter bWrite = null;
            Socket sock = null;
            try
            {
                IPAddress ipaddress = IPAddress.Parse(ipAddress);
                IPEndPoint EP = new IPEndPoint(ipaddress, PORT);
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                Console.WriteLine($"Connecting to server({EP.Address.ToString()})");
                sock.Connect(EP);
                Console.WriteLine($"Downloading file from {sock.RemoteEndPoint.ToString()}...");

                byte[] clientData = new byte[fileSize + 1024];

                int receivedByteLen = sock.Receive(clientData);

                int fileNameLen = BitConverter.ToInt32(clientData, 0);
                string fileName = Encoding.ASCII.GetString(clientData, 4, fileNameLen);

                bWrite = new BinaryWriter(File.Open(AppDomain.CurrentDomain.BaseDirectory + fileName, FileMode.Create));
                bWrite.Write(clientData, 4 + fileNameLen, receivedByteLen - 4 - fileNameLen);
                Console.WriteLine("Download complete!");
                bWrite.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (bWrite != null) try { bWrite.Close(); } catch { }
                if (sock != null) try { sock.Close(); } catch { }
            }
        }

        public void StartSend(string fileName, string filePath)
        {
            IPEndPoint ipEP = new IPEndPoint(IPAddress.Any, PORT);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            sock.Bind(ipEP);
            sock.Listen(100);
            Console.WriteLine($"now Listening to {PORT}...");

            Socket clientSock = sock.Accept();
            Console.WriteLine($"Connected with {clientSock.RemoteEndPoint.ToString()}");

            byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);
            byte[] fileData = File.ReadAllBytes(filePath + "\\" + fileName);
            byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
            byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);

            fileNameLen.CopyTo(clientData, 0);
            fileNameByte.CopyTo(clientData, 4);
            fileData.CopyTo(clientData, 4 + fileNameByte.Length);

            clientSock.Send(clientData);
            Console.WriteLine($"File send complete. ({fileName})");
            clientSock.Close();
        }
    }
}
