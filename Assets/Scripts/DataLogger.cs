using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Globalization;

public class DataLogger : MonoBehaviour
{
    [Header("Настройки логирования")]
    [SerializeField] private string fileName = "game_analytics.csv";
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool autoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;
    
    private List<AnalyticsData> dataBuffer = new List<AnalyticsData>();
    private float saveTimer;
    
    public enum EventType
    {
        Click,
        Movement,
        Spawn,
        Despawn,
        GameStart,
        GameEnd,
        Error
    }
    
    void Start()
    {
        InitializeLogFile();
        saveTimer = autoSaveInterval;
        
        // Логируем начало игры
        LogEvent(EventType.GameStart, "Игра запущена");
    }
    
    void Update()
    {
        if (autoSave)
        {
            saveTimer -= Time.deltaTime;
            if (saveTimer <= 0)
            {
                SaveBufferToFile();
                saveTimer = autoSaveInterval;
            }
        }
    }
    
    private void InitializeLogFile()
    {
        string filePath = GetFilePath();
        
        if (!File.Exists(filePath))
        {
            string header = "timestamp,session_id,player_id,event_type,object_id,x,y,reaction_time,score,additional_info";
            File.WriteAllText(filePath, header + Environment.NewLine);
        }
    }
    
    public void LogClickEvent(int sessionId, int playerId, int objectId, 
                             float x, float y, float reactionTime, int score)
    {
        AnalyticsData data = new AnalyticsData
        {
            timestamp = DateTime.Now,
            session_id = sessionId,
            player_id = playerId,
            event_type = EventType.Click.ToString(),
            object_id = objectId,
            x = x,
            y = y,
            reaction_time = reactionTime,
            score = score,
            additional_info = $"Клик на объекте {objectId}"
        };
        
        dataBuffer.Add(data);
        
        if (logToConsole)
        {
            Debug.Log($"[CLICK] Объект {objectId} - Время реакции: {reactionTime:F2}с, Очки: {score}");
        }
    }
    
    public void LogMovementEvent(int sessionId, int playerId, int objectId, 
                                float x, float y, string info = "")
    {
        AnalyticsData data = new AnalyticsData
        {
            timestamp = DateTime.Now,
            session_id = sessionId,
            player_id = playerId,
            event_type = EventType.Movement.ToString(),
            object_id = objectId,
            x = x,
            y = y,
            reaction_time = 0,
            score = 0,
            additional_info = info
        };
        
        dataBuffer.Add(data);
    }
    
    public void LogEvent(EventType eventType, string message, 
                        int sessionId = 0, int playerId = 0, 
                        int objectId = -1, float x = 0, float y = 0)
    {
        AnalyticsData data = new AnalyticsData
        {
            timestamp = DateTime.Now,
            session_id = sessionId,
            player_id = playerId,
            event_type = eventType.ToString(),
            object_id = objectId,
            x = x,
            y = y,
            reaction_time = 0,
            score = 0,
            additional_info = message
        };
        
        dataBuffer.Add(data);
        
        if (logToConsole)
        {
            Debug.Log($"[{eventType}] {message}");
        }
    }
    
    public void SaveBufferToFile()
    {
        if (dataBuffer.Count == 0) return;
        
        string filePath = GetFilePath();
        
        foreach (var data in dataBuffer)
        {
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5:F2},{6:F2},{7:F3},{8},{9}",
                data.timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                data.session_id,
                data.player_id,
                data.event_type,
                data.object_id,
                data.x,
                data.y,
                data.reaction_time,
                data.score,
                data.additional_info
            );
            
            File.AppendAllText(filePath, line + Environment.NewLine);
        }
        
        Debug.Log($"Сохранено {dataBuffer.Count} записей в файл: {filePath}");
        dataBuffer.Clear();
    }
    
    private string GetFilePath()
    {
        #if UNITY_EDITOR
        return Application.dataPath + "/../Analytics/" + fileName;
        #else
        return Application.persistentDataPath + "/" + fileName;
        #endif
    }
    
    void OnApplicationQuit()
    {
        // Сохраняем оставшиеся данные при выходе
        if (dataBuffer.Count > 0)
        {
            SaveBufferToFile();
        }
        
        LogEvent(EventType.GameEnd, "Игра завершена");
    }
}

[System.Serializable]
public struct AnalyticsData
{
    public DateTime timestamp;
    public int session_id;
    public int player_id;
    public string event_type;
    public int object_id;
    public float x;
    public float y;
    public float reaction_time;
    public int score;
    public string additional_info;
}