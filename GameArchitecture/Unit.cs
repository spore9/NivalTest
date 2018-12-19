using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс для выбора и визиализации юнитом его пути
/// </summary>
public class Unit : MonoBehaviour
{
    /// <summary>
    /// Номер юнита в таблице юнитов
    /// </summary>
    public int ID;

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
    /// Занял ли юнит точку в режиме стояния
    /// </summary>
    bool isKeepTile;

    /// <summary>
    /// Инициализация юнита
    /// </summary>
    /// <param name="id">Номер юнита в таблице юнитов</param>
    /// <param name="currentTile">Где юнит находится</param>
    /// <param name="unitColor">Цвет юнита</param>
    public void Initialize(int id, Tile currentTile, Color unitColor)
    {
        ID = id;
        isWalking = false;
        isKeepTile = false;
        GetComponent<Renderer>().material.color = unitColor;
        // Настройка маркера цели
        destinationMark = Instantiate(Resources.Load(@"Particles\DestinationMark")) as GameObject;
        ParticleSystem particleSystem = destinationMark.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startColor = unitColor;
        // Подписываемся на события, которые могут вызвать изменение пути
        Camera.main.GetComponent<PlayerControl>().OnBlockersCountChanged += changePath;
        Camera.main.GetComponent<PlayerControl>().OnStopsCountChanged += checkStop;
        Camera.main.GetComponent<ButtonProcessing>().OnModeChanged += selectNextDestanation;
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
    /// Проверка необходимости смены направления при появлении нового блокиратора
    /// </summary>
    void changePath()
    {
        // Если юнит уже занял клетку, то необходимо лишь убедиться что блокиратор не появился на нём
        if (isKeepTile)
        {
            Tile currentTile = GameManager.NodeFromPosition(transform.position);
            if (!currentTile.IsPassable)
            {
                currentTile.IsOccupied = false;
                isKeepTile = false;
                selectNextDestanation();
            }
            return;
        }
        // Иначе проверяем достижимость клетки
        onPathFound(transform.position, path[path.Length - 1]);
    }

    // Движение юнита в пространстве каждый кадр
    void Update()
    {
        if (isWalking)
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

                // если мы достигли конечного пункта
                if (currentIndex >= path.Length)
                {
                    isWalking = false;
                    Tile endNode = GameManager.NodeFromPosition(currentWaypoint);
                    // если мы достили клетки для остановки
                    if (GameManager.IsTakingCellsMode && endNode.IsStop)
                    {
                        endNode.IsOccupied = true;
                        isKeepTile = true;
                        // сообщаем всем, что клетка занята
                        OnTileOccupied();
                    }
                    // иначе выбираем следующую цель
                    else
                    {
                        endNode.IsOccupied = false;
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
        if (isKeepTile)
        {
            Tile currentNode = GameManager.NodeFromPosition(currentWaypoint);
            if (!currentNode.IsStop)
            {
                currentNode.IsOccupied = false;
                isKeepTile = false;
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
    void selectNextDestanation()
    {
        // Не в режиме занятия остановок просто выбираем случайную
        if (!GameManager.IsTakingCellsMode)
        {
            if (isKeepTile)
            {
                Tile currentNode = GameManager.NodeFromPosition(currentWaypoint);
                currentNode.IsOccupied = false;
                isKeepTile = false;
            }
            goToRandomPoint();
        }
        // Иначе, если мы уже не заняли клетку ищем подходящие
        else if (!isKeepTile)
        {
            float minDistance = float.MaxValue;
            Tile destination = null;
            foreach (Tile tile in GameManager.Tiles)
            {
                if (!tile.IsOccupied && tile.IsPassable && tile.IsStop)
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
            if (destination!=null)
            {
                onPathFound(transform.position, destination.transform.position);
            }
            // Если подходящих не нашли, идём в случайную
            else
            {
                goToRandomPoint();
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
                onPathFound(transform.position, GameManager.Tiles[tileID].transform.position);
                GameManager.Tiles[tileID].IsOccupied = true;
                return;
            }
            j++;
        }
        // Если не нашли, останавливаемся
        isWalking = false;
    }

    /// <summary>
    /// Попытаться построить путь от точки А до точки Б
    /// </summary>
    /// <param name="startPosition">Точка А</param>
    /// <param name="targetPosition">Точка Б</param>
    void onPathFound(Vector3 startPosition, Vector3 targetPosition)
    {
        // Перемещаем отметку цели
        destinationMark.transform.position = targetPosition;
        Vector3[] newPath = GameManager.Pathfinder.FindPath(startPosition, targetPosition);
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
            isWalking = true;
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
