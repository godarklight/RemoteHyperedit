using System;
using System.Net.Sockets;
namespace RémiMod
{
    public class ClientObject
    {
        public TcpClient client;
        public byte[] buffer;
        public int writePos;
    }
}
