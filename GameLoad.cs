using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Скрипт инициализации всех основных объектов на сцене
/// </summary>
public class GameLoad : MonoBehaviour
{

    // Построение поля и юнитов при запуске приложение
    void Awake()
    {
        // Строим поле
        GameManager.FieldSideSize = Random.Range(GameSettings.MinFieldSideSize, GameSettings.MaxFieldSideSize + 1);
        for (int i = 0; i < GameManager.FieldSideSize; i++)
        {
            for (int j = 0; j < GameManager.FieldSideSize; j++)
            {
                // отступ, чтобы край нулевого тайла распологался в нуле (для корректного рассчёта тайлов по координатам)
                int offset = (int)(GameSettings.TileSize / 2f - 1);
                GameObject tile = Instantiate(Resources.Load(@"Prefabs\Tile"),
                    new Vector3(i * GameSettings.TileSize + offset, 0, j * GameSettings.TileSize + offset),
                    Quaternion.identity) as GameObject;
                int ID = j + i * GameManager.FieldSideSize;
                tile.GetComponent<Tile>().Initialize(ID, i, j);
                tile.name = ID.ToString();
                tile.GetComponent<Renderer>().material.color = GameSettings.NotStopColor;
                GameManager.Tiles.Add(tile.GetComponent<Tile>());
            }
        }

        // Строим юнитов
        int unitCount = Random.Range(GameSettings.MinUnitCount, GameSettings.MaxUnitCount + 1);
        int tileCount = GameManager.FieldSideSize * GameManager.FieldSideSize;
        for (int i = 0; i < unitCount; i++)
        {
            int j = 0;
            // пытаемся инициализировать в случайном тайле юнита, пока не получится или число попыток не привысит число тайлов
            while (j < tileCount)
            {
                int tileID = Random.Range(0, tileCount);
                if (!GameManager.Tiles[tileID].GetComponent<Tile>().IsOccupied)
                {
                    Vector3 newPosition = new Vector3(GameManager.Tiles[tileID].transform.position.x, GameSettings.UnitSize, GameManager.Tiles[tileID].transform.position.z);
                    GameObject unit = Instantiate(Resources.Load(@"Prefabs\Unit"), newPosition, Quaternion.identity) as GameObject;
                    unit.GetComponent<Unit>().Initialize(i, GameManager.Tiles[tileID].GetComponent<Tile>(), GameSettings.UnitsColors[i]);
                    GameManager.Units.Add(unit.GetComponent<Unit>());
                    break;
                }
                j++;
            }
        }

        // подписываем юнитов друг на друга, чтобы в случае занятия точки одним, остальные на неё не претендовали
        for (int i = 0; i < unitCount; i++)
            foreach (Unit unit in GameManager.Units)
            {
                if (unit != this)
                {
                    unit.OnTileOccupied += GameManager.Units[i].ChangeDestination;
                }
            }

    }
}
