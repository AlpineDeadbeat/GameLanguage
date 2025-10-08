using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportation;
    CinemachineConfiner confiner;

    enum Direction { Up, Down, Left, Right , Teleport}

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);

            MapController_Manual.Instance?.HighlightArea(mapBoundry.name);
            MapController_Dynamic.Instance?.UpdateCurrentArea(mapBoundry.name);
        }
    }

    void UpdatePlayerPosition(GameObject player)
    {
        if(direction == Direction.Teleport)
        {
            player.transform.position = teleportation.position;
            return;
        }

        Vector3 additivePos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                additivePos.y += 2;
                break;
            case Direction.Down:
                additivePos.y += -2;
                break;
            case Direction.Left:
                additivePos.x += -2;
                break;
            case Direction.Right:
                additivePos.x += 2;
                break;
        }

        player.transform.position = additivePos;
    }
}
