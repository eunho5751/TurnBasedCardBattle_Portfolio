using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SFXManager : SerializedMonoBehaviour
{
    private static SFXManager _instance;

    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private Dictionary<SFXType, AudioClip> _typeToClipMap = new();

    private readonly HashSet<SFXType> _playingSFXsInCurrentFrame = new();

    private void LateUpdate()
    {
        _playingSFXsInCurrentFrame.Clear();
    }

    public void Play(SFXType type)
    {
        if (!_typeToClipMap.TryGetValue(type, out var clip) || _playingSFXsInCurrentFrame.Contains(type))
        {
            return;
        }
        
        _audioSource.PlayOneShot(clip);
        _playingSFXsInCurrentFrame.Add(type);
    }    

    public static SFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SFXManager>();
                if (_instance == null)
                {
                    GameObject go = new("SFXManager");
                    _instance = go.AddComponent<SFXManager>();
                }
            }
            return _instance;
        }
    }    
}