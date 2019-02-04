public enum ServerPackets {
    SWelcomeMessage = 1,
    SInGame,
    SPlayerData,
    SPlayerDisconnected,
    SPlayerMove,

}

public enum ClientPackets {
    CDisconnect,
    CPlayerMovement,
}