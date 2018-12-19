using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Игровые параметры
/// </summary>
public static class GameSettings {
    /// <summary>
    /// Физический размер клетки
    /// </summary>
    public const int TileSize = 5;
    /// <summary>
    /// Физический размер юнита
    /// </summary>
    public const int UnitSize = 1;
    /// <summary>
    /// Скорость юнита
    /// </summary>
    public const float UnitSpeed = 0.1f;

    /// <summary>
    /// Минимальный размер игрового поля
    /// </summary>
    public const int MinFieldSideSize = 5;
    /// <summary>
    /// Максимальный размер игрового поля
    /// </summary>
    public const int MaxFieldSideSize = 10;

    /// <summary>
    /// Минимальное число юнитов
    /// </summary>
    public const int MinUnitCount = 1;
    /// <summary>
    /// Максимальное число юнитов
    /// </summary>
    public const int MaxUnitCount = 5;

    /// <summary>
    /// Цвет выделенной клетки
    /// </summary>
    public static Color StopColor = Color.green;
    /// <summary>
    /// Цвет не выделенной клетки
    /// </summary>
    public static Color NotStopColor = Color.yellow;

    public static bool IsDiagonalEnabled = false;

    /// <summary>
    /// Список цветов юнитов
    /// </summary>
    public static List<Color> UnitsColors = new List<Color>()
    {
        Color.red,
        Color.blue,
        Color.magenta,
        Color.gray,
        Color.white
    };
}
