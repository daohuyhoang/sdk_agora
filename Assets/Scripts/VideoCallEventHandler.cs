using System.Collections;
using Agora_RTC_Plugin.API_Example;
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
        Debug.Log("Join channel: " + connection.channelId + " with UID: " + connection.localUid);
    }

    public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
    {
        Debug.Log("Leave channel: " + connection.channelId);
    }

    public override void OnError(int err, string msg)
    {
        Debug.LogError("Error: " + err + " " + msg);
    }

    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        Debug.Log("User joined: " + uid);
    
        MakeVideoView(uid, manager.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
    
        GameObject go = GameObject.Find("RemoteVideo_" + uid.ToString());
        if (go != null)
        {
            VideoSurface videoSurface = go.GetComponent<VideoSurface>();
            if (videoSurface != null)
            {
                videoSurface.StartCoroutine(CheckAndDestroyIfNoTexture(go, 1f));
            }
        }
    }

    private IEnumerator CheckAndDestroyIfNoTexture(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        RawImage rawImage = go.GetComponent<RawImage>();
        if (rawImage != null && rawImage.texture == null)
        {
            Object.Destroy(go);
        }
    }

    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        Debug.Log("User offline: " + uid);
    }

    internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
    {
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return;
        }

        VideoSurface videoSurface = new VideoSurface();
        
        if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            videoSurface = MakeImageSurface("LocalCameraView");
        }
        else if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN)
        {
            videoSurface = MakeImageSurface("LocalScreenShareView");
        }
        else if (videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE)
        {
            videoSurface = MakeImageSurface("RemoteVideo_" + uid.ToString());
        }
        else
        {
            videoSurface = MakeImageSurface(uid.ToString());
        }
        
        videoSurface.SetForUser(uid, channelId, videoSourceType);
        
        videoSurface.SetEnable(true);

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            RectTransform rectTransform = videoSurface.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(width / 2, height / 2);
                rectTransform.localScale = videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN ? new Vector3(-1, 1, 1) : Vector3.one;
            }
            Debug.Log("OnTextureSizeModify: " + width + " " + height);
        };
    }

    private static VideoSurface MakeImageSurface(string goName)
    {
        var go = new GameObject();
        if (go == null)
        {
            return null;
        }

        go.name = goName;
        RawImage rawImage = go.AddComponent<RawImage>();
        
        rawImage.uvRect = new Rect(1, 0, -1, 1);
        
        go.AddComponent<UIElementDrag>();
        var canvas = GameObject.Find("VideoCanvas");
        if (canvas != null)
        {
            go.transform.parent = canvas.transform;
            Debug.Log("Add video view to VideoCanvas: " + goName);
        }
        else
        {
            Debug.LogWarning("VideoCanvas not found in scene!");
        }

        go.transform.Rotate(0.0f, 0.0f, 180f);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(3f, 4f, 1f);

        var videoSurface = go.AddComponent<VideoSurface>();
        
        return videoSurface;
    }
    
}