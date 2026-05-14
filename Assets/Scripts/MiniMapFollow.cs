using UnityEngine;
using System.Collections;

public class MiniMapFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float height = 20f; // How high up the camera sits

    void LateUpdate()
    {
        if (!player) return;

        // Follow the player's position, but keep our own rotation
        Vector3 newPosition = player.position;
        newPosition.y = height;
        transform.position = newPosition;

        // Force the camera to look straight down
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
