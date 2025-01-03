using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// NetAction
/// Author: Colin Doubrough
/// 
/// A wrapper for the Unity Action system that allows for cross network notification and message passing.
/// Please only use with Unity structs (Vector3, Quaternions, etc) and primitive types. Other objects are not serializable
/// </summary>
/// <example>
/*
    private NetAction action = new NetAction();
    private NetAction<Float> timeAction = new NetAction<Float>();

    private void OnEnable()
    { 
        //Both are accurate ways to interface with these listeners.
        //action.BindListener(OnAction);
        action += OnAction;
    }

    private void OnDisable()
    {
        //action.UnbindListener(OnAction);
        action -= OnAction;
    }

    private void OnAction()
    {
        Debug.Log("If player 1 can send this, it has Succeeded!");
    }
 */
/// </example>
public class NetAction
{
    private UnityAction wrappedAction;
    private Hash128 _id;
    private string _name;
    
    
    public NetAction(string name)
    {
        _id = GetHashFromStr(name);
        _name = name;
        //Synchronized variables can never be entirely deleted, DO NOT USE THEM AS MEMBER VARIABLES!!!!!
        if (EventID.usedHashes.Contains(_id))
        {
            Debug.LogWarning("ID collision for NetAction" + " [" + name + "] - If this occurs outside of a level reload, this is an error.");
        }
        EventID.usedHashes.Add(_id);
        SteamLobby.netActionReceive += RecieveActionPacket;
        EventManager.Instance.GameEnd += CleanUp;
    }
    /// <summary>
    /// Wrapper for the Invoke function provided in a typical unity action, but this one will handle network communications for you.
    /// </summary>
    /// <param name="lambda">Function to describe which player sends the message.</param>
    public void Invoke(Func<bool> lambda)
    {
        if (lambda())
        {
            SendActionPacket();
            wrappedAction?.Invoke(); //This won't support invoking with objects, if you need that, call NetAction<T>
        }    
    }
    private void SendActionPacket()
    {
        string packet = "";
        packet += _id;
        packet += "|";

        SteamLobby.Unchecked_Instance?.SendP2PPacketReliably(packet, Channel.ID.NetActionPacket);
    }

    private void RecieveActionPacket(string packet, Hash128 id)
    {
        if (id == _id)
        {
            if(wrappedAction == null)
            {
                Debug.LogWarning("Packet_ID: " + id + " has attempted to call a action without any bound method. ["  +packet + "]");
            }
            else
            {
                wrappedAction.Invoke();
            }
        }
    }

    public void BindListener(UnityAction function)
    {
        wrappedAction += function;
    }

    public void UnbindListener(UnityAction function)
    {
        if (wrappedAction != null)
        {
            wrappedAction -= function;
        }
    }

    public static NetAction operator+ (NetAction a, UnityAction function)
    {
        a.BindListener(function);
        return a;
    }

    public static NetAction operator- (NetAction a, UnityAction function)
    {
        a.UnbindListener(function);
        return a;
    }
    public static Hash128 GetHashFromStr(string str)
    {
        Hash128 hash = new Hash128();
        hash = Hash128.Compute(str);
        return hash;
    }
    public void Dispose()
    {
        SteamLobby.netActionReceive -= RecieveActionPacket;
        EventManager.Instance.GameEnd -= CleanUp;
        EventID.usedHashes.RemoveAll((Hash128 h) => h == _id);

        CleanUp();
    }
    private void CleanUp()
    {
        wrappedAction = null;
    }
}
public class NetAction<T>
{
    private UnityAction<T> wrappedAction;
    private Hash128 _id;
    private string _name;
    private readonly string[] INVALID_TYPES = { "System.Boolean", "System.Int32", "System.Int64", "System.Int16", "System.Byte", "System.Single", "System.Double", "System.Char" };
    public NetAction(string name)
    {
        if (INVALID_TYPES.Contains<string>(typeof(T).ToString()))
        {
            Debug.LogAssertion(typeof(T).ToString() + " is not a valid type for NetAction");
            Debug.Break();
        }
        _id = GetHashFromStr(name);
        _name = name;
        //Synchronized variables can never be entirely deleted, DO NOT USE THEM AS MEMBER VARIABLES!!!!!
        if (EventID.usedHashes.Contains(_id))
        {
            Debug.LogWarning("ID collision for NetAction" + " [" + name + "]");
        }
        EventID.usedHashes.Add(_id);

        SteamLobby.netActionDataReceive += RecieveActionPacket;
        EventManager.Instance.GameEnd += CleanUp;
    }

    ~NetAction()
    {
        SteamLobby.netActionDataReceive -= RecieveActionPacket;
    }
    /// <summary>
    /// Wrapper for the Invoke function provided in a typical unity action, but this one will handle network communications for you.
    /// </summary>
    /// <param name="lambda">Function to describe which player sends the message.</param>
    /// <param name="value">Value to use as a parameter.</param>
    public void Invoke(Func<bool> lambda, T value)
    {
        if (lambda())
        {
            SendActionPacket(value);
            wrappedAction?.Invoke(value); //This won't support invoking with objects, if you need that, call NetAction<T>
        }
    }
    private void SendActionPacket(T value)
    {
        string packet = "";
        packet += _id;
        packet += "|";
        packet += JsonUtility.ToJson(value);

        SteamLobby.Unchecked_Instance?.SendP2PPacketReliably(packet, Channel.ID.NetDataActionPacket);
    }

    private void RecieveActionPacket(string packet, Hash128 id)
    {
        if (id == _id)
        {
            if (id == _id)
            {
                if (wrappedAction == null)
                {
                    Debug.LogWarning("Packet_ID: " + id + " has attempted to call a action without any bound method. [" + packet + "] - If this occurs outside of a level reload, this is an error.");
                }
                else
                {
                    T obj = JsonUtility.FromJson<T>(packet);
                    wrappedAction.Invoke(obj);
                }
            }
        }
    }

    public void BindListener(UnityAction<T> function)
    {
        wrappedAction += function;
    }

    public void UnbindListener(UnityAction<T> function)
    {
        wrappedAction -= function;
    }

    public static NetAction<T> operator +(NetAction<T> a, UnityAction<T> function)
    {
        a.BindListener(function);
        return a;
    }

    public static NetAction<T> operator -(NetAction<T> a, UnityAction<T> function)
    {
        a.UnbindListener(function);
        return a;
    }
    public static Hash128 GetHashFromStr(string str)
    {
        Hash128 hash = new Hash128();
        hash = Hash128.Compute(str);
        return hash;
    }
    public void Dispose()
    {
        SteamLobby.netActionDataReceive -= RecieveActionPacket;
        EventManager.Instance.GameEnd -= CleanUp;
        EventID.usedHashes.RemoveAll((Hash128 h) => h == _id);
        CleanUp();
    }
    private void CleanUp()
    {
        wrappedAction = null;
    }
}    
public class EventID
{
    public static ulong netID = 0;
    public static ulong netDataId = 0;
    public static List<Hash128> usedHashes = new List<Hash128>();
}