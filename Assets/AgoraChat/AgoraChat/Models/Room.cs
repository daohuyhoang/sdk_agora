﻿using System.Collections.Generic;
using AgoraChat.SimpleJSON;
#if !_WIN32
using UnityEngine.Scripting;
#endif

namespace AgoraChat
{
    /**
     *  The chat room class, which defines chat room information.
     */
    [Preserve]
    public class Room : BaseModel
    {
        /**
         * The chat room ID.
         *
         */
        public string RoomId { get; internal set; }

        /**
         * The chat room name.
         */
        public string Name { get; internal set; }
        /**
         * The chat room description.
         */
        public string Description { get; internal set; }

        /**
         * The chat room announcement.
         */
        public string Announcement { get; internal set; }

        /**
        * The number of online members.
        * This property is available once join the chat room.
        * This property will be updated when others join or exit the chat room.
        */
        public int MemberCount { get; internal set; }

        /**
         * The admin list of the chat room.
         */
        public List<string> AdminList { get; internal set; }

        /**
         * The member list of the chat room.
         * 
         * To get the member list of the chat room from the server, you can call `{@link IRoomManager#FetchRoomMembers(String, String, int, ValueCallBack)}`.
         */
        public List<string> MemberList { get; internal set; }

        /**
         * The block list of the chat room.
         * 
         * To get the block list of the chat room from the server, you can call `{@link IRoomManager#FetchRoomBlockList(String, int, int, ValueCallBack)}`.         
         */
        public List<string> BlockList { get; internal set; }

        /**
         * The mute list of the chat room.
         * 
         * To get the mute list of the chat room from the server, you can call `{@link IRoomManager#FetchRoomMuteList(String, int, int, ValueCallBack)}`.
         *
         */
        public List<string> MuteList { get; internal set; }

        /**
         * The maximum number of members allowed in the chat room, which is determined during chat room creation.
         * 
         * To get the latest data, you can call `{@link IRoomManager#FetchRoomInfoFromServer(String,ValueCallBack)}` to get details of a chat room from the server. 
         */
        public int MaxUsers { get; internal set; }

        /**
         * The chat room owner.
         * 
         * To get the latest data, you can call `{@link IRoomManager#FetchRoomInfoFromServer(String,ValueCallBack)}` to get details of a chat room from the server.
         */
        public string Owner { get; internal set; }

        /**
         * Whether all members are muted. This propery is available once join the chat room.
         * - `true`: Yes.  
         * - `false`: No.
         * 
         * **Note**
         * - Once all members are muted or unmuted, the callback is triggered to notify and update the mute or unmute status. You can call the method to get the current status.
         * 
         */
        public bool IsAllMemberMuted { get; internal set; }

        /**
         * The role of the current user in the chat room.
         */
        public RoomPermissionType PermissionType { get; internal set; }

        /**
         * The timestamp when the chat room was created.
         * This property ONLY can be available when join the chat room.
         */
        public long CreateTimeStamp { get; internal set; }

        /**
         * Current user is in allow-list or not.
         * This property is available once join the chat room.
         * This property will be updated when current user is added or removed from the allow list.
         */
        public bool IsInAllowList { get; internal set; }

        /**
         * The timestamp(ms) when Current user will be unmuted.
         * Current use is not muted if it is zero.
         * Means cannot get MuteUntilTimeStamp correctly if it is be set with -1;
         * This property is available once join the chat room.
         * This property will be updated when current use is muted or unmuted.
         */
        public long MuteUntilTimeStamp { get; internal set; }

        [Preserve]
        internal Room() { }

        [Preserve]
        internal Room(string jsonString) : base(jsonString) { }

        [Preserve]
        internal Room(JSONObject jsonObject) : base(jsonObject) { }

        internal override void FromJsonObject(JSONObject jsonObject)
        {
            RoomId = jsonObject["roomId"];
            Name = jsonObject["name"];
            Description = jsonObject["desc"];
            Announcement = jsonObject["announcement"];
            MemberCount = jsonObject["memberCount"];
            AdminList = List.StringListFromJsonArray(jsonObject["adminList"]);
            MemberList = List.StringListFromJsonArray(jsonObject["memberList"]);
            BlockList = List.StringListFromJsonArray(jsonObject["blockList"]);
            MuteList = List.StringListFromJsonArray(jsonObject["muteList"]);
            MaxUsers = jsonObject["maxUsers"];
            Owner = jsonObject["owner"];
            IsAllMemberMuted = jsonObject["isMuteAll"];
            PermissionType = jsonObject["permissionType"].AsInt.ToRoomPermissionType();

            if (jsonObject["createTimestamp"] != null)
            {
                CreateTimeStamp = (long)jsonObject["createTimestamp"].AsDouble;
            }
            else
            {
                CreateTimeStamp = 0;
            }

            if (jsonObject["isInAllowList"] != null)
            {
                IsInAllowList = jsonObject["isInAllowList"].AsBool;
            }
            else
            {
                IsInAllowList = false;
            }

            if (jsonObject["muteUntilTimeStamp"] != null)
            {
                MuteUntilTimeStamp = (long)jsonObject["muteUntilTimeStamp"].AsDouble;
            }
            else
            {
                MuteUntilTimeStamp = -1;
            }
        }

        internal override JSONObject ToJsonObject()
        {
            JSONObject jo = new JSONObject();
            jo.AddWithoutNull("roomId", RoomId);
            jo.AddWithoutNull("name", Name);
            jo.AddWithoutNull("desc", Description);
            jo.AddWithoutNull("announcement", Announcement);
            jo.AddWithoutNull("memberCount", MemberCount);
            jo.AddWithoutNull("adminList", JsonObject.JsonArrayFromStringList(AdminList));
            jo.AddWithoutNull("memberList", JsonObject.JsonArrayFromStringList(MemberList));
            jo.AddWithoutNull("blockList", JsonObject.JsonArrayFromStringList(BlockList));
            jo.AddWithoutNull("muteList", JsonObject.JsonArrayFromStringList(MuteList));
            jo.AddWithoutNull("maxUsers", MaxUsers);
            jo.AddWithoutNull("owner", Owner);
            jo.AddWithoutNull("isMuteAll", IsAllMemberMuted);
            jo.AddWithoutNull("permissionType", PermissionType.ToInt());
            jo.AddWithoutNull("createTimestamp", CreateTimeStamp);
            jo.AddWithoutNull("isInAllowList", IsInAllowList);
            jo.AddWithoutNull("muteUntilTimeStamp", MuteUntilTimeStamp);
            return jo;
        }
    }
}
