using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт для обработки элементов интерфейса
/// </summary>
public class ButtonProcessing : MonoBehaviour {

    /// <summary>
    /// Переключатель режима поиска остановок
    /// </summary>
    [SerializeField]
    Toggle toggleSeek;
    /// <summary>
    /// Переключатель режима хождения по диагоналям
    /// </summary>
    [SerializeField]
    Toggle toggleDiagonal;


    /// <summary>
    /// Поиск остановок переключен
    /// </summary>
    public void ToggleSeek()
    {
        GameManager.IsTakingCellsMode = toggleSeek.isOn;
        // Это для избежания сообщения об ошибке в редакторе
        if (Application.isPlaying)
        {
            // Все юниты отпускают тайлы, т.к. сейчас начнут искать подходящие для остановки
            // Или наоборот перестанут
            foreach (Tile tile in GameManager.Tiles)
            {
                tile.IsOccupied = false;
            }
            // Сообщаем всем, что игровой режим сменился
            OnModeChanged();
        }
    }

    /// <summary>
    /// Отображать ли маркеры пути
    /// </summary>
    public void ToggleShowMarks()
    {
        Camera.main.cullingMask ^= 1 << LayerMask.NameToLayer("Marks");
    }

    /// <summary>
    /// Разрешено ли хождение по диагоналям
    /// </summary>
    public void ToggleDiagonal()
    {
        GameSettings.IsDiagonalEnabled = toggleDiagonal.isOn;
        OnDiagonalModeChanged();
    }
    /// <summary>
    /// Выход из игры
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
    public delegate void ModeChanged();
    /// <summary>
    /// Событие смены режима
    /// </summary>
    public event ModeChanged OnModeChanged;
    public delegate void DiagonalModeChanged();
    /// <summary>
    /// Событие смены режима прохождения диагоналей
    /// </summary>
    public event DiagonalModeChanged OnDiagonalModeChanged;
}
