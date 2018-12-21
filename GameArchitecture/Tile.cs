using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс описывающий свойства тайла для его визуализации и построения путей
/// </summary>
public class Tile : MonoBehaviour
{
    /// <summary>
    /// Номер тайла в списке тайлов
    /// </summary>
    public int ID
    {
        get { return node.ID; }
    }

    /// <summary>
    /// Информация о тайле для построения путей
    /// </summary>
    public PathNode node;

    /// <summary>
    /// Является ли подходящей для стояния точкой
    /// </summary>
    public bool IsStop { get; set; }

    /// <summary>
    /// Проходим ли тайл
    /// </summary>
    public bool IsPassable
    {
        get { return node.IsPassable; }
        set { node.IsPassable = value; }
    }

    /// <summary>
    /// Претендует ли на тайл другой юнит
    /// </summary>
    public bool IsOccupied { get; set; }

    /// <summary>
    /// Инициализация основных переменных
    /// </summary>
    /// <param name="id">Номер тайла в списке тайлов </param>
    /// <param name="x">X координата в таблице тайлов</param>
    /// <param name="y">Y координата в таблице тайлов</param>
    public void Initialize(int id, int x, int y)
    {
        node = new PathNode(id, x, y);
        IsStop = false;
        IsPassable = true;
        IsOccupied = false;
    }
}

/// <summary>
/// Вспомогательный класс для тайла, который содержит информацию для построения путей,
/// Также работает с классом heap
/// </summary>
public class PathNode : IHeapItem<PathNode>
{
    int heapIndex;
    int id;
    /// <summary>
    /// Номер тайла в списке тайлов
    /// </summary>
    public int ID
    {
        get
        {
            return id;
        }
    }

    /// <summary>
    /// Расстояние от стартовой точки до этой
    /// </summary>
    public int gCost;
    /// <summary>
    /// Расстояние от этой точки до конечной
    /// </summary>
    public int hCost;
    /// <summary>
    /// Общая стоимость точки (сумма расстояний)
    /// </summary>
    public int fCost
    {
        get
        { return gCost + hCost; }
    }

    /// <summary>
    /// X координата в таблице тайлов
    /// </summary>
    public int X;
    /// <summary>
    /// Y координата в таблице тайлов
    /// </summary>
    public int Y;

    /// <summary>
    /// Проходим ли тайл
    /// </summary>
    public bool IsPassable { get; set; }

    /// <summary>
    /// Из какого тайла пришли в этот
    /// </summary>
    public PathNode ParentNode;

    /// <summary>
    /// Класс, который содержит информацию для построения путей
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public PathNode(int _id, int x, int y)
    {
        id = _id;
        X = x;
        Y = y;
    }
    /// <summary>
    /// Индекс в классе heap (для быстрого поиска)
    /// </summary>
    public int Index
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }
    /// <summary>
    /// Сравнить стоимость точки
    /// </summary>
    /// <param name="comparingTile">С какой точкой сравниваем</param>
    /// <returns></returns>
    public int CompareTo(PathNode comparingTile)
    {
        int compare = fCost.CompareTo(comparingTile.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(comparingTile.hCost);
        }
        return -compare;
    }
}
