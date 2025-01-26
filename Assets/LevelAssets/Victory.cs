using System;
using UnityEngine;

public class Victory : MonoBehaviour
{
    [SerializeField]
    GameObject victoryScreen;
    [SerializeField] 
    private GameObject player;
    private void OnTriggerEnter(Collider other)
    {
        victoryScreen.SetActive(true);
        player.GetComponent<PlayerBubble>().enabled = false;
    }
}
