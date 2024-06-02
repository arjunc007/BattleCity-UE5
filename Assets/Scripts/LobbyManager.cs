using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    private NetworkVariable<bool> _p1Ready = new ();
    private NetworkVariable<bool> _p2Ready = new ();
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
        if (isHost)
        {
            P1Image.gameObject.SetActive(true);
            P1ReadyButton.interactable = true;
            P1ReadyButton.onClick.AddListener(() => { 
                _p1Ready.Value = !_p1Ready.Value;
                P1ReadyButtonText.text = _p1Ready.Value ?  READY : UNREADY;
            });
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(false);
            P2ReadyButton.interactable = false;
            P1ReadyButtonText.text = UNREADY;
            _p1Ready.Value = false;
            _p2Ready.Value = false;  
        }
        else 
        {
            P1Image.gameObject.SetActive(true);
            P1ReadyButton.interactable = false;
            P2ReadyButton.onClick.AddListener(() => {
                _p2Ready.Value = !_p2Ready.Value;
                P2ReadyButtonText.text = _p2Ready.Value ? READY : UNREADY;
            });
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(true);
            P2ReadyButton.interactable = true;
            P1ReadyButtonText.text = UNREADY;
            _p1Ready.Value = false;
            _p2Ready.Value = false;
        }
    }

    public void AddClient()
    {
        P2Image.gameObject.SetActive(true);
        P2ReadyButton.gameObject.SetActive(true);
        P2ReadyButtonText.text = _p2Ready.Value ? READY : UNREADY;
    }

    private void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsHost)
        {
            if (_p1Ready.Value && _p2Ready.Value)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
