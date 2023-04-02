using System.Collections.Generic;
using Farm.Map;
using UnityEngine;

namespace Farm.AStar
{
    public class AStar : Singleton<AStar>
    {
        private GridNodes _gridNodes;
        private Node _startNode;
        private Node _targetNode;
        private int _gridWidth;
        private int _gridHeight;
        private int _originX;
        private int _originY;

        private List<Node> _openNodeList; // 当前选中Node周围的8个点
        private HashSet<Node> _closedNodeList; // 所有被选中的点

        private bool _pathFound; // 是否找到路径

        // 构建路径更新Stack的每一步
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos, Stack<MovementStep> npcMovementStack)
        {
            _pathFound = false;

            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                //查找最短路径
                if (FindShortestPath())
                {
                    //构建NPC移动路径
                    UpdatePathOnMovementStepStack(sceneName, npcMovementStack);
                }
            }
        }
        
        // 构建网格节点信息，初始化两个列表
        // startPos 起点 endPos 终点
        private bool GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (GridMapManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin))
            {
                //根据瓦片地图范围构建网格移动节点范围数组
                _gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                _gridWidth = gridDimensions.x;
                _gridHeight = gridDimensions.y;
                _originX = gridOrigin.x;
                _originY = gridOrigin.y;

                _openNodeList = new List<Node>();

                _closedNodeList = new HashSet<Node>();
            }
            else
            {
                return false;
            }
            
            //gridNodes的范围是从0,0开始所以需要减去原点坐标得到实际位置
            _startNode = _gridNodes.GetGridNode(startPos.x - _originX, startPos.y - _originY);
            _targetNode = _gridNodes.GetGridNode(endPos.x - _originX, endPos.y - _originY);

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x + _originX, y + _originY, 0);
                    var key = tilePos.x + "x" + tilePos.y + "y" + sceneName;

                    TileDetails tile = GridMapManager.Instance.GetTileDetails(key);

                    if (tile != null)
                    {
                        Node node = _gridNodes.GetGridNode(x, y);

                        if (tile.isNPCObstacle)
                            node.isObstacle = true;
                    }
                }
            }

            return true;
        }
        
        // 找到最短路径所有node添加到 closedNodeList
        private bool FindShortestPath()
        {
            //添加起点
            _openNodeList.Add(_startNode);

            while (_openNodeList.Count > 0)
            {//节点排序，Node内涵比较函数
                _openNodeList.Sort();

                Node closeNode = _openNodeList[0];

                _openNodeList.RemoveAt(0);
                _closedNodeList.Add(closeNode);

                if (closeNode == _targetNode)
                {
                    _pathFound = true;
                    break;
                }

                //计算周围8个Node补充到OpenList
                EvaluateNeighbourNodes(closeNode);
            }

            return _pathFound;
        }
        
        // 评估周围8个点，并生成对应消耗值
        private void EvaluateNeighbourNodes(Node currentNode)
        {
            Vector2Int currentNodePos = currentNode.gridPosition;
            Node validNeighbourNode;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    validNeighbourNode = GetValidNeighbourNode(currentNodePos.x + x, currentNodePos.y + y);

                    if (validNeighbourNode != null)
                    {
                        if (!_openNodeList.Contains(validNeighbourNode))
                        {
                            validNeighbourNode.gCost = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                            validNeighbourNode.hCost = GetDistance(validNeighbourNode, _targetNode);
                            //链接父节点
                            validNeighbourNode.parentNode = currentNode;
                            _openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
        
        // 找到有效的Node,非障碍，非已选择
        private Node GetValidNeighbourNode(int x, int y)
        {
            if (x >= _gridWidth || y >= _gridHeight || x < 0 || y < 0)
                return null;

            Node neighbourNode = _gridNodes.GetGridNode(x, y);

            if (neighbourNode.isObstacle || _closedNodeList.Contains(neighbourNode))
                return null;
            else
                return neighbourNode;
        }
        
        // 返回两点距离值
        private int GetDistance(Node nodeA, Node nodeB)
        {
            int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            if (xDistance > yDistance)
            {
                return 14 * yDistance + 10 * (xDistance - yDistance);
            }
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }
        
        /// 更新路径每一步的坐标和场景名字
        private void UpdatePathOnMovementStepStack(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            Node nextNode = _targetNode;

            while (nextNode != null)
            {
                MovementStep newStep = new MovementStep();
                newStep.sceneName = sceneName;
                newStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + _originX, nextNode.gridPosition.y + _originY);
                //压入堆栈
                npcMovementStep.Push(newStep);
                nextNode = nextNode.parentNode;
            }
        }
    }   
}
