using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private PartyManager partyManager;

    private int currHealth;
    private int maxHealth;
    private int level;

    private Animator anim;

    private const string LEVEL_ABB = "Lvl: ";
    private const string IS_ATTACK_PARAM = "IsAttack";
    private const string IS_HIT_PARAM = "IsHit";
    private const string IS_DEAD_PARAM = "IsDead";

    // Start is called before the first frame update
    void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    public void SetStartingValues(int currHealth, int maxHealth, int level)
    {
        this.currHealth = currHealth;
        this.maxHealth = maxHealth; 
        this.level = level;
        levelText.text = LEVEL_ABB + this.level.ToString();
        UpdateHealthBar();
    }

    //public void SetNewLevel(int level)
    //{
    //    this.level = level;
    //    levelText.text = LEVEL_ABB + this.level.ToString();
    //}

    
    public void ChangeHealth(int currHealth) 
    { 
        this.currHealth = currHealth;
        //if hp = 0->play death anim->destroy battle visual
        if (currHealth <= 0) 
        { 
            PlayDeadAnimation();
            Destroy(gameObject, 1f);
        }
        UpdateHealthBar();
    }

    public void ChangeLevel(int currLevel) 
    { 
        this.level = currLevel;
        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        levelText.text = LEVEL_ABB + this.level.ToString();
    }

    private void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currHealth;
    }

    public void PlayAttackAnimation()
    {
        anim.SetTrigger(IS_ATTACK_PARAM);
    }
    public void PlayHitAnimation()
    {
        anim.SetTrigger(IS_HIT_PARAM);
    }
    public void PlayDeadAnimation()
    {
        anim.SetTrigger(IS_DEAD_PARAM);
    }
}
