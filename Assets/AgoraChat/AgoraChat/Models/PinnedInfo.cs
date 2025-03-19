using AgoraChat.SimpleJSON;
#if !_WIN32
using UnityEngine.Scripting;
#endif

namespace AgoraChat
{
    /**
     * The message pinning information class.
     */
    [Preserve]
    public class PinnedInfo : BaseModel
    {
        /**
         * The user ID of the operator.
         *
         * If there is no message pinning information, the value is an empty string.
         * 
         */
        public string PinnedBy;

        /**
         * The time when the message is pinned.
         *
         * If there is no message pinning information, this value is 0.
         */
        public long PinnedAt;

        [Preserve]
        internal PinnedInfo() { }

        [Preserve]
        internal PinnedInfo(string jsonString) : base(jsonString) { }

        [Preserve]
        internal PinnedInfo(JSONObject jsonObject) : base(jsonObject) { }

        internal override void FromJsonObject(JSONObject jsonObject)
        {
            //IsPinned = jsonObject["isPinned"];
            PinnedBy = jsonObject["pinnedBy"];
            PinnedAt = (long)jsonObject["pinnedAt"].AsDouble;
        }

        internal override JSONObject ToJsonObject()
        {
            JSONObject jo = new JSONObject();
            //jo.AddWithoutNull("isPinned", IsPinned);
            jo.AddWithoutNull("pinnedBy", PinnedBy);
            jo.AddWithoutNull("pinnedAt", PinnedAt);
            return jo;
        }
    }
}
