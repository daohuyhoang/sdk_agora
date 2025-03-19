using AgoraChat.SimpleJSON;
#if !_WIN32
using UnityEngine.Scripting;
#endif

namespace AgoraChat
{
    /**
     * The message recall information object.
     */
    [Preserve]
    public class RecallMessageInfo : BaseModel

    {
        /**
         * The user that recalls the message.
         * 
         */
        public string RecallBy { get; private set; }

        /**
         * The ID of the recalled message.
         * 
         */
        public string RecallMessageId { get; private set; }

        /**
         * The information to be transparently transmitted during message recall.
         */
        public string Ext { get; private set; }

        /**
         * The recalled message.
         * The value of this property is nil if the recipient is offline during message recall.
         */
        public Message RecallMessage;

        /**
         * The conversation ID which the message recalled belongs to.
         */
        public string ConversationId { get; private set; }

        [Preserve]
        internal RecallMessageInfo() { }

        [Preserve]
        internal RecallMessageInfo(string jsonString) : base(jsonString) { }

        [Preserve]
        internal RecallMessageInfo(JSONObject jsonObject) : base(jsonObject) { }

        internal override void FromJsonObject(JSONObject jsonObject)
        {
            RecallBy = jsonObject["recallBy"];
            RecallMessageId = jsonObject["recallMessageId"];
            Ext = jsonObject["ext"];
            ConversationId = jsonObject["conversationId"];

            if (null != jsonObject["recallMessage"] && jsonObject["recallMessage"].IsObject)
            {
                RecallMessage = new Message(jsonObject["recallMessage"].AsObject);
            }
        }

        internal override JSONObject ToJsonObject()
        {
            JSONObject jo = new JSONObject();
            jo.AddWithoutNull("recallBy", RecallBy);
            jo.AddWithoutNull("recallMessageId", RecallMessageId);
            jo.AddWithoutNull("ext", Ext);
            jo.AddWithoutNull("conversationId", ConversationId);
            if (null != RecallMessage)
            {
                jo.AddWithoutNull("recallMessage", RecallMessage.ToJsonObject());
            }
            return jo;
        }
    }
}
