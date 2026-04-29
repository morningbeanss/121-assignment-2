using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class GameManager 
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER
    }
    public GameState state;

    public int countdown;
    private static GameManager theInstance;
    public static GameManager Instance {  get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    public GameObject player;
    
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;

    private List<GameObject> enemies;
    public int enemy_count { get { return enemies.Count; } }

    //i (clr) added this one so that i can call it in GameManager & use it for wave/gameEnd stats
    public int total_enemies_killed = 0;

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);

        //every time an enemy is removed (killed by player), increment player's kill count
        //but only do this if player actually killed them;
        //if player is dead, the machine is wiping them not the player so no increment
        if (player.GetComponent<PlayerController>().hp.hp > 0)
        {
            total_enemies_killed++;
        }
    }

    //this function i (clr) added so that when the game ends due to player death, all the enemies left in the wave are wiped
    public void KillAllRemainingEnemies() 
    {
        //creating a copy of the og enemies list so if its still iterating thru it wont freak out and break
        List<GameObject> enemiesToKill = new List<GameObject>(enemies);
        //go thru each and wipe 1 by 1
        foreach(GameObject enemy in enemiesToKill)
        {
            if (enemy != null)
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null && !enemyController.dead)
                {
                    enemyController.Die(); //directly call death method to avoid kill counter being incremented
                }
                else if (enemyController == null)
                {
                    UnityEngine.Object.Destroy(enemy); //fallback just in case smth weird
                }
            }
        }
        enemies.Clear(); //clear list after done destroying all of them
    }

    //this function i (clr) added to make game resetting easier
    public void Reset()
    {
        total_enemies_killed = 0; //reset kill counter
        enemies.Clear(); //totally clear list (in case it wasn't cleared in KillAllRemainingEnemies)
        state = GameState.PREGAME; //update to correct game state
        countdown = 0; 
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a,b) => (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        enemies = new List<GameObject>();
    }
}
