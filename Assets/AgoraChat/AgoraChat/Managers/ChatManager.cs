using AgoraChat.SimpleJSON;
using System;
using System.Collections.Generic;

namespace AgoraChat
{
    public class ChatManager : BaseManager
    {
        internal List<IChatManagerDelegate> delegater;

        Object msgMapLocker;
        Dictionary<string, Message> msgMap;

        internal ChatManager(NativeListener listener) : base(listener, SDKMethod.chatManager)
        {
            listener.ChatManagerEvent += NativeEventHandle;
            delegater = new List<IChatManagerDelegate>();
            msgMapLocker = new Object();
            msgMap = new Dictionary<string, Message>();
        }

        private void AddMsgMap(string cbid, Message msg)
        {
            lock (msgMapLocker)
            {
                msgMap.Add(cbid, msg);
            }
        }

        private void UpdatedMsg(string cbid, JSONNode jsonNode)
        {
            lock (msgMapLocker)
            {
                if (msgMap.ContainsKey(cbid))
                {
                    var msg = msgMap[cbid];
                    if (jsonNode != null && jsonNode.IsObject)
                    {
                        msg.FromJsonObject(jsonNode.AsObject);
                    }
                }
            }
        }

        private void DeleteFromMsgMap(string cbid)
        {
            lock (msgMapLocker)
            {
                if (msgMap.ContainsKey(cbid))
                {
                    msgMap.Remove(cbid);
                }
            }
        }



        /**
        * Deletes a conversation from the local database.
        * 
        * If you set `deleteMessages` to `true`, local historical messages will be deleted with the conversation.
        *
        * @param conversationId     The conversation ID.
        * @param deleteMessages 	Whether to delete local historical messages with the conversation.
        *                           - `true`: Yes.
        *                           - `false`: No.
        *
        * @return 					Whether the conversation is successfully deleted:
        *                           - `true`: Yes. 
        *                           - `false`: No.
        */
        public bool DeleteConversation(string conversationId, bool deleteMessages = true)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("deleteMessages", deleteMessages);
            NativeCall(SDKMethod.deleteConversation, jo_param);
            return true;
        }

        /**
	     * Downloads the message attachment.
	     * 
	     * You can also call this method if the attachment fails to be downloaded automatically.
	     *
	     * @param messageId The ID of the message with the attachment to be downloaded.
	     * @param callback  The download status callback. See {@link CallBack}.
	     */
        public void DownloadAttachment(string messageId, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            NativeCall(SDKMethod.downloadAttachment, jo_param, callback);
        }

        /**
	     * Downloads the message thumbnail.
		 * 
	     * You can also call this method if the message thumbnail fails to be downloaded automatically.
		 * 
	     * @param messageId The ID of the message with the thumbnail to be downloaded.
	     * @param callback  The download status callback. See {@link CallBack}.
	     */
        public void DownloadThumbnail(string messageId, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            NativeCall(SDKMethod.downloadThumbnail, jo_param, callback);
        }

        /**
	     * Gets historical messages of the conversation from the server.
	     * 
	     * Historical messages of a conversation can also be obtained with pagination.
	     *
	     * This is an asynchronous method.
	     *
	     * @param conversationId 		The conversation ID.
	     * @param type 					The conversation type. See {@link ConversationType}.
	     * @param startMessageId 		The starting message ID for the query. 
		 *                              If `null` is passed, the SDK gets messages in the reverse chronological order of when the server received the messages.
	     * @param count 				The number of messages that you expect to get on each page.
	     * @param direction     		The direction in which the message is fetched. MessageSearchDirection can be set with following:
         *                   				- `UP`: Gets messages before the timestamp of the specified message ID;
         *                  				- `DOWN`: Gets messages after the timestamp of the specified message ID.
	     * @param callback				The result callback. Returns the list of obtained messages. 
	     */
        [Obsolete]
        public void FetchHistoryMessagesFromServer(string conversationId, ConversationType type = ConversationType.Chat, string startMessageId = null, int count = 20, MessageSearchDirection direction = MessageSearchDirection.UP, ValueCallBack<CursorResult<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", type.ToInt());
            jo_param.AddWithoutNull("startMsgId", startMessageId ?? "");
            jo_param.AddWithoutNull("direction", direction == MessageSearchDirection.UP ? 0 : 1);
            jo_param.AddWithoutNull("count", count);

            Process process = (_, jsonNode) =>
            {
                CursorResult<Message> cursor_msg = new CursorResult<Message>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<Message>(jn);
                });

                cursor_msg.FromJsonObject(jsonNode.AsObject);
                return cursor_msg;

            };

            NativeCall<CursorResult<Message>>(SDKMethod.fetchHistoryMessages, jo_param, callback, process);
        }

        /**
	     * Gets historical messages of a conversation from the server according to the parameter configuration class for pulling historical messages `FetchServerMessagesOption`.
	     *
	     * Historical messages of a conversation can be obtained with pagination.
	     *
	     * This is an asynchronous method.
	     *
	     * @param conversationId 		The conversation ID.
	     * @param type 					The conversation type. See {@link ConversationType}.
	     * @param cursor                The cursor position from which to start querying data.
	     * @param pageSize              The number of messages that you expect to get on each page. The value range is [1,50].
	     * @param option                The parameter configuration class for pulling historical messages from the server. See {@link FetchServerMessagesOption}.
	     * @param callback				The result callback. The SDK returns the list of obtained messages.
	     */
        public void FetchHistoryMessagesFromServerBy(string conversationId, ConversationType type = ConversationType.Chat, string cursor = null, int pageSize = 10, FetchServerMessagesOption option = null, ValueCallBack<CursorResult<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", type.ToInt());
            jo_param.AddWithoutNull("cursor", cursor ?? "");
            jo_param.AddWithoutNull("pageSize", pageSize);
            if (null != option) jo_param.AddWithoutNull("options", option.ToJsonObject());

            Process process = (_, jsonNode) =>
            {
                CursorResult<Message> cursor_msg = new CursorResult<Message>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<Message>(jn);
                });

                cursor_msg.FromJsonObject(jsonNode.AsObject);
                return cursor_msg;

            };

            NativeCall<CursorResult<Message>>(SDKMethod.fetchHistoryMessagesBy, jo_param, callback, process);
        }

        /**
	     * Gets the local conversation object.
	     * 
	     * The SDK wil return `null` if the conversation is not found.
	     *
	     * @param conversationId 	The conversation ID.
	     * @param type              The conversation type. See {@link ConversationType}.
	     * @param createIfNeed      Whether to automatically create a conversation if the conversation is not found. 
	     *                          - `true`: Yes.
	     *                          - `false`: No.
	     * @return 		            The conversation found. Returns `null` if the conversation is not found.
	     */
        public Conversation GetConversation(string conversationId, ConversationType type = ConversationType.Chat, bool createIfNeed = true)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", type.ToInt());
            jo_param.AddWithoutNull("createIfNeed", createIfNeed);
            jo_param.AddWithoutNull("isThread", false);

            JSONNode jn = NativeGet(SDKMethod.getConversation, jo_param).GetReturnJsonNode();

            if (null == jn) return null;
            return new Conversation(jn.AsObject);
        }

        /**
	     * Gets the local thread conversation object.
	     * 
	     * The SDK wil return `null` if the conversation is not found.
	     *
	     * @param threadId 			The thread ID.
	     * 
	     * @return 		            The conversation found. Returns `null` if the conversation is not found.
	     */
        public Conversation GetThreadConversation(string threadId)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", threadId);
            jo_param.AddWithoutNull("convType", ConversationType.Group.ToInt());
            jo_param.AddWithoutNull("createIfNeed", true);
            jo_param.AddWithoutNull("isThread", true);

            string json = NativeGet(SDKMethod.getThreadConversation, jo_param);

            if (null == json || json.Length == 0) return null;
            return new Conversation(json);
        }

        /**
	     * Gets all conversations from the server.
	     * 
	     * An empty list will be returned if no conversation is found.
	     *
	     * @param callback    The list of obtained conversations. See {@link ValueCallBack}.
	     */
        [Obsolete]
        public void GetConversationsFromServer(ValueCallBack<List<Conversation>> callback = null)
        {
            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Conversation>(jsonNode);
            };

            NativeCall<List<Conversation>>(SDKMethod.getConversationsFromServer, null, callback, process);
        }

        /**
	     * Gets the conversations from the server.
	     *
	     * @param pingOnly    Whether to return pinned conversations only:
         * - `true`: Yes. The SDK only returns pinned conversations in the reverse chronological order of their pinning.
         * - `false`: No.
         *
	     * @param cursor      The position from which to start getting data.
	     * @param limit       The number of conversations that you expect to get on each page. The value range is [1,50].
	     * @param callback    The list of obtained conversations. See {@link ValueCallBack}.
	     */
        public void GetConversationsFromServerWithCursor(bool pinOnly, string cursor = "", int limit = 20, ValueCallBack<CursorResult<Conversation>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("pinOnly", pinOnly);
            jo_param.AddWithoutNull("cursor", cursor);
            jo_param.AddWithoutNull("limit", limit);

            Process process = (_, jsonNode) =>
            {
                CursorResult<Conversation> cursor_conversation = new CursorResult<Conversation>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<Conversation>(jn);
                });

                cursor_conversation.FromJsonObject(jsonNode.AsObject);
                return cursor_conversation;
            };

            NativeCall<CursorResult<Conversation>>(SDKMethod.getConversationsFromServerWithCursor, jo_param, callback, process);
        }

        /**
         * Gets the conversations from the server based on the conversation mark.
         * 
         * @param mark        The mark value used for searching.
         * @param cursor      The position from which to start getting data.
         * @param limit       The number of conversations that you expect to get on each page. The value range is [1,50].
         * @param callback    The list of obtained conversations. See {@link ValueCallBack}.
         */
        public void GetConversationsFromServerWithCursor(MarkType mark, string cursor = "", int limit = 20, ValueCallBack<CursorResult<Conversation>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("needMark", true);
            jo_param.AddWithoutNull("mark", (int)mark);
            jo_param.AddWithoutNull("cursor", cursor);
            jo_param.AddWithoutNull("limit", limit);

            Process process = (_, jsonNode) =>
            {
                CursorResult<Conversation> cursor_conversation = new CursorResult<Conversation>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<Conversation>(jn);
                });

                cursor_conversation.FromJsonObject(jsonNode.AsObject);
                return cursor_conversation;
            };

            NativeCall<CursorResult<Conversation>>(SDKMethod.getConversationsFromServerWithCursorAndMark, jo_param, callback, process);
        }

        /**
	     * Gets the unread message count.
	     *
	     * @return		The count of unread messages.
	     *
	     */
        public int GetUnreadMessageCount()
        {
            string json = NativeGet(SDKMethod.getUnreadMessageCount);

            if (null == json || json.Length == 0) return 0;

            JSONObject jo = JSON.Parse(json).AsObject;
            return int.Parse(jo["ret"].Value);
        }

        /**
        * Gets the message count in DB.
        *
        * @return           The count of messages in DB.
        * @param callback   Returns the total number of messages on success, -1 on failure. See {@link ValueCallBack}。
        *
        */
        public void GetMessageCount(ValueCallBack<int> callback = null)
        {
            Process process = (_, jsonNode) =>
            {
                return jsonNode["ret"].IsNumber ? jsonNode["ret"].AsInt : -1;
            };

            NativeCall<int>(SDKMethod.getMessagesCount, null, callback, process);
        }

        /**
        * Imports messages to the local database.
        * 
        * You can only import messages that you sent or received.
        *
        * @param messages The messages to be imported.
        * 
        * @return Whether messages are successfully imported to the local database.
        *         - `true`: Yes.
        *         - `false`: No.

        */
        public void ImportMessages(List<Message> messages, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("list", JsonObject.JsonArrayFromList(messages));
            NativeCall(SDKMethod.importMessages, jo_param, callback);
        }

        /**
	     * Loads all conversations from the local database into the memory.
		 * 
	     * To accelerate the loading, call this method immediately after the user is logged in.
	     * 
	     * @return            The list of loaded conversations.
	     */
        public List<Conversation> LoadAllConversations()
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("isSort", true);

            JSONNode jn = NativeGet(SDKMethod.loadAllConversations, jo_param).GetReturnJsonNode();

            if (null == jn) return new List<Conversation>();

            return List.BaseModelListFromJsonArray<Conversation>(jn);
        }

        /**
	     * Loads a specified message from the local database into the memory.
	     * 
	     * @param messageId		The ID of the message to be loaded.
	     * @return				The loaded message object.
	     */
        public Message LoadMessage(string messageId)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            JSONNode jn = NativeGet(SDKMethod.getMessage, jo_param).GetReturnJsonNode();

            if (null == jn) return null;
            return new Message(jn.AsObject);
        }

        /**
		 * Marks all local conversations as read.
		 * 
		 *  @return	 Whether all local conversations are marked as read. 
		 *           - `true`: Yes. 
		 *           - `false`: No.
		 */
        public bool MarkAllConversationsAsRead()
        {
            string json = NativeGet(SDKMethod.markAllChatMsgAsRead);
            if (null == json || json.Length == 0) return false;

            JSONNode jn = JSON.Parse(json);
            return jn["ret"].AsBool;
        }

        /**
		 * Recalls the message.
		 *
		 * This is an asynchronous method.
		 *
		 * @param message   The ID of the message to be recalled.
		 * @param ext       Prompt information when recalled the message.
		 * @param callback  The recall status callback. See {@link CallBack}.
		 */
        public void RecallMessage(string messageId, string ext, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("ext", ext);

            NativeCall(SDKMethod.recallMessage, jo_param, callback);
        }

        /**
		 * Resends the message.[Deprecated]
		 *
		 * This is an asynchronous method.
		 *
		 * @param message	The ID of the message to be resent.
		 * @param callback  The resending status callback. See {@link CallBack}.
		 * @return			The message that is resent.
		 */
        [Obsolete("ResendMessage is deprecated", false)]
        public Message ResendMessage(string messageId, CallBack callback = null)
        {
            /*
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);

            JSONNode jn = NativeGet(SDKMethod.resendMessage, jo_param, callback).GetReturnJsonNode();

            if (null == jn) return null;

            return new Message(jn);
            */
            return null;
        }

        /**
		 * Retrieves local messages of a certain quantity.
		 * 
		 * **Note**
		 * If you want to query a great number of messages, pay attention to the memory consumption. A maximum number of 200 messages can be retrieved each time.
		 *
		 * @param keywords   The keyword for query. The data format is String.
		 * @param timestamp  The starting Unix timestamp for query, which is in milliseconds. After this parameter is set, the SDK retrieves messages, starting from the specified one, according to the message search direction.
		 *                   If you set this parameter as a negative value, the SDK retrieves messages, starting from the current time, in the descending order of the the Unix timestamp included in them.
		 * @param maxCount   The maximum number of messages to retrieve.
		 * @param from       The user ID of the message sender. If you do not set this parameter, the SDK ignores this parameter when retrieving messages.
		 * @param direction	 The query direction. See {@link MessageSearchDirection}.
		 * @param callback   If success, a list of original messages included in the combined message are returned; otherwise, an error is returned. See {@link ValueCallBack}.
		 */
        public void SearchMsgFromDB(string keywords, long timestamp = 0, int maxCount = 20, string from = null, MessageSearchDirection direction = MessageSearchDirection.UP, ValueCallBack<List<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("keywords", keywords);
            jo_param.AddWithoutNull("from", from ?? "");
            jo_param.AddWithoutNull("count", maxCount);
            jo_param.AddWithoutNull("timestamp", timestamp);
            jo_param.AddWithoutNull("direction", direction.ToInt());

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Message>(jsonNode);
            };

            NativeCall<List<Message>>(SDKMethod.searchChatMsgFromDB, jo_param, callback, process);
        }

        /**
        * Queries local messages based on the message scope.
        *
        * **Note**
        * If you want to query a great number of messages, pay attention to the memory consumption. A maximum number of 200 messages can be retrieved each time.
        *
        * @param keywords   The keyword for query. The data format is String.
        * @param timestamp  The starting Unix timestamp for query, which is in milliseconds.
        * @param maxCount   The maximum number of messages to retrieve.
        * @param from       The user ID of the message sender. If you do not set this parameter, the SDK ignores this parameter when retrieving messages.
        * @param direction	The query direction. See {@link MessageSearchDirection}.
        * @param scope	    The query direction. See {@link MessageSearchScope}.
        * @param callback   If success, a list of original messages included in the combined message are returned; otherwise, an error is returned. See {@link ValueCallBack}.
        */
        public void SearchMsgFromDB(string keywords, long timestamp = 0, int maxCount = 20, string from = null, MessageSearchDirection direction = MessageSearchDirection.UP, MessageSearchScope scope = MessageSearchScope.CONTENT, ValueCallBack<List<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("keywords", keywords);
            jo_param.AddWithoutNull("from", from ?? "");
            jo_param.AddWithoutNull("count", maxCount);
            jo_param.AddWithoutNull("timestamp", timestamp);
            jo_param.AddWithoutNull("direction", direction.ToInt());
            jo_param.AddWithoutNull("scope", scope.ToInt());

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Message>(jsonNode);
            };

            NativeCall<List<Message>>(SDKMethod.searchChatMsgFromDBWithScope, jo_param, callback, process);
        }

        /**
		 * Sends the conversation read receipt to the server.
         *
		 * After this method is called, the sever will set the message status from unread to read. 
         *
		 * The SDK triggers the {@link IChatManagerDelegate#OnConversationRead(string from, string to)} callback on the message sender's client, notifying that the messages are read. This also applies to multi-device scenarios.
		 *
		 * @param conversationId	The conversation ID.
		 * @param callback			The result callback. See {@link CallBack}.
		 */
        public void SendConversationReadAck(string conversationId, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);

            NativeCall(SDKMethod.ackConversationRead, jo_param, callback);
        }

        /**
		 * Sends a message。
		 *
		 * This is an asynchronous method.
		 *
		 * For attachment messages such as voice or image, the SDK will automatically upload the attachment by default. See {@link Options#ServerTransfer}.
		 *
		 *
		 * @param msg		 The message object to be sent. Ensure that you set this parameter. 
		 * @param callback	 The result callback. See {@link CallBack}.
		 */
        public void SendMessage(ref Message message, CallBack callback = null)
        {
            Process process = (_cbid, _json) =>
            {
                UpdatedMsg(_cbid, _json["ret"]);
                DeleteFromMsgMap(_cbid);
                return null;
            };

            callbackManager.AddCallbackAction(callback, process);

            AddMsgMap(callback.callbackId, message);

            JSONObject jo_param = message.ToJsonObject();

            JSONNode jn = CWrapperNative.NativeGet(managerName, SDKMethod.sendMessage, jo_param, callback?.callbackId ?? "").GetReturnJsonNode();

            if (null != jn)
            {
                UpdatedMsg(callback.callbackId, jn["ret"]);
            }
        }

        /**
		 * Sends the read receipt of a one-to-one message to the server.
		 * 
		 * After this method is called, the sever will set the message status from unread to read. 
		 *
		 * The SDK triggers the {@link IChatManagerDelegate#OnMessagesRead(List<Message>)} callback on the message sender's client, notifying that the messages are read. This also applies to multi-device scenarios.
		 *
		 * @param messageId		The message ID.
		 * @param callback		The result callback. See {@link CallBack}.
		 */
        public void SendMessageReadAck(string messageId, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            NativeCall(SDKMethod.ackMessageRead, jo_param, callback);
        }

        /**
        * Sends a read receipt for a group message to the server.
        *
        * You can only call the method after setting {@link Options#RequireAck(boolean)} and {@link Message#IsNeedGroupAck(boolean)}.
        *
        * To send the read recipient for a one-to-one chat message to the server, call {@link #SendMessageReadAck(String)}.
        *
        * To send the conversation read receipt to the server, call {@link #SendConversationReadAck(String)}.
        *
        * @param messageId     The message ID.
        * @param ackContent    The content of the read receipt. The content is a self-defined string that can be used for specifying custom action/command.
        * @param callback	   The result callback. See{@ link CallBack}.
    */
        public void SendReadAckForGroupMessage(string messageId, string ackContent, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("content", ackContent);
            NativeCall(SDKMethod.ackGroupMessageRead, jo_param, callback);
        }

        /**
		 * Updates the local message.
		 * 
		 * After this method is called, messages in both the local memory and local database will be updated.
		 *
		 * @param message	The message object to update.
		 * @return			Whether the local message is successfully updated.	 
		 *                  - `true`: Yes. 
		 *                  - `false`: No.
		 */
        public bool UpdateMessage(Message message)
        {
            JSONObject jo_param = message.ToJsonObject();

            string json = NativeGet(SDKMethod.updateChatMessage, jo_param);

            if (null == json || json.Length == 0) return false;

            JSONObject jsonObject = JSON.Parse(json).AsObject;
            return jsonObject["ret"].AsBool;
        }

        /**
         * Modifies a message.
         *
         * After this method is called to modify a message, both the local message and the message on the server are modified.
         *
         * This method can only modify a text message in one-to-one chats or group chats, but not in chat rooms.
         *
         * @param messageId The ID of the message to modify.
         * @param body      The modified message body.
         * @param callBack The result callback. See {@link #CallBack()}.
         */
        public void ModifyMessage(string messageId, MessageBody.TextBody body, ValueCallBack<Message> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("body", body.ToJsonObject());

            Process process = (_, jsonNode) =>
            {
                return ModelHelper.CreateWithJsonObject<Message>(jsonNode);
            };

            NativeCall<Message>(SDKMethod.modifyMessage, jo_param, callback, process);
        }

        /**
		 * Removes messages that are sent and received before the Unix timestamp from the local memory and database.
		 *
		 * @param timeStamp	The starting Unix timestamp for removal.
		 * @param callback	The removal result callback. See {@link CallBack}.
		 */

        public void RemoveMessagesBeforeTimestamp(long timeStamp, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("timestamp", timeStamp);
            NativeCall(SDKMethod.deleteMessagesBeforeTimestamp, jo_param, callback);
        }

        /**
		 * Deletes the specified conversation and its historical messages from the server.
		 * 
		 * This is an asynchronous method.
		 *
		 * @param conversationId 			The conversation ID.
		 * @param conversationType          The conversation type. See {@link ConversationType}.
		 * @param isDeleteServerMessages 	Whether to delete the historical messages with the conversation.
		 *                                  - `true`: Yes.
		 *                                  - `false`: No.
		 * @param callback					Callback for whether the conversation is deleted. See {@link CallBack}.
		 */
        public void DeleteConversationFromServer(string conversationId, ConversationType conversationType, bool isDeleteServerMessages, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", conversationType.ToInt());
            jo_param.AddWithoutNull("isDeleteServerMessages", isDeleteServerMessages);

            NativeCall(SDKMethod.deleteRemoteConversation, jo_param, callback);
        }

        /**
         * 获取翻译服务支持的语言。
         *
         * @param callBack 完成的回调，详见 {@link #ValueCallBack()}。
         *
         * ~english
         * Gets all languages supported by the translation service.
         *
         * @param callBack The result callback. See {@link #ValueCallBack()}.
         */
        public void FetchSupportLanguages(ValueCallBack<List<SupportLanguage>> callback = null)
        {

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<SupportLanguage>(jsonNode);
            };

            NativeCall<List<SupportLanguage>>(SDKMethod.fetchSupportedLanguages, null, callback, process);
        }

        /**
         * Translates a message.
         * @param message The message object.
         * @param languages The code list of the target languages.
         * @param callBack The result callback. See {@link #CallBack()}.
         */
        public void TranslateMessage(Message message, List<string> targetLanguages, ValueCallBack<Message> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("message", message.ToJsonObject());
            jo_param.AddWithoutNull("languages", JsonObject.JsonArrayFromStringList(targetLanguages));

            Process process = (_, jsonNode) =>
            {
                return ModelHelper.CreateWithJsonObject<Message>(jsonNode);
            };

            NativeCall<Message>(SDKMethod.translateMessage, jo_param, callback, process);
        }

        /**
         * Uses the pagination to get read receipts for a group message from the server.
         *
         * To send a read receipt for a group message, you can call {@link #SendReadAckForGroupMessage}.
         *
         * This is an asynchronous method.
         *
         * @param msgId			The message ID.
		 * @param groupId		The group ID。
         * @param pageSize		The number of read receipts for the group message that you expect to get on each page. The value range is [1,50].
         * @param startAckId    The starting read receipt ID for query. After this parameter is set, the SDK retrieves read receipts, from the specified one, in the reverse chronological order of when the server receives them.
         *                      If you set this parameter as null, the SDK will retrieve from the latest read receipt.
         * @param callBack		The result callback. If the call succeeds, the SDK executes {@link ValueCallBack#onSuccess(Object)};
         *                      if the call fails, the SDK executes {@link ValueCallBack#onError(int, String)}.
         */
        public void FetchGroupReadAcks(string messageId, string groupId, int pageSize = 20, string startAckId = null, ValueCallBack<CursorResult<GroupReadAck>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("pageSize", pageSize);
            jo_param.AddWithoutNull("groupId", groupId);
            jo_param.AddWithoutNull("ackId", startAckId ?? "");


            Process process = (_, jsonNode) =>
            {
                CursorResult<GroupReadAck> cursor_msg = new CursorResult<GroupReadAck>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<GroupReadAck>(jn);
                });

                cursor_msg.FromJsonObject(jsonNode.AsObject);
                return cursor_msg;
            };
            NativeCall<CursorResult<GroupReadAck>>(SDKMethod.asyncFetchGroupAcks, jo_param, callback, process);
        }

        /**
         * Reports an inappropriate message.
         *
         * @param messageId		The ID of the message to report.
         * @param tag			The tag of the inappropriate message. You need to type a custom tag, like `porn` or `ad`.
         * @param reason		The reporting reason. You need to type a specific reason.
         *
         * @param callBack The result callback，see {@link #CallBack()}.
         */
        public void ReportMessage(string messageId, string tag, string reason, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("tag", tag);
            jo_param.AddWithoutNull("reason", reason);

            NativeCall(SDKMethod.reportMessage, jo_param, callback);
        }

        /**
         * Adds a Reaction.
         *
         * This is an asynchronous method.
         *
         * @param messageId The message ID.
         * @param reaction  The message Reaction.
         * @param callback  The result callback which contains the error information if the method fails.
         */
        public void AddReaction(string messageId, string reaction, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("reaction", reaction);

            NativeCall(SDKMethod.addReaction, jo_param, callback);
        }

        /**
         * Deletes a Reaction.
         *
         * This is an asynchronous method.
         *
         * @param messageId The message ID.
         * @param reaction  The Reaction content.
         * @param callback  The result callback which contains the error information if the method fails.
         */

        public void RemoveReaction(string messageId, string reaction, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("reaction", reaction);

            NativeCall(SDKMethod.removeReaction, jo_param, callback);
        }

        /**
        * Gets the list of Reactions.
        *
        * This is an asynchronous method.
        *
        * @param messageIdList  The message ID.
        * @param chatType       The chat type. Only one-to-one chat ({@link ConversationType.Chat} and group chat ({@link ConversationType.Group}) are allowed.
        * @param groupId        The group ID, which is valid only when the chat type is group chat.
        * @param callback       The result callback, which contains the Reaction list under the specified message ID（The user list of EMMessageReaction is the summary data, which only contains the information of the first three users）.
        */
        public void GetReactionList(List<string> messageIdList, MessageType chatType, string groupId, ValueCallBack<Dictionary<string, List<MessageReaction>>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgIds", JsonObject.JsonArrayFromStringList(messageIdList));
            jo_param.AddWithoutNull("groupId", groupId);
            //TODO: need to check
            jo_param.AddWithoutNull("type", chatType == MessageType.Group ? "groupchat" : "chat");
            Process process = (_, jsonNode) =>
            {
                return Dictionary.ListBaseModelDictionaryFromJsonObject<MessageReaction>(jsonNode);
            };

            NativeCall<Dictionary<string, List<MessageReaction>>>(SDKMethod.fetchReactionList, jo_param, callback, process);
        }

        /**
        * Gets the Reaction details.
        *
        * This is an asynchronous method. 
        *
        * @param messageId    The message ID.
        * @param reaction     The Reaction content.
        * @param cursor       The query cursor.
        * @param pageSize     The number of Reactions you expect to get on each page.
        * @param callback     The result callback, which contains the reaction list obtained from the server and the cursor for the next query. Returns null if all the data is fetched.
        */
        public void GetReactionDetail(string messageId, string reaction, string cursor = null, int pageSize = 20, ValueCallBack<CursorResult<MessageReaction>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("reaction", reaction);
            jo_param.AddWithoutNull("cursor", cursor ?? "");
            jo_param.AddWithoutNull("pageSize", pageSize);


            Process process = (_, jsonNode) =>
            {
                CursorResult<MessageReaction> cursor_msg = new CursorResult<MessageReaction>(_, (jn) =>
                {
                    return ModelHelper.CreateWithJsonObject<MessageReaction>(jn);
                });

                cursor_msg.FromJsonObject(jsonNode.AsObject);
                return cursor_msg;
            };

            NativeCall<CursorResult<MessageReaction>>(SDKMethod.fetchReactionDetail, jo_param, callback, process);
        }

        /**
         * Gets the conversations from the server.
         *
         * An empty list will be returned if no conversation is found.
         *
         * @param pageNum     The current page number.
         * @param pageSize    The number of conversations to get on each page.
         * @param callback    The list of obtained conversations. See {@link ValueCallBack}.
         */
        public void GetConversationsFromServerWithPage(int pageNum, int pageSize, ValueCallBack<List<Conversation>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("pageNum", pageNum);
            jo_param.AddWithoutNull("pageSize", pageSize);

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Conversation>(jsonNode);
            };

            NativeCall<List<Conversation>>(SDKMethod.getConversationsFromServerWithPage, jo_param, callback, process);
        }

        /**
         * Removes messages in a conversation (from both local storage and the server).
         *
         * This is an asynchronous method.
         *
         * @param conversationId     The conversation ID.
         * @param conversationType   The conversation type. See {@link ConversationType}.
         * @param messageIdList      The list of IDs of messages to be removed.
         * @param callback           Callback for the operation. See {@link CallBack}.
         */

        public void RemoveMessagesFromServer(string conversationId, ConversationType conversationType, List<string> messageIdList, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", conversationType.ToInt());
            jo_param.AddWithoutNull("msgIds", JsonObject.JsonArrayFromStringList(messageIdList));

            NativeCall(SDKMethod.removeMessagesFromServerWithMsgIds, jo_param, callback);
        }

        /**
         * Removes messages in a conversation (from both local storage and the server).
         *
         * This is an asynchronous method.
         *
         * @param conversationId     The conversation ID.
         * @param conversationType   The conversation type. See {@link ConversationType}.
         * @param timeStamp          The specified Unix timestamp in milliseconds. Messages with a timestamp before the specified one will be removed from the conversation.
         * @param callback           Callback for the operation. See {@link CallBack}.
         */

        public void RemoveMessagesFromServer(string conversationId, ConversationType conversationType, long timeStamp, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("convType", conversationType.ToInt());
            jo_param.AddWithoutNull("timestamp", timeStamp);

            NativeCall(SDKMethod.removeMessagesFromServerWithTs, jo_param, callback);
        }

        /**
         * Sets whether to pin a conversation.
         *
         * This is an asynchronous method.
         *
         * @param conversationId     The conversation ID.
         * @param isPinned           Whether to pin the conversation:
         *                           - `true`: Yes.
         *                           - `false`: No.
         * @param callback           Callback for the operation. See {@link CallBack}.
         */

        public void PinConversation(string conversationId, bool isPinned, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);
            jo_param.AddWithoutNull("isPinned", isPinned);

            NativeCall(SDKMethod.pinConversation, jo_param, callback);
        }

        /**
         * Gets and parses the combined message.
         *
         * @param msg               The combined message to get and parse.
         * @param callback          If success, a list of original messages included in the combined message are returned; otherwise, an error is returned. See {@link ValueCallBack}.
         */
        public void FetchCombineMessageDetail(Message msg, ValueCallBack<List<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msg", msg.ToJsonObject());

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Message>(jsonNode);
            };

            NativeCall<List<Message>>(SDKMethod.downloadCombineMessages, jo_param, callback, process);
        }

        /**
        * Marks or unmarks conversations.
        *
        * This is an asynchronous method.
        *
        * This method marks conversations both locally and on the server.
        *
        * @param conversationIds    The list of conversation IDs.
        * @param isMarked           Whether to add or remove the mark for the conversations.
        *                           - `true`: add. 
        *                           - `false`: remove.
        * @param mark               The conversation mark to add or remove.
        * @param callback           Callback for the operation. See {@link CallBack}.
        */
        public void MarkConversations(List<string> conversationIds, bool isMarked, MarkType mark, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convIds", JsonObject.JsonArrayFromStringList(conversationIds));
            jo_param.AddWithoutNull("isMarked", isMarked);
            jo_param.AddWithoutNull("mark", (int)mark);

            NativeCall(SDKMethod.markConversations, jo_param, callback);
        }

        /**
        * Clears all conversations and all messages in them.
        *
        * This is an asynchronous method.
        *
        * @param clearServerData   Whether to clear all conversations and all messages in them on the server. 
        *   - `true`：Yes. All conversations and all messages in them will be cleared on the server side. 
            The current user cannot retrieve messages and conversations from the server, while this has no impact on other users.
        *  - (Default) `false`：No. All local conversations and all messages in them will be cleared, while those on the server remain.
        * @param callback           Callback for the operation. See {@link CallBack}.
        */
        public void DeleteAllMessagesAndConversations(bool clearServerData, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("clearServerData", clearServerData);

            NativeCall(SDKMethod.deleteAllMessagesAndConversations, jo_param, callback);
        }

        /**
        * Pins or unpins a message.
        *
        * This is an asynchronous method.
        *
        * @param messageId          The message ID to be pinned or unpinned.
        * @param isPinned           Whether to pin the message:
        *   - `true`: pin.
        *   - `false`: unpin.
        * @param callback           Callback for the operation. See {@link CallBack}.
        */
        public void PinMessage(string messageId, bool isPinned, CallBack callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("msgId", messageId);
            jo_param.AddWithoutNull("isPinned", isPinned);

            NativeCall(SDKMethod.pinMessage, jo_param, callback);
        }

        /**
         * Gets the list of pinned messages in the conversation from the server.
         *
         * @param msg               The conversation ID.
         * @param callback          If success, the list of pined messages in the conversation are returned; otherwise, an error is returned. See {@link ValueCallBack}.
         */
        public void GetPinnedMessagesFromServer(string conversationId, ValueCallBack<List<Message>> callback = null)
        {
            JSONObject jo_param = new JSONObject();
            jo_param.AddWithoutNull("convId", conversationId);

            Process process = (_, jsonNode) =>
            {
                return List.BaseModelListFromJsonArray<Message>(jsonNode);
            };

            NativeCall<List<Message>>(SDKMethod.getPinnedMessagesFromServer, jo_param, callback, process);
        }

        /**
		 * Adds a chat manager listener.
		 *
		 * @param chatManagerDelegate 	The chat manager listener to add. It is inherited from {@link IChatManagerDelegate}.
		 *
		 */
        public void AddChatManagerDelegate(IChatManagerDelegate chatManagerDelegate)
        {
            if (!delegater.Contains(chatManagerDelegate))
            {
                delegater.Add(chatManagerDelegate);
            }
        }

        /**
		 * 移除聊天管理器的监听器。
		 *
		 * @param chatManagerDelegate 	要移除的聊天管理器的监听器，继承自 {@link IChatManagerDelegate}。
		 *
		 * ~english
		 * Removes a chat manager listener.
		 * 
		 * @param chatManagerDelegate 	The chat manager listener to remove. It is inherited from {@link IChatManagerDelegate}.
		 *
		 */
        public void RemoveChatManagerDelegate(IChatManagerDelegate chatManagerDelegate)
        {
            if (delegater.Contains(chatManagerDelegate))
            {
                delegater.Remove(chatManagerDelegate);
            }
        }

        internal void ClearDelegates()
        {
            delegater.Clear();
        }

        internal void NativeEventHandle(string method, JSONNode jsonNode)
        {
            if (delegater.Count == 0) return;

            switch (method)
            {
                case SDKMethod.onMessagesReceived:
                    {
                        List<Message> list = List.BaseModelListFromJsonArray<Message>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnMessagesReceived(list);
                        }
                    }
                    break;
                case SDKMethod.onCmdMessagesReceived:
                    {
                        List<Message> list = List.BaseModelListFromJsonArray<Message>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnCmdMessagesReceived(list);
                        }
                    }
                    break;
                case SDKMethod.onMessagesRead:
                    {
                        List<Message> list = List.BaseModelListFromJsonArray<Message>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnMessagesRead(list);
                        }
                    }
                    break;
                case SDKMethod.onMessagesDelivered:
                    {
                        List<Message> list = List.BaseModelListFromJsonArray<Message>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnMessagesDelivered(list);
                        }
                    }
                    break;
                /*case SDKMethod.onMessagesRecalled:
                    {
                        List<Message> list = List.BaseModelListFromJsonArray<Message>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnMessagesRecalled(list);
                        }
                    }
                    break;*/
                case SDKMethod.onMessagesRecalledByExt:
                    {
                        List<RecallMessageInfo> list = List.BaseModelListFromJsonArray<RecallMessageInfo>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnMessagesRecalled(list);
                        }
                    }
                    break;
                case SDKMethod.onReadAckForGroupMessageUpdated:
                    {
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.OnReadAckForGroupMessageUpdated();
                        }
                    }
                    break;
                case SDKMethod.onGroupMessageRead:
                    {
                        List<GroupReadAck> list = List.BaseModelListFromJsonArray<GroupReadAck>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.OnGroupMessageRead(list);
                        }
                    }
                    break;
                case SDKMethod.onConversationsUpdate:
                    {
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.OnConversationsUpdate();
                        }
                    }
                    break;
                case SDKMethod.onConversationRead:
                    {
                        string from = jsonNode["from"];
                        string to = jsonNode["to"];
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.OnConversationRead(from, to);
                        }
                    }
                    break;
                case SDKMethod.onMessageReactionDidChange:
                    {
                        List<MessageReactionChange> list = List.BaseModelListFromJsonArray<MessageReactionChange>(jsonNode);
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            if (list.Count > 0) it.MessageReactionDidChange(list);
                        }
                    }
                    break;
                case SDKMethod.onMessageContentChanged:
                    {
                        Message msg = new Message(jsonNode["msg"].AsObject);
                        string operatorId = jsonNode["operatorId"];
                        long operationTime = (long)jsonNode["operationTime"].AsDouble;

                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.OnMessageContentChanged(msg, operatorId, operationTime);
                        }
                    }
                    break;
                case SDKMethod.onMessagePinChanged:
                    {
                        string messageId = jsonNode["msgId"];
                        string conversationId = jsonNode["convId"];
                        bool isPinned = jsonNode["isPinned"].AsBool;
                        string operatorId = jsonNode["operatorId"];
                        long operationTime = (long)jsonNode["ts"].AsDouble;

                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.OnMessagePinChanged(messageId, conversationId, isPinned, operatorId, operationTime);
                        }
                    }
                    break;
                case SDKMethod.onMessageIdChanged:
                    {
                        /*
                        string conversationId = jsonNode["convId"];
                        string oldMsgId = jsonNode["oldMsgId"];
                        string newMsgId = jsonNode["newMsgId"];
                        foreach (IChatManagerDelegate it in delegater)
                        {
                            it.onMessageIdChanged(conversationId, oldMsgId, newMsgId);
                        }
                        */
                    }
                    break;
            }
        }
    }
}
