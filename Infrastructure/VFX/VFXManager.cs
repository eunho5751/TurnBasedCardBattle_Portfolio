using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class VFXManager : SerializedMonoBehaviour
{
    private static VFXManager _instance;

    [SerializeField]
    private Dictionary<VFXType, GameObject> _typeToPrefabMap = new();

    public GameObject Spawn(VFXType type, float duration)
    {
        var instance = Spawn(type);
        if (instance != null)
            DespawnAfter(instance, duration).Forget();
        return instance;
    }

    public GameObject Spawn(VFXType type)
    {
        if (!_typeToPrefabMap.TryGetValue(type, out var prefab) || prefab == null)
        {
            Debug.LogError($"No prefab registered for VFXType.{type}");
            return null;
        }

        var instance = PoolManager.Instance.Get(prefab);
        instance.SetActive(true);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        PoolManager.Instance.Release(instance);
    }

    private async UniTaskVoid DespawnAfter(GameObject instance, float duration)
    {
        await UniTask.WaitForSeconds(duration, cancellationToken: instance.GetCancellationTokenOnDestroy());
        Despawn(instance);
    }

    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<VFXManager>();
                if (_instance == null)
                {
                    GameObject go = new("VFXManager");
                    _instance = go.AddComponent<VFXManager>();
                }
            }
            return _instance;
        }
    }    
}