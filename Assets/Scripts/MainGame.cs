using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections.Generic;
using System.Globalization;

public class MainGame : MonoBehaviour
{
    [Header("Настройки игры")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private int initialTargets = 5;
    [SerializeField] private float spawnAreaMinX = -8f;
    [SerializeField] private float spawnAreaMaxX = 8f;
    [SerializeField] private float spawnAreaMinY = -4f;
    [SerializeField] private float spawnAreaMaxY = 4f;
    
    [Header("UI элементы")]
    [SerializeField] private TMP_Text clickCountText;
    [SerializeField] private TMP_Text sessionInfoText;
    [SerializeField] private string clickTextFormat = "Кликов: {0}";
    
    [Header("Настройки логирования")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private string dataFileName = "game_data.csv";
    
    // Игровые данные
    private int totalClicks = 0;
    private int currentSessionId;
    private int playerId = 1;
    private List<GameDataEntry> dataEntries = new List<GameDataEntry>();
    private DateTime sessionStartTime;
    
    private Camera mainCamera;
    private List<GameObject> targets = new List<GameObject>();
    
    void Start()
    {
        mainCamera = Camera.main;
        
        InitializeSession();
        
        CreateInitialTargets();
        
        UpdateUI();
    }
    
    private void InitializeSession()
    {
        sessionStartTime = DateTime.Now;
        currentSessionId = GenerateSessionId();
        
        // Создание заголовка CSV файла
        if (enableLogging)
        {
            CreateCSVFile();
        }
        
        Debug.Log($"Сессия #{currentSessionId} начата в {sessionStartTime}");
    }
    
    private int GenerateSessionId()
    {
        return UnityEngine.Random.Range(1000, 9999);
    }
    
    private void CreateInitialTargets()
    {
        for (int i = 0; i < initialTargets; i++)
        {
            CreateNewTarget();
        }
    }
    
    public void CreateNewTarget()
    {
        Vector3 randomPosition = GetRandomPosition();
        GameObject target = Instantiate(targetPrefab, randomPosition, Quaternion.identity);
        
        ClickableObject clickableObject = target.GetComponent<ClickableObject>();
        if (clickableObject != null)
        {
            clickableObject.Initialize(this, targets.Count);
        }
        
        targets.Add(target);
    }
    
    private Vector3 GetRandomPosition()
    {
        float randomX = UnityEngine.Random.Range(spawnAreaMinX, spawnAreaMaxX);
        float randomY = UnityEngine.Random.Range(spawnAreaMinY, spawnAreaMaxY);
        return new Vector3(randomX, randomY, 0);
    }
    
    // Вызывается при клике на объект
    public void OnObjectClicked(int objectId, Vector3 position, float reactionTime)
    {
        totalClicks++;
        UpdateUI();
        
        if (enableLogging)
        {
            LogGameEvent("click", objectId, position, reactionTime, 1);
        }
        
        // Перемещаем объект в новую позицию
        targets[objectId].transform.position = GetRandomPosition();
        
        ClickableObject clickableObj = targets[objectId].GetComponent<ClickableObject>();
        if (clickableObj != null)
        {
            clickableObj.ResetAppearanceTime();
        }
    }
    
    // Логирование движения
    public void LogMovement(int objectId, Vector3 position)
    {
        if (enableLogging)
        {
            LogGameEvent("movement", objectId, position, 0f, 0);
        }
    }
    
    private void LogGameEvent(string eventType, int objectId, Vector3 position, float reactionTime, int score)
    {
        GameDataEntry entry = new GameDataEntry
        {
            player_id = playerId,
            session_id = currentSessionId,
            timestamp = DateTime.Now,
            event_type = eventType,
            object_id = objectId,
            x_position = position.x,
            y_position = position.y,
            reaction_time = reactionTime,
            score = score
        };
        
        dataEntries.Add(entry);
        SaveDataToCSV(entry);
    }
    
    private void CreateCSVFile()
    {
        string filePath = GetFilePath();
        
        // Записываем заголовок только если файл не существует
        if (!File.Exists(filePath))
        {
            string header = "player_id,session_id,timestamp,event_type,object_id,x_position,y_position,reaction_time,score";
            File.WriteAllText(filePath, header + Environment.NewLine);
        }
    }
    
    private void SaveDataToCSV(GameDataEntry entry)
    {
        string filePath = GetFilePath();
        
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0},{1},{2},{3},{4},{5:F2},{6:F2},{7:F3},{8}",
            entry.player_id,
            entry.session_id,
            entry.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            entry.event_type,
            entry.object_id,
            entry.x_position,
            entry.y_position,
            entry.reaction_time,
            entry.score
        );
        
        File.AppendAllText(filePath, line + Environment.NewLine);
    }
    
    private string GetFilePath()
    {
        #if UNITY_EDITOR
        return Application.dataPath + "/../" + dataFileName;
        #else
        return Application.persistentDataPath + "/" + dataFileName;
        #endif
    }
    
    private void UpdateUI()
    {
        if (clickCountText != null)
        {
            clickCountText.text = string.Format(clickTextFormat, totalClicks);
        }
        
        if (sessionInfoText != null)
        {
            sessionInfoText.text = $"Сессия: {currentSessionId}\nИгрок: {playerId}";
        }
    }
    
    public void PrintAllData()
    {
        Debug.Log($"Всего записей: {dataEntries.Count}");
        foreach (var entry in dataEntries)
        {
            Debug.Log(entry.ToString());
        }
    }
    
    // При завершении игры
    void OnApplicationQuit()
    {
        Debug.Log($"Сессия #{currentSessionId} завершена. Всего кликов: {totalClicks}");
        Debug.Log($"Данные сохранены в: {GetFilePath()}");
    }
}

// Структура для хранения данных игры
[System.Serializable]
public struct GameDataEntry
{
    public int player_id;
    public int session_id;
    public DateTime timestamp;
    public string event_type;
    public int object_id;
    public float x_position;
    public float y_position;
    public float reaction_time;
    public int score;
    
    public override string ToString()
    {
        return $"[{timestamp:HH:mm:ss}] {event_type} на объекте {object_id} в ({x_position:F1}, {y_position:F1}) за {reaction_time:F2}с";
    }
}