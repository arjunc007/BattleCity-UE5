using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    
    [SerializeField] private Button P1ReadyButton;
    [SerializeField] private TextMeshProUGUI P1ReadyButtonText;
    [SerializeField] private Button P2ReadyButton;
    [SerializeField] private TextMeshProUGUI P2ReadyButtonText;
    [SerializeField] private Image P1Image;
    [SerializeField] private Image P2Image;

    private const string READY = "READY";
    private const string UNREADY = "UNREADY";

    public void Initialise(bool isHost)
    {
        Debug.Log("Lobby Initialise");
        if (isHost)
        {
            P1Image.gameObject.SetActive(true);
            P1ReadyButton.interactable = true;
            P1ReadyButton.onClick.AddListener(() => { 
                GameManager.Instance.P1Ready.Value = !GameManager.Instance.P1Ready.Value;
                P1ReadyButtonText.text = GameManager.Instance.P1Ready.Value ?  READY : UNREADY;
            });
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(false);
            P2ReadyButton.interactable = false;
            P1ReadyButtonText.text = UNREADY;
            GameManager.Instance.P1Ready.Value = false;
            GameManager.Instance.P2Ready.Value = false;
        }
        else 
        {
            P1Image.gameObject.SetActive(true);
            P1ReadyButton.interactable = false;
            P2ReadyButton.onClick.AddListener(() => {
                GameManager.Instance.P2Ready.Value = !GameManager.Instance.P2Ready.Value;
                P2ReadyButtonText.text = GameManager.Instance.P2Ready.Value ? READY : UNREADY;
            });
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(true);
            P2ReadyButton.interactable = true;
            P1ReadyButtonText.text = UNREADY;
            GameManager.Instance.P2Ready.Value = false;
        }
    }

    public void AddClient()
    {
        P2Image.gameObject.SetActive(true);
        P2ReadyButton.gameObject.SetActive(true);
        P2ReadyButtonText.text = GameManager.Instance.P2Ready.Value ? READY : UNREADY;
    }

    private void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (GameManager.Instance.P1Ready.Value && GameManager.Instance.P2Ready.Value)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
