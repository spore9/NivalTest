using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Скрипт по обработке нажатия игроком кнопок мыши
/// </summary>
public class PlayerControl : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        // Если нажата правая или левая кнопка мыши
		if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(0))
        {
            // Пускаем луч, чтобы узнать, по какому объекту в пространстве щёлкнули
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Если по тайлу
                if (hit.collider.gameObject.tag.Equals("Tile"))
                {
                    GameObject tile = hit.collider.gameObject;
                    // То при правой кнопке мыши сменили его статус остановки
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (tile.GetComponent<Tile>().IsStop)
                        {
                            tile.GetComponent<Renderer>().material.color = GameSettings.NotStopColor;
                        }
                        else
                        {
                            tile.GetComponent<Renderer>().material.color = GameSettings.StopColor;
                        }
                        tile.GetComponent<Tile>().IsStop = !tile.GetComponent<Tile>().IsStop;
                        // И если это режим поиска остановок, то оповестили об этом юнитов
                        if (GameManager.IsTakingCellsMode)
                        {
                            OnStopsCountChanged();
                        }
                    }
                    // То при левой кнопке мыши сделали непроходимым
                    else if (Input.GetMouseButtonDown(0))
                    {
                        if (tile.GetComponent<Tile>().IsPassable)
                        {
                            Instantiate(Resources.Load(@"Prefabs\Blocker"), tile.transform);
                            tile.GetComponent<Tile>().IsPassable = false;
                            OnBlockersCountChanged();
                        }
                    }
                }
                // Если это блокиратор, то удалили его и сделали клетку проходимой
                else if (hit.collider.gameObject.tag.Equals("Blocker"))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject blocker = hit.collider.gameObject;
                        if (!blocker.transform.parent.GetComponent<Tile>().IsPassable)
                        {
                            blocker.transform.parent.GetComponent<Tile>().IsPassable = true;
                            Destroy(blocker);
                            OnBlockersCountChanged();
                        }
                    }
                }
            }
        }
	}
    public delegate void BlockersCountChanged();
    /// <summary>
    /// Событие изменения числа заблокированных клеток
    /// </summary>
    public event BlockersCountChanged OnBlockersCountChanged;

    public delegate void StopsCountChanged();
    /// <summary>
    /// Событие изменения числа доступных для остановки клеток
    /// </summary>
    public event StopsCountChanged OnStopsCountChanged;
}
