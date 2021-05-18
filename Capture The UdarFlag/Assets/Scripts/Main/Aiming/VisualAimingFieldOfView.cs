using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VisualAimingFieldOfView : MonoBehaviour
{
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private LayerMask _aimingLayer;


    private float _fieldOfView = 90f;
    private float _viewMaxDistance = 10f;
    private float _currentViewMaxDistance;
    private Vector3 _origin = Vector3.zero;
    private float _startingAngle;
    private Mesh _mesh;

    private Camera _mainCamera;
    private PlayerLinks _connPlayer;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>().playerManager.playerLinks;

    }
    private void OnEnable()
    {
        SetAimDirection(_connPlayer.transform.forward);
        SetOrigin(_connPlayer.transform.position);
    }
    private void Update()
    {
        if (_connPlayer == null) { return; }

        SetAimDirection(_connPlayer.transform.forward);
        SetOrigin(_connPlayer.transform.position);

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float point))
        {
            Vector3 hitPoint = ray.GetPoint(point);
            _currentViewMaxDistance = Mathf.Min(_viewMaxDistance, (hitPoint - _origin).magnitude);
        }
        else
            _currentViewMaxDistance = _viewMaxDistance;

    }

    private void LateUpdate()
    {
        int rayCount = 50;
        float currentAngle = _startingAngle;
        float angleIncrease = _fieldOfView / rayCount;
        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[3 * rayCount];

        vertices[0] = _origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex = _origin + GetVectorFromAngle(currentAngle) * _currentViewMaxDistance;
            Vector3 shotPoint = (_connPlayer.activeItem.GetActiveItem() as Weapon).GetShotPoint();
            Vector3 raycastOffset = Vector3.up * (shotPoint.y - 0.1f);
            if (Physics.Raycast(_origin + raycastOffset, GetVectorFromAngle(currentAngle), out RaycastHit hitInfo, _currentViewMaxDistance, _aimingLayer))
            {
                vertex = hitInfo.point - raycastOffset;
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }
            currentAngle -= angleIncrease;
            vertexIndex++;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
    }


    private Vector3 GetVectorFromAngle(float angle)
    {
        float rad = angle * (Mathf.PI / 180f);

        return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad));
    }
    private float GetAngleFromVector(Vector3 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle += 360;
        return angle;
    }

    public void SetOrigin(Vector3 origin)
    {
        this._origin = origin;
    }
    public void SetAimDirection(Vector3 dir)
    {
        _startingAngle = GetAngleFromVector(dir) + _fieldOfView / 2f;

    }
    public void SetAimingProperties(AimingProperties aimingProperties)
    {
        _viewMaxDistance = aimingProperties.maxViewDistance;
        _fieldOfView = aimingProperties.fieldOfView;
    }
}
