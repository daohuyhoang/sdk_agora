using UnityEngine;

[CreateAssetMenu(fileName = "AgoraConfig", menuName = "Agora/AgoraConfig", order = 1)]
public class AgoraConfig : ScriptableObject
{
    [SerializeField] private string appId = "";
    [SerializeField] private string token = "";
    [SerializeField] private string channelName = "";

    public string AppId => appId;
    public string Token => token;
    public string ChannelName => channelName;
}
