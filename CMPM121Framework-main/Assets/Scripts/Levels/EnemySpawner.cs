using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    private Level level;
    private int wave = 0;
    private List<Enemy> enemies;
    private Dictionary<string, int> dict;
    
    private RPNEvaluator.RPNEvaluator RPN; // used to deduce spawn info

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
        
        dict.Add("wave", wave);

        string enemies_json = Resources.Load<TextAsset>("enemies").text;
        enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemies_json);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    /*
    Note to self, level here means level of difficulty!!!
    Here we need to use $levelname to pass the relevent data to SpawnWave()
    */
    {

        string level_json = Resources.Load<TextAsset>("levels").text; // added by calvin
        level = JsonConvert.DeserializeObject<List<Level>>(level_json).Find(l => l.name == levelname); // added by calvin

        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        StartCoroutine(SpawnWave());
        wave++; // added by calvin
    }

    public void NextWave()
    {
        wave++;
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {

        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;





        // *** OUR CODE GOES HERE *** //

        dict["wave"] = wave; // IMPORTANT

        List<int> sequence_points = new List<int>(level.spawns.Count);
        foreach (int i in sequence_points)
        {
            sequence_points[i] = 0;
        }
        // default sequence is [1]
        // default delay is 2
        foreach (Spawn s in level.spawns)
        {
            yield return SpawnEnemy(s); // make more specific later
        }





        // *** WAVE CHANGE LOGIC *** //

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0); 

        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        if (level.name == "Endless")
        {
            NextWave();
        }
        else if (wave < level.waves)
        {
            NextWave();
        }
        else
        {
            // restart
            GameManager.Instance.state = GameManager.GameState.GAMEOVER; // maybe remove this?
        }
    }

    IEnumerator SpawnEnemy(Spawn spawn) // changed from SpawnZombie()
    {
        
        Enemy enemy_data = enemies.Find(e => e.name == spawn.name);

        // parse spawn.location string
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)]; 
        // spawn_point.kind = SpawnName.RED/GREEN/BONE
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        
        
        
        // *** WE DON'T TOUCH *** // 
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemy_data.sprite); // * I did touch this
        
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        
        
        
        
        // *** WE DO TOUCH THIS *** //
        dict["base"] = enemy_data.hp;
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy); // change to RPN evaluation
        
        dict["base"] = enemy_data.damage;
        en.damage = enemy_data.damage; // change to RPN evaluation

        en.speed = enemy_data.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f); // change this to work with the delay
    }
}
