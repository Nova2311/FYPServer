using System;

namespace TotalAnarchyGameServer {
    class General
    {
        public static int GetTickCount()
        {
            return Environment.TickCount;
        }

        public static void InitializeServer()
        {
            int startTime = 0; int endTime = 0;
            startTime = GetTickCount();
            Logger.WriteDebug("Initializing Server...");
            //Intializing all game data arrays
            Logger.WriteDebug("Initializing Game Arrays...");
            for (int i = 0; i < Constants.MAX_PLAYERS; i++)
            {
                ServerTCP.Client[i] = new Clients();
            }

            //Start the Networking
            Logger.WriteDebug("Initializing Network...");
            ServerHandleData.InitializePackets();
            ServerTCP.InitializeNetwork();

            endTime = GetTickCount();
            Logger.WriteInfo("Initialization complete. Server loaded in " + (endTime - startTime) + " ms.");
        }

        public static void JoinGame(int connectionID) {
            ServerTCP.SendInGame(connectionID);
            ServerTCP.SendInWorld(connectionID);
        }
    }
}
