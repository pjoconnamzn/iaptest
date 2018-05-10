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

using PasswordProtector;
using System;
using System.Collections;
using TwitchConnectTv;
using UnityEngine;

namespace UniTwitchIRC.TwitchInterface
{
    /// <summary>
    /// Provides a connection to the Twitch API and TMI information
    /// <para>This class polls for the viewers in the chat room at the present time</para>
    /// </summary>
    public class TwitchConnect : MonoBehaviour
    {            
        const string JSON_EXECPTION_MESSAGE = "<color=red>Exception</color>: {0}\n<color=green>Trying to read jsonn again from stream in {1} seconds.</color>";

        /// <summary>
        /// Event invoked when the JSON data has been refreshed or reloaded
        /// </summary>
        public static event Action<TwitchChat, TwitchTv.TMI> OnRefreshed;

        [PasswordProtect]
        [SerializeField, Tooltip("This is the client id for connection to Twitch API.")]
        string m_ClientId = string.Empty;

        [SerializeField, RequiredInHierarchy(typeof(TwitchChat))]
        TwitchChat m_TwitchIRC = null;

        [SerializeField, Tooltip("Delay between the polling for chat room information.")]
        float m_RefreshDelay = 120.0f;

        [SerializeField, Tooltip("Show the JSON exception formatted in the debug Console window.\nSeeing this execption usually means a misses responce and will be resolved when the connection is succusful.")]
        bool m_DebugLog = true;

        [SerializeField, Tooltip("Collection of present chat viewers.")]
        TwitchTv.TMI.Chatters m_Chatters = null;

        Broadcaster m_Broadcaster = null;

        /// <summary>
        /// Collection of present chat viewers.
        /// </summary>
        public TwitchTv.TMI.Chatters chatters { get { return m_Chatters; } }

        public Broadcaster broadcaster { get { return m_Broadcaster; } }

        IEnumerator Start()
        {
            m_Broadcaster = new Broadcaster(m_TwitchIRC.messenger.channel, m_ClientId);

            while(enabled)
            {
                WWW www = new WWW(m_Broadcaster.url.chatters);

                yield return www;

                string rawResult = System.Text.Encoding.UTF8.GetString(www.bytes);
                string result = rawResult.Replace("\n", "").Replace("\r", "").Replace(" ", "");

                TwitchTv.TMI twitchTMI = null;
                try
                {
                    twitchTMI = m_Broadcaster.ParseJsonChatters(result);
                    m_Chatters = twitchTMI.chatters;
                    if(TwitchConnect.OnRefreshed != null)
                    {
                        TwitchConnect.OnRefreshed.Invoke(m_TwitchIRC, twitchTMI);
                    }
                }
                catch(Exception ex)
                {
                    if(m_DebugLog)
                    {
                        Debug.LogFormat(JSON_EXECPTION_MESSAGE, ex.Message, m_RefreshDelay);
                    }
                }
                yield return new WaitForSeconds(m_RefreshDelay);
            }
        }
    }
}
