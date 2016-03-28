using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class CameraMove : MonoBehaviour {
    [Header ("Camera Target")]
    public GameObject target;
    [Header("Fallow Settings")]
    [Tooltip("Camera height")]
    public float yOffset;
    [Tooltip("Camera distance to target")]
    public float zOffset;
    [Header("Look Settings")]
    [Range(1,4)]
    public float smooth;

    void Update()
    {
        SmoothFallow(target, yOffset, zOffset);
        SmoothLookAt(target.transform.position, smooth);
    }

    /// <summary>
    /// Плавное следование камеры за игроком
    /// </summary>
    /// <param name="target"> Цель </param>
    /// <param name="yOffset"> Высота полёта камеры </param>
    /// /// <param name="yOffset"> Расстояние от камеры к игроку (проекция) </param>
    void SmoothFallow(GameObject target, float yOffset, float zOffset)
    {
        Vector3 targetPosition = (new Vector3(target.transform.position.x, target.transform.position.y + yOffset, target.transform.position.z)) + (target.transform.forward * (-zOffset));

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > 0f)
        {
            float fallowSpeed;

            if (distance >= 1f) fallowSpeed = Mathf.Pow(distance, 2);
            else if (distance >= 0.5f) fallowSpeed = distance * 1.3f;
            else fallowSpeed = 0.7f;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * fallowSpeed);
        }
    }

    /// <summary>
    /// Плавный поворот камеры
    /// </summary>
    /// <param name="target"> Цель </param>
    /// <param name="smooth"> Коэффициент плавности </param>
    void SmoothLookAt(Vector3 target, float smooth)
    {
        Vector3 dir = target - transform.position;
        dir = new Vector3(dir.x, dir.y + 2, dir.z);
        transform.forward = Vector3.Slerp(transform.forward, dir, Time.deltaTime * smooth);
    }
}
