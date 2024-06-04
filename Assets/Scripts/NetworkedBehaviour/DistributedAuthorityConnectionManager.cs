using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class DistributedAuthorityConnectionManager : MonoBehaviour
{
   private string _profileName;
   private string _sessionName;
   private int _maxPlayers = 10;
   private ConnectionState _state = ConnectionState.Disconnected;
   private ISession _session;

   private enum ConnectionState
   {
       Disconnected,
       Connecting,
       Connected,
   }

   private async void Awake()
   {
       await UnityServices.InitializeAsync();
   }

   private void OnGUI()
   {
       if (_state == ConnectionState.Connected)
           return;

       GUI.enabled = _state != ConnectionState.Connecting;

       using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
       {
           GUILayout.Label("Profile Name", GUILayout.Width(100));
           _profileName = GUILayout.TextField(_profileName);
       }

       using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
       {
           GUILayout.Label("Session Name", GUILayout.Width(100));
           _sessionName = GUILayout.TextField(_sessionName);
       }

       GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(_profileName) && !string.IsNullOrEmpty(_sessionName);

       if (GUILayout.Button("Create or Join Session"))
       {
           CreateOrJoinSessionAsync();
       }
   }

   private void OnDestroy()
   {
       _session?.LeaveAsync();
   }

   private async Task CreateOrJoinSessionAsync()
   {
       _state = ConnectionState.Connecting;

       try
       {
           AuthenticationService.Instance.SwitchProfile(_profileName);
           await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new CreateSessionOptions(_maxPlayers) {
                Name = _sessionName
            }.WithDistributedConnection();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

           _state = ConnectionState.Connected;
       }
       catch (Exception e)
       {
           _state = ConnectionState.Disconnected;
           Debug.LogException(e);
       }
   }
}