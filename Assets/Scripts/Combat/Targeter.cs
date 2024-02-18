using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Targeter : NetworkBehaviour
{
    
    [SerializeField] private Targetable target;

    public Targetable GetTarget(){
        return target;
    }
    #region Server
    public override void OnStartServer(){
        GameOverHandler.ServerOnGameOver+=ServerHandleGameOver;
    }
    public override void OnStopServer(){
        GameOverHandler.ServerOnGameOver-=ServerHandleGameOver;
    }
    [Command]
    public void CmdSetTarget(GameObject targetGameObject){
        //Debug.Log(targetGameObject+" TGO");
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable ntarget)) return;
        //Debug.Log(ntarget.gameObject+" ntarget");
        target=ntarget;
        
    }
    [Server]
    public void ClearTarget(){
        target=null;
    }
    [Server]
    private void ServerHandleGameOver(){
        ClearTarget();
    }
    #endregion

    #region Client
    #endregion
}
