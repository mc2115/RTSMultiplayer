using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health=null;
    [SerializeField] private GameObject healthBarParent=null;
    [SerializeField] private Image healthBarImage=null;

    private void Awake(){
        health.ClientOnHealthUpdate +=HandleHealthUpdated;
    }
    private void OnDestroy(){
        health.ClientOnHealthUpdate-=HandleHealthUpdated;
    }
    private void OnMouseEnter(){
        healthBarParent.SetActive(true);
    }
    private void OnMouseExit(){
        healthBarParent.SetActive(false);
    }
    private void HandleHealthUpdated(int current, int max){
        healthBarImage.fillAmount= (float)current/max;
    }
}
