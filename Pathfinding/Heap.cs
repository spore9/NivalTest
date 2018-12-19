using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Класс куча, для более быстрого поиска минимальной точки
/// Но написан с использованием шаблона, так что может быть использован
/// И другими типами данных
/// </summary>
/// <typeparam name="T">Тип данных</typeparam>
public class Heap<T> where T: IHeapItem<T> {

    /// <summary>
    /// Коллекция предметов
    /// </summary>
    T[] items;
    int count;

    /// <summary>
    /// Конструктор кучи
    /// </summary>
    /// <param name="maxHeapSize">Максимальное число элементов</param>
    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    /// <summary>
    /// Добавить элемент к куче
    /// </summary>
    /// <param name="item">Элемент</param>
    public void Add(T item)
    {
        item.Index = count;
        items[count] = item;
        sortUp(item);
        count++;
    }

    /// <summary>
    /// Извлечение минимального элемента
    /// </summary>
    /// <returns>Минимальный элемент</returns>
    public T RemoveFirst()
    {
        T firstItem = items[0];
        count--;
        items[0] = items[count];
        items[0].Index = 0;
        sortDown(items[0]);
        return firstItem;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void UpdateItem(T item)
    {
        sortUp(item);
    }

    /// <summary>
    /// Число элементов в куче
    /// </summary>
    public int Count
    {
        get { return count; }
    }

    /// <summary>
    /// Содержит ли куча данный элемент
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>True содержит, False не содержит</returns>
    public bool Contains(T item)
    {
        return Equals(items[item.Index], item);
    }

    /// <summary>
    /// Сортировка сверху вниз
    /// </summary>
    /// <param name="item">Для какого элемента проводится</param>
    void sortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.Index * 2 + 1;
            int childIndexRight = item.Index * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < count)
            {
                swapIndex = childIndexLeft;
                if (childIndexRight < count)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight])<0)
                    {
                        swapIndex = childIndexRight;
                    }
                }
                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// Сортировка снизу вверх
    /// </summary>
    /// <param name="item">Сортируемый элемент</param>
    void sortUp(T item)
    {
        int parentIndex = (item.Index - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.Index - 1) / 2;
        }
    }

    /// <summary>
    /// Поменять два элемента кучи местами
    /// </summary>
    /// <param name="itemA">Элемент А</param>
    /// <param name="itemB">Элемент Б</param>
    void swap(T itemA, T itemB)
    {
        items[itemA.Index] = itemB;
        items[itemB.Index] = itemA;
        int itemAindex = itemA.Index;
        itemA.Index = itemB.Index;
        itemB.Index = itemAindex;
    }
}

/// <summary>
/// Интерфейс для элементов кучи
/// </summary>
/// <typeparam name="T">Тип данных элемента</typeparam>
public interface IHeapItem<T> : IComparable<T>
{
    int Index { get; set; }
}
