using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для выбора и визиализации юнитом его пути
/// </summary>
public class Unit : MonoBehaviour
{
    int id;
    /// <summary>
    /// Номер юнита в таблице юнитов
    /// </summary>
    public int ID
    {
        get
        {
            return id;
        }
    }

    /// <summary>
    /// ID занятой точки остановки (-1 если не занял точку)
    /// </summary>
    public int OccupiedStopID { get; set; }

    /// <summary>
    /// Объект обозначающий цель юнита
    /// </summary>
    GameObject destinationMark;

    /// <summary>
    /// Отметки, показывающие путь юнита
    /// </summary>
    List<GameObject> marks = new List<GameObject>();

    /// <summary>
    /// Все точки текущего пути юнита
    /// </summary>
    Vector3[] path;
    /// <summary>
    /// Текущий номер точки в пути
    /// </summary>
    int currentIndex;
    /// <summary>
    /// Текущая точка в пути
    /// </summary>
    Vector3 currentWaypoint;
    /// <summary>
    /// Может ли юнит идти
    /// </summary>
    bool isWalking;

    /// <summary>
    /// Инициализация юнита
    /// </summary>
    /// <param name="id">Номер юнита в таблице юнитов</param>
    /// <param name="currentTile">Где юнит находится</param>
    /// <param name="unitColor">Цвет юнита</param>
    public void Initialize(int _id, Tile currentTile, Color unitColor)
    {
        id = _id;
        isWalking = false;
        OccupiedStopID = -1;
        GetComponent<Renderer>().material.color = unitColor;
        // Настройка маркера цели
        destinationMark = Instantiate(Resources.Load(@"Particles\DestinationMark")) as GameObject;
        ParticleSystem particleSystem = destinationMark.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startColor = unitColor;
        // Подписываемся на события, которые могут вызвать изменение пути
        Camera.main.GetComponent<PlayerControl>().OnBlockersCountChanged += changePath;
        Camera.main.GetComponent<PlayerControl>().OnStopsCountChanged += checkStop;
        Camera.main.GetComponent<ButtonProcessing>().OnModeChanged += ChangeDestination;
        Camera.main.GetComponent<ButtonProcessing>().OnDiagonalModeChanged += checkNewPath;
        // Выбираем свою первую точку
        selectNextDestanation();
    }

    /// <summary>
    /// Проверка необходимости смены клетки назначения при занятии другим юнитом клетки
    /// </summary>
    public void ChangeDestination()
    {
        selectNextDestanation();
    }

    /// <summary>
    /// Проверка достижимости пути при смене режима прохождения диагоналей
    /// </summary>
    void checkNewPath()
    {
        pathSearch(transform.position, path[path.Length - 1]);
    }

    /// <summary>
    /// Проверка необходимости смены направления при появлении нового блокиратора
    /// </summary>
    void changePath()
    {
        // Если режим поиска остановки, то проверим, не появилось ли вариантов
        if (GameManager.IsTakingCellsMode)
        {
            selectNextDestanation(true);
        }
        else
        {
            // Иначе проверяем достижимость клетки
            pathSearch(transform.position, path[path.Length - 1]);
        }
    }

    // Движение юнита в пространстве каждый кадр
    void Update()
    {
        if (isWalking && OccupiedStopID == -1)
        {
            // Если достигли текущий цели
            if (transform.position == currentWaypoint)
            {
                // Удаляем маркер пути, который прошли
                if (marks.Count > 0)
                {
                    Destroy(marks[0]);
                    marks.RemoveAt(0);
                }

                currentIndex++;

                // Если мы достигли конечного пункта
                if (currentIndex >= path.Length)
                {
                    isWalking = false;
                    Tile endNode = GameManager.NodeFromPosition(currentWaypoint);
                    // Если мы достили клетки для остановки
                    if (GameManager.IsTakingCellsMode && endNode.IsStop)
                    {
                        bool isOccupied = false;
                        // Проверка на случай, если пришли одновременно
                        foreach (Unit unit in GameManager.Units)
                        {
                            if (unit.OccupiedStopID == endNode.ID)
                            {
                                isOccupied = true;
                                break;
                            }
                        }
                        if (!isOccupied)
                        {
                            OccupiedStopID = endNode.ID;
                            // Сообщаем всем, что клетка занята
                            OnTileOccupied();
                        }
                        // Если не успели, ищем дальше
                        else
                        {
                            selectNextDestanation();
                        }
                    }
                    // Иначе выбираем следующую цель
                    else
                    {
                        selectNextDestanation();
                    }
                    return;
                }
                else
                {
                    currentWaypoint = path[currentIndex];
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, GameSettings.UnitSpeed);
        }
    }

    /// <summary>
    /// Проверка остановок при появлении или удалении новых
    /// </summary>
    void checkStop()
    {
        // Если мы уже заняли клетку для остановки, то проверяем не перестала ли она таковой быть
        if (OccupiedStopID!=-1)
        {
            Tile currentNode = GameManager.NodeFromPosition(currentWaypoint);
            if (!currentNode.IsStop)
            {
                OccupiedStopID = -1;
            }
            else
            {
                return;
            }
        }
        // Если у нас и не было клетки или мы её потеряли, то ищем новую
        selectNextDestanation();
    }

    /// <summary>
    /// Выбор цели
    /// </summary>
    /// <param name="isJustStopSearch">Просто ли проверяем, нет ли новых остановок</param>
    void selectNextDestanation(bool isJustStopSearch = false)
    {
        // Не в режиме занятия остановок просто выбираем случайную
        if (!GameManager.IsTakingCellsMode)
        {
            if (OccupiedStopID != -1)
            {
                OccupiedStopID = -1;
            }
            goToRandomPoint();
        }
        // Иначе, если мы уже не заняли клетку ищем подходящие
        else if (OccupiedStopID == -1)
        {
            float minDistance = float.MaxValue;
            Tile destination = null;
            foreach (Tile tile in GameManager.Tiles)
            {
                if (tile.IsPassable && tile.IsStop)
                {
                    bool isOccupied = false;
                    foreach(Unit unit in GameManager.Units)
                    {
                        if (unit.OccupiedStopID == tile.ID)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                    if (isOccupied)
                    {
                        continue;
                    }
                    else
                    {
                        // Желательно пойти к близжайщей свободной остановке
                        float distance = Vector3.Distance(transform.position, tile.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            destination = tile;
                        }
                    }
                }
            }
            if (destination!=null)
            {
                pathSearch(transform.position, destination.transform.position);
            }
            // Если подходящих не нашли, идём в случайную
            else
            {
                // Если не нужно идти в предыдущую точку
                if (!isJustStopSearch)
                {
                    goToRandomPoint();
                }
                // Иначе проверяем достижимость клетки
                else
                {
                    pathSearch(transform.position, path[path.Length - 1]);
                }
            }
        }
    }

    /// <summary>
    /// Выбор случайной точки
    /// </summary>
    void goToRandomPoint()
    {
        int tileCount = GameManager.FieldSideSize * GameManager.FieldSideSize;
        int j = 0;
        // Ищем точку пока не найдём или пока не число попыток не привысит число тайлов
        while (j < tileCount)
        {
            int tileID = Random.Range(0, tileCount);
            if (!GameManager.Tiles[tileID].IsOccupied && GameManager.Tiles[tileID].IsPassable)
            {
                pathSearch(transform.position, GameManager.Tiles[tileID].transform.position);
                return;
            }
            j++;
        }
        // Если не нашли, останавливаемся, попробуем через 3 секунды снова
        isWalking = false;
        Invoke("ChangeDestination", 3f);
    }

    /// <summary>
    /// Попытаться построить путь от точки А до точки Б
    /// </summary>
    /// <param name="startPosition">Точка А</param>
    /// <param name="targetPosition">Точка Б</param>
    void pathSearch(Vector3 startPosition, Vector3 targetPosition)
    {
        // Перемещаем отметку цели
        destinationMark.transform.position = targetPosition;
        // Перевод точек из пространственных координат в тайлы
        PathNode startNode = GameManager.NodeFromPosition(startPosition).node;
        PathNode targetNode = GameManager.NodeFromPosition(targetPosition).node;

        // Отпускаем предыдущую цель
        if (path != null)
        {
            if (path.Length > 0)
            {
                GameManager.Tiles[GameManager.NodeFromPosition(path[path.Length - 1]).ID].IsOccupied = false;
            }
        }

        Vector3[] newPath;

        // Если мы идём в ту же точку, где уже находимся
        if (startNode.ID == targetNode.ID)
        {
            newPath = new Vector3[1];
            Vector3 newPosition = new Vector3(targetPosition.x, 1, targetPosition.z);
            newPath[0] = newPosition;
        }
        else
        {
            newPath = GameManager.Pathfinder.FindPath(startNode, targetNode);
        }
        // Если путь содержит точки, значит он существует
        if (newPath!=null)
        {
            path = newPath;
            // Удаляем все оставшиеся маркеры пути
            foreach (GameObject mark in marks)
                Destroy(mark);
            marks.Clear();
            // Строим новые маркеры пути
            for (int i = 0; i< path.Length;i++)
            {
                marks.Add(Instantiate(Resources.Load(@"Prefabs\Mark"), path[i], Quaternion.identity) as GameObject);
                marks[marks.Count-1].GetComponent<Renderer>().material.color = GameSettings.UnitsColors[ID];
            }
            // начинаем движение с 0 точки
            currentWaypoint = path[0];
            currentIndex = 0;
            GameManager.Tiles[targetNode.ID].IsOccupied = true;
            if (GameManager.NodeFromPosition(startPosition).IsPassable)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }
        }
        // Если пути нет, стоим на месте
        else
        {
            isWalking = false;
        }
    }
    public delegate void TileOccupied();
    /// <summary>
    /// Событие занятия точки остановки юнитом
    /// </summary>
    public event TileOccupied OnTileOccupied;
}
