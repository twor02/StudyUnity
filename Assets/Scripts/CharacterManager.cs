using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject joinPopup;
    [SerializeField] private GameObject TeleMenu;
    [SerializeField] private TextMeshProUGUI joinPopupText;

    private bool infrontOfPartyMember;
    private GameObject joinableMember;
    private GameObject goNextWorld;
    private GameObject goBackWorld;
    private PlayerControls playerControls;
    private PlayerController playerController;
    private PartyManager partyManager;
    private List<GameObject> overworldCharacters = new List<GameObject>();

    private const string NPC_JOINABLE_TAG = "NPCJoinable";
    private const string ORIGINALWORLD_TAG = "OriginalWorld";
    private const string NEWWORLD_TAG = "NewWorld";
    private const string PARTY_JOINED_MESSAGE = " Joined The Party!";
    private const string OVERWORLD3_SCENE = "OverworldScene3";
    private const string OVERWORLD_SCENE = "OverworldScene";
    private const string TESTMAP_SCENE = "testmap";





    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        //dang ki su kien Interact trong Input Action khi bam E se thuc hien Interact()
        playerControls.Player.Interact.performed += _ => Interact();
        SpawnOverworldMembers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Interact()
    {
        if (infrontOfPartyMember == true && joinableMember != null)
        {
            //add member
            MemberJoined(joinableMember.GetComponent<JoinableCharacterScript>().MemberToJoin);
            infrontOfPartyMember = false;
            joinableMember = null;
        }

        if(goNextWorld != null)
        {
            TeleMenu.SetActive(true);
            goNextWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(false);
            //SceneManager.LoadScene(OVERWORLD2_SCENE);


        }
        if (goBackWorld != null)
        {

            TeleMenu.SetActive(true);
            goBackWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(false);
            //SceneManager.LoadScene(OVERWORLD_SCENE);

        }
    }

    public void LoadWorld_1()
    {
        SceneManager.LoadScene(TESTMAP_SCENE);
        TeleMenu.SetActive(false);
    }
    public void LoadWorld_2()
    {
        SceneManager.LoadScene(OVERWORLD3_SCENE);
        TeleMenu.SetActive(false);
    }
    public void BackWorld()
    {
        SceneManager.LoadScene(OVERWORLD_SCENE);
        TeleMenu.SetActive(false);
    }



    private void MemberJoined(PartyMemberInfo partyMember)
    {
        GameObject.FindFirstObjectByType<PartyManager>().AddMemberToPartyByName(partyMember.MemberName);//add party membere
        joinableMember.GetComponent<JoinableCharacterScript>().CheckcIfJoined();//disable joinable member
        joinPopup.SetActive(true);
        joinPopupText.text = partyMember.MemberName + PARTY_JOINED_MESSAGE;//join pop up
        SpawnOverworldMembers();//adding an overworld member
    }

    private void SpawnOverworldMembers()
    {
        for (int i = 0; i < overworldCharacters.Count; i++)
        {
            Destroy(overworldCharacters[i]);
        }
        overworldCharacters.Clear();

        List<PartyMember> currentParty = GameObject.FindFirstObjectByType<PartyManager>().GetCurrentParty();
        
        for (int i = 0; i < currentParty.Count; i++)
        {
            if(i == 0) //first member will be player
            {
                GameObject player = gameObject; //get the player (in Unity)
                GameObject playerVisual = Instantiate(currentParty[i].MemberOverworldVisualPrefab, 
                    player.transform.position, Quaternion.identity); //spawn the member visual

                playerVisual.transform.SetParent(player.transform);//thiet lap vi tri cho visual

                player.GetComponent<PlayerController>().SetOverworldVisuals(playerVisual.GetComponent<Animator>(),
                    playerVisual.GetComponent<SpriteRenderer>()); //assign the player controller values
                playerVisual.GetComponent<MemberFollowAI>().enabled = false;
                overworldCharacters.Add(playerVisual);
            }
            else //any member will be follower
            {
                //thiet lap vi tri spawn cho follower
                Vector3 positionToSpawn = transform.position;
                positionToSpawn.x -= 1;

                //spawn visual cho follower
                GameObject tempFollower = Instantiate(currentParty[i].MemberOverworldVisualPrefab,
                    positionToSpawn, Quaternion.identity);

                tempFollower.GetComponent<MemberFollowAI>().SetFollowDistance(i); //set follow AI setting
                overworldCharacters.Add(tempFollower);
            }
        }
    }

    //kiem tra di vao collider cua gameobject
    private void OnTriggerEnter(Collider other)
    {
        //kiem tra tag
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            //enable our prompt
            infrontOfPartyMember = true;
            joinableMember = other.gameObject;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(true);
        }
        if(other.gameObject.tag == ORIGINALWORLD_TAG)
        {
            goNextWorld = other.gameObject;
            goNextWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(true);
        }
        if (other.gameObject.tag == NEWWORLD_TAG)
        {
            goBackWorld = other.gameObject;
            goBackWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(true);
        }

    }

    //kiem tra roi khoi collider cua gameobject
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            //disable our prompt
            infrontOfPartyMember = false;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(false);
            joinableMember = null;
        }
        if (other.gameObject.tag == ORIGINALWORLD_TAG)
        {
            goNextWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(false);
            TeleMenu.SetActive(false);
            goNextWorld = null;
        }
        if (other.gameObject.tag == NEWWORLD_TAG)
        {
            goBackWorld.GetComponent<ToAnotherWorld>().ShowInteractPrompt(false);
            TeleMenu.SetActive(false);
            goBackWorld = null;
        }
    }
}
