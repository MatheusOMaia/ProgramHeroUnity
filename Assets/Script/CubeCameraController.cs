using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CubeCameraController : MonoBehaviour
{
    [Header("Target")]
    public Vector3 targetPosition = Vector3.zero;

    [Header("Camera")]
    public float distance = 8f;
    public float rotationSpeed = 0.25f;
    public float zoomSpeed = 2f;
    public float minDistance = 4f;
    public float maxDistance = 15f;

    [Header("Snap / Rubik Rotation")]
    public float snapTime = 0.2f;
    public float queuedSnapTime = 0.12f;

    private bool dragging = false;
    private Coroutine rotationCoroutine;

    private Queue<RubikCommand> rotationQueue = new Queue<RubikCommand>();
    private bool isRotating = false;

    // Eixos Rubik atuais.
    // Eles sempre apontam para eixos globais puros: ±X, ±Y, ±Z.
    private Vector3 rubikX = Vector3.right;
    private Vector3 rubikY = Vector3.up;
    private Vector3 rubikZ = Vector3.forward;

    [Header("Planet Target")]
    public CubePlanet cubePlanet;
    public bool autoFindPlanetCenter = true;

    private struct RubikCommand
    {
        public string axis;
        public int direction;

        public RubikCommand(string axis, int direction)
        {
            this.axis = axis;
            this.direction = direction;
        }
    }

    private void Start()
    {
        if (autoFindPlanetCenter)
            UpdateTargetFromPlanet();

        ApplyRubikStateInstant();
    }

    private void Update()
    {
        HandleMouseCamera();
    }

    private void HandleMouseCamera()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
            dragging = true;

        if (mouse.rightButton.wasReleasedThisFrame)
            dragging = false;

        float scroll = mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * 0.01f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            ApplyCurrentRotationWithDistance();
        }

        if (dragging)
        {
            Vector2 delta = mouse.delta.ReadValue();

            transform.RotateAround(
                targetPosition,
                transform.up,
                delta.x * rotationSpeed
            );

            transform.RotateAround(
                targetPosition,
                transform.right,
                -delta.y * rotationSpeed
            );
        }
    }

    private void ApplyCurrentRotationWithDistance()
    {
        Vector3 directionFromTarget = (transform.position - targetPosition).normalized;

        transform.position = targetPosition + directionFromTarget * distance;
        transform.LookAt(targetPosition, transform.up);
    }

    private int AxisSign(float value)
    {
        return value >= 0f ? 1 : -1;
    }

    private Vector3 SnapAxis(Vector3 v)
    {
        float ax = Mathf.Abs(v.x);
        float ay = Mathf.Abs(v.y);
        float az = Mathf.Abs(v.z);

        if (ax >= ay && ax >= az)
            return new Vector3(AxisSign(v.x), 0, 0);

        if (ay >= ax && ay >= az)
            return new Vector3(0, AxisSign(v.y), 0);

        return new Vector3(0, 0, AxisSign(v.z));
    }

    private Vector3 SnapToCubeVertex(Vector3 dir)
    {
        return new Vector3(
            AxisSign(dir.x),
            AxisSign(dir.y),
            AxisSign(dir.z)
        ).normalized;
    }

    private Vector3 GetCurrentCameraDirection()
    {
        // Direção do centro do planeta até a câmera.
        return (transform.position - targetPosition).normalized;
    }

    private void SetRubikAxesFromDirection(Vector3 dir)
    {
        Vector3 vertex = SnapToCubeVertex(dir);

        rubikX = new Vector3(AxisSign(vertex.x), 0, 0);
        rubikY = new Vector3(0, AxisSign(vertex.y), 0);
        rubikZ = new Vector3(0, 0, AxisSign(vertex.z));
    }

    private Vector3 GetRubikCameraDirection()
    {
        return (rubikX + rubikY + rubikZ).normalized;
    }

    private Quaternion GetRotationFromRubikState()
    {
        Vector3 viewDir = GetRubikCameraDirection();

        // A câmera fica em target + viewDir * distance.
        // Portanto ela olha na direção contrária.
        Vector3 forward = -viewDir;

        // O eixo Y do Rubik vira referência de "cima" visual.
        Vector3 projectedUp = rubikY - viewDir * Vector3.Dot(rubikY, viewDir);

        if (projectedUp.sqrMagnitude < 0.001f)
            projectedUp = Vector3.up;

        Vector3 up = projectedUp.normalized;

        return Quaternion.LookRotation(forward, up);
    }

    private void ApplyRubikStateInstant()
    {
        Vector3 viewDir = GetRubikCameraDirection();

        transform.position = targetPosition + viewDir * distance;
        transform.rotation = GetRotationFromRubikState();
    }

    private void SnapToRubikState(float duration)
    {
        Vector3 targetViewDir = GetRubikCameraDirection();
        Quaternion targetRotation = GetRotationFromRubikState();

        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);

        rotationCoroutine = StartCoroutine(
            AnimateCameraOrbitTo(targetViewDir, targetRotation, duration)
        );
    }

    private IEnumerator AnimateCameraOrbitTo(Vector3 targetViewDir, Quaternion targetRot, float duration)
    {
        Vector3 startViewDir = GetCurrentCameraDirection();
        Vector3 startUp = transform.up;
        Vector3 targetUp = targetRot * Vector3.up;

        targetViewDir = targetViewDir.normalized;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            Vector3 viewDir = GetSafeOrbitDirection(startViewDir, targetViewDir, t);

            transform.position = targetPosition + viewDir * distance;

            Vector3 up = Vector3.Slerp(startUp, targetUp, t);
            transform.rotation = BuildCameraRotation(viewDir, up);

            yield return null;
        }

        transform.position = targetPosition + targetViewDir * distance;
        transform.rotation = targetRot;

        OnRubikRotationFinished();
    }

    // para nao bugar quando o vértice oposto for exatamente oposto
    private Vector3 GetSafeOrbitDirection(Vector3 startDir, Vector3 targetDir, float t)
    {
        startDir = startDir.normalized;
        targetDir = targetDir.normalized;

        float dot = Vector3.Dot(startDir, targetDir);

        // Caso normal: usa Slerp
        if (dot > -0.999f)
        {
            return Vector3.Slerp(startDir, targetDir, t).normalized;
        }

        // Caso especial: direções opostas.
        // Escolhe um eixo de rotação seguro para dar a volta no planeta.
        Vector3 axis = transform.up;

        axis = axis - startDir * Vector3.Dot(axis, startDir);

        if (axis.sqrMagnitude < 0.001f)
        {
            axis = transform.right;
            axis = axis - startDir * Vector3.Dot(axis, startDir);
        }

        axis.Normalize();

        Quaternion rotation = Quaternion.AngleAxis(180f * t, axis);

        return (rotation * startDir).normalized;
    }

    // camera continua olhando para o centro do planeta
    private Quaternion BuildCameraRotation(Vector3 viewDir, Vector3 upHint)
    {
        viewDir = viewDir.normalized;

        // A câmera está em target + viewDir * distance,
        // então ela precisa olhar na direção contrária.
        Vector3 forward = -viewDir;

        Vector3 up = upHint - viewDir * Vector3.Dot(upHint, viewDir);

        if (up.sqrMagnitude < 0.001f)
            up = Vector3.up - viewDir * Vector3.Dot(Vector3.up, viewDir);

        if (up.sqrMagnitude < 0.001f)
            up = Vector3.forward;

        up.Normalize();

        return Quaternion.LookRotation(forward, up);
    }
    private void SyncRubikStateIfNeeded()
    {
        Vector3 expectedDir = GetRubikCameraDirection();
        Vector3 realDir = GetCurrentCameraDirection();

        if (Vector3.Dot(expectedDir, realDir) < 0.99f)
        {
            SetRubikAxesFromDirection(realDir);
        }
    }

    private void QueueRubikRotation(string axisName, int direction)
    {
        if (!isRotating && rotationQueue.Count == 0)
            SyncRubikStateIfNeeded();

        rotationQueue.Enqueue(new RubikCommand(axisName, direction));

        if (!isRotating)
            ProcessNextRubikRotation();
    }

    private void ProcessNextRubikRotation()
    {
        if (rotationQueue.Count == 0)
        {
            isRotating = false;
            return;
        }

        isRotating = true;

        RubikCommand command = rotationQueue.Dequeue();

        ApplyRubikRotationToState(command.axis, command.direction);

        float duration = rotationQueue.Count > 0 ? queuedSnapTime : snapTime;

        SnapToRubikState(duration);
    }

    private void OnRubikRotationFinished()
    {
        ProcessNextRubikRotation();
    }

    private void ApplyRubikRotationToState(string axisName, int direction)
    {
        Vector3 axis;

        switch (axisName)
        {
            case "x":
                axis = rubikX;
                break;

            case "y":
                axis = rubikY;
                break;

            case "z":
                axis = rubikZ;
                break;

            default:
                return;
        }

        Quaternion rotation = Quaternion.AngleAxis(90f * direction, axis.normalized);

        rubikX = SnapAxis(rotation * rubikX);
        rubikY = SnapAxis(rotation * rubikY);
        rubikZ = SnapAxis(rotation * rubikZ);
    }

    // Botão 7:
    // Vai para o vértice oposto, olhando para o outro lado do cubo.
    public void GoToOppositeVertex()
    {
        rotationQueue.Clear();
        isRotating = true;

        rubikX = -rubikX;
        rubikY = -rubikY;
        rubikZ = -rubikZ;

        SnapToRubikState(snapTime);
    }

    // Botões:
    // 1 = z'
    // 2 = x
    // 3 = y
    // 4 = y'
    // 5 = x'
    // 6 = z

    public void CameraButton1()
    {
        RotateZPrime();
    }

    public void CameraButton2()
    {
        RotateX();
    }

    public void CameraButton3()
    {
        RotateY();
    }

    public void CameraButton4()
    {
        RotateYPrime();
    }

    public void CameraButton5()
    {
        RotateXPrime();
    }

    public void CameraButton6()
    {
        RotateZ();
    }

    public void RotateX()
    {
        QueueRubikRotation("x", 1);
    }

    public void RotateXPrime()
    {
        QueueRubikRotation("x", -1);
    }

    public void RotateY()
    {
        QueueRubikRotation("y", 1);
    }

    public void RotateYPrime()
    {
        QueueRubikRotation("y", -1);
    }

    public void RotateZ()
    {
        QueueRubikRotation("z", 1);
    }

    public void RotateZPrime()
    {
        QueueRubikRotation("z", -1);
    }

    public void SetTargetPosition(Vector3 newTarget)
    {
        targetPosition = newTarget;
        ApplyRubikStateInstant();
    }

    public void UpdateTargetFromPlanet()
    {
        if (cubePlanet == null)
            cubePlanet = FindObjectOfType<CubePlanet>();

        if (cubePlanet == null)
        {
            Debug.LogWarning("CubePlanet não encontrado. Usando targetPosition manual.");
            return;
        }

        targetPosition = cubePlanet.GetPlanetCenterWorld();
    }
}