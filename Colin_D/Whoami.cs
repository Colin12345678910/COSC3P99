using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Whoami
/// Colin Doubrough
/// An extremely helpful helper class that lets us determine which player we are, it allows us to share code bewteen both the client and the host.
/// </summary>
public class Whoami : MonoBehaviour
{
    public static bool AmIP1()
    {
        if (SteamLobby.Unchecked_Instance == null)
        {
            return true;
        }    
        if (SteamLobby.Instance.LobbyID == (CSteamID)0)
        {
            return true;
        }

        if (SteamLobby.Player1 == SteamUser.GetSteamID())
        {
            return true;
        }

        return false;
    }
    public static bool AmIP2()
    {
        if (SteamLobby.Unchecked_Instance == null)
        {
            return true;
        }
        if (SteamLobby.Instance.LobbyID == (CSteamID)0)
        {
            return true;
        }

        if (SteamLobby.Player2 == SteamUser.GetSteamID())
        {
            return true;
        }

        return false;
    }
    public static Ownership WhoAmI()
    {
        if (!AmIOnline())
        {
            return Ownership.Both;
        }
        if (AmIP1())
        {
            return Ownership.Navigator;
        }
        else if (AmIP2())
        {
            return Ownership.Vitalist;
        }
        return Ownership.None;
    }

    public static bool AmIOnline()
    {
        if(SteamLobby.Unchecked_Instance == null)
        {
            return false;
        }
        return true;
    }
    public static void ReversePlayers()
    {
        SteamLobby.SwapPlayers();
    }    
}
