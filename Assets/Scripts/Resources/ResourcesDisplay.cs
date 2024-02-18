using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText=null;
    private RTSPlayer player;
    private void Start(){
        player=NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    }
    private void Update(){
        if (player!=null) {
            ClientHandleResourcesUpdate(player.getResources());
            player.ClientOnResourcesUpdate+=ClientHandleResourcesUpdate;
        }
    }
    private void OnDestroy(){
        player.ClientOnResourcesUpdate-=ClientHandleResourcesUpdate;
    }
    private void ClientHandleResourcesUpdate(int resources){
        resourcesText.text=$"Resources: {resources}";
    }
}
