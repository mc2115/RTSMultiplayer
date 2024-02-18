using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler=null;
    [SerializeField] private LayerMask layermask=new LayerMask();
    private Camera mainCamera;
    private void Start(){
        mainCamera=Camera.main;
        GameOverHandler.ClientOnGameOver+=ClientHandleGameOver;
    }
    private void OnDestroy(){
        GameOverHandler.ClientOnGameOver-=ClientHandleGameOver;
    }
    private void Update(){
        if(!Mouse.current.rightButton.wasPressedThisFrame){return;}
        Ray ray=mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layermask)) {
            //Debug.Log("XD");
            return;
        }
        //Debug.Log("not XD");
        if(hit.collider.TryGetComponent<Targetable>(out Targetable target)){
            if (target.hasAuthority){
                TryMove(hit.point);
                return;
            }
            //Debug.Log(target.gameObject);
            TryTarget(target);
            return;
        }

        TryMove(hit.point);
    }
    private void TryMove(Vector3 point){
        foreach(Unit u in unitSelectionHandler.SelectedUnits){

            u.GetUnitMovement().CmdMove(point);
        }
    }
    private void TryTarget(Targetable target){
        foreach(Unit u in unitSelectionHandler.SelectedUnits){
            //Debug.Log(u.gameObject);
            u.GetTargeter().CmdSetTarget(target.gameObject);
        }
    }
    private void ClientHandleGameOver(string winnerName){
        enabled=false;
    }
}
