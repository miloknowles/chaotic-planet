﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    // I decided to hardcode the earthRadius, since we probably just want to settle
    // on a single value and stick with it. Making it a configurable param can lead
    // to some annoying gotchas where the Unity editor has a different value than the
    // one in here.
    private float earthRadius = 5.0f;

    // The number of tiles on earth's surface.
    public int numTiles = 36;

    // All tiles are instantiated from the 'Tile' prefab in Prefab folder.
    public GameObject tilePrefab;

    // Each Tile prefab has a TileManager script attached.
    // We store them all here to manipulate the tile later.
    private List<TileManager> tiles_ = new List<TileManager>();
    private TileManager activeTile = null;

    // Start is called before the first frame update
    void Start()
    {
        InitializeTiles();
    }

    void Update()
    {
        // if (Input.GetMouseButtonDown(0)) {
        Vector2 mousePos = Input.mousePosition;

        // The mouse position is in screen pixel coordinates, and we need to
        // center it relative to the world.
        Vector2 centeredPos = mousePos - 0.5f * new Vector2(Screen.width, Screen.height);
        TileManager active = GetActiveTile(centeredPos.x, centeredPos.y);

        if (activeTile != null) {
            activeTile.Highlight(false); // Turn off previously active tile.
        }
        activeTile = active;
        activeTile.Highlight(true);
    }

    private void InitializeTiles()
    {
        // Tile angular width in degrees and radians.
        float tileDeg = 360.0f / numTiles;
        float tileRad = tileDeg * Mathf.PI / 180.0f;
        float tileArcLength = tileRad * earthRadius;

        for (int i = 0; i < numTiles; ++i) {
            float tileCenterDeg = (float)i * tileDeg - 90.0f;
            float tileCenterRad = (float)i * tileRad;

            // Instantiate the tile and retrieve its TileManager script.
            GameObject tile = Instantiate(tilePrefab);
            TileManager manager = tile.GetComponent<TileManager>();

            // We want to scale the tile width so that it takes up all of the available arc length.
            // This allows us to instantiate the tile without knowing about the units / px of the sprite.
            float spriteWidth = manager.GetSprite().bounds.max[0] - manager.GetSprite().bounds.min[0];
            float spriteHeight = manager.GetSprite().bounds.max[1] - manager.GetSprite().bounds.min[1];
            float tileScaleFactor = tileArcLength / spriteWidth;

            // Rotate and translate the tile so that it is on the edge of the earth.
            // Quaternion.Euler expects degrees!
            Quaternion tileRot = Quaternion.Euler(0, 0, tileCenterDeg);

            // Move the tile radially outward by earthRadius.
            Vector3 tileNormal = new Vector3(Mathf.Cos(tileCenterRad), Mathf.Sin(tileCenterRad), 0.0f);
            Vector3 tilePos = (earthRadius + 0.5f *spriteHeight) * tileNormal;
            tile.transform.position = tilePos;
            tile.transform.rotation = tileRot;

            tiles_.Add(manager);
        }
    }

    // Get the tile that is 'active' given the current angular mouse location.
    TileManager GetActiveTile(float x, float y)
    {
        float tileRad = 2.0f * Mathf.PI / numTiles;
        float theta = Mathf.Atan2(y, x);

        // Atan2 maps angles between [-Pi, Pi]. For the math below to work, we
        // want to map these angles from 0 to 2PI.
        if (theta < 0) { theta = 2*Mathf.PI + theta; }

        int below = (int)Mathf.Floor(theta / tileRad);
        int above = (below + 1) % numTiles;

        float belowDelta = (theta - (float)below * tileRad);
        float aboveDelta = (float)above * tileRad - theta;

        // Note: need to handle the wraparound case here where above = 0 and below = 35.
        if (aboveDelta < 0) { aboveDelta = 2.0f * Mathf.PI + aboveDelta; }

        int closest_idx = (belowDelta <= aboveDelta) ? below : above;
        return tiles_[closest_idx];
    }
}
