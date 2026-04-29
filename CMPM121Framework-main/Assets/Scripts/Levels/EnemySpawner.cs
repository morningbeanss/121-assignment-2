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
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

    private Level level;
    private List<Level> levels; //list to hold all levels after reading them from json file
    private int wave;
    private List<Enemy> enemies;
    private Dictionary<string, int> dict = new Dictionary<string, int>();

    int activeSpawns;
    int wavesDone; //this n ones below used in SpawnWave as post-wave/post-game stats
    int playerHealth;
    int enemiesKilled;

    public TextMeshProUGUI wave_end_stats; //wave/endgame text stats

    public Button restartButton; //button for restarting the game after loss/win

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        //start button below wuz part of the og code, commenting it out cuz i think it's unnecessary
        //GameObject selector = Instantiate(button, level_selector.transform);
        //selector.transform.localPosition = new Vector3(0, 130);
        //selector.GetComponent<MenuSelectorController>().spawner = this;
        //selector.GetComponent<MenuSelectorController>().SetLevel("Start");

        

        wave = 0;

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

        
        dict["wave"] = wave;

        string enemies_json = Resources.Load<TextAsset>("enemies").text;
        enemies = JsonConvert.DeserializeObject<List<Enemy>>(enemies_json);
        //Debug.Log("enemies: " + enemies);

        //setting up a listener for the restartButton
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame); //if clicked, trigger RestartGame to run
        }
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
            //    Debug.Log("Waves: " + level.waves);
            }
        }
        //Debug.Log("Level: " + level);
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();

        StartCoroutine(SpawnWave());
        //Debug.Log("Starting wave");
        wave++; // added by calvin
    }

    public void NextWave()
    {
        wave++;
        wave_end_stats.text = "";
      //  Debug.Log("Next wave starting");
        StartCoroutine(SpawnWave());
    }

    //i (clr) wrote this helper function to restart the game,
    //like resetting all variables and getting level selector buttons to show up again
    public void RestartGame()
    {
        wave_end_stats.text = ""; //make sure its clear
        //reset all counters
        wave = 0;
        wavesDone = 0;
        activeSpawns = 0;
        enemiesKilled = 0;

        GameManager.Instance.Reset(); //function i wrote in game manager that resets some variables over there

        //reset player
        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.hp.hp = playerController.hp.max_hp;
            GameManager.Instance.player.transform.position = Vector3.zero; //put back to middle
            Unit playerUnit = GameManager.Instance.player.GetComponent<Unit>();
            if (playerUnit != null)
            {
                playerUnit.movement = Vector2.zero;
            }
        }

        //destroy original buttons, they will be reset when start() is re-called
        foreach (Transform button in level_selector.transform)
        {
            Destroy(button.gameObject);
        }

        Start(); //calling this shuld hopefully just make everything reset ok

        GameManager.Instance.state = GameManager.GameState.PREGAME; //reset game state
        level_selector.gameObject.SetActive(true); //set active so u can see the buttons
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

        activeSpawns = level.spawns.Count; // number of spawning events that are occuring
        
        // default sequence is [1]
        // default delay is 2


        foreach (Spawn s in level.spawns)
        {
            //Debug.Log("Spawn name: " + s.enemy +
            //"\nSpawn count");
            /*
            Without having written the real behavior, this currently
            Spawns one zombie, one skeleton, and one warlock
            */
            //Log("Spawning Enemies...\nEnemy type: " + s.enemy);
            StartCoroutine(SpawnEnemies(s)); // make more specific later
            
        }


        // *** WAVE CHANGE LOGIC *** //
        // Track the number of spawn coroutines,
        // so it will be yield return new WaitUntil(() => activeSpawns == 0 && GameManager.Instance.enemy_count == 0);
        yield return new WaitUntil(() => activeSpawns <= 0 && GameManager.Instance.enemy_count <= 0); 

        
        if (level.name == "Endless" || wave < level.waves)
        {
            
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            Debug.Log("WAVEEND");
        }
        else
        {
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
            Debug.Log("GAMEOVER triggered");
        }

        int maxHealth = GameManager.Instance.player.GetComponent<PlayerController>().hp.max_hp;
        //Debug.Log("WAVE OVER");
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            //make a button pop up to trigger next wave starting
            GameObject waveButt = Instantiate(button, level_selector.transform);
            waveButt.transform.localPosition = new Vector3(0, 0);
            waveButt.GetComponent<MenuSelectorController>().SetLevel("Next Wave");

            wavesDone = wave;
            playerHealth = GameManager.Instance.player.GetComponent<PlayerController>().hp.hp;
            enemiesKilled = GameManager.Instance.total_enemies_killed;
            wave_end_stats.text = $"Waves Completed: {wavesDone}\nHealth: {playerHealth} / {maxHealth}\nEnemies Killed: {enemiesKilled}";
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            //make sure variables are updated
            playerHealth = GameManager.Instance.player.GetComponent<PlayerController>().hp.hp;
            wavesDone = wave;
            enemiesKilled = GameManager.Instance.total_enemies_killed;
            if (playerHealth <= 0) //GAMEOVER by death case (as opposed to finishing the waves)
            {
                Debug.Log("GAMEOVER due to DEATH");
                //display loser text
                wave_end_stats.text = $"You Died!\nWaves Completed: {wavesDone}\nEnemies Killed: {enemiesKilled}";
                //kill off remaining enemies
                GameManager.Instance.KillAllRemainingEnemies();
            }
            else //only other reason game would be over is if they won
            {
                //display winner text
                wave_end_stats.text = $"You Won!\nWaves Completed: {wavesDone}\nFinal Health: {playerHealth} / {maxHealth}\nEnemies Killed: {enemiesKilled}";
            }
            //Debug.Log("GAMEOVER");

            //make a restart button
            GameObject restartButt = Instantiate(button, level_selector.transform);
            restartButt.transform.localPosition = new Vector3(0, 0);
            restartButt.GetComponent<MenuSelectorController>().SetLevel("Play Again"); //in menuSelectorController, if in GAMEOVER, button should trigger restart

            //GameManager.Instance.state = GameManager.GameState.PREGAME; //i dont think this is needed here bc i think resetGame deals w this?
        }
        if (GameManager.Instance.state != GameManager.GameState.GAMEOVER && GameManager.Instance.state != GameManager.GameState.WAVEEND)
        {
            wave_end_stats.text = ""; //wanna make sure this doesn't show up any other time
        }
    }

    void GameWaveOver() //adding this as a separate function so it can be called as needed; stuff was spawnWave() b4
    {

    }

    void SpawnEnemy(Spawn spawn) // changed from SpawnZombie()
    {
        
        Enemy enemy_data = enemies.Find(e => e.name == spawn.enemy); // this should probably work now
        
        ////Debug.Log("enemy data: " + enemy_data); 

        // parse spawn.location string
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)]; 
        
        string[] spawn_location = spawn.location.Split(' ');
        
        if (spawn_location.Length > 1)
        {
            switch (spawn_location[1])
        {
            case "red":
                spawn_point.kind = SpawnPoint.SpawnName.RED;
            break;
            case "bone":
                spawn_point.kind = SpawnPoint.SpawnName.BONE;
            break;
            case "green":
                spawn_point.kind = SpawnPoint.SpawnName.GREEN;
            break;
            default:
            break;
        }
        }
        
        // spawn_point.kind = SpawnName.RED/GREEN/BONE
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        // *** WE DON'T TOUCH *** // 
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        //Debug.Log("enemy sprite#: " + enemy_data.sprite);
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemy_data.sprite); // out of bounds error?
        
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
        //yield return new WaitForSeconds(0.5f); // change this to work with the delay
    }

	IEnumerator SpawnEnemies(Spawn s) {

        // "spawns all enemies of one type" - Markus Eger via Discord
        int spawned = 0;
        int spawn_total = RPNEvaluator.RPNEvaluator.Evaluate(s.count, dict);

        // this is safe because Spawn uses default values for these member variables
        List<int> sequence = s.sequence;
        sequence ??= new List<int>() { 1 };
        int delay = s.delay;
        //Debug.Log("Spawn total is " + spawn_total);

        //[1,2,3]
        int sequenceIndex = 0;
        string seq = "[";
        foreach (int a in sequence)
        {
            seq += a + " ";
        }
        seq += "]";
        //Debug.Log("Sequence = " + seq);
        while (spawned < spawn_total) 
        {

            // *** WHAT TO ADD ***
            // Claire can add the sequencing logic

            // moving through the numbers in sequence and changing number to spawn etc

            //int numToSpawn = sequence[0]; // *** I put this to avoid a compilation error, change this to whatever it needs to be 
            

            for (int i = 0; i < sequence[sequenceIndex]; i++)
            {
                //Debug.Log("Spawning " + sequence[sequenceIndex] + " many enemies");
                //Debug.Log("Enemy type: " + s.enemy);
                if (spawned < spawn_total)
                {
                    SpawnEnemy(s); // used to be yield return
                    spawned++;
                }
            }

            sequenceIndex++;
            if (sequenceIndex >= sequence.Count)
            {
                sequenceIndex = 0;
            }
            if (spawned < spawn_total)
            {
                yield return new WaitForSeconds(delay); // the delay between spawns 
            }
            
        }
        //Debug.Log("Finished Spawning " + s.enemy + " wave" +
        //    "\nSpawn total = " + spawn_total);
        activeSpawns--;
	}
}
