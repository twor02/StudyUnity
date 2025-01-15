using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "New Party Member")] //tao ra option ms trong menu khi chon create
public class PartyMemberInfo : ScriptableObject
{
    //ScriptableObject cho phep luu tru du lieu va chia se du lieu giua cac doi tuong khac nhau
    public string MemberName;
    public int StartingLevel;
    public int BaseHealth;
    public int BaseStr;
    public int BaseInitiative;
    public GameObject MemberBattleVisualPrefab;     //hien thi trong battle scene
    public GameObject MemberOverworldVisualPrefab;  //hien thi trong overworld scene

}
