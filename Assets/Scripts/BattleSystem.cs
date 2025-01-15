using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static Cinemachine.DocumentationSortingAttribute;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState {Start, Selection, Battle, Won, Lost, Run, Healing}

    [Header("Battle State")]
    [SerializeField] private BattleState state;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomTextPopUp;
    [SerializeField] private TextMeshProUGUI bottomText;

    public static BattleSystem instance;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPlayer;

    private const string ACTION_MESSAGE = "'s Actions:";
    private const int TURN_DURATION = 2;
    private const string WIN_MESSAGE = "Your party won the battle";
    private const string LOSE_MESSAGE = "Your party has been defeated";
    private const string OVERWORLD_SCENE = "OverworldScene";
    private const string BATTLE_SCENE_1 = "BattleScene";
    private const string GAMEOVER_SCENE = "GameOver";
    private const string OVERWORLD3_SCENE = "OverworldScene3";
    private const string BATTLE_SCENE_2 = "BattleScene2";
    private const string BATTLE_SCENE_3 = "BattleScene3";
    private const int RUN_CHANCE = 50;
    private const string SUCCESSFULLY_RAN_MESSAGE = "Your party ran away";
    private const string UNSUCCESSFULLY_RAN_MESSAGE = "Your party failed to run away";
    private const float LEVEL_MODIFIER = 0.5f;
    private const string LEVELUP_MESSAGE = "Your party leveled up";

    private const string TESTMAP_SCENE = "testmap";


    void Start()
    {
        //tim kiem cac doi tuong co kieu tuong ung de dem
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false); //enemy select menu disable
        state = BattleState.Battle; //change state to battle
        bottomTextPopUp.SetActive(true);//enable our bottom text

        //loop through all our battlers
        //do their approriate action
      
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if(state == BattleState.Battle && allBattlers[i].CurrHealth > 0)
            {
                switch (allBattlers[i].BattleAction)
                {
                   case BattleEntities.Action.Attack:
                        yield return StartCoroutine(AttackRoutine(i));
                        break;
                   case BattleEntities.Action.Run:
                        yield return StartCoroutine(RunRoutine());
                        break;
                    case BattleEntities.Action.Heal:
                        yield return StartCoroutine(HealRoutine(i));
                        break;
                    default:
                        Debug.Log("Error");
                        break;
                }
            }
        }
        

        RemoveDeadBattlers();


        if(state == BattleState.Battle)
        {
            bottomTextPopUp.SetActive(false);
            currentPlayer = 0;
            ShowBattleMenu(); 
        }

        yield return null;
        //if we not won or lost, repeat the loop by opening the battle menu
    }

    private IEnumerator AttackRoutine(int i) 
    {
        //players turn
        if (allBattlers[i].IsPlayer) 
        {
            BattleEntities currAttacker = allBattlers[i];
            if (allBattlers[currAttacker.Target].CurrHealth <= 0)
            {
                currAttacker.SetTarget(GetRandomEnemy());
            }
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            AttackAction(currAttacker, currTarget); //attack selected enemy
            AudioManager.instance.PlaySfx(0);
            yield return new WaitForSeconds(TURN_DURATION); //wait a few seconds
            //kill the enemy
            if (currTarget.CurrHealth <= 0) 
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION); //wait a few seconds
                enemyBattlers.Remove(currTarget);

                if(enemyBattlers.Count <= 0)
                {
                    state = BattleState.Won;
                    bottomText.text = WIN_MESSAGE;
                    for ( i = 0; i < allBattlers.Count; i++)
                    {
                        if (allBattlers[i].IsPlayer && allBattlers[i].Name == "John")
                        {
                            LevelUp();
                            yield return new WaitForSeconds(TURN_DURATION); //wait a few seconds
                            bottomText.text = LEVELUP_MESSAGE;
                        }
                    }
                    yield return new WaitForSeconds(TURN_DURATION); //wait a few seconds
                    if(SceneManager.GetActiveScene().name == BATTLE_SCENE_1)
                    {
                        SceneManager.LoadScene(OVERWORLD_SCENE);
                       
                    }
                    if (SceneManager.GetActiveScene().name == BATTLE_SCENE_2)
                    {
                        SceneManager.LoadScene(TESTMAP_SCENE);
                       
                    }
                    if (SceneManager.GetActiveScene().name == BATTLE_SCENE_3)
                    {
                        SceneManager.LoadScene(OVERWORLD3_SCENE);
                       
                    }
                }
            }
        }

        //enemy turns
        if (i < allBattlers.Count && allBattlers[i].IsPlayer == false) 
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTarget(GetRandomPartyMember());  //get random party member(target)
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            
            AttackAction(currAttacker, currTarget);   //attack the selected party member
            AudioManager.instance.PlaySfx(1);
            yield return new WaitForSeconds(TURN_DURATION);    //wait a few seconds

            if(currTarget.CurrHealth <= 0)
            {
                //kill the party member
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);  //wait a few seconds
                playerBattlers.Remove(currTarget);
            }

            if (playerBattlers.Count <= 0 || playerBattlers[0].CurrHealth == 0)  //if no party member remain
            {
                //we lost
                state = BattleState.Lost;
                bottomText.text = LOSE_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION); //wait a few seconds
                
                SceneManager.LoadScene(GAMEOVER_SCENE);    
            }
        }    
    }


    private IEnumerator RunRoutine() 
    {
        if (state == BattleState.Battle)
        {
            if(Random.Range(1,101) >= RUN_CHANCE)
            {
               //we have ran away
                bottomText.text = SUCCESSFULLY_RAN_MESSAGE; //set our bottom text to tell us we ran away
                state = BattleState.Run;//set our battle state to run
                allBattlers.Clear();//clear our battlers list
                yield return new WaitForSeconds(TURN_DURATION);//wait a few second
                if (SceneManager.GetActiveScene().name == BATTLE_SCENE_1)
                {
                    //DynamicGI.UpdateEnvironment();
                    SceneManager.LoadScene(OVERWORLD_SCENE);
                }
                if (SceneManager.GetActiveScene().name == BATTLE_SCENE_2)
                {
                    //DynamicGI.UpdateEnvironment();
                    SceneManager.LoadScene(TESTMAP_SCENE);
                }
                if (SceneManager.GetActiveScene().name == BATTLE_SCENE_3)
                {
                    //DynamicGI.UpdateEnvironment();
                    SceneManager.LoadScene(OVERWORLD3_SCENE);
                }
                yield break;
            }
            else
            {
                //we failed to run away
                bottomText.text = UNSUCCESSFULLY_RAN_MESSAGE; //set bottom text to say we failed
                yield return new WaitForSeconds(TURN_DURATION);//wait a few second
            }
        }
        
    }

    private IEnumerator HealRoutine(int i)
    {
        if (allBattlers[i].IsPlayer)
        {
            BattleEntities currAttacker = allBattlers[i];
            HealAction(currAttacker);
            AudioManager.instance.PlaySfx(2);
            state = BattleState.Healing;
            yield return new WaitForSeconds(TURN_DURATION);
        }
        state = BattleState.Battle;
    }

    private void RemoveDeadBattlers()
    {
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].CurrHealth <= 0)
            {
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void LevelUp()
    {
        if(SceneManager.GetActiveScene().name == BATTLE_SCENE_1)
        {
            List<PartyMember> currentParty = new List<PartyMember>();
            currentParty = partyManager.GetAliveParty();

            for (int i = 0; i < currentParty.Count; i++)
            {
                currentParty[i].Level++;
                float levelModifier = (LEVEL_MODIFIER * currentParty[i].Level);
                currentParty[i].CurrHealth += Mathf.RoundToInt(3 + (1 * levelModifier));
                currentParty[i].CurrHealth = Mathf.Min(currentParty[i].CurrHealth, currentParty[i].MaxHealth);
                currentParty[i].MaxHealth += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Strength += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Initiative += Mathf.RoundToInt(1 * levelModifier);
            }
        }

        if (SceneManager.GetActiveScene().name == BATTLE_SCENE_3)
        {
            List<PartyMember> currentParty = new List<PartyMember>();
            currentParty = partyManager.GetAliveParty();
     
            for (int i = 0; i < currentParty.Count; i++)
            {
                currentParty[i].Level += 1;
                float levelModifier = (LEVEL_MODIFIER * currentParty[i].Level);
                currentParty[i].CurrHealth += Mathf.RoundToInt(6 + (1 * levelModifier));
                currentParty[i].CurrHealth = Mathf.Min(currentParty[i].CurrHealth, currentParty[i].MaxHealth);
                currentParty[i].MaxHealth += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Strength += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Initiative += Mathf.RoundToInt(1 * levelModifier);
            }
        }

        if (SceneManager.GetActiveScene().name == BATTLE_SCENE_2)
        {
            List<PartyMember> currentParty = new List<PartyMember>();
            currentParty = partyManager.GetAliveParty();

            for (int i = 0; i < currentParty.Count; i++)
            {
                currentParty[i].Level += 3;
                float levelModifier = (LEVEL_MODIFIER * currentParty[i].Level);
                currentParty[i].CurrHealth += Mathf.RoundToInt(9 + (1 * levelModifier));
                currentParty[i].CurrHealth = Mathf.Min(currentParty[i].CurrHealth, currentParty[i].MaxHealth);
                currentParty[i].MaxHealth += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Strength += Mathf.RoundToInt(1 * levelModifier);
                currentParty[i].Initiative += Mathf.RoundToInt(1 * levelModifier);
            }
        }
    }
    private void CreatePartyEntities()
    {
        //lay vao list current party o PartyManager
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();
        //tao battle entities cho cac party members
        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            //gan cac gia tri
            tempEntity.SetEntityValues(currentParty[i].MemberName, currentParty[i].CurrHealth, currentParty[i].MaxHealth, 
                currentParty[i].Initiative, currentParty[i].Strength , currentParty[i].Level, true);

            //tao ra clone cua doi tuong duoc gan (la prefabs)
            //tai vi tri tuong ung(spawnpoint) voi phep quay mac dinh sau do gan no vao bien dc tao
            BattleVisuals tempBattleVisuals = Instantiate(currentParty[i].MemberBattleVisualPrefab, 
            partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentParty[i].CurrHealth, currentParty[i].MaxHealth, currentParty[i].Level);

            //lien ket doi tuong he thong cho doi tuong hien thi
            tempEntity.BattleVisuals = tempBattleVisuals;


            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }


    private void CreateEnemyEntities()
    {
        //lay list current enemy o EnemyManager
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();
        //tao battle entities cho cac enemy
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            //gan cac gia tri
            tempEntity.SetEntityValues(currentEnemies[i].EnemyName, currentEnemies[i].CurrHealth, currentEnemies[i].MaxHealth, 
                currentEnemies[i].Initiative, currentEnemies[i].Strength, currentEnemies[i].Level, false);

            //tao ra clone cua doi tuong duoc gan (la prefabs)
            //tai vi tri tuong ung(spawnpoint) voi phep quay mac dinh sau do gan no vao bien dc tao
            BattleVisuals tempBattleVisuals = Instantiate(currentEnemies[i].EnemyVisualPrefab,
            enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisuals.SetStartingValues(currentEnemies[i].MaxHealth, currentEnemies[i].MaxHealth, currentEnemies[i].Level);

            //lien ket doi tuong he thong cho doi tuong hien thi
            tempEntity.BattleVisuals = tempBattleVisuals;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
         }
    }

    public void ShowBattleMenu()
    {
        //who action
        actionText.text = playerBattlers[currentPlayer].Name + ACTION_MESSAGE;
        //enable battlemenu
        battleMenu.SetActive(true);
    }

    public void ShowEnemySelectionMenu()
    {
        //disable battlemenu
        battleMenu.SetActive(false);
        //set enemy selction buttons
        SetEnemySelectButtons();
        //enable seclection menu
        enemySelectionMenu.SetActive(true);
    }
    
    private void SetEnemySelectButtons()
    {
        //disable all buttons
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }
        //enable button for each enemy
        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            //change button text
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }
    }

    public void SelectEnemy(int currentEnemy)
    {
        //setting the current member and target
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));
        //tell the system this member intends to attack
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;
        //increment through our party members
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count) //if all players have slected an action
        {
            //start the battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false); //show the battle menu for the next player
            ShowBattleMenu();
        }
    }

    public void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        int damage = currAttacker.Strength;//get damage
        currAttacker.BattleVisuals.PlayAttackAnimation();//play the attack anim
        currTarget.CurrHealth -= damage;//dealing the damage
        currTarget.BattleVisuals.PlayHitAnimation();//play hit anim
        currTarget.UpdateUI();//update the UI
        bottomText.text = string.Format("{0} attacks {1} for {2} damage", currAttacker.Name, currTarget.Name, damage);
        SaveHealth();
    }

    public void HealAction(BattleEntities currAttacker)
    {
        float levelModifier = (LEVEL_MODIFIER * currAttacker.Level);
        currAttacker.CurrHealth += Mathf.RoundToInt(3 + (1 * levelModifier));
        currAttacker.CurrHealth = Mathf.Min(currAttacker.CurrHealth, currAttacker.MaxHealth);
        currAttacker.UpdateUI();
        bottomText.text = string.Format("{0} has healed!", currAttacker.Name);
        SaveHealth();
    }

    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>(); //create a temp list of type int (index)
        //find all the party members -> add them to our list
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrHealth > 0)//we have party member
            {
                partyMembers.Add(i);
            }
        }
        return partyMembers[Random.Range(0,partyMembers.Count)];//random a party member
    }

    private int GetRandomEnemy()
    {
        List<int> enemies = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == false && allBattlers[i].CurrHealth > 0)
            {
                enemies.Add(i);
            }
        }
        return enemies[Random.Range(0,enemies.Count)];
    }

    private void SaveHealth()
    {
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrHealth);
        }
    }

    private void DetermineBattleOrder()
    {
        allBattlers.Sort((bi1, bi2) => -bi1.Initiative.CompareTo(bi2.Initiative)); //sort list by initiative in ascending order
    }

    public void SelectRunAction()
    {
        state = BattleState.Selection;
        //setting the current member and target
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        //tell the system this member intends to run
        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;
        battleMenu.SetActive(false);
        //increment through our party members
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count) //if all players have slected an action
        {
            //start the battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false); //show the battle menu for the next player
            ShowBattleMenu();
        }
    }

    public void SelectHealAction()
    {
        state = BattleState.Selection;
        BattleEntities currentPlayerEntity = playerBattlers[currentPlayer];
        //tell the system this member intends to run
        currentPlayerEntity.BattleAction = BattleEntities.Action.Heal;
        battleMenu.SetActive(false);
        //increment through our party members
        currentPlayer++;

        if (currentPlayer >= playerBattlers.Count) //if all players have slected an action
        {
            //start the battle
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false); //show the battle menu for the next player
            ShowBattleMenu();
        }
    }
}

[System.Serializable]
public class BattleEntities
{
    //thiet lap cac state
    public enum Action {Attack, Run, Heal};
    public Action BattleAction;

    public string Name;
    public int CurrHealth;
    public int MaxHealth;
    public int Initiative;
    public int Strength;
    public int Level;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public int Target;

    

    public void SetEntityValues(string name, int currHealth, int maxHealth, int initiative, int strength, int level, bool isPlayer)
    {
        Name = name;
        CurrHealth = currHealth;
        MaxHealth = maxHealth;
        Initiative = initiative;
        Strength = strength;
        Level = level;
        IsPlayer = isPlayer;
    }

    public void SetTarget(int target) 
    { 
        Target = target; 
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrHealth);
    }
    public void UpdateUILevel()
    {
        BattleVisuals.ChangeLevel(Level);
    }
}
