using CustomCommandApi;
using Il2Cpp;
using MelonLoader;
using System;
using UnityEngine;
using Il2CppFishNet;
using Il2CppFishNet.Object;

namespace wilbupanel
{
    public class Main : MelonMod
    {
        private bool editingSpeed = false;
        private bool menuOpen = true;
        private PlayerMovement playermov;
        private string moveSpeedInput = "";
        private string serverMoveSpeedInput = "1";
        private PlayerMovement[] players;
        private int selectedPlayerIndex = 0;
        private bool dropdownOpen = false;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("wilbupanel v1.0 loaded");
            LoggerInstance.Msg("i see you!");
            MelonEvents.OnGUI.Subscribe(DrawMenu, 100);
        }
        private static bool IsHost()
        {
            return InstanceFinder.IsHostStarted;
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            selectedPlayerIndex = 0;
            LoggerInstance.Msg($"scene loaded: {sceneName}");
            playermov = null;
            playermov = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();

            if (playermov != null)
            {
                moveSpeedInput = playermov.moveSpeed.ToString();
            }
        }
        private static PlayerMovement GetLocalPlayer()
        {
            var all = UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

            foreach (var p in all)
            {
                var netObj = p.GetComponent<NetworkObject>();

                if (netObj != null && netObj.IsOwner)
                    return p;
            }

            return null;
        }
        public override void OnUpdate()
        {
            players = UnityEngine.Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

            if (IsHost())
            {
                if (players != null && players.Length > 0)
                {
                    if (selectedPlayerIndex >= players.Length)
                        selectedPlayerIndex = 0;

                    playermov = players[selectedPlayerIndex];
                }
            }
            else
            {
                playermov = GetLocalPlayer();
                dropdownOpen = false;
            }

            if (playermov != null && !editingSpeed)
            {
                moveSpeedInput = playermov.moveSpeed.ToString();
            }
        }
        private void DrawMenu()
        {
            if (GUI.Button(new Rect(10, 10, 120, 25), menuOpen ? "close wilbupanel" : "open wilbupanel"))
            {
                menuOpen = !menuOpen;
            }

            if (!menuOpen) return;

            Rect boxRect = new(10, 40, 300, 500);
            GUI.Box(boxRect, "wilbupanel v1.0 (heheheh.)");

            float x = boxRect.x + 10;
            float y = boxRect.y + 30;
            float currentY = y;

            string hostText = IsHost() ? "status: host" : "status: client";
            GUI.Label(new Rect(x, currentY, 200, 20), hostText);

            currentY += 25;

            if (playermov != null)
            {
                GUI.Label(new Rect(x, currentY, 120, 20), "move speed:");
                GUI.SetNextControlName("SpeedField");
                moveSpeedInput = GUI.TextField(new Rect(x + 120, currentY, 100, 20), moveSpeedInput);

                editingSpeed = GUI.GetNameOfFocusedControl() == "SpeedField";

                currentY += 30;

                if (GUI.Button(new Rect(x, currentY, 100, 25), "apply"))
                {
                    if (float.TryParse(moveSpeedInput, out float newSpeed))
                    {
                        playermov.moveSpeed = newSpeed;
                    }
                }

                currentY += 40;

                GUI.enabled = IsHost();

                GUI.Label(new Rect(x, currentY, 120, 100), "Server speed multiplier:");
                GUI.SetNextControlName("SpeedField");
                serverMoveSpeedInput = GUI.TextField(new Rect(x + 120, currentY, 100, 20), serverMoveSpeedInput);

                currentY += 50;

                if (GUI.Button(new Rect(x, currentY, 100, 25), "apply"))
                {
                    if (float.TryParse(serverMoveSpeedInput, out float newSpeed))
                    {
                        LobbySettingsManager.Instance.Server_UpdatePlayerSpeedMultiplier(newSpeed);
                    }
                }

                currentY += 60;

                if (players != null && players.Length > 0)
                {
                    bool isHost = IsHost();

                    string label = isHost
                        ? $"player {selectedPlayerIndex}"
                        : "you";

                    GUI.enabled = isHost;

                    if (GUI.Button(new Rect(x, currentY, 200, 25), label))
                    {
                        if (isHost)
                            dropdownOpen = !dropdownOpen;
                    }

                    currentY += 30;

                    if (isHost && dropdownOpen)
                    {
                        for (int i = 0; i < players.Length; i++)
                        {
                            if (GUI.Button(new Rect(x, currentY, 200, 25), $"player {i}"))
                            {
                                selectedPlayerIndex = i;
                                playermov = players[i];
                                moveSpeedInput = playermov.moveSpeed.ToString();
                                dropdownOpen = false;
                            }

                            currentY += 25;
                        }
                    }

                    GUI.enabled = true;
                }
            }
            else
            {
                GUI.Label(new Rect(x, currentY, 200, 20), "player not found");
            }
        }
    }

}

// approx. 3 uses of chatgpt