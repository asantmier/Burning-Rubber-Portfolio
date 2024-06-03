using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class CreateandJoinRooms : MonoBehaviourPunCallbacks
{

    public TMP_InputField createField;
    public TMP_InputField joinField;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateRoom() {

        PhotonNetwork.CreateRoom(createField.text);
    }

    public void JoinRoom()
    {

        PhotonNetwork.JoinRoom(joinField.text);
    }


    public override void OnJoinedRoom() {
        PhotonNetwork.LoadLevel("MPCity");
    }
}
