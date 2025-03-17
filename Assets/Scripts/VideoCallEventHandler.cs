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
        
        GameObject remoteVideoObj = new GameObject("RemoteVideo_" + uid);
        remoteVideoObj.transform.SetParent(manager.GetLocalVideoTransform());
        
        RawImage remoteImage = remoteVideoObj.AddComponent<RawImage>();
        remoteImage.rectTransform.sizeDelta = new Vector2(320, 240);
        
        VideoSurface videoSurface = remoteVideoObj.AddComponent<VideoSurface>();
        videoSurface.SetForUser(uid, manager.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        videoSurface.SetEnable(true);
        videoSurface.transform.rotation = Quaternion.Euler(0, 0, 180);
    }
}
