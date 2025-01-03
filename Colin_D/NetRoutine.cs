using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// A simpler API for interfacing with the current NetUtil networked communication libraries, it is meant to have an 
/// coroutine like syntax in order to be more easily understand by other programmers.
///
/// Moreover, it also remains as a notstatic library and can be constructed and deconstructed without concern.
/// </summary>
///<example> 
/// private NetRoutine example(SomeMethod, Ownership.Vitalist);
/// private NetRoutine<Float> floatExample(SomeMethodFloat, Ownership.Navigator)
/// 
/// void OnEnable()
/// {
///     example.Invoke();
/// }
/// 
/// SomeMethod()
/// {
///     //Do things
/// }
/// SomeMethodFloat(Float data)
/// {
///     //Do things
/// }
///</example>
public class NetRoutine
{
    private NetAction wrappedNetAction;
    private Ownership owner;
    /// <summary>
    /// Given a method and an owner, this constructor set's up objects to handle message passing across two devices
    /// </summary>
    /// <param name="Method">Method to call when invoked</param>
    /// <param name="owner">Which player owns this routine? Navigator or Vitalist, or Both?</param>
    public NetRoutine(UnityAction Method, Ownership owner)
    {
        
        this.owner = owner;
        string name = "net_routine_" + Method.Method.DeclaringType.ToString().ToLower() + "_" + Method.Method.ReturnParameter.ToString().ToLower() + "_"+ Method.Method.Name.ToLower();
        wrappedNetAction = new NetAction(name);
        wrappedNetAction.BindListener(Method);
    }
    /// <summary>
    /// This creates an empty netroutine with a given name, this is usable in place of NetAction, add listeners by using 
    /// Netroutine += methodListener;
    /// </summary>
    /// <param name="name">The given name to use as a token.</param>
    /// <param name="owner">What player has authority.</param>
    public NetRoutine(string name, Ownership owner)
    {
        this.owner = owner;
        wrappedNetAction = new NetAction(name);
    }
    /// <summary>
    /// Binds a listener to the wrapped netaction, only use this if you need to invoke multiple listeners to one netaction
    /// IE as a EventManager.
    /// </summary>
    /// <param name="a">Given netroutine</param>
    /// <param name="function">Listener to bind.</param>
    /// <returns></returns>
    public static NetRoutine operator +(NetRoutine a, UnityAction function)
    {
        a.wrappedNetAction += function;
        return a;
    }
    /// <summary>
    /// Unbinds a listener to the wrapped netaction, only use this if you need to invoke multiple listeners to one netaction
    /// IE as a EventManager.
    /// </summary>
    /// <param name="a">Given netroutine</param>
    /// <param name="function">Listener to unbind.</param>
    /// <returns></returns>
    public static NetRoutine operator -(NetRoutine a, UnityAction function)
    {
        a.wrappedNetAction -= function;
        return a;
    }
    ~NetRoutine()
    {
        wrappedNetAction.Dispose();
    }
    /// <summary>
    /// Calls the Method previously described in the constructor.
    /// </summary>
    public void Invoke()
    {
        wrappedNetAction?.Invoke(NetRoutineShared.GetOwner(owner));
    }
}
public class NetRoutine<T>
{
    private NetAction<T> wrappedNetAction;
    private Ownership owner;
    /// <summary>
    /// Given a method and an owner, this constructor set's up objects to handle message passing across two devices
    /// </summary>
    /// <param name="Method">Method to call when invoked</param>
    /// <param name="owner">Which player owns this routine? Navigator or Vitalist, or Both?</param>
    public NetRoutine(UnityAction<T> Method, Ownership owner) 
    {
        this.owner = owner;
        string name = "net_routine_" + Method.Method.DeclaringType.ToString().ToLower() + "_" + Method.Method.ReturnParameter.ToString().ToLower() + "_"+ Method.Method.Name.ToLower();
        wrappedNetAction = new NetAction<T>(name);
        wrappedNetAction.BindListener(Method);
    }
    /// <summary>
    /// This creates an empty netroutine with a given name, this is usable in place of NetAction, add listeners by using 
    /// Netroutine += methodListener;
    /// </summary>
    /// <param name="name">The given name to use as a token.</param>
    /// <param name="owner">What player has authority.</param>
    public NetRoutine(string name, Ownership owner)
    {
        this.owner = owner;
        wrappedNetAction = new NetAction<T>(name);
    }
    /// <summary>
    /// Binds a listener to the wrapped netaction, only use this if you need to invoke multiple listeners to one netaction
    /// IE as a EventManager.
    /// </summary>
    /// <param name="a">Given netroutine</param>
    /// <param name="function">Listener to bind.</param>
    /// <returns></returns>
    public static NetRoutine<T> operator +(NetRoutine<T> a, UnityAction<T> function)
    {
        a.wrappedNetAction += function;
        return a;
    }
    /// <summary>
    /// Unbinds a listener to the wrapped netaction, only use this if you need to invoke multiple listeners to one netaction
    /// IE as a EventManager.
    /// </summary>
    /// <param name="a">Given netroutine</param>
    /// <param name="function">Listener to unbind.</param>
    /// <returns></returns>
    public static NetRoutine<T> operator -(NetRoutine<T> a, UnityAction<T> function)
    {
        a.wrappedNetAction -= function;
        return a;
    }
    ~NetRoutine()
    {
        wrappedNetAction.Dispose();
    }
    /// <summary>
    /// Calls the Method previously described in the constructor.
    /// </summary>
    public void Invoke(T value)
    {
        wrappedNetAction?.Invoke(NetRoutineShared.GetOwner(owner), value);
    }
}
/// <summary>
/// Contains shared functions used by both NetRoutine and NetRoutine<>.
/// </summary>
public class NetRoutineShared
{
    public static Func<bool> GetOwner(Ownership owner)
    {
        switch (owner)
        {
            case Ownership.None:
                return () => { return false; };
            case Ownership.Navigator:
                return () => Whoami.AmIP1();
            case Ownership.Vitalist:
                return () => Whoami.AmIP2();
            case Ownership.Both:
                return () => { return true; };
        }
        return null;
    }
}
[Serializable]
public enum Ownership
{
    None = 0,
    Navigator,
    Vitalist,
    Both
}
