using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Unit unitPrefab=null;
    [SerializeField] private Transform location=null;
    [SerializeField] private Health health=null;
    [SerializeField] private TMP_Text remainingUnitsText=null;
    [SerializeField] private Image unitProgressImage=null;
    [SerializeField] private int maxUnitQueue=5;
    [SerializeField] private float spawnMoveRange=7f;
    [SerializeField] private float unitSpawnDuration=5f;

    [SyncVar(hook=nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update(){
        if(isServer){
            ProduceUnits();
        }
        if(isClient){
            UpdateTimerDisplay();
        }
    }

    public override void OnStartServer(){
        health.ServerOnDie +=HandleServerOnDie;
    }
    public override void OnStopServer(){
        health.ServerOnDie -=HandleServerOnDie;
    }
    [Server]
    private void HandleServerOnDie(){
        NetworkServer.Destroy(gameObject);
    }
    [Server]
    private void ProduceUnits(){
        if(queuedUnits==0) return;
        unitTimer+=Time.deltaTime;
        if(unitTimer<unitSpawnDuration) return;
        GameObject unitInstance=Instantiate(unitPrefab.gameObject, location.position, location.rotation);
        NetworkServer.Spawn(unitInstance, connectionToClient);
        Vector3 unitSpawnOffset=Random.insideUnitSphere*spawnMoveRange;
        unitSpawnOffset.y=location.position.y;
        UnitMovement unitMovement=unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(location.position+unitSpawnOffset);
        queuedUnits--;
        unitTimer=0f;
    }
    [Command]
    private void CmdSpawnUnit(){
        if(queuedUnits==maxUnitQueue) return;
        RTSPlayer player=connectionToClient.identity.GetComponent<RTSPlayer>();
        if (player.getResources()<unitPrefab.getResourceCost()) return;
        queuedUnits++;
        player.SetResources(player.getResources()-unitPrefab.getResourceCost());
    }

    public void OnPointerClick(PointerEventData eventData){
        if (eventData.button!=PointerEventData.InputButton.Left) return;
        if (!hasAuthority) return;
        CmdSpawnUnit();
    }
    private void ClientHandleQueuedUnitsUpdated(int old, int newUnits){
        remainingUnitsText.text=newUnits.ToString();
    }
    private void UpdateTimerDisplay(){
        float newProgress=unitTimer/unitSpawnDuration;
        if(newProgress<unitProgressImage.fillAmount){
            unitProgressImage.fillAmount=newProgress;
        } else {
            unitProgressImage.fillAmount=Mathf.SmoothDamp(unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }
    
}
