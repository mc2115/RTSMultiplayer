using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb=null;
    [SerializeField] private float destroyAfterSeconds=4f;
    [SerializeField] private float launchForce=10f;
    [SerializeField] private int damage=20;

    void Start(){
        rb.velocity=transform.forward*launchForce;
    }
    public override void OnStartServer(){
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }
    [Server]
    private void DestroySelf(){
        NetworkServer.Destroy(gameObject);
    }
    [ServerCallback]
    private void OnTriggerEnter(Collider other){
        if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity)){
            if(networkIdentity.connectionToClient==connectionToClient) return;
        }
        if(other.TryGetComponent<Health>(out Health health)){
            health.DealDamage(damage);

        }
        DestroySelf();
    }
}
