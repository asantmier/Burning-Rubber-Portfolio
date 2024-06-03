using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public RectTransform playerIcon;
    public RectTransform passengerIcon;
    public RectTransform goalIcon;
    public Camera minimapCamera;

    Transform cameraLocation;
    Transform player;
    RectTransform minimap;
    

    void Awake()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            player = go.transform;
        } else
        {
            Debug.LogError("Couldn't find player in scene!");
        }
        // TODO make this getting player shit part of a singleton or something cause its really common

        minimap = GetComponent<RectTransform>();
        if (minimapCamera != null)
        {
            cameraLocation = minimapCamera.transform;
        } else
        {
            Debug.LogError("Please give the Minimap a minimap camera!");
        }
        
        passengerIcon.gameObject.SetActive(false);
        goalIcon.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // Find the location of the player relative to the camera transform
        Vector3 playerInCamera = WorldToMinimap(player.position);
        // And now we know where the player is on the minimap!
        playerIcon.anchoredPosition = playerInCamera;
        // Rotate it too!
        playerIcon.rotation = Quaternion.Euler(0, 0, -player.rotation.eulerAngles.y);
    }

    public void SetPassenger(Vector3 pos)
    {
        Vector3 passengerInCamera = WorldToMinimap(pos);
        passengerIcon.anchoredPosition = passengerInCamera;
        passengerIcon.gameObject.SetActive(true);
    }

    public void HidePassenger()
    {
        passengerIcon.gameObject.SetActive(false);
    }

    public void SetGoal(Vector3 pos)
    {
        Vector3 goalInCamera = WorldToMinimap(pos);
        goalIcon.anchoredPosition = goalInCamera;
        goalIcon.gameObject.SetActive(true);
    }

    public void HideGoal()
    {
        goalIcon.gameObject.SetActive(false);
    }

    private Vector3 WorldToMinimap(Vector3 worldPos)
    {
        // Find the location of the point relative to the camera transform
        Vector3 posInCamera = cameraLocation.InverseTransformPoint(worldPos);
        // Since orthographics size is half the viewing volume's height, we use half the height of the minimap rect
        float scale = (minimap.rect.height / 2) / minimapCamera.orthographicSize;
        // Convert the location of the point relative to the camera to be relative to the minimap with a scale transformation
        posInCamera.Scale(Vector3.one * scale);
        return posInCamera;
    }
}
