using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AgoraChatConfig", menuName = "Agora/AgoraChatConfig", order = 1)]
public class AgoraChatConfig : ScriptableObject
{
    [SerializeField] private string userId = "";
    [SerializeField] private string token = "";
    [SerializeField] private string appKey = "";

    public string UserId => userId;
    public string Token => token;
    public string AppKey => appKey;
}
