using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TotalAnarchyGameServer {
    class ServerTCP {
        static TcpListener serverSocket;
        public static Clients[] Client = new Clients[Constants.MAX_PLAYERS];

        public static void InitializeNetwork() {
            serverSocket = new TcpListener(IPAddress.Any, 5555);
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(OnClientConnect, null);
        }
        static void OnClientConnect(IAsyncResult result) {
            TcpClient client = serverSocket.EndAcceptTcpClient(result);
            serverSocket.BeginAcceptTcpClient(OnClientConnect, null);
            for (int i = 0; i < Constants.MAX_PLAYERS; i++) {
                if (Client[i].socket == null) {
                    Client[i].socket = client;
                    Client[i].connectionID = i;
                    Client[i].ip = client.Client.RemoteEndPoint.ToString();
                    Client[i].Start();
                    Logger.WriteLog("Connection received from " + Client[i].ip + " | ConnectionID: " + Client[i].connectionID);
                    General.JoinGame(i);
                    return;
                }
            }
        }

        static void SendDataTo(int connectionID, byte[] data) {
            try{
                ByteBuffer buffer = new ByteBuffer();
                buffer.WriteLong((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
                buffer.WriteBytes(data);
                Client[connectionID].myStream.BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
                buffer.Dispose();
            }catch (Exception e){
                Logger.WriteError(e.ToString());
                return;
            }
            
        }
        static void SendDataToAll(byte[] data) {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++) {
                if (Client[i].socket != null) {
                    SendDataTo(i, data);
                }
            }
        }
        static void SendDataToAllBut(int connectionID, byte[] data) {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++) {
                if (connectionID != i)
                    if (Client[i].socket != null)
                        SendDataTo(i, data);
            }
        }

        #region SendPackages
        public static void SendWelcomeMessage(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SWelcomeMessage); //Packet Identifier.
            buffer.WriteInteger(connectionID);
            buffer.WriteString("Welcome to Total Anarchy. You are connected to TestServer");
            SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
        public static void SendInGame(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SInGame); //Packet Identifier.
            buffer.WriteInteger(connectionID);

            SendDataTo(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
        public static byte[] PlayerData(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerData); //Packet Identifier.
            buffer.WriteInteger(connectionID);
            //send the players details

            //Logger.WriteDebug("Sending data to all players");
            return buffer.ToArray();
        }
        public static void SendInWorld(int connectionID) {
            for (int i = 0; i < Constants.MAX_PLAYERS; i++) {
                if (i != connectionID && Client[i].socket != null) { //gets all the other players
                    SendDataTo(connectionID, PlayerData(i));
                }
            }
            //sends the connectionID player data to everyone including himself
            SendDataToAll(PlayerData(connectionID));
        }
        public static void PlayerDisconnected(int connectionID) {
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteLong((long)ServerPackets.SPlayerDisconnected); //Packet Identifier.
            buffer.WriteInteger(connectionID);

            SendDataToAll(buffer.ToArray());
            buffer.Dispose();
        }
        public static void PlayerMovement(int connectionID, float x, float y, float z, float rotx, float roty, float rotz) {
            ByteBuffer buffer = new ByteBuffer();

            buffer.WriteLong((long)ServerPackets.SPlayerMove);
            buffer.WriteInteger(connectionID);
            buffer.WriteFloat(x);
            buffer.WriteFloat(y);
            buffer.WriteFloat(z);
            buffer.WriteFloat(rotx);
            buffer.WriteFloat(roty);
            buffer.WriteFloat(rotz);

            SendDataToAllBut(connectionID, buffer.ToArray());
            buffer.Dispose();
        }
        #endregion
    }
}

