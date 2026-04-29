using UnityEngine;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string level;
    public EnemySpawner spawner;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(string text)
    {
        level = text;
        label.text = text;
    }

    public void StartLevel()
    {
        switch (GameManager.Instance.state)
        {
            case GameManager.GameState.PREGAME:
                spawner.StartLevel(level);
            break;
            case GameManager.GameState.WAVEEND:
                spawner.NextWave();
            break;
            case GameManager.GameState.GAMEOVER:
                spawner.RestartGame();
            break;
        }
        

    }
}
