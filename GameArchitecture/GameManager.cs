using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс хранящий все важные игровые объекты
/// </summary>
public static class GameManager {

    /// <summary>
    /// Размер одной стороны игрового поля
    /// </summary>
    public static int FieldSideSize;
    /// <summary>
    /// Список юнитов
    /// </summary>
    private static List<Unit> units = new List<Unit>();
    public static List<Unit> Units
    {
        get
        {
            return units;
        }

        set
        {
            units = value;
        }
    }
    /// <summary>
    /// Список клеток
    /// </summary>
    private static List<Tile> tiles = new List<Tile>();
    public static List<Tile> Tiles
    {
        get
        {
            return tiles;
        }

        set
        {
            tiles = value;
        }
    }

    /// <summary>
    /// Построитель путей
    /// </summary>
    public static Pathfinding Pathfinder = new Pathfinding();

    /// <summary>
    /// Активен ли режим занятия свободных клеток
    /// </summary>
    public static bool IsTakingCellsMode = false;

    /// <summary>
    /// Получить тайл, распологающийся на заданных пространственных координатах
    /// </summary>
    /// <param name="position">Пространственные координаты </param>
    /// <returns></returns>
    static public Tile NodeFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt((position.x - 1) / GameSettings.TileSize);
        int y = Mathf.RoundToInt((position.z - 1) / GameSettings.TileSize);
        // Если полученные координаты не за пределами поля
        if (y + x * FieldSideSize < FieldSideSize * FieldSideSize)
            return Tiles[y + x * FieldSideSize];
        else
            return null;
    }

    /// <summary>
    /// Получить манхэттенское расстояние между двумя тайлами
    /// </summary>
    /// <param name="firstTile">Первый тайл</param>
    /// <param name="secondTile">Второй тайл</param>
    /// <returns></returns>
    static public int getDistance(PathNode firstTile, PathNode secondTile)
    {
        // Без учёта диагоналей
        if (!GameSettings.IsDiagonalEnabled)
        {
            return Mathf.Abs(firstTile.X - secondTile.X) + Mathf.Abs(firstTile.Y - secondTile.Y);
        }
        // С учётом диагоналей
        else
        {
            int dstX = Mathf.Abs(firstTile.X - secondTile.X);
            int dstY = Mathf.Abs(firstTile.Y - secondTile.Y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
