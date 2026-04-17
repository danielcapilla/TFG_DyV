using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera virtualCamera;
    public Transform grid;

    [Header("Ajustes de Margen")]
    [Range(0f, 0.1f)] // 10% de margen
    public float marginPercentage = 0.0f; 

    public void FitCameraToLevel()
    {
        if (grid == null || virtualCamera == null)
            return;

        virtualCamera.orthographic = true;

        // Pillamos los colliders de los hijos de walls (donde estan los limites del mapa)
        Collider[] colliders = grid.GetChild(1).GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No se encontraron colliders.");
            return;
        }
        Bounds totalBounds = colliders[0].bounds;
        foreach (Collider col in colliders.Skip(1))
            totalBounds.Encapsulate(col.bounds);

        // Calculamos el tamano de la camara con la rotacion inicial considerada
        float orthoSize = CalculateOrthographicSize(totalBounds, virtualCamera);

        // Aplicamos un margen porcentual si queremos
        orthoSize *= (1f + marginPercentage);

        virtualCamera.orthographicSize = orthoSize;
        // Ajustamos la posicion de la camara considerando la rotacion
        Vector3 cameraPosition = CalculateCameraPosition(totalBounds, virtualCamera.transform.rotation, orthoSize);
        virtualCamera.transform.position = cameraPosition;
    }

    private Vector3 CalculateCameraPosition(Bounds bounds, Quaternion cameraRotation, float orthoSize)
    {
        Vector3 center = bounds.center;
        Vector3 cameraForward = cameraRotation * Vector3.forward;
        float distance = orthoSize * 2.0f;
        Vector3 cameraPosition = center + (-cameraForward) * distance;
        return cameraPosition;
    }

    private Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(min.x, min.y, max.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(min.x, max.y, max.z);
        corners[4] = new Vector3(max.x, min.y, min.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(max.x, max.y, min.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        return corners;
    }

    private float CalculateOrthographicSize(Bounds bounds, Camera camera)
    {
        Vector3[] corners = GetBoundsCorners(bounds);

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector3 corner in corners)
        {
            Vector3 viewSpaceCorner = camera.worldToCameraMatrix.MultiplyPoint3x4(corner);
            minX = Mathf.Min(minX, viewSpaceCorner.x);
            maxX = Mathf.Max(maxX, viewSpaceCorner.x);
            minY = Mathf.Min(minY, viewSpaceCorner.y);
            maxY = Mathf.Max(maxY, viewSpaceCorner.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float aspect = (float)Screen.width / Screen.height;
        float orthoSizeWidth = (width / 2f) / aspect;
        float orthoSizeHeight = height / 2f;
        return Mathf.Max(orthoSizeWidth, orthoSizeHeight);
    }
}
