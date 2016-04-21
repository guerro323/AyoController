
//---------------------------------------------------//
//
//			Credits : Flo
//
//---------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace ShootManiaXMLRPC.XmlRpc
{
    public class XmlRpc
    {
        private static bool SendRpc(Socket inSocket, byte[] inData)
        {
            int offset = 0;
            int len = inData.Length;
            int bytesSent;
            try
            {
                while (len > 0)
                {
                    bytesSent = inSocket.Send(inData, offset, len, SocketFlags.None);
                    len -= bytesSent;
                    offset += bytesSent;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ReceiveRpc(Socket inSocket, int inLength)
        {
            byte[] data = new byte[inLength];
            int offset = 0;
            byte[] buffer;
            while (inLength > 0)
            {
                int read = Math.Min(inLength, 1024);
                buffer = new byte[read];
                int bytesRead = inSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Array.Copy(buffer, 0, data, offset, buffer.Length);
                inLength -= bytesRead;
                offset += bytesRead;
            }
            return data;
        }

        public static int SendCall(Socket inSocket, GbxCall inCall)
        {
            if (inSocket == null || inCall == null) return 0;
            if (inSocket.Connected)
            {
                lock (inSocket)
                {
                    try
                    {
                        // create request body ...
                        byte[] body = Encoding.UTF8.GetBytes(inCall.Xml);

                        // create response header ...
                        byte[] bSize = BitConverter.GetBytes(body.Length);
                        byte[] bHandle = BitConverter.GetBytes(inCall.Handle);

                        // create call data ...
                        byte[] call = new byte[bSize.Length + bHandle.Length + body.Length];
                        Array.Copy(bSize, 0, call, 0, bSize.Length);
                        Array.Copy(bHandle, 0, call, 4, bHandle.Length);
                        Array.Copy(body, 0, call, 8, body.Length);

                        // send call ...
                        inSocket.Send(call);

                        return inCall.Handle;
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
            throw new NotConnectedException();
        }

        public static GbxCall ReceiveCall(Socket inSocket, byte[] inHeader)
        {
            if (inSocket.Connected)
            {
                lock (inSocket)
                {
                    // read response size and handle ...
                    byte[] bSize = new byte[4];
                    byte[] bHandle = new byte[4];
                    if (inHeader == null)
                    {
                        inSocket.Receive(bSize);
                        inSocket.Receive(bHandle);
                    }
                    else
                    {
                        Array.Copy(inHeader, 0, bSize, 0, 4);
                        Array.Copy(inHeader, 4, bHandle, 0, 4);
                    }
                    int size = BitConverter.ToInt32(bSize, 0);
                    int handle = BitConverter.ToInt32(bHandle, 0);

                    // receive response body ...
                    byte[] data = ReceiveRpc(inSocket, size);

                    // parse the response ...
                    GbxCall call = new GbxCall(handle, data);

                    return call;
                }
            }
            else
            {
                Console.WriteLine("ERROR! <NotConnectedException");
                return null;
                //throw new NotConnectedException();
            }
        }
    }
}
