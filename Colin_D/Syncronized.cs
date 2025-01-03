
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Properties;


using System;
using UnityEditor;
/// <summary>
/// Synchronized
/// An object that handles network communication of various simple objects across a steam networked game. Use these in place of the actual object or primitive you want 
/// to be synchronized between online players. This works in both local and online play.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <example>
/// Synchronized<Vector3> syncVector = new Synchronized<Vector3>(true);
/// Creates a reliable Syncronized Vector.
/// 
/// Player 1.
/// syncVector.setValue(Vector3.one, () => AmIPlayer1());
/// Sets the synchronized vector to a paticular value if the lambda on the right succeeds (IE. the player is the host).
/// 
/// Player 2.
/// syncVector.getValue();
/// Returns the vector synchronized by the first player.
/// </example>
public class Synchronized<T>
{
    private static List<Synchronized<T>> list = new List<Synchronized<T>>();
    public static List<Hash128> usedID = new List<Hash128>();
    public string _name;
    public Hash128 _id;
    private bool sendPacketsReliably = false;
    private T _value;

    public Synchronized(string name, bool isReliable, T startingValue)
    {
        if (_value is long || _value is int || _value is short || _value is byte)
        {
            throw new Exception("Invalid synchronized Type, primitives are not supported with Synchronized!!!\nUse the Integer Object instead!");
        }
        if (_value is string || _value is char)
        {
            throw new Exception("Invalid synchronized Type, primitives are not supported with Synchronized!!!\nUse the String Object instead!");
        }
        if (_value is float || _value is double)
        {
            throw new Exception("Invalid synchronized Type, primitives are not supported with Synchronized!!!\nUse the Float Object instead!");
        }
        sendPacketsReliably = isReliable;
        _id = GetHashFromStr(name);
        _name = name;
        _value = startingValue;
        //Synchronized variables can never be entirely deleted, DO NOT USE THEM AS MEMBER VARIABLES!!!!!
        if (usedID.Contains(_id))
        {
            Debug.LogAssertion("ID collision for Syncronized" + " [" + name + "]");
        }    
        usedID.Add(_id);
        
        SteamLobby.synchronizationReceive += OnPacketReceived;
        list.Add(this);
    }
    public Synchronized(string name, bool isReliable)
    {
        sendPacketsReliably = isReliable;
        _id = GetHashFromStr(name);
        _name = name;
        //Synchronized variables can never be entirely deleted, DO NOT USE THEM AS MEMBER VARIABLES!!!!!
        if (usedID.Contains(_id))
        {
            Debug.LogError("ID collision for Syncronized" + " [" + name + "]");
        }
        usedID.Add(_id);

        SteamLobby.synchronizationReceive += OnPacketReceived;
        list.Add(this);
    }
    public Synchronized(string name, T startingValue) : this(name, false, startingValue) { }
    public Synchronized(string name) : this(name, false) { }
    ~Synchronized()
    {
        SteamLobby.synchronizationReceive -= OnPacketReceived;
    }
    /// <summary>
    /// Sets the value stored in the syncronized Variable
    /// </summary>
    /// <param name="value">The generic value</param>
    /// <param name="lambda">Function</param>
    public void SetValue(T value, Func<bool> lambda)
    {
        bool condition = lambda();
        if(condition)
        {
            _value = value;
            SendPacket(_value);
        }
    }
    public static Hash128 GetHashFromStr(string str)
    {
        Hash128 hash = new Hash128();
        hash = Hash128.Compute(str);
        return hash;
    }
    /// <summary>
    /// Get the value stored in the syncronized Variable
    /// </summary>
    /// <returns>Stored Generic</returns>
    public T GetValue()
    {
        return _value;
    }
    private void SendPacket(T obj)
    {
        //string packet = JsonSerializer.ToJsonString(obj);
        string packet = JsonUtility.ToJson(obj);

        if (packet.Length > 400) { throw new System.Exception("JSON Packet is greater than 400 chars in size!"); }
        SteamLobby.Instance?.SendP2PPacket(_id + ";" + packet, Channel.ID.SynchronizePacket); 
    }
    public void OnPacketReceived(string packet)
    {
        if (packet.Length > 400) { throw new System.Exception("JSON Packet is greater than 400 chars in size!"); }
        string[] data = packet.Split(';');

        if (Hash128.Parse(data[0]) == _id)
        {
            _value = JsonUtility.FromJson<T>(data[1]);
        }
        
    }
}
public class GlobalID
{
    public static ulong allIDs = 0;
}