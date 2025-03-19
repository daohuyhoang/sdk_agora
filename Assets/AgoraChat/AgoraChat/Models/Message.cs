using AgoraChat.SimpleJSON;
using System;
using System.Collections.Generic;
#if !_WIN32
using UnityEngine.Scripting;
#endif

namespace AgoraChat
{
    [Preserve]
    public class Message : BaseModel
    {
        /**
         * The message ID.
         */
        public string MsgId = ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds) + Tools.GetRandom()).ToString();

        /**
         * The ID of the conversation to which the message belongs.
         * 
         */
        public string ConversationId = "";

        /**
         * The user ID of the message sender.
         * 
         */
        public string From = "";

        /**
         * The message recipient.
         * - For a one-to-one chat, it is the user ID of the peer user.
         * - For a group chat, it is the group ID.
         * - For a chat room, it is the chat room ID.
         * 
         */
        public string To = "";


        /**
         * The chat type.
         * 
         * - `Chat`: One-to-one chat.
         * - `Group`: Group chat.
         * - `Room`: Chat room.
         */
        public MessageType MessageType;

        private RoomMessagePriority Priority = RoomMessagePriority.Normal;

        /**
         * Sets the priority of the chat room message.
         */
        public void SetRoomMessagePriority(RoomMessagePriority priority)
        {
            Priority = priority;
        }

        /**
        * Whether the message is delivered only when the recipient(s) is/are online:
        * - `true`：The message is delivered only when the recipient(s) is/are online. If the recipient is offline, the message is discarded.
        * - (Default) `false`：The message is delivered when the recipient(s) is/are online. If the recipient(s) is/are offline, the message will not be delivered to them until they get online.
        *
        */
        public bool DeliverOnlineOnly = false;

        /**
         * The message direction, that is, whether the message is received or sent. 
         *
         * - `SEND`: This message is sent from the local client.
         * - `RECEIVE`: The message is received by the local client.
         *
         * See {@link Direct}.
         *  
         */
        public MessageDirection Direction;

        /**
         * The message status, which can be one of the following:
         *
         * - `CREATE`：The message is created.
         * - `PROGRESS`：The message is being delivered.
         * - `SUCCESS`：The message is successfully delivered.
         * - `FAIL`：The message fails to be delivered.
         */
        public MessageStatus Status;

        /**
         * The local Unix timestamp for creating the message. The unit is millisecond.
         * 
         */
        public long LocalTime = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);

        /**
         * The Unix timestamp when the message is received by the server. The unit is millisecond.
         * 
         */
        public long ServerTime = 0;

        /**
         * Whether the message is delivered.
         *
         * - `true`: Yes.
         * - `false`: No.
         * 
         */
        public bool HasDeliverAck = false;

        /**
         * Whether the message is read.
         *
         * -`true`: Yes. 
         * -`false`: No.
         * 
         */
        public bool HasReadAck = false;

        /**
         * Whether read receipts are required for group messages.
         * - `true`: Yes.
         * - `false`: No.
         */
        public bool IsNeedGroupAck = false;

        /**
         * Whether the message is read or not.
         *
         * @note
         * To set the message as read, you are advised to use {@link Conversation#MarkAllMessagesAsRead()} in a conversation.
         *
        */
        public bool IsRead = false;

        /**
         * Whether the message is an online message.
         * @return Whether the message is an online message.
         * - `true`: Yes.
         * - `false`: No.
         */
        public bool MessageOnlineState = false;

        /**
         * Gets the number of read receipts for a group message.
         *
         * @return The number of read receipts for a group message.
         */
        public int GroupAckCount
        {
            get
            {
                if (IsNeedGroupAck)
                {
                    return SDKClient.Instance.MessageManager.GetGroupAckCount(MsgId);
                }
                else
                {
                    return 0;
                }
            }
        }

        /**
         * Gets the message pinning information.
         *
         * @return The message pinning information.
         */
        public PinnedInfo PinnedInfo
        {
            get
            {
                return SDKClient.Instance.MessageManager.GetPinnedInfo(MsgId);
            }
        }

        /**
         * The message body.
         * 
         */
        public IMessageBody Body;

        /**
         * The message extension.
         * 
         */
        public Dictionary<string, AttributeValue> Attributes;
        /**
         * Gets the list of Reactions.
         *
         * @return The list of Reactions.
         */
        public List<MessageReaction> ReactionList()
        {
            return SDKClient.Instance.MessageManager.GetReactionList(MsgId);
        }

        /**
         * The recipient list of a targeted message.
         *
         * This property is used only for messages in groups and chat rooms.
         */
        public List<string> ReceiverList
        {
            internal get { return _receiverList; }
            set { _receiverList = value; }
        }

        private List<string> _receiverList;

        /**
         * Whether the message is in a message thread:
         * - `true`: Yes.
         * - `false`: No.
         * 
         * This property is read only.
         */
        public bool IsThread = false;

        /**
         * Whether it is a global broadcast message for all chat rooms in an app:
         * - `true`: Yes.
         * - `false`: No.
         * 
         * This property is read only.
         */
        public bool Broadcast = false;

        /**
         * Whether the content of message is replaced:
         * - `true`: Yes.
         * - `false`: No.
         * 
         * This property is read only.
         */
        public bool IsContentReplaced = false;

        /**
         * Gets the overview of the message thread.
         *
         * The overview of the message thread exists only after you create a message thread.
         */
        public ChatThread ChatThread
        {
            get
            {
                return SDKClient.Instance.MessageManager.GetChatThread(MsgId);
            }
        }

        public Message(IMessageBody body = null)
        {
            Body = body;
        }

        /**
         * Creates a received message instance.
         */
        static public Message CreateReceiveMessage()
        {
            string user_name = SDKClient.Instance.CurrentUsername;

            if (string.IsNullOrEmpty(user_name))
            {
                user_name = "";
            }

            Message msg = new Message()
            {
                Direction = MessageDirection.RECEIVE,
                HasReadAck = false,
                From = user_name
            };

            return msg;
        }

        /**
         * Creates a message instance for sending.
         *
         * @param to        The message recipient:
         *                  - One-to-one chat: The user ID of the peer user.
         *                  - Group chat: Group ID.
         *                  - Chat room: Chat room ID.
         *                  - Thread: Thread ID.
         * @param body      The message body.
         * @param direction The message direction, that is, whether the message is received or sent. This parameter is set to `SEND`.
         *                  - `SEND`: This message is sent from the local client.
         *                  - `RECEIVE`: The message is received by the local client.
         * @param hasRead   Whether a read receipt is required.
         *                  - `true`: Yes.
         *                  - `false`: No.
         */
        static public Message CreateSendMessage(string to, IMessageBody body, MessageDirection direction = MessageDirection.SEND, bool hasRead = true)
        {
            string user_name = SDKClient.Instance.CurrentUsername;

            if (string.IsNullOrEmpty(user_name))
            {
                user_name = "";
            }

            if (string.IsNullOrEmpty(to))
            {
                to = "";
            }

            Message msg = new Message(body: body)
            {
                Direction = direction,
                HasReadAck = hasRead,
                To = to,
                From = user_name,
                ConversationId = to,
                IsRead = true,
            };

            return msg;
        }

        /**
         * Creates a text message for sending.
         *
         * @param userId  The message recipient:
         *                  - One-to-one chat: The user ID of the peer user.
         *                  - Group chat: Group ID.
         *                  - Chat room: Chat room ID.
         *                  - Thread: Thread ID.
         * @param content The text content.
         */
        static public Message CreateTextSendMessage(string userId, string content)
        {
            return CreateSendMessage(userId, new MessageBody.TextBody(content));
        }

        /**
         * Creates a file message for sending.
         * @param userId            The message recipient:
         *                          - One-to-one chat: The user ID of the peer user.
         *                          - Group chat: Group ID.
         *                          - Chat room: Chat room ID.
         *                          - Thread: Thread ID.
         * @param localPath         The local path of the file.
         * @param displayName       The display name of the file.
         * @param fileSize          The file size in bytes.
         */
        static public Message CreateFileSendMessage(string userId, string localPath, string displayName = "", long fileSize = 0)
        {
            return CreateSendMessage(userId, new MessageBody.FileBody(localPath, displayName, fileSize: fileSize));
        }

        /**
         * Creates an image message for sending.
         *
         * @param userId                The message recipient:
         *                              - One-to-one chat: The user ID of the peer user.
         *                              - Group chat: Group ID.
         *                              - Chat room: Chat room ID.
         *                              - Thread: Thread ID.
         * @param localPath             The local path of the image.
         * @param displayName           The display name of the image.
         * @param fileSize              The image size in bytes.
         * @param original              Whether to send the original image.
         *                              - `true`: Yes. 
         *                              - (Default) `false`: No. The thumbnail is sent. For an image greater than 100 KB, the SDK will compress it before sending its thumbnail.
         * @param width                 The image width in pixels.
         * @param heigh                 The image height in pixels.
         * 
         *
         */
        static public Message CreateImageSendMessage(string userId, string localPath, string displayName = "", long fileSize = 0, bool original = false, double width = 0, double height = 0)
        {
            return CreateSendMessage(userId, new MessageBody.ImageBody(localPath, displayName: displayName, fileSize: fileSize, original: original, width: width, height: height));
        }

        /**
         * Creates a video message for sending.
         *
         * @param userId                The message recipient:
         *                              - One-to-one chat: The user ID of the peer user.
         *                              - Group chat: Group ID.
         *                              - Chat room: Chat room ID.
         *                              - Thread: Thread ID.
         * @param localPath             The URI of the video file.
         * @param displayName           The display name of the video file.
         * @param thumbnailLocalPath    The local path of the thumbnail of the video file.
         * @param fileSize              The size of the video file, in bytes. 
         * @param duration              The video duration in seconds.
         * @param width                 The video width in pixels.
         * @param heigh                 The video height in pixels.
         * 
         */
        static public Message CreateVideoSendMessage(string userId, string localPath, string displayName = "", string thumbnailLocalPath = "", long fileSize = 0, int duration = 0, double width = 0, double height = 0)
        {
            return CreateSendMessage(userId, new MessageBody.VideoBody(localPath, displayName: displayName, thumbnailLocalPath: thumbnailLocalPath, fileSize: fileSize, duration: duration, width: width, height: height));
        }

        /**
         * Creates a voice message for sending.
         *
         * @param userId        The message recipient:
         *                      - One-to-one chat: The user ID of the peer user.
         *                      - Group chat: Group ID.
         *                      - Chat room: Chat room ID.
         *                      - Thread: Thread ID.
         * @param localPath     The local path of the voice file.
         * @param displayName   The display name of f the voice file.
         * @param fileSize      The size of the voice file, in bytes.
         * @param duration      The voice duration in seconds.
         *
         */
        static public Message CreateVoiceSendMessage(string userId, string localPath, string displayName = "", long fileSize = 0, int duration = 0)
        {
            return CreateSendMessage(userId, new MessageBody.VoiceBody(localPath, displayName: displayName, fileSize: fileSize, duration: duration));
        }

        /**
         * Creates a location message for sending.
         *
         * @param userId        The message recipient:
         *                      - One-to-one chat: The user ID of the peer user.
         *                      - Group chat: Group ID.
         *                      - Chat room: Chat room ID.
         *                      - Thread: Thread ID.
         * @param latitude      The latitude.
         * @param longitude     The longitude.
         * @param address       The location details.
         * @param buildingName  The building name.
         * 
         */
        static public Message CreateLocationSendMessage(string userId, double latitude, double longitude, string address = "", string buildingName = "")
        {
            return CreateSendMessage(userId, new MessageBody.LocationBody(latitude: latitude, longitude: longitude, address: address, buildName: buildingName));
        }

        /**
         * Creates a command message for sending.
         *
         * @param userId                The message recipient:
         *                              - One-to-one chat: The user ID of the peer user.
         *                              - Group chat: Group ID.
         *                              - Chat room: Chat room ID.
         *                              - Thread: Thread ID.
         * @param action                The command action.
         * @param deliverOnlineOnly     Whether this command message is delivered only to the online users.
         *                              - `true`: Yes.
         *                              - (Default) `false`: No. The command message is delivered to users, regardless of their online or offline status.
         * 
         */
        static public Message CreateCmdSendMessage(string userId, string action, bool deliverOnlineOnly = false)
        {
            return CreateSendMessage(userId, new MessageBody.CmdBody(action, deliverOnlineOnly: deliverOnlineOnly));
        }

        /**
         * Creates a custom message for sending.
         *
         * @param userId            The message recipient:
         *                          - One-to-one chat: The user ID of the peer user.
         *                          - Group chat: Group ID.
         *                          - Chat room: Chat room ID.
         *                          - Thread: Thread ID.
         * @param customEvent       The custom event.
         * @param customParams      The dictionary of custom parameters.
         * 
         */
        static public Message CreateCustomSendMessage(string userId, string customEvent, Dictionary<string, string> customParams = null)
        {
            return CreateSendMessage(userId, new MessageBody.CustomBody(customEvent, customParams: customParams));
        }

        /**
        * Creates a combined message for sending.
        *
        * @param userId                 The message recipient:
         *                              - One-to-one chat: The user ID of the peer user.
         *                              - Group chat: Group ID.
         *                              - Chat room: Chat room ID.
         *                              - Thread: Thread ID.
        * @param title                  The title of the combined message. It can be `null` or an empty string ("").
        * @param summary                The summary of the combined message. It can be `null` or an empty string ("").
        * @param compatibleText         The compatible text of the combined message. It can be `null` or an empty string ("").
        * @param messageList            The ID list of messages included in the combined message. The list cannot be `null` or empty. It can contain a maximum of 300 message IDs.
        *
        *
        */
        static public Message CreateCombineSendMessage(string userId, string title, string summary, string compatibleText, List<string> messageList)
        {
            return CreateSendMessage(userId, new MessageBody.CombineBody(title: title, summary: summary, compatibleText: compatibleText, messageList: messageList));
        }

        /**
         * Gets the type of the message extension attribute.
         *
         * @param value          The extension attribute instance.
         */
        static public AttributeValueType GetAttributeValueType(AttributeValue value)
        {
            if (null == value) return AttributeValueType.NULLOBJ;

            return value.GetAttributeValueType();
        }

        /**
         * Sets an extension attribute.
         *
         * @param arriMap        The dictionary to which the new extension attribute will be added.
         * @param key            The keyword of the extension attribute.
         * @param type           The type of the extension attribute.
         * @param value          The value of the extension attribute.
         */
        static public void SetAttribute(Dictionary<string, AttributeValue> arriMap, string key, in object value, AttributeValueType type)
        {
            if (null == arriMap)
            {
                return;
            }
            AttributeValue attr = AttributeValue.Of(value, type);
            if (null != attr)
            {
                arriMap[key] = attr;
            }
        }

        /**
         * Gets the data of the generic <T> type of an extension attribute.
         *
         * @param value          The value of the extension attribute.
         * @param found          Whether the data of the generic <T> type is included in the value of the extension attribute.
         *
         * @return               The data of the generic <T> type.
         *                       - If `found` is `true`, the data of the generic <T> type is returned.
         *                       - If `found` is `false`, `null` is returned.
         */
        static public T GetAttributeValue<T>(AttributeValue value, out bool found)
        {
            if (null == value)
            {
                found = false;
                return default(T);
            }

            AttributeValueType type = value.GetAttributeValueType();
            object v = value.GetAttributeValue(type);
            if (null != v)
            {
                found = true;
                return (T)v;
            }
            else
            {
                found = false;
                return default(T);
            }
        }

        /**
         * Gets the data of the generic <T> type of an extension attribute from the extension attribute dictionary.
         *
         * @param arriMap        The dictionary which contains attributes.
         * @param key            The keyword in the dictionary for the extension attribute.
         * @param found          Whether the data of the generic <T> type is included in the value of the extension attribute.
         *
         * @return               The data of the generic <T> type.
         *                       - If `found` is `true`, the data of the generic <T> type is returned.
         *                       - If `found` is `false`, `null` is returned.
         *
         */
        static public T GetAttributeValue<T>(Dictionary<string, AttributeValue> arriMap, string key, out bool found)
        {
            if (null == arriMap)
            {
                found = false;
                return default(T);
            }

            if (!arriMap.ContainsKey(key))
            {
                found = false;
                return default(T);
            }

            AttributeValue value = arriMap[key];
            return GetAttributeValue<T>(value, out found);
        }

        [Preserve]
        internal Message() { }

        [Preserve]
        internal Message(string jsonString) : base(jsonString) { }

        [Preserve]
        internal Message(JSONObject jsonObject) : base(jsonObject) { }

        internal override void FromJsonObject(JSONObject jn)
        {
            if (null != jn)
            {
                if (!jn.IsNull && jn.IsObject)
                {
                    JSONObject jo = jn.AsObject;
                    From = jo["from"].Value;
                    To = jo["to"].Value;
                    HasReadAck = jo["hasReadAck"].AsBool;
                    HasDeliverAck = jo["hasDeliverAck"].AsBool;
                    LocalTime = (long)jo["localTime"].AsDouble;
                    ServerTime = (long)jo["serverTime"].AsDouble;
                    ConversationId = jo["convId"].Value;
                    MsgId = jo["msgId"].Value;
                    Status = jo["status"].AsInt.ToMessageStatus();
                    DeliverOnlineOnly = jo["deliverOnlineOnly"].AsBool;
                    MessageType = jo["chatType"].AsInt.ToMessageType();
                    Direction = jo["direction"].AsInt.ToMesssageDirection();
                    Attributes = AttributeValue.DictFromJsonObject(jo["attr"].AsObject);
                    // body:{type:iType, "body":{object}}
                    Body = ModelHelper.CreateBodyWithJsonObject(jo["body"]);
                    IsNeedGroupAck = jo["isNeedGroupAck"].AsBool;
                    IsRead = jo["isRead"].AsBool;
                    MessageOnlineState = jo["messageOnlineState"].AsBool;
                    IsThread = jo["isThread"].AsBool;
                    Broadcast = jo["broadcast"].AsBool;
                    IsContentReplaced = jo["isContentReplaced"].AsBool;
                }
            }
        }

        internal override JSONObject ToJsonObject()
        {
            JSONObject jo = new JSONObject();
            jo.AddWithoutNull("from", From);
            jo.AddWithoutNull("to", To);
            jo.AddWithoutNull("hasReadAck", HasReadAck);
            jo.AddWithoutNull("hasDeliverAck", HasDeliverAck);
            jo.AddWithoutNull("localTime", LocalTime);
            jo.AddWithoutNull("serverTime", ServerTime);
            jo.AddWithoutNull("convId", ConversationId);
            jo.AddWithoutNull("msgId", MsgId);
            jo.AddWithoutNull("priority", Priority.ToInt());
            jo.AddWithoutNull("deliverOnlineOnly", DeliverOnlineOnly);
            jo.AddWithoutNull("status", Status.ToInt());
            jo.AddWithoutNull("chatType", MessageType.ToInt());
            jo.AddWithoutNull("direction", Direction.ToInt());
            JSONNode jn = JsonObject.JsonObjectFromAttributes(Attributes);
            if (jn != null)
            {
                jo.AddWithoutNull("attr", jn);
            }
            jo.AddWithoutNull("body", Body.ToJsonObject());
            jo.AddWithoutNull("isNeedGroupAck", IsNeedGroupAck);
            jo.AddWithoutNull("isRead", IsRead);
            jo.AddWithoutNull("messageOnlineState", MessageOnlineState);
            jo.AddWithoutNull("isThread", IsThread);
            jo.AddWithoutNull("broadcast", Broadcast);
            jo.AddWithoutNull("isContentReplaced", IsContentReplaced);

            if (null != _receiverList && _receiverList.Count > 0)
            {
                jn = JsonObject.JsonArrayFromStringList(_receiverList);
                jo.AddWithoutNull("receiverList", jn);
            }

            return jo;
        }

    }
}
