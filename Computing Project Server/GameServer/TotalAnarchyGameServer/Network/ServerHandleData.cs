using System;
using System.Collections.Generic;

namespace TotalAnarchyGameServer {
    public class ServerHandleData{
        private delegate void Packet_(long connectionID, byte[] data);
        static Dictionary<long, Packet_> packets;
        static long pLength;

        public static void InitializePackets()
        {
            Logger.WriteDebug("Initializing Network Packets...");
            packets = new Dictionary<long, Packet_>
            {
                { (long)ClientPackets.CDisconnect, PACKET_DISCONNECT },
                { (long)ClientPackets.CPlayerMovement, PACKET_PLAYERMOVEMENT },
            };
        }

        public static void HandleData(long connectionID, byte[] data)
        {
            byte[] Buffer;
            Buffer = (byte[])data.Clone();

            if (ServerTCP.Client[connectionID].playerBuffer == null)
            {
                ServerTCP.Client[connectionID].playerBuffer = new ByteBuffer();
            }
            ServerTCP.Client[connectionID].playerBuffer.WriteBytes(Buffer);

            if (ServerTCP.Client[connectionID].playerBuffer.Count() == 0)
            {
                ServerTCP.Client[connectionID].playerBuffer.Clear();
                return;
            }

            if (ServerTCP.Client[connectionID].playerBuffer.Length() >= 8)
            {
                pLength = ServerTCP.Client[connectionID].playerBuffer.ReadLong(false);
                if (pLength <= 0)
                {
                    ServerTCP.Client[connectionID].playerBuffer.Clear();
                    return;
                }
            }

            while (pLength > 0 & pLength <= ServerTCP.Client[connectionID].playerBuffer.Length() - 8)
            {
                if (pLength <= ServerTCP.Client[connectionID].playerBuffer.Length() - 8)
                {
                    ServerTCP.Client[connectionID].playerBuffer.ReadLong();
                    data = ServerTCP.Client[connectionID].playerBuffer.ReadBytes((int)pLength);
                    HandleDataPackets(connectionID, data);
                }

                pLength = 0;

                if (ServerTCP.Client[connectionID].playerBuffer.Length() >= 8)
                {
                    pLength = ServerTCP.Client[connectionID].playerBuffer.ReadLong(false);

                    if (pLength < 0)
                    {
                        ServerTCP.Client[connectionID].playerBuffer.Clear();
                        return;
                    }
                }
            }
        }
        static void HandleDataPackets(long connectionID, byte[] data)
        {
            long packetIdentifier;
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);
            packetIdentifier = buffer.ReadLong();
            buffer.Dispose();

            if (packets.TryGetValue(packetIdentifier, out Packet_ packet))
            {
                packet.Invoke(connectionID, data);
            }
        }

        #region RecievePackets
        static void PACKET_DISCONNECT(long connectionID, byte[] data) {
            for (int i = 0; i < ServerTCP.Client.Length; i++) {
                if (ServerTCP.Client[i].connectionID == connectionID) {
                    ServerTCP.Client[i].CloseSocket();
                }
            } 
        }
        static void PACKET_PLAYERMOVEMENT(long connectionID, byte[] data) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(data);

            long packetIdentifier = buffer.ReadLong();

            float x = buffer.ReadFloat();
            float y = buffer.ReadFloat();
            float z = buffer.ReadFloat();

            //float rotx = buffer.ReadFloat();
            //float roty = buffer.ReadFloat();
            //float rotz = buffer.ReadFloat();

            Logger.WriteInfo("Recieved position data from: " + connectionID.ToString());
            ServerTCP.PlayerMovement((int)connectionID, x, y, z, 0, 0, 0);
            buffer.Dispose();
        }
        #endregion
    }
}
