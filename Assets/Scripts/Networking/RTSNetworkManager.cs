using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{   
    [SerializeField] private GameObject spawnPrefab=null;
    [SerializeField] private GameOverHandler gameOverHandler=null;

    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    private bool inProgress=false;
    public List<RTSPlayer> players {get;}=new List<RTSPlayer>();

    public override void OnServerConnect(NetworkConnectionToClient conn){
        if (!inProgress) return;
        conn.Disconnect();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn){
        RTSPlayer player=conn.identity.GetComponent<RTSPlayer>();
        players.Remove(player);
        base.OnServerDisconnect(conn);
    }
    public override void OnStopServer(){
        players.Clear();
        inProgress=false;
    }
    public void StartGame(){
        if (players.Count<2) return;
        inProgress=true;
        ServerChangeScene("Scene_01");
    }
    public override void OnClientConnect(){
        base.OnClientConnect();
        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect(){
        base.OnClientDisconnect();
        ClientOnDisconnected?.Invoke();
    }
    public override void OnStopClient(){
        players.Clear();
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn){
        base.OnServerAddPlayer(conn);
        RTSPlayer player=conn.identity.GetComponent<RTSPlayer>();
        players.Add(player);

        player.SetDisplayName($"Player {players.Count}");
        player.SetTeamColor(new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f)));
        //GameObject unitInstance=Instantiate(spawnPrefab, conn.identity.transform.position, conn.identity.transform.rotation);
        //NetworkServer.Spawn(unitInstance, conn);
        player.SetPartyOwner(players.Count==1);
    }
    public override void OnServerSceneChanged(string sceneName){
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_")){
            GameOverHandler gameOverHandlerInstance=Instantiate(gameOverHandler);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            foreach(RTSPlayer player in players){
                GameObject baseInstance= Instantiate(spawnPrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }
    
}
