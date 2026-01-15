using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки объекта")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    
    private MainGame gameManager;
    private int objectId;
    private float appearanceTime;
    private bool isActive = true;
    
    private int clickCount = 0;
    private Vector3 lastPosition;
    
    public void Initialize(MainGame manager, int id)
    {
        gameManager = manager;
        objectId = id;
        lastPosition = transform.position;
        ResetAppearanceTime();
        
        RandomizeAppearance();
    }
    
    public void ResetAppearanceTime()
    {
        appearanceTime = Time.time;
    }
    
    private void RandomizeAppearance()
    {
        // Случайный цвет
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f)
            );
        }
        
        // Случайный размер
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, 1);
    }
    
    // Обработка клика мыши
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive) return;
        
        clickCount++;
        
        // Расчет времени реакции
        float reactionTime = Time.time - appearanceTime;
        
        // Уведомляем менеджер игры
        if (gameManager != null)
        {
            gameManager.OnObjectClicked(objectId, transform.position, reactionTime);
        }
        
        PlayClickAnimation();
        
        // Логирование
        Debug.Log($"Объект {objectId} кликнут. Время реакции: {reactionTime:F2}с");
    }
    
    void OnMouseDown()
    {
        if (!isActive) return;
        
        clickCount++;
        float reactionTime = Time.time - appearanceTime;
        
        if (gameManager != null)
        {
            gameManager.OnObjectClicked(objectId, transform.position, reactionTime);
        }
        
        PlayClickAnimation();
    }
    
    private void PlayClickAnimation()
    {
        StartCoroutine(ClickAnimation());
    }
    
    private System.Collections.IEnumerator ClickAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 0.8f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
        
        RandomizeAppearance();
    }
    
    // Обновление позиции
    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
        {
            lastPosition = transform.position;
        
        }
    }
    
    public int GetClickCount() => clickCount;
    public int GetObjectId() => objectId;
}