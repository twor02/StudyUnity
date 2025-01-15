using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private int stepsInGrass;
    [SerializeField] private int minStepsToEncounter;
    [SerializeField] private int maxStepsToEncounter;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;
    //private Vector3 initialPosition;
    //public Vector3 AISpawn;
    private bool movingInGrass;
    private float stepTimer;
    private int stepsToEncounter;
    private PartyManager partyManager;

    //phai goi dung ten bien duoc dat trong animator
    private const string IS_WALK_PARAM = "isWalk";
    private const string OVERWORLD_SCENE = "OverworldScene";
    private const string OVERWORLD3_SCENE = "OverworldScene3";
    private const string BATTLE_SCENE_1 = "BattleScene";
    private const string BATTLE_SCENE_2 = "BattleScene2";
    private const string BATTLE_SCENE_3 = "BattleScene3";
    //thoi gian de buoc 1 buoc
    private const float TIME_PER_STEP = 0.5f;

    private const string TESTMAP_SCENE = "testmap";


    private void Awake()
    {
        playerControls = new PlayerControls();
        CalculateStepsToNextEncounter();
    }
    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        //initialPosition = transform.position;
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();

        //neu co vi tri da luu thi dung
        if (partyManager.GetPosition() != Vector3.zero)
        {
            transform.position = partyManager.GetPosition();
            partyManager.SetPosition(Vector3.zero);// xoa vi tri da luu 
        }
        
    }

    

    // Update is called once per frame
    void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        //normalized de chuyen ve vector don vi(do dai = 1)
        movement = new Vector3(x, 0, z).normalized;
        anim.SetBool(IS_WALK_PARAM, movement != Vector3.zero);

        if (x != 0 && x < 0)
        {
            playerSprite.flipX = true;
        }
        if(x!= 0 && x > 0)
        {
            playerSprite.flipX = false;
        }

    }
    //ham fixedupdate dung de xu li vat li va chay theo thoi gian
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);

        //kiem tra va cham voi cac collider grasslayer tai vi tri cua player voi ban kinh bang 1
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1, grassLayer);
        // so luong va cham khac 0 va di chuyen khac 0 bien nhan gia tri true
        movingInGrass = colliders.Length != 0 && movement != Vector3.zero;
        if (movingInGrass == true)
        {
            //moi khi ham fixedupdate chay thi bien se tang len
            stepTimer += Time.fixedDeltaTime;
            //neu thoi gian buoc chan hien tai lon hon thoi gian buoc 1 buoc
            if(stepTimer > TIME_PER_STEP)
            {
                //tang so buoc chan trong co
                stepsInGrass++;
                stepTimer = 0;

                if (stepsInGrass >= stepsToEncounter && SceneManager.GetActiveScene().name == OVERWORLD_SCENE)
                {
                    partyManager.SetPosition(transform.position);
                    SceneManager.LoadScene(BATTLE_SCENE_1);
                    

                }
                if (stepsInGrass >= stepsToEncounter && SceneManager.GetActiveScene().name == TESTMAP_SCENE)
                {
                    partyManager.SetPosition(transform.position);
                    
                    SceneManager.LoadScene(BATTLE_SCENE_2);
                }

                if (stepsInGrass >= stepsToEncounter && SceneManager.GetActiveScene().name == OVERWORLD3_SCENE)
                {
                    partyManager.SetPosition(transform.position);

                    SceneManager.LoadScene(BATTLE_SCENE_3);
                }

                //kiem tra xem co gap encounter ko
                //neu gap encounter thi chuyen scene
            }
        }
    }

    //tinh toan step de encounter random trong khoang min va max
    private void CalculateStepsToNextEncounter()
    {
        stepsToEncounter = Random.Range(minStepsToEncounter, maxStepsToEncounter);
    }

    public void SetOverworldVisuals(Animator animator, SpriteRenderer spriteRenderer)
    {
        anim = animator;
        playerSprite = spriteRenderer;
    }
}
