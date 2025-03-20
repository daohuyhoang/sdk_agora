using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AgoraChat;
using AgoraChat.MessageBody;

public class AgoraChatManager : MonoBehaviour, IChatManagerDelegate, IConnectionDelegate
{
    [SerializeField] private TMP_Text messageList;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_InputField targetUserIdInput;
    [SerializeField] private AgoraChatConfig chatConfig;

    private bool isJoined = false;
    private SDKClient agoraChatClient;

    void Start()
    {
        joinButton.onClick.AddListener(JoinLeave);
        sendButton.onClick.AddListener(SendMessage);
        SetupChatSDK();

        messageList.text = "";
        joinButton.GetComponentInChildren<TMP_Text>().text = "Join";
        
        targetUserIdInput.text = "";
    }

    void SetupChatSDK()
    {
        if (string.IsNullOrEmpty(chatConfig.AppKey))
        {
            Debug.Log("You should set your appKey in AgoraChatConfig!");
            return;
        }

        Options options = new Options(chatConfig.AppKey);
        options.UsingHttpsOnly = true;
        options.DebugMode = true;
        agoraChatClient = SDKClient.Instance;
        agoraChatClient.InitWithOptions(options);
        agoraChatClient.ChatManager.AddChatManagerDelegate(this);
    }

    public void OnMessagesReceived(List<Message> messages)
    {
        foreach (Message msg in messages)
        {
            if (msg.Body.Type == MessageBodyType.TXT)
            {
                TextBody txtBody = msg.Body as TextBody;
                string Msg = msg.From + ": " + txtBody.Text;
                DisplayMessage(Msg, false, msg.From);
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
                    joinButton.GetComponentInChildren<TMP_Text>().text = "Join";
                },
                onError: (code, desc) =>
                {
                    Debug.Log($"Logout failed, code: {code}, desc: {desc}");
                }));
        }
        else
        {
            agoraChatClient.LoginWithAgoraToken(chatConfig.UserId, chatConfig.Token, callback: new CallBack(
                onSuccess: () =>
                {
                    Debug.Log("Login succeed");
                    isJoined = true;
                    joinButton.GetComponentInChildren<TMP_Text>().text = "Leave";
                },
                onError: (code, desc) =>
                {
                    Debug.Log($"Login failed, code: {code}, desc: {desc}");
                }));
        }
    }

    public void SendMessage()
    {
        string msgText = messageInput.text.Trim();
        string targetUserId = targetUserIdInput.text.Trim();

        if (string.IsNullOrEmpty(msgText) || string.IsNullOrEmpty(targetUserId))
        {
            Debug.Log("You did not type your message or target user ID");
            return;
        }
        
        if (!isJoined)
        {
            Debug.Log("You must join the chat before sending a message");
            return;
        }

        Message msg = Message.CreateTextSendMessage(targetUserId, msgText);
        DisplayMessage("Me: " + msgText, true);
        agoraChatClient.ChatManager.SendMessage(ref msg, new CallBack(
            onSuccess: () =>
            {
                Debug.Log($"Send message succeed");
                messageInput.text = "";
            },
            onError: (code, desc) =>
            {
                Debug.Log($"Send message failed, code: {code}, desc: {desc}");
            }));
    }

    public void DisplayMessage(string messageText, bool isSentMessage, string senderId = null)
    {
        if (isSentMessage)
        {
            messageList.text += $"<align=\"right\"><color=black><mark=#dcf8c655 padding=\"10, 10, 0, 0\">{messageText}</color></mark>\n";
        }
        else
        {
            messageList.text += $"<align=\"left\"><color=black><mark=#ffffff55 padding=\"10, 10, 0, 0\">{messageText}</color></mark>\n";
        }
    }

    void OnApplicationQuit()
    {
        agoraChatClient.ChatManager.RemoveChatManagerDelegate(this);
        agoraChatClient.Logout(true, callback: new CallBack(
            onSuccess: () => Debug.Log("Logout succeed"),
            onError: (code, desc) => Debug.Log($"Logout failed, code: {code}, desc: {desc}")));
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