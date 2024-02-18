using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building=null;
    [SerializeField] private Image iconImage=null;
    [SerializeField] private TMP_Text priceText=null;

    [SerializeField] private LayerMask floorMask=new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;

    private void Start(){
        mainCamera=Camera.main;
        player=NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        iconImage.sprite=building.GetIcon();
        priceText.text=building.GetPrice().ToString();
        buildingCollider=building.GetComponent<BoxCollider>();
    }
    private void Update(){
        if (buildingPreviewInstance==null) return;
        UpdateBuildingPreview();
    }
    public void OnPointerDown(PointerEventData eventData){
        if (eventData.button!=PointerEventData.InputButton.Left) return;
        if(player.getResources()<building.GetPrice()) return;
        buildingPreviewInstance=Instantiate(building.GetBuildingPreview());
        //Debug.Log("I AM DOING THE TASK!");
        buildingRendererInstance=buildingPreviewInstance.GetComponentInChildren<Renderer>();

        //buildingPreviewInstance.SetActive(false);
    }
    public void OnPointerUp(PointerEventData eventData){
        if (buildingPreviewInstance==null) return;
        Ray ray=mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)){
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }
    private void UpdateBuildingPreview(){
        Ray ray=mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        //Debug.Log(ray.origin+" origin");
        //Debug.Log(ray.direction+" direction");
        //Debug.DrawRay(ray.origin, ray.direction, Color.green);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
            Debug.Log(hit.point);
            Debug.Log(mainCamera);
            return;
        }
        
        buildingPreviewInstance.transform.position=hit.point;
        if(!buildingPreviewInstance.activeSelf){
            buildingPreviewInstance.SetActive(true);
        }
        Color color=player.CanPlaceBuilding(buildingCollider, hit.point)? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
