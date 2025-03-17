using Agora_RTC_Plugin.API_Example;
using Agora.Rtc;
using UnityEngine;
using UnityEngine.UI;

public class VideoCallManager : MonoBehaviour
{
    [SerializeField] private AgoraConfig agoraConfig;
    
    private IRtcEngine rtcEngine;
    
    [SerializeField] private RawImage localVideo;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button leaveButton;
    
    public Transform GetLocalVideoTransform()
    {
        return localVideo.transform.parent;
    }

    private void Start()
    {
        if (agoraConfig == null)
        {
            Debug.LogError("AgoraConfig is not set.");
            return;
        }
        
        joinButton.onClick.AddListener(JoinChannel);
        leaveButton.onClick.AddListener(LeaveChannel);
        
        SetupAgoraEngine();
    }

    private void Update()
    {
        PermissionHelper.RequestCameraPermission();
        PermissionHelper.RequestMicrophontPermission();
    }

    private void SetupAgoraEngine()
    {
        rtcEngine = RtcEngine.CreateAgoraRtcEngine();

        RtcEngineContext context = new RtcEngineContext();
        context.appId = agoraConfig.AppId;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        
        int result = rtcEngine.Initialize(context);
        Debug.Log("Initialize Agora RTC Engine: " + result);
        rtcEngine.EnableVideo();
        rtcEngine.InitEventHandler(new VideoCallEventHandler(this));
    }

    private void JoinChannel()
    {
        rtcEngine.JoinChannel(agoraConfig.Token, agoraConfig.ChannelName, "", 0);
        Debug.Log("Joining channel: " + agoraConfig.ChannelName);
        
        VideoSurface videoSurface = localVideo.gameObject.AddComponent<VideoSurface>();
        videoSurface.SetForUser(0, agoraConfig.ChannelName);
        videoSurface.SetEnable(true);
        videoSurface.transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    private void LeaveChannel()
    {
        rtcEngine.LeaveChannel();
        Debug.Log("Leaving channel: " + agoraConfig.ChannelName);

        VideoSurface videoSurface = localVideo.GetComponent<VideoSurface>();
        
        if (videoSurface != null)
        {
            Destroy(videoSurface);
        }
    }

    private void OnDestroy()
    {
        if (rtcEngine != null)
        {
            rtcEngine.LeaveChannel();
            rtcEngine.Dispose();
            rtcEngine = null;
        }
    }

    public string GetChannelName()
    {
        return agoraConfig.ChannelName;
    }
}
