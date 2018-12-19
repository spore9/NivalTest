using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для построения путей
/// </summary>
public class Pathfinding
{
    /// <summary>
    /// Попытка построить путь с помощью алгоритма А*
    /// </summary>
    /// <param name="startPosition">Начальная точка</param>
    /// <param name="targetPosition">Конечная точка</param>
    /// <returns>Массив точек пути</returns>
    public Vector3[] FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        // Heap (в целях оптимизации) с потенциальными для прохода точек
        Heap<PathNode> openNodes = new Heap<PathNode>(GameManager.FieldSideSize * GameManager.FieldSideSize);
        // HashSet для быстрого определения, есть ли в нём объект или нет
        HashSet<PathNode> closeNodes = new HashSet<PathNode>();

        // Перевод точек из пространственных координат в тайлы
        PathNode startNode = GameManager.NodeFromPosition(startPosition).node;
        PathNode targetNode = GameManager.NodeFromPosition(targetPosition).node;
        // Если мы идём в ту же точку, где уже находимся
        if (startNode.ID == targetNode.ID)
        {
            Vector3[] waypoints = new Vector3[1];
            waypoints[0] = new Vector3(targetPosition.x, 1, targetPosition.z);
            return waypoints;
        }
        else
        {
            Vector3[] waypoints = new Vector3[0];
            bool success = false;
            openNodes.Add(startNode);
            while (openNodes.Count > 0)
            {
                // Самую дешёвую по прохождению точку считаем текущей
                PathNode currentNode = openNodes.RemoveFirst();
                closeNodes.Add(currentNode);

                // Если мы достигли пункта назначения
                if (currentNode == targetNode)
                {
                    success = true;
                    break;
                }
                // Получаем все соседние тайлы и проверяем их
                foreach (PathNode neighbour in getNeighbours(currentNode))
                {
                    // Если уже были в данной точке или она не проходима, то пропускаем
                    if (!neighbour.IsPassable ||
                        closeNodes.Contains(neighbour))
                    {
                        continue;
                    }
                    // Считаем стоимость прохода к данному соседу
                    int newCostToNeighbour = currentNode.gCost + GameManager.getDistance(currentNode, neighbour);
                    // Если рядом с нашей точком или ещё не добавлен в число доступных
                    if (newCostToNeighbour < neighbour.gCost || !openNodes.Contains(neighbour))
                    {
                        // Считаем показатели данной точки
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GameManager.getDistance(neighbour, targetNode);
                        // Указываем откуда к ней пришли
                        neighbour.ParentNode = currentNode;
                        // Добаляем к числу доступных
                        if (!openNodes.Contains(neighbour))
                        {
                            openNodes.Add(neighbour);
                        }
                    }
                }
            }
            // Если путь найден
            if (success)
            {
                // Обрабатываем путь перед возвращением
                waypoints = retracePath(startNode, targetNode);
                return waypoints;
            }
        }

        // Иначе возвращаем null
        return null;
    }

    /// <summary>
    /// Обработка пути для подходящего к прохождению вида
    /// </summary>
    /// <param name="startNode">Начальная точка</param>
    /// <param name="targetNode">Конечная точка</param>
    /// <returns>Массив точек пути</returns>
    Vector3[] retracePath(PathNode startNode, PathNode targetNode)
    {
        List<PathNode> path = new List<PathNode>();
        // Начинаем с конца
        PathNode currentNode = targetNode;
        // И пока не дойдём до начала
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }
        // Переводим в координаты
        Vector3[] wayponts = simplifyPath(path);
        return wayponts;
    }

    /// <summary>
    /// Переводим данные точки из тайлов в пространственные координаты
    /// </summary>
    /// <param name="path">Список тайлов</param>
    /// <returns>Массив координат пути</returns>
    Vector3[] simplifyPath(List<PathNode> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        for (int i = 0; i < path.Count; i++)
        {
            // Непосредственно перевод в пространственные координаты
            Vector3 tilePosition = GameManager.Tiles[path[i].ID].transform.position;
            Vector3 unitPosition = new Vector3(tilePosition.x, 1, tilePosition.z);
            waypoints.Add(unitPosition);
        }
        // Переворачиваем список, чтобы начало пути оказалось в начале
        waypoints.Reverse();
        return waypoints.ToArray();
    }

    /// <summary>
    /// Получить соседние тайлы
    /// </summary>
    /// <param name="tile">Тайл, чьих соседей необходимо получить</param>
    /// <returns></returns>
    List<PathNode> getNeighbours(PathNode tile)
    {

        List<PathNode> tileNeighbours = new List<PathNode>();
        // Проверяем по 4 направлениям
        if (!GameSettings.IsDiagonalEnabled)
        {
            checkDirection(tile.X + 1, tile.Y, ref tileNeighbours);
            checkDirection(tile.X - 1, tile.Y, ref tileNeighbours);
            checkDirection(tile.X, tile.Y + 1, ref tileNeighbours);
            checkDirection(tile.X, tile.Y - 1, ref tileNeighbours);
        }
        // Или по 9 если активен соответсвующий режим
        else
        {
            for (int x=-1;x<=1;x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x==0 && y==0)
                    {
                        continue;
                    }
                    int checkingX = tile.X + x;
                    int checkingY = tile.Y + y;
                    if (checkingX >= 0 && checkingX < GameManager.FieldSideSize &&
                    checkingY >= 0 && checkingY < GameManager.FieldSideSize)
                    {
                        tileNeighbours.Add(GameManager.Tiles[checkingY + checkingX * GameManager.FieldSideSize].GetComponent<Tile>().node);
                    }
                }
            }
        }
        return tileNeighbours;
    }
    /// <summary>
    /// Проверка доступности тайла
    /// </summary>
    /// <param name="checkingX">Его X координата</param>
    /// <param name="checkingY">Его Y координата</param>
    /// <param name="tileNeighbours">Список, куда добавлять подошедшие тайлы</param>
    void checkDirection(int checkingX, int checkingY, ref List<PathNode> tileNeighbours)
    {
        // Если не за пределами карты
        if (checkingX >= 0 && checkingX < GameManager.FieldSideSize &&
                    checkingY >= 0 && checkingY < GameManager.FieldSideSize)
        {
            tileNeighbours.Add(GameManager.Tiles[checkingY + checkingX * GameManager.FieldSideSize].GetComponent<Tile>().node);
        }
    }
}
