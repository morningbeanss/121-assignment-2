using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using RPNEvaluator;
using System.Globalization;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    private Level level;
    private List<Level> levels; //list to hold all levels after reading them from json file
    private int wave = 0;
    private List<Enemy> enemies;
    private Dictionary<string, int> dict = new Dictionary<string, int>();
    
    //private RPNEvaluator.RPNEvaluator RPN; // dis doesn't work w the way my (claire) RPNevaluator is set up

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //start button below wuz part of the og code, commenting it out cuz i think it's unnecessary
        //GameObject selector = Instantiate(button, level_selector.transform);
        //selector.transform.localPosition = new Vector3(0, 130);
        //selector.GetComponent<MenuSelectorController>().spawner = this;
        //selector.GetComponent<MenuSelectorController>().SetLevel("Start");


        //adding buttons
        string level_json = Resources.Load<TextAsset>("levels").text; //read json file
        levels = JsonConvert.DeserializeObject<List<Level>>(level_json);

        int padding = 60; //space between each level button
        int startLoc = 60; //first button loc
        //list to hold all the button variables (so they can easily be set to inactive after level start)
        List<Button> buttons = new List<Button>(); 
        //instructions say buttons have to be "dynamically" spawned so foreach loop
        //(if a new lvl is added to the json, this file shouldn't have to be edited for there to be a button for it)
        foreach (Level l in levels)
        {
            //set up button
            GameObject lvlButt = Instantiate(button, level_selector.transform);
            lvlButt.transform.localPosition = new Vector3(0, startLoc);
            lvlButt.GetComponent<MenuSelectorController>().spawner = this;
            lvlButt.GetComponent<MenuSelectorController>().SetLevel(l.name);

            //set up location for next button
            startLoc -= padding;

        }

        
        dict.Add("wave", wave);

        string enemies_json = Resources.Load<TextAsset>("enemies").text;
        enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemies_json);
        //Debug.Log("enemies: " + enemies);
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

        //string level_json = Resources.Load<TextAsset>("levels").text; // added by calvin
        //level = JsonConvert.DeserializeObject<List<Level>>(level_json).Find(l => l.name == levelname); // added by calvin
        foreach (Level l in levels)
        {
            if (l.name == levelname)
            {
                level = l;
            }
        }
        //Debug.Log("Level: " + level);
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
            //Debug.Log("Spawn name: " + s.enemy);
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
        
        //Enemy enemy_data = enemies.Find(e => e.name == spawn.name);
        Enemy enemy_data = null;
        foreach (Enemy e in enemies)
        {
            if (e.name == spawn.enemy)
            {
                enemy_data = e;
            }
        }
        //Debug.Log("enemy data: " + enemy_data); 

        // parse spawn.location string
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)]; 
        // spawn_point.kind = SpawnName.RED/GREEN/BONE
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        
        
        
        // *** WE DON'T TOUCH *** // 
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        ////Debug.Log("enemy sprite#: " + enemy_data.sprite);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0); // flagged for error
        
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        
        
         
        
        // *** WE DO TOUCH THIS *** //
        
        dict["base"] = enemy_data.hp;
        if (string.IsNullOrEmpty(spawn.hp)) {
			// EnemyController.hp is a "Hittable", not a simple int	
			en.hp = new Hittable(enemy_data.hp, Hittable.Team.MONSTERS, new_enemy);
		}
		else {
            //ik the line below is long to call RPNEvaluator, but idk, thats the only way ik how to make it work
			int hp_value = RPNEvaluator.RPNEvaluator.Evaluate(spawn.hp, dict);
			en.hp = new Hittable(hp_value, Hittable.Team.MONSTERS, new_enemy);	
		}
        
        dict["base"] = enemy_data.damage;
       	if (string.IsNullOrEmpty(spawn.damage)) {
			
			en.damage = enemy_data.damage;
		} 
		else {
            //the line below, it was an Evaluatef line, but that had errors
            //!! might need to be changed back idk !!!
			en.damage = RPNEvaluator.RPNEvaluator.Evaluate(spawn.damage, dict);
		}

		dict["base"] = enemy_data.speed;
		if (string.IsNullOrEmpty(spawn.speed)) {
			// speed inside of enemy controller is an int
			en.speed = enemy_data.speed;
		}
		else {
			en.speed = RPNEvaluator.RPNEvaluator.Evaluate(spawn.speed, dict);
		}

        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f); // change this to work with the delay
    }

	IEnumerator SpawnEnemies() {

        // "spawns all enemies of one type" - Markus Eger via Discord
        return null;
	}
}
