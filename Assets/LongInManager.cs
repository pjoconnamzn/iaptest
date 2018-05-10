using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Core;
using GameSparks.Api.Responses;
using System.Text;
using System.Linq;
using UnityEngine.SceneManagement;

public class LongInManager : MonoBehaviour
{

    // Class referenced by this code: LobbyManager.cs
    public Text userId, connectionStatus;
    public InputField userNameInput, passwordInput;
    public GameObject loginPanel;


    // Class referenced by this code: LobbyManager.cs

    public Button loginBttn, startGameBttn;

    //public Text matchDetails;
    // public GameObject matchDetailsPanel;
    // private GameSparksManager.RTSessionInfo tempRTSessionInfo;
    // Use this for initialization
    void Start()
    {
       
        startGameBttn.onClick.AddListener(() =>
        {
            Debug.Log("load screen");
            SceneManager.LoadScene(1);
        });
       

        // we wont start with a user logged in so lets show this also
        userId.text = "No User Logged In...";

        // we wont immediately have connection, so at the start of the lobby we
        // will set the connection status to show this
        connectionStatus.text = "No Connection...";
        GS.GameSparksAvailable += (isAvailable) =>
        {
            if (isAvailable)
            {
                connectionStatus.text = "GameSparks Connected...";
            }
            else
            {
                connectionStatus.text = "GameSparks Disconnected...";
            }
        };

        startGameBttn.gameObject.SetActive(false);
        // we add a custom listener to the on-click delegate of the login button so we don't need to create extra methods //
        loginBttn.onClick.AddListener(() =>
        {
            GameSparksManager.Instance().AuthenticateUser(userNameInput.text, passwordInput.text, OnRegistration, OnAuthentication);
        });
    }
    /// <summary>
    /// this is called when a player is registered
    /// </summary>
    /// <param name="_resp">Resp.</param>
    //  private void OnRegistration(RegistrationResponse _resp)
    // {
    //    userId.text = "User ID: " + _resp.UserId;
    //   connectionStatus.text = "New User Registered...";
    // }
    /// <summary>
    /// This is called when a player is authenticated
    /// </summary>
    /// <param name="_resp">Resp.</param>
    //   private void OnAuthentication(AuthenticationResponse _resp)
    //  {
    //      userId.text = "User ID: " + _resp.UserId;
    //     connectionStatus.text = "User Authenticated...";
    //  }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRegistration(RegistrationResponse _resp)
    {
        userId.text = "User ID: " + _resp.UserId;
        connectionStatus.text = "New User Registered...";
        loginPanel.SetActive(false);
        loginBttn.gameObject.SetActive(false);
        // matchDetailsPanel.SetActive(true);
        startGameBttn.gameObject.SetActive(true);
    }
    /// <summary>
    /// This is called when a player is authenticated
    /// </summary>
    /// <param name="_resp">Resp.</param>
    private void OnAuthentication(AuthenticationResponse _resp)
    {
        userId.text = "User ID: " + _resp.UserId;
        //userID.text = "User ID: " + _resp.UserId;
        connectionStatus.text = "User Authenticated...";
        loginPanel.SetActive(false);
        loginBttn.gameObject.SetActive(false);
        startGameBttn.gameObject.SetActive(true);

    }



}
