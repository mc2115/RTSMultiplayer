using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private LayerMask buildingBlockLayer=new LayerMask();
    [SerializeField] private Building[] buildings=new Building[0];
    [SerializeField] private float buildingRangeLimit=5;
    [SerializeField] private Transform cameraTransform=null;
    [SyncVar(hook=nameof(ClientHandleResourcesUpdate))]
    private int resources=500;
    [SyncVar(hook=nameof(AuthorityHandlePartyOwnerStateUpdated))]
    private bool isPartyOwner=false;

    [SyncVar(hook=nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;
    
    public event Action<int> ClientOnResourcesUpdate;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    private List<Unit> myUnits=new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    private Color teamColor=new Color();

    public string getDisplayName(){
        return displayName;
    }

    public bool GetIsPartyOwner(){
        return isPartyOwner;
    }

    public Transform GetCameraTransform(){
        return cameraTransform;
    }
    public Color getTeamColor(){
        return teamColor;
    }
    public int getResources(){
        return resources;
    }
    public List<Unit> getUnits(){
        return myUnits;
    }
    public List<Building> getBuildings(){
        return myBuildings;
    }
 

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point){
        
        if(Physics.CheckBox(point+buildingCollider.center, buildingCollider.size/2, Quaternion.identity, buildingBlockLayer)){
            return false;
        }
        //Debug.Log(myBuildings.Count);

        foreach(Building b in myBuildings){
            //Debug.Log((point-b.transform.position).sqrMagnitude);
            if((point-b.transform.position).sqrMagnitude<=buildingRangeLimit*buildingRangeLimit){
                return true;           
            }
        }
        //Debug.Log("FU");
        return false;
    }
    #region Server
    public override void OnStartServer(){
        Unit.ServerOnUnitSpawned+=ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned+=ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned+=ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned+=ServerHandleBuildingDespawned;
        DontDestroyOnLoad(this);
    }

    public override void OnStopServer(){
        Unit.ServerOnUnitSpawned-=ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned-=ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned-=ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned-=ServerHandleBuildingDespawned;
    }
    [Server] 
    public void SetDisplayName(string name){
        displayName=name;
    }
    [Server]
    public void SetTeamColor(Color nTeamColor){
        teamColor=nTeamColor;
    }
    
    [Server]
    public void SetResources(int newResources){
        resources=newResources;
    }
    [Server]
    public void SetPartyOwner(bool state){
        isPartyOwner=state;
    }
    [Command]
    public void CmdStartGame(){
        if (!isPartyOwner) return;
        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point){
        Debug.Log("attempted to place "+buildingId);
        Building buildingToPlace=null;
        foreach(Building b in buildings){
            if(b.GetId()==buildingId){
                buildingToPlace=b;
                break;
            }
        }
        if (buildingToPlace==null) return;
        if (resources<buildingToPlace.GetPrice()) return;

        BoxCollider buildingCollider=buildingToPlace.GetComponent<BoxCollider>();
        if(!CanPlaceBuilding(buildingCollider, point)) return;

        GameObject buildingInstance=Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInstance,connectionToClient);
        SetResources(resources-buildingToPlace.GetPrice());
    }

    
    private void ServerHandleUnitSpawned(Unit u){
        if(u.connectionToClient.connectionId!=connectionToClient.connectionId) return;
        myUnits.Add(u);
    }
    private void ServerHandleUnitDespawned(Unit u){
        if(u.connectionToClient.connectionId!=connectionToClient.connectionId) return;
        myUnits.Remove(u);
    }
    private void ServerHandleBuildingSpawned(Building b){
        if(b.connectionToClient.connectionId!=connectionToClient.connectionId) return;
        myBuildings.Add(b);
    }
    private void ServerHandleBuildingDespawned(Building b){
        if(b.connectionToClient.connectionId!=connectionToClient.connectionId) return;
        myBuildings.Remove(b);
    }
    #endregion
    #region Client
    public override void OnStartAuthority(){
        if (NetworkServer.active) return;
        Unit.AuthorityOnUnitSpawned+=AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned+=AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned+=AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned+=AuthorityHandleBuildingDespawned;
    }
     public override void OnStartClient(){
        if (NetworkServer.active) return;
        DontDestroyOnLoad(this);
        ((RTSNetworkManager) NetworkManager.singleton).players.Add(this);
    }
    public override void OnStopClient(){
        ClientOnInfoUpdated?.Invoke();
        if (!isClientOnly) return;
        ((RTSNetworkManager) NetworkManager.singleton).players.Remove(this);

        if (!hasAuthority) return;
        Unit.AuthorityOnUnitSpawned-=AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned-=AuthorityHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned-=AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned-=AuthorityHandleBuildingDespawned;
    }
   
    private void ClientHandleDisplayNameUpdated(string old, string name){
        ClientOnInfoUpdated?.Invoke();
    }
    private void ClientHandleResourcesUpdate(int old, int newResources){
        ClientOnResourcesUpdate?.Invoke(newResources);
    }
    private void AuthorityHandlePartyOwnerStateUpdated(bool old, bool nState){
        if (!hasAuthority) return;
        AuthorityOnPartyOwnerStateUpdated?.Invoke(nState);
    }
    private void AuthorityHandleUnitSpawned(Unit u){
        myUnits.Add(u);
    }
    private void AuthorityHandleUnitDespawned(Unit u){
        myUnits.Remove(u);
    }
    private void AuthorityHandleBuildingSpawned(Building b){
        myBuildings.Add(b);
    }
    private void AuthorityHandleBuildingDespawned(Building b){
        myBuildings.Remove(b);
    }
    #endregion
}
