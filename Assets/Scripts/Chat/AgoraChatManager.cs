using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using AgoraChat;
using AgoraChat.MessageBody;
using UnityEngine;

public class AgoraChatManager : MonoBehaviour, IChatManagerDelegate, IConnectionDelegate
{
    private TMP_Text messageList;
    [SerializeField] string userId = "";
    [SerializeField] string token = ""; 
    [SerializeField] string appKey = "";
    private bool isJoined = false;
    SDKClient agoraChatClient;
    
    void Start()
    {
        messageList = GameObject.Find("ChatScrollView/Viewport/Content/MessageList").GetComponent<TextMeshProUGUI>();
        messageList.text = "";
        
        GameObject button = GameObject.Find("JoinButton");
        button.GetComponent<Button>().onClick.AddListener(JoinLeave);
        button = GameObject.Find("SendButton");
        button.GetComponent<Button>().onClick.AddListener(SendMessage);
        SetupChatSDK();
    }
    
    void SetupChatSDK()
    {
        if (appKey == "")
        {
            Debug.Log("You should set your appKey first!");
            return;
        }
        Options options = new Options(appKey);
        options.UsingHttpsOnly = true;
        options.DebugMode = true;
        agoraChatClient = SDKClient.Instance;
        agoraChatClient.InitWithOptions(options);
    }

    public void OnMessagesReceived(List<Message> messages)
    {
        foreach (Message msg in messages) 
        {
            if (msg.Body.Type == MessageBodyType.TXT)
            {
                TextBody txtBody = msg.Body as TextBody;
                string Msg = msg.From + ":" + txtBody.Text;
                DisplayMessage(Msg, false);
            }
        }
    }
    
    public void JoinLeave()
    {
        if (isJoined)
        {
            agoraChatClient.Logout(true, callback: new CallBack(
                onSuccess: () =>
                {
                    Debug.Log("Logout succeed");
                    isJoined = false;
                    GameObject.Find("JoinButton").GetComponentInChildren<TextMeshProUGUI>().text = "Join";
                },
                onError: (code, desc) =>
                {
                    Debug.Log($"Logout failed, code: {code}, desc: {desc}");
                }));
        }
        else
        {
            agoraChatClient.LoginWithAgoraToken(userId, token, callback: new CallBack(
                onSuccess: () =>
                {
                    Debug.Log("Login succeed");
                    isJoined = true;
                    GameObject.Find("JoinButton").GetComponentInChildren<TextMeshProUGUI>().text = "Leave";
                },
                onError: (code, desc) =>
                {
                    Debug.Log($"Login failed, code: {code}, desc: {desc}");
                }));
        }
    }

    public void SendMessage()
    {
        string Msg = GameObject.Find("message").GetComponent<TMP_InputField>().text;
        if (Msg == "" || userId == "")
        {
            Debug.Log("You did not type your message");
            return;
        }
        Message msg = Message.CreateTextSendMessage(userId, Msg);
        DisplayMessage(Msg, true);
        agoraChatClient.ChatManager.SendMessage(ref msg, new CallBack(
            onSuccess: () =>
            {
                Debug.Log($"Send message succeed");
                GameObject.Find("message").GetComponent<TMP_InputField>().text = "";
            },
            onError: (code, desc) =>
            {
                Debug.Log($"Send message failed, code: {code}, desc: {desc}");
            }));
    }

    public void DisplayMessage(string messageText, bool isSentMessage)
    {
        if (isSentMessage)
        {
            messageList.text += "<align=\"right\"><color=black><mark=#dcf8c655 padding=\"10, 10, 0, 0\">" + messageText + "</color></mark>\n";
        }
        else
        {
            messageList.text += "<align=\"left\"><color=black><mark=#ffffff55 padding=\"10, 10, 0, 0\">" + messageText + "</color></mark>\n";
        }
    }
    
    void OnApplicationQuit()
    {
        agoraChatClient.ChatManager.RemoveChatManagerDelegate(this);
        agoraChatClient.Logout(true, callback: new CallBack(
            onSuccess: () => 
            {
                Debug.Log("Logout succeed");
            },
            onError: (code, desc) => 
            {
                Debug.Log($"Logout failed, code: {code}, desc: {desc}");
            }));
    }


    public void OnCmdMessagesReceived(List<Message> messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnMessagesRead(List<Message> messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnMessagesDelivered(List<Message> messages)
    {
        throw new System.NotImplementedException();
    }

    public void OnMessagesRecalled(List<RecallMessageInfo> recallMessagesInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnReadAckForGroupMessageUpdated()
    {
        throw new System.NotImplementedException();
    }

    public void OnGroupMessageRead(List<GroupReadAck> list)
    {
        throw new System.NotImplementedException();
    }

    public void OnConversationsUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnConversationRead(string from, string to)
    {
        throw new System.NotImplementedException();
    }

    public void MessageReactionDidChange(List<MessageReactionChange> list)
    {
        throw new System.NotImplementedException();
    }

    public void OnMessageContentChanged(Message msg, string operatorId, long operationTime)
    {
        throw new System.NotImplementedException();
    }

    public void OnMessagePinChanged(string messageId, string conversationId, bool isPinned, string operatorId, long operationTime)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnected()
    {
        throw new System.NotImplementedException();
    }

    public void OnDisconnected()
    {
        throw new System.NotImplementedException();
    }

    public void OnLoggedOtherDevice(string deviceName, string info)
    {
        throw new System.NotImplementedException();
    }

    public void OnRemovedFromServer()
    {
        throw new System.NotImplementedException();
    }

    public void OnForbidByServer()
    {
        throw new System.NotImplementedException();
    }

    public void OnChangedIMPwd()
    {
        throw new System.NotImplementedException();
    }

    public void OnLoginTooManyDevice()
    {
        throw new System.NotImplementedException();
    }

    public void OnKickedByOtherDevice()
    {
        throw new System.NotImplementedException();
    }

    public void OnAuthFailed()
    {
        throw new System.NotImplementedException();
    }

    public void OnTokenExpired()
    {
        throw new System.NotImplementedException();
    }

    public void OnTokenWillExpire()
    {
        throw new System.NotImplementedException();
    }

    public void OnAppActiveNumberReachLimitation()
    {
        throw new System.NotImplementedException();
    }

    public void OnOfflineMessageSyncStart()
    {
        throw new System.NotImplementedException();
    }

    public void OnOfflineMessageSyncFinish()
    {
        throw new System.NotImplementedException();
    }
}