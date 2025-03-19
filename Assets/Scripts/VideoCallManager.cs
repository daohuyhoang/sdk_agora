using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using Agora_RTC_Plugin.API_Example;

public class VideoCallManager : MonoBehaviour
{
    [SerializeField] private AgoraConfig agoraConfig;

    [SerializeField] private RawImage localVideo;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startShareButton;
    [SerializeField] private Button stopShareButton;

    private IRtcEngineEx rtcEngine;
    
    private uint cameraUid = 0;
    private uint screenUid = 1;
    public uint ScreenUid => screenUid;

    private void Start()
    {
        if (agoraConfig == null)
        {
            Debug.LogError("AgoraConfig is not set.");
            return;
        }
        
        joinButton.onClick.AddListener(JoinChannel);
        leaveButton.onClick.AddListener(LeaveChannel);
        startShareButton.onClick.AddListener(StartScreenShare);
        stopShareButton.onClick.AddListener(StopScreenShare);
        stopShareButton.gameObject.SetActive(false);

        SetupAgoraEngine();
    }

    private void Update()
    {
        PermissionHelper.RequestCameraPermission();
        PermissionHelper.RequestMicrophontPermission();
    }

    private void SetupAgoraEngine()
    {
        rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();

        RtcEngineContext context = new RtcEngineContext
        {
            appId = agoraConfig.AppId,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
        };

        int result = rtcEngine.Initialize(context);
        Debug.Log("Initialize Agora RTC Engine: " + result);
        rtcEngine.EnableVideo();
        rtcEngine.InitEventHandler(new VideoCallEventHandler(this));
    }

    private void JoinChannel()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(true);
        options.publishCameraTrack.SetValue(true);
        options.publishScreenTrack.SetValue(false);
        options.enableAudioRecordingOrPlayout.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        rtcEngine.JoinChannel(agoraConfig.Token, agoraConfig.ChannelName, "", cameraUid);
        Debug.Log("Joining channel: " + agoraConfig.ChannelName + " UID camera: " + cameraUid);

        VideoSurface videoSurface = localVideo.gameObject.AddComponent<VideoSurface>();
        videoSurface.SetForUser(cameraUid, agoraConfig.ChannelName);
        videoSurface.SetEnable(true);
        videoSurface.transform.rotation = Quaternion.Euler(0, 0, 180);
    }
    
    private void LeaveChannel()
    {
        rtcEngine.LeaveChannel();
        ScreenShareLeaveChannel();

        Debug.Log("Leaving channel: " + agoraConfig.ChannelName);

        VideoSurface videoSurface = localVideo.GetComponent<VideoSurface>();
        if (videoSurface != null)
        {
            Destroy(videoSurface);
        }
    }
    
    private void StartScreenShare()
    {
        if (rtcEngine == null) return;

        startShareButton.gameObject.SetActive(false);
        stopShareButton.gameObject.SetActive(true);

#if UNITY_ANDROID || UNITY_IPHONE
        var parameters = new ScreenCaptureParameters2
        {
            captureAudio = true,
            captureVideo = true
        };
        int nRet = rtcEngine.StartScreenCapture(parameters);
        Debug.Log("StartScreenCapture: " + nRet);
#else
        int nRet = rtcEngine.StartScreenCaptureByDisplayId(0, default(Rectangle),
            new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
        Debug.Log("StartScreenCaptureByDisplayId: " + nRet);
#endif
        ScreenShareJoinChannel();
    }
    
    private void ScreenShareJoinChannel()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(false);
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);
        options.enableAudioRecordingOrPlayout.SetValue(false);
#if UNITY_ANDROID || UNITY_IPHONE
        options.publishScreenCaptureAudio.SetValue(true);
        options.publishScreenCaptureVideo.SetValue(true);
#endif
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        int ret = rtcEngine.JoinChannelEx(agoraConfig.Token, new RtcConnection(agoraConfig.ChannelName, screenUid), options);
        Debug.Log("ScreenShare JoinChannelEx returns: " + ret);
    }
    
    private void StopScreenShare()
    {
        ScreenShareLeaveChannel();

        rtcEngine.StopScreenCapture();

        startShareButton.gameObject.SetActive(true);
        stopShareButton.gameObject.SetActive(false);
    }

    private void ScreenShareLeaveChannel()
    {
        rtcEngine.LeaveChannelEx(new RtcConnection(agoraConfig.ChannelName, screenUid));
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

    public Transform GetLocalVideoTransform()
    {
        return localVideo.transform.parent;
    }

    public string GetChannelName()
    {
        return agoraConfig.ChannelName;
    }
}
