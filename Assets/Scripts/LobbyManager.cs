using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private bool _p1Ready;
    private bool _p2Ready;
    [SerializeField] private Button StartButton;
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
            P1ReadyButton.onClick.AddListener(AddPlayer);
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(false);
            P2ReadyButton.interactable = false;
            P2ReadyButton.gameObject.SetActive(false);
            _p1Ready = false;
            _p2Ready = false;
        }
        else 
        {
            P1Image.gameObject.SetActive(true);
            P1ReadyButton.interactable = false;
            P2ReadyButton.onClick.AddListener(AddPlayer);
            P1ReadyButtonText.text = UNREADY;
            P2Image.gameObject.SetActive(true);
            P2ReadyButton.interactable = true;
            P2ReadyButtonText.text = UNREADY;
            P2ReadyButton.gameObject.SetActive(true);
            _p2Ready = false;
        }
    }

    public void AddClient()
    {
        P2Image.gameObject.SetActive(true);
        P2ReadyButton.gameObject.SetActive(true);
        P2ReadyButtonText.text = _p2Ready ? READY : UNREADY;
    }

    public void TogglePlayerReady(ulong clientId)
    {
        Debug.Log("Ready Player" + clientId);
        if (clientId == 0)
        {
            _p1Ready = !_p1Ready;
            P1ReadyButtonText.text = _p1Ready ? READY : UNREADY;
        }
        else 
        {
            _p2Ready = !_p2Ready;
            P2ReadyButtonText.text = _p2Ready ? READY : UNREADY;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            StartButton.gameObject.SetActive(_p1Ready && _p2Ready);
        }
    }

    public void CleanUp()
    {
        P1Image.gameObject.SetActive(false);
        P1ReadyButton.onClick.RemoveListener(AddPlayer);
        P2ReadyButton.onClick.RemoveListener(AddPlayer);
        P2Image.gameObject.SetActive(false);
        P2ReadyButton.interactable = false;
        P2ReadyButton.gameObject.SetActive(false);
        _p1Ready = false;
        _p2Ready = false;
    }

    private void AddPlayer()
    {
        GameManager.Instance.ReadyPlayerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
