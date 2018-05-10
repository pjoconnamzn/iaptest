#region Author
/*
     
     Jones St. Lewis Cropper (caLLow)
     
     Another caLLowCreation
     
     Visit us on Google+ and other social media outlets @caLLowCreation
     
     Thanks for using our product.
     
     Send questions/comments/concerns/requests to 
      e-mail: caLLowCreation@gmail.com
      subject: UniTwirchIRC
     
*/
#endregion

using System.Diagnostics;
using System.IO;
using System.Linq;
using TwitchUnityIRC;
using UniTwitchIRC.TwitchInterface;
using UnityEditor;
using UnityEngine;

namespace UniTwitchIRCEditor
{
    /// <summary>
    /// UniTwitch IRC drawer for combined basic components
    /// </summary>
    [CustomEditor(typeof(UniTwitchIRCBehaviours))]
    public class UniTwitchIRCBehavioursEditor : Editor
    {
        const string BOX_STYLE_NAME = "Box";
        const string OPEN_CHAT_POPUP_BUTTON_LABEL = "Open UniTwitch ChatPopup";
        const string DOWNLOAD_CHAT_POPUP_BUTTON_LABEL = "Download UniTwitch ChatPopup";

        static GUIContent s_ViewAboutChatButtonLabel = new GUIContent("?", "Read About UniTwitch Chat Popup");
        static GUIContent s_OpenOnRunLabel = new GUIContent("On Run", "Open the chat popu on playmode start.");
        
        static string s_ChatPopupURL = "http://callowcreation.com/products/unity/unitwitch-irc/";
        static string s_ChatPopupInformationURL = "http://callowcreation.com/home/unitwitch-irc-chat-popup/";
        static string s_ChatPopupName = Path.GetFileName("UniTwitchChatPopup");
        static string s_ChatPopupFileName = Path.GetFileName(s_ChatPopupName + ".exe");
        static string s_ChatPopupZipFileName = Path.GetFileName(s_ChatPopupName + ".zip");
        static string s_ChatPopupUrlZipFileName = s_ChatPopupURL + s_ChatPopupName + ".zip";
        static Process[] m_ChatPopupProcesses;

        UniTwitchIRCBehaviours m_UniTwitchIRCBehaviours = null;
        TwitchChat m_TwitchChat = null;
        TwitchChatEditor m_TwitchChatEditor = null;
        TwitchConnect m_TwitchConnect = null;
        TwitchConnectEditor m_TwitchConnectEditor = null;
        SerializedProperty m_ChannelProperty = null;
        SerializedProperty m_OpenOnRunProperty = null;

        bool m_Downloading = false;
        bool m_StartingProcess = false;

        void OnEnable()
        {
            m_UniTwitchIRCBehaviours = target as UniTwitchIRCBehaviours;

            CreateEditorsIfNeeded();
            SubscribeToUpdate();
        }

        void CreateEditorsIfNeeded()
        {
            m_TwitchChat = m_UniTwitchIRCBehaviours.GetComponentInChildren<TwitchChat>();
            if(m_TwitchChat)
            {
                if(m_TwitchChatEditor == null)
                {
                    m_TwitchChatEditor = Editor.CreateEditor(m_TwitchChat, typeof(TwitchChatEditor)) as TwitchChatEditor;
                }
                m_ChannelProperty = m_TwitchChatEditor.serializedObject.FindProperty("m_Channel");
                m_OpenOnRunProperty = serializedObject.FindProperty("m_OpenOnRun");
            }

            m_TwitchConnect = m_UniTwitchIRCBehaviours.GetComponentInChildren<TwitchConnect>();
            if(m_TwitchConnect)
            {
                if (m_TwitchConnectEditor == null)
                {
                    m_TwitchConnectEditor = Editor.CreateEditor(m_TwitchConnect) as TwitchConnectEditor;
                }
            }
        }

        void DestroyEditorsIfNeeded()
        {
            if (m_TwitchChatEditor != null)
            {
                DestroyImmediate(m_TwitchChatEditor);
                m_TwitchChatEditor = null;
            }
            if (m_TwitchConnectEditor != null)
            {
                DestroyImmediate(m_TwitchConnectEditor);
                m_TwitchConnectEditor = null;
            }
        }

        void OnDisable()
        {
            UnSubscribeFromUpdate();
            DestroyEditorsIfNeeded();
        }

        void OnDestroy()
        {
            UnSubscribeFromUpdate();
            DestroyEditorsIfNeeded();
        }

        /// <summary>
        /// Draws the editor to the inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            if(m_TwitchChatEditor != null)
            {
                GUILayout.BeginVertical(BOX_STYLE_NAME);
                {
                    EditorGUILayout.Space();
                    m_TwitchChatEditor.OnInspectorGUI();
                    EditorGUILayout.Space();
                }
                GUILayout.EndVertical();

                ShowOpenProcessButton();
            }
            if(m_TwitchConnectEditor)
            {
                GUILayout.BeginVertical(BOX_STYLE_NAME);
                {
                    EditorGUILayout.Space();
                    m_TwitchConnectEditor.OnInspectorGUI();
                    EditorGUILayout.Space();
                }
                GUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }


        void OnUpdateEditor()
        {
            MonitorChatPopupProcess();
        }

        void UnSubscribeFromUpdate()
        {
            EditorApplication.update -= OnUpdateEditor;
        }

        void SubscribeToUpdate()
        {
            EditorApplication.update -= OnUpdateEditor;
            EditorApplication.update += OnUpdateEditor;
        }

        void MonitorChatPopupProcess()
        {
#if UNITY_EDITOR_WIN
            m_ChatPopupProcesses = Process.GetProcessesByName(s_ChatPopupName);
            if(m_ChatPopupProcesses.Length == 0)
            {
                Repaint();
            }
#endif
        }

        void ShowOpenProcessButton()
        {
#if UNITY_EDITOR_WIN
            if(m_ChatPopupProcesses != null && m_ChatPopupProcesses.Length == 0 && !m_StartingProcess)
            {
                if(File.Exists(s_ChatPopupFileName) && !File.Exists(s_ChatPopupZipFileName))
                {
                    if(!string.IsNullOrEmpty(m_ChannelProperty.stringValue))
                    {
                        GUILayout.BeginHorizontal(BOX_STYLE_NAME);
                        {
                            if(GUILayout.Button(OPEN_CHAT_POPUP_BUTTON_LABEL))
                            {
                                StartChatPopupProcess();
                            }
                            m_OpenOnRunProperty.boolValue = EditorGUILayout.ToggleLeft(s_OpenOnRunLabel, m_OpenOnRunProperty.boolValue);
                            if(m_OpenOnRunProperty.boolValue && EditorApplication.isPlayingOrWillChangePlaymode)
                            {
                                if(EditorApplication.isPlaying)
                                {
                                    StartChatPopupProcess();
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else if(!m_Downloading)
                {
                    GUILayout.BeginHorizontal(BOX_STYLE_NAME);
                    {
                        if(GUILayout.Button(DOWNLOAD_CHAT_POPUP_BUTTON_LABEL, GUI.skin.button))
                        {
                            m_Downloading = true;
                            ZipUtility.Compression.Download(s_ChatPopupUrlZipFileName, s_ChatPopupZipFileName);
                            ZipUtility.Compression.UnZip(s_ChatPopupZipFileName);
                            File.Delete(s_ChatPopupZipFileName);
                            m_Downloading = false;
                        }
                        if(GUILayout.Button(s_ViewAboutChatButtonLabel, GUILayout.MaxWidth(20)))
                        {
                            Application.OpenURL(s_ChatPopupInformationURL);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
#endif
        }

        void StartChatPopupProcess()
        {
            m_StartingProcess = true;
            Process.Start(s_ChatPopupFileName, m_ChannelProperty.stringValue);
        }    
    }
}
