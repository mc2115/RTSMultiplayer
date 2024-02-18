using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
using System;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost=10;
    [SerializeField] private UnitMovement unitMovement=null;
    [SerializeField] private UnityEvent onSelected=null;
    [SerializeField] private UnityEvent deSelected=null;
    [SerializeField] private Targeter targeter=null;
    [SerializeField] private Health health=null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    public int getResourceCost(){
        return resourceCost;
    }

    public Targeter GetTargeter(){
        return targeter;
    }

    public UnitMovement GetUnitMovement(){
        return unitMovement;
    }

    public override void OnStartServer(){
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie+=ServerHandleDie;
    }

    public override void OnStopServer(){
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie-=ServerHandleDie;
    }
    [Server]
    private void ServerHandleDie(){
        NetworkServer.Destroy(gameObject);
    }

    public override void OnStartAuthority(){
        
        AuthorityOnUnitSpawned?.Invoke(this);
    }
    public override void OnStopClient(){
        if (!hasAuthority) return;
        AuthorityOnUnitDespawned?.Invoke(this);
    }
    [Client]
    public void Select(){
        if(!hasAuthority) return;
        onSelected?.Invoke();
    }

    [Client]
    public void Deselect(){
        if(!hasAuthority) return;
        deSelected?.Invoke();
    }
}
