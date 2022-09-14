public static class PlayerManager
{
    public static PlayerManagerSystem System;

    public static Player CreateLocalPlayer()
    {
        return System.CreateLocalPlayer();
    }

    public static Player localPlayer
    {
        get => System.localPlayer;
    }
}