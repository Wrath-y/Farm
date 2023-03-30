using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Map;
using UnityEngine;

namespace Farm.CropPlant
{
    public class CropGenerator : MonoBehaviour
    {
        private Grid _curGrid;
        public int seedItemID;
        public int growthDays;

        private void Awake()
        {
            _curGrid = FindObjectOfType<Grid>();
            GenerateCrop();
        }

        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += GenerateCrop;
        }

        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= GenerateCrop;
        }

        private void GenerateCrop()
        {
            Vector3Int cropGridPos = _curGrid.WorldToCell(transform.position);
            if (seedItemID != 0)
            {
                var tile = GridMapManager.Instance.GetTileDetailsByMouseGridPos(cropGridPos);
                if (tile == null)
                {
                    tile = new TileDetails();
                    tile.gridX = cropGridPos.x;
                    tile.gridY = cropGridPos.y;
                }

                tile.daysSinceWatered = -1;
                tile.seedItemId = seedItemID;
                tile.growthDays = growthDays;
                
                GridMapManager.Instance.UpdateTileDetails(tile);
            }
        }
    }
}
