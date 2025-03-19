using Agora.Rtc;
using UnityEngine;
using UnityEngine.UI;

public class VideoCallEventHandler : IRtcEngineEventHandler
{
    private VideoCallManager manager;

    public VideoCallEventHandler(VideoCallManager mgr)
    {
        manager = mgr;
    }
    
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log("Join channel: " + connection.channelId);
    }
    
    public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
    {
        Debug.Log("Leave channel");
    }

    public override void OnError(int err, string mgr)
    {
        Debug.Log("Error: " + err + " " + mgr);
    }
    
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        Debug.Log("User joined: " + uid);

        // Kiểm tra xem GameObject đã tồn tại chưa
        GameObject remoteVideoObj = GameObject.Find("RemoteVideo_" + uid);
        if (remoteVideoObj == null)
        {
            remoteVideoObj = new GameObject("RemoteVideo_" + uid);
            remoteVideoObj.transform.SetParent(manager.GetLocalVideoTransform());

            RawImage remoteImage = remoteVideoObj.AddComponent<RawImage>();
            RectTransform rectTransform = remoteImage.rectTransform;

            if (uid == manager.ScreenUid)
            {
                // Đặt chia sẻ màn hình toàn màn hình
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                remoteVideoObj.transform.SetSiblingIndex(0); // Đặt dưới cùng
            }
            else
            {
                // Đặt video từ xa ở góc trên bên phải
                rectTransform.anchorMin = new Vector2(0.75f, 0.75f);
                rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                remoteVideoObj.transform.SetAsLastSibling(); // Đặt lên trên
            }
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            remoteImage.uvRect = new Rect(1, 0, 1, -1);

            VideoSurface videoSurface = remoteVideoObj.AddComponent<VideoSurface>();
            videoSurface.SetForUser(uid, manager.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            videoSurface.SetEnable(true);
        }
        else
        {
            Debug.Log("GameObject cho UID " + uid + " đã tồn tại.");
        }
    }
}
