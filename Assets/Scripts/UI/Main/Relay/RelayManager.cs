using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace UI.Main.Relay
{
    public enum ConnectionTypes
    {
        Host,
        Join
    }
    public class RelayManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI joinCodeText;
        [SerializeField] private TMP_InputField joinCodeInputField;
        [SerializeField] private TextMeshProUGUI connectionStatusText;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private async void Start()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        public async void StartHost()
        {
            var joinCode = await StartHostWithRelayAsync(10);
            joinCodeText.text = joinCode;
        }
        
        public async void StartClient()
        {
            var joinCode = joinCodeInputField.text;
            var results =await StartClientWithRelayAsync(joinCode);
            connectionStatusText.text = results ? "Connected" : "Failed to connect";
        }
        
        private async Task<string> StartHostWithRelayAsync(int maxConnections)
        {
            try
            {
                // Create allocation for the relay
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

                // Get the UnityTransport component and set relay server data
                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (unityTransport == null)
                {
                    Debug.LogError("UnityTransport component not found on NetworkManager.");
                    return null;
                }
                unityTransport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

                // Get the join code for the allocation
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                // Start the host and return the join code if successful
                return NetworkManager.Singleton.StartHost() ? joinCode : null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Allocation failed to create: {e.Message}");
                throw;
            }
        }
        
        private async Task<bool> StartClientWithRelayAsync(string joinCode)
        {
            try
            {
                // Join allocation for the relay
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                // Get the UnityTransport component and set relay server data
                var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (unityTransport == null)
                {
                    Debug.LogError("UnityTransport component not found on NetworkManager.");
                    return false;
                }
                unityTransport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

                // Start the client and return true if successful
                return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join allocation: {e.Message}");
                throw;
            }
        }
    }
}
