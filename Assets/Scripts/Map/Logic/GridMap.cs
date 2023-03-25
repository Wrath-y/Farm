using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class GridMap : MonoBehaviour
{
    public MapData_SO mapData;
    public GridType gridType;
    private Tilemap _curTileMap;

    private void OnEnable()
    {
        if (!Application.IsPlaying(this))
        {
            _curTileMap = GetComponent<Tilemap>();
            if (mapData != null)
            {
                mapData.tileProperties.Clear();
            }
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            _curTileMap = GetComponent<Tilemap>();
            UpdateTileProperties();
            #if UNITY_EDITOR
            if (mapData != null)
            {
                EditorUtility.SetDirty(mapData);
            }
            #endif
        }
    }

    private void UpdateTileProperties()
    {
        _curTileMap.CompressBounds();
        if (!Application.IsPlaying(this))
        {
            if (mapData != null)
            {
                Vector3Int starPos = _curTileMap.cellBounds.min;
                Vector3Int endPos = _curTileMap.cellBounds.max;

                for (int x = starPos.x; x < endPos.x; x++)
                {
                    for (int y = starPos.y; y < endPos.y; y++)
                    {
                        TileBase tile = _curTileMap.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            TileProperty newTile = new TileProperty
                            {
                                tileCoordinate = new Vector2Int(x, y),
                                gridType = gridType,
                                boolTypeValue = true
                            };
                            
                            mapData.tileProperties.Add(newTile);
                        }
                    }
                }
            }
        }
    }
}
