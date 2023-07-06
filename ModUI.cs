using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide;
using CMS21MP.ServerSide;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CMS21MP
{
    public class ModUI : MonoBehaviour
    {
        public static ModUI Instance;
        public string username = "player";
        private string save_name = "save1";
        public string ipAddress = "127.0.0.1";

        public bool showMainInterface;
        public bool showHostInterface;
        public bool saveExists; // Example: save files exist

        private Dictionary<string, ProfileData> saves = new Dictionary<string, ProfileData>();

        private Il2CppReferenceArray<ProfileData> profileDatas =
            new Il2CppReferenceArray<ProfileData>(MainMod.MAX_SAVE_COUNT + 1);

        private GUIStyle labelStyle;
        private GUIStyle textStyle;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;
        private GUIStyle backgroundStyle;

        private bool isDragging;
        private Vector2 offset;
        private Rect windowRect = new Rect(Screen.width / 2.5f, Screen.height / 3.5f, 200, 250);
        private Vector2 hostInterfaceOffset = new Vector2(220, 0);

        private Vector2 scrollPosition = Vector2.zero; // Variable de classe pour stocker la position de défilement

        private float buttonHeight = 20f; // Hauteur des boutons
        private float maxScrollViewHeight = 121f; // Hauteur maximale du ScrollView

        private bool showLobbyInterface;
        private string[] playerNames = new string[4];
        private bool[] playerReadyStates = new bool[4];
        private bool[] playerKickedStates = new bool[4];
        private bool allPlayersReady;

        public void Initialize()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                MelonLogger.Msg("Instance already exists, destroying object!");
                Destroy(this);
            }
        }

        public void OnMPGUI()
        {
            CheckAllPlayersReady();
            // Capture les événements de la souris
            Event currentEvent = Event.current;
            Vector2 mousePosition = currentEvent.mousePosition;

            if (currentEvent.type == EventType.MouseDown && windowRect.Contains(mousePosition))
            {
                // Capture le début du déplacement de l'interface
                isDragging = true;
                offset = mousePosition - windowRect.position;
                GUI.FocusWindow(0);
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                // Capture la fin du déplacement de l'interface
                isDragging = false;
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging)
            {
                // Met à jour la position de l'interface en fonction du déplacement de la souris
                windowRect.position = mousePosition - offset;
            }

            // Rendu de l'interface principale et de l'interface de l'hôte
            InitializeStyles();

            if (showMainInterface)
            {
                RenderMainInterface();
            }

            // Rendu de l'interface de l'hôte attachée à l'interface principale
            if (showHostInterface)
            {
                // Met à jour la position de l'interface de l'hôte en fonction de l'interface principale
                Rect hostWindowRect = new Rect(windowRect.position + hostInterfaceOffset, new Vector2(220, 300));
                // Rendu de l'interface de l'hôte
                RenderHostInterface(hostWindowRect);
            }

            if (showLobbyInterface)
                RenderLobbyInterface(new Rect(windowRect.position + hostInterfaceOffset, new Vector2(400, 400)));
        }

        private Texture2D CreateRoundedBackgroundTexture(Color color, float alpha, float cornerRadius)
        {
            int width = 128; // Largeur de la texture
            int height = 128; // Hauteur de la texture

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            // Remplir les pixels de la texture avec la couleur et l'alpha spécifiés
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(color.r, color.g, color.b, alpha);
            }

            // Dessiner les coins arrondis
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Vérifier si le pixel est à l'intérieur du coin arrondi
                    if ((x < cornerRadius && y < cornerRadius) || (x >= width - cornerRadius && y < cornerRadius) ||
                        (x < cornerRadius && y >= height - cornerRadius) ||
                        (x >= width - cornerRadius && y >= height - cornerRadius))
                    {
                        // Vérifier si le pixel est à l'extérieur du rayon du coin arrondi
                        if (new Vector2(cornerRadius - x, cornerRadius - y).magnitude > cornerRadius)
                        {
                            // Mettre le pixel en transparent
                            pixels[y * width + x] = new Color(color.r, color.g, color.b, 0f);
                        }
                    }
                }
            }

            // Appliquer les pixels à la texture
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private void InitializeStyles()
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                //margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                // margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                // margin = new RectOffset(10, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };

            Texture2D
                backgroundTexture =
                    CreateRoundedBackgroundTexture(Color.black, 0.5f,
                        2f); // Utilisez la fonction appropriée pour créer une texture avec des coins arrondis
            backgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = backgroundTexture }, // Utilisez votre texture de fond avec des coins arrondis
                // border = new RectOffset(10, 10, 10, 10), // Marge pour les coins arrondis
                // margin = new RectOffset(10, 10, 10, 10) // Marge globale de la boîte
            };
        }

        private void RenderMainInterface()
        {
            GUI.Box(new Rect(windowRect.x - 5, windowRect.y, windowRect.width, windowRect.height), "", backgroundStyle);
            GUILayout.BeginArea(windowRect);


            GUILayout.Label(MainMod.MOD_VERSION, labelStyle, GUILayout.Width(190), GUILayout.Height(40));

            GUILayout.Label("Username:", textStyle);
            username = GUILayout.TextField(username, textFieldStyle, GUILayout.Width(190));

            GUILayout.Label("IP Address:", textStyle);
            ipAddress = GUILayout.TextField(ipAddress, textFieldStyle, GUILayout.Width(190));

            GUILayout.Space(20);

            if (GUILayout.Button("Join Server", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                if (username != "" && ipAddress != "")
                {
                    Client.Instance.ConnectToServer(ipAddress);
                }
            }

            if (GUILayout.Button("Host Game", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                showHostInterface = true;
                Server.Start();
            }

            GUILayout.EndArea();
        }

        private void RenderHostInterface(Rect hostWindowRect)
        {
            GUI.Box(new Rect(hostWindowRect.x - 5, hostWindowRect.y, hostWindowRect.width, hostWindowRect.height), "",
                backgroundStyle);
            GUILayout.BeginArea(hostWindowRect);


            GUILayout.Label("Save Management", labelStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                showHostInterface = false;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("New Game", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                if (!saves.ContainsKey(save_name) & saves.Count < MainMod.MAX_SAVE_COUNT)
                {
                    BinaryWriter writer = new BinaryWriter();
                    ProfileData saveData = new ProfileData();

                    saveData.Init();
                    saveData.WriteSaveHeader(writer);
                    saveData.WriteSaveVersion(writer);

                    saves.Add(save_name, saveData);

                    for (int i = 0; i <= 3; i++) // Add vanillaSave
                        profileDatas[i] = Singleton<GameManager>.Instance.GameDataManager.ProfileData[i];

                    profileDatas[3 + saves.Count] = saveData;
                    Singleton<GameManager>.Instance.GameDataManager.ProfileData =
                        profileDatas; // Set back new ProfileData
                }
            }

            GUILayout.Space(5);

            GUILayout.Label("Save Name:", textStyle);
            save_name = GUILayout.TextField(save_name, textFieldStyle, GUILayout.Width(190));

            GUILayout.Space(10);

            if (saves.Count == 0)
            {
                GUILayout.Label("No Save", labelStyle);
            }
            else
            {
                float scrollViewHeight = Mathf.Min(saves.Count * (buttonHeight + 20), maxScrollViewHeight);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(220),
                    GUILayout.Height(scrollViewHeight));

                string[] keys = saves.Keys.ToArray();
                for (int i = 0; i < saves.Count; i++)
                {

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(buttonHeight)))
                    {
                        // Actions à effectuer lors du clic sur le bouton de suppression
                        RemoveSave(i);
                        break;
                    }

                    if (GUILayout.Button(keys[i], buttonStyle, GUILayout.Width(160), GUILayout.Height(buttonHeight)))
                    {
                        ShowLobbyInterface();
                        Singleton<GameManager>.Instance.ProfileManager.selectedProfile = 3 + i + 1;
                        //PlayerPrefs.SetInt("selectedProfile", 3 + i + 1);
                        Singleton<GameManager>.Instance.ProfileManager.SetDifficultyForCurrentProfile(DifficultyLevel
                            .Sandbox);
                        Singleton<GameManager>.Instance.ProfileManager.SetNameForCurrentProfile(keys[i]);
                        Singleton<GameManager>.Instance.ProfileManager.Save();
                        Singleton<GameManager>.Instance.ProfileManager.Load();
                    }

                    GUILayout.EndHorizontal();
                }


                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndArea();
        }

        private void RenderLobbyInterface(Rect lobbyWindowRect)
        {
            GUI.Box(
                new Rect(lobbyWindowRect.x - 5, lobbyWindowRect.y, lobbyWindowRect.width + 5, lobbyWindowRect.height),
                "", backgroundStyle);

            GUILayout.BeginArea(lobbyWindowRect);



            GUILayout.Label("Lobby", labelStyle);

            GUILayout.Space(10);

            // Affichage des joueurs
            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("Player " + (i + 1) + ": " + playerNames[i], textStyle);

                if (playerReadyStates[i])
                {
                    GUILayout.Label("Ready", textStyle);
                }
                else
                {
                    GUILayout.Label("Not Ready", textStyle);
                }

                if (GUILayout.Button("Toggle Ready", buttonStyle, GUILayout.Width(120), GUILayout.Height(buttonHeight)))
                {
                    // Actions à effectuer lors du clic sur le bouton pour changer l'état de ready
                    playerReadyStates[i] = !playerReadyStates[i];
                }

                if (GUILayout.Button("Kick", buttonStyle, GUILayout.Width(60), GUILayout.Height(buttonHeight)))
                {
                    // Actions à effectuer lors du clic sur le bouton pour kicker le joueur
                    playerKickedStates[i] = true;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Start Game", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)) &&
                allPlayersReady)
            {
                // Actions à effectuer lors du clic sur le bouton pour lancer la partie
                StartGame();
                showLobbyInterface = false;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.Width(190), GUILayout.Height(30)))
            {
                // Actions à effectuer lors du clic sur le bouton pour retourner au menu principal
                showLobbyInterface = false;
                showHostInterface = true;
                showMainInterface = true;
            }

            GUILayout.EndArea();
        }

        private void ShowLobbyInterface()
        {
            // Cacher les autres interfaces
            showMainInterface = false;
            showHostInterface = false;

            // Afficher l'interface du lobby
            showLobbyInterface = true;
        }

        // Fonction pour vérifier si tous les joueurs sont prêts
        private void CheckAllPlayersReady()
        {
            allPlayersReady = true;
            for (int i = 0; i < 4; i++)
            {
                if (!playerReadyStates[i] || playerKickedStates[i])
                {
                    allPlayersReady = false;
                    break;
                }
            }
        }

        // Fonction pour démarrer le jeu
        private void StartGame()
        {
            Singleton<GameManager>.Instance.GameDataManager.LoadProfile();
            Singleton<GameManager>.Instance.GameDataManager.LoadInstant();
            SceneManager.LoadScene("garage");
        }

        private void RemoveSave(int index)
        {
            // TODO: Implement save deletion
        }

        public void showGui()
        {
            if(Input.GetKeyDown(MainMod.MOD_GUI_KEY))
                showMainInterface = !showMainInterface;
        }
    }
}
