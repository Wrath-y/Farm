using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.AStar
{
    public class GridNodes
    {
        // 地图宽度
        private int _width;
        // 地图高度
        private int _height;
        private Node[,] _gridNode;

        // 构造函数初始化节点范围数组
        public GridNodes(int width, int height)
        {
            _width = width;
            _height = height;

            _gridNode = new Node[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }

        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos < _width && yPos < _height)
            {
                return _gridNode[xPos, yPos];
            }
            Debug.Log("超出网格范围");
            return null;
        }
    }   
}
