using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Enemy")] //tao ra option ms trong menu khi chon create
public class EnemyInfo : ScriptableObject
{
    //ScriptableObject cho phep luu tru du lieu va chia se du lieu giua cac doi tuong khac nhau
    public string EnemyName;
    public int BaseHealth;
    public int BaseStr;
    public int BaseInitiative;
    public GameObject EnemyVisualPrefab; //xuat hien o battle scene
}
