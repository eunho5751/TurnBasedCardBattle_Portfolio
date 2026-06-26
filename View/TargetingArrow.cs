using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class TargetingArrow : MonoBehaviour
{
    [SerializeField]
    private Color _color = new(1f, 0.85f, 0.2f, 1f);
    [SerializeField]
    private float _width = 0.25f;
    [SerializeField]
    private float _arrowHeadSize = 0.6f;
    [SerializeField]
    private int _segmentCount = 24;
    [SerializeField]
    private float _curveStrength = 0.6f;
    [SerializeField]
    private float _startZOffset;
    [SerializeField]
    private int _sortingOrder = 1000;

    private Camera _camera;
    private LineRenderer _line;
    private Transform _arrowHead;
    private Transform _startTransform;
    private bool _isActive;

    private void Awake()
    {
        _camera = Camera.main;
        BuildArrow();
        Hide();
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }
        UpdateArrow();
    }

    public void Show(Transform startTransform)
    {
        _startTransform = startTransform;
        _isActive = true;
        _line.enabled = true;
        _arrowHead.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _isActive = false;
        _line.enabled = false;
        _arrowHead.gameObject.SetActive(false);
    }

    private void BuildArrow()
    {
        Material material = new(Shader.Find("Sprites/Default"));

        _line = gameObject.AddComponent<LineRenderer>();
        _line.material = material;
        _line.useWorldSpace = true;
        _line.numCornerVertices = 4;
        _line.numCapVertices = 4;
        _line.widthCurve = AnimationCurve.Linear(0f, _width * 0.4f, 1f, _width);
        _line.startColor = _color;
        _line.endColor = _color;
        _line.sortingOrder = _sortingOrder;
        _line.shadowCastingMode = ShadowCastingMode.Off;
        _line.receiveShadows = false;

        _arrowHead = BuildArrowHead(material);
    }

    private Transform BuildArrowHead(Material material)
    {
        GameObject headObject = new("ArrowHead");
        headObject.transform.SetParent(transform, false);

        MeshFilter filter = headObject.AddComponent<MeshFilter>();
        filter.mesh = CreateTriangleMesh();

        MeshRenderer renderer = headObject.AddComponent<MeshRenderer>();
        renderer.material = material;
        renderer.sortingOrder = _sortingOrder;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return headObject.transform;
    }

    private Mesh CreateTriangleMesh()
    {
        float half = _arrowHeadSize * 0.6f;
        Mesh mesh = new()
        {
            vertices = new[]
            {
                new Vector3(0f, 0f, _arrowHeadSize),
                new Vector3(-half, 0f, 0f),
                new Vector3(half, 0f, 0f)
            },
            triangles = new[] { 0, 1, 2 },
            colors = new[] { _color, _color, _color }
        };
        mesh.RecalculateBounds();
        return mesh;
    }

    private void UpdateArrow()
    {
        Vector3 start = _startTransform.position;
        start.z += _startZOffset;
        Vector3 end = GetCursorWorldPosition();
        Vector3 mid = (start + end) * 0.5f;
        mid.z -= Mathf.Abs(end.x - start.x) * _curveStrength;

        Vector3 forward = (end - QuadraticBezier(start, mid, end, 0.92f)).normalized;
        Vector3 lineEnd = end - forward * _arrowHeadSize;

        _line.positionCount = _segmentCount;
        for (int i = 0; i < _segmentCount; i++)
        {
            float t = i / (float)(_segmentCount - 1);
            _line.SetPosition(i, QuadraticBezier(start, mid, lineEnd, t));
        }

        _arrowHead.SetPositionAndRotation(lineEnd, Quaternion.LookRotation(forward, Vector3.up));
    }

    private Vector3 GetCursorWorldPosition()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null)
        {
            return _startTransform.position;
        }

        Ray ray = _camera.ScreenPointToRay(pointer.position.ReadValue());
        Plane plane = new(Vector3.up, _startTransform.position);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return _startTransform.position;
    }

    private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }
}
