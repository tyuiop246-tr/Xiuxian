using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    [Header("障碍物向左移动速度")]
    public float moveSpeed = 5f;
    public float destroyX = -10f;

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
