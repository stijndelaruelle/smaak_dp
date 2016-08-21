﻿using UnityEngine;
using System.Collections;
using System;
using Assets.Helpers;

public class GPSManagerDebug : GPSManager
{
    private float m_Latitude;
    private float m_Longitude;
    private double m_Timestamp;

    protected override void Awake()
    {
        //Just to fix the debug renderer
        base.Awake();
        m_Timestamp = GetTimeSince1970();
    }

    private void Update()
    {
        //On click
        if (Input.GetMouseButtonDown(0))
        {
            UpdateLocationInfo();   
        }
    }

    private void UpdateLocationInfo()
    {
        //Get the mouse position in world coordinates (works only if we have an orthograpic camera!)
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //Figure out where we clicked in Latitude & Longitude
        //This is the complete opposite of the marker (UpdateMarker) code.

        //Get the map id & zoom (thanks to the debug collider)
        RaycastHit hit;
        bool success = Physics.Raycast(new Ray(mousePosition, -Vector3.up), out hit, 200.0f);

        if (!success)
            return;

        Tile tile = hit.collider.gameObject.GetComponent<Tile>();
        if (!tile)
            return;

        Vector3 worldPosition = hit.point;

        //Shave off the world offset
        Vector2 offset = GameObject.FindObjectOfType<World>().CalculateOffset(tile.MapID); //Find object SUPER DIRTY, but this is debug code and otherwise passing the variable trough will clutter the actual live code.
        worldPosition.x -= offset.x;
        worldPosition.z -= offset.y;

        //Add the centers
        Rect rect = GM.TileBounds(tile.MapID, tile.Zoom);
        worldPosition.x += rect.center.x;
        worldPosition.z += rect.center.y;

        Vector2 latLon = GM.MetersToLatLon(new Vector2(worldPosition.x, worldPosition.z));

        m_Latitude = latLon.y;
        m_Longitude = latLon.x;

        m_Timestamp = GetTimeSince1970();
    }

    //GPSManager
    protected override void StartTracking()
    {
        m_InitializeTime = GetTimeSince1970();
        m_Timestamp = m_InitializeTime;

        m_GPSState = GPSState.Running;
    }

    public override CustomLocationInfo GetLocationInfo()
    {
        CustomLocationInfo info = new CustomLocationInfo();

        info.latitude = m_Latitude;
        info.longitude = m_Longitude;
        info.timestamp = m_Timestamp; 

        info.altitude = 0.0f;
        info.horizontalAccuracy = 0.0f;
        info.verticalAccuracy = 0.0f;

        return info;
    }

    public override float GetLatitude()
    {
        return m_Latitude;
    }

    public override float GetLongitude()
    {
        return m_Longitude;
    }

    public override double GetTimestamp()
    {
        return m_Timestamp;
    }

    //Helper (should be in a better place, but I only use it here for now)
    private double GetTimeSince1970()
    {
        return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}
