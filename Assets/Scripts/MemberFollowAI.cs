using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemberFollowAI : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private int speed;

    private float followDist;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private const string IS_WALK_PARAM = "IsWalk";
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        followTarget = GameObject.FindFirstObjectByType<PlayerController>().transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, followTarget.position) > followDist)
        {
            //walk toward player
            anim.SetBool(IS_WALK_PARAM, true);
            float step = Time.deltaTime * (speed + 4);
            transform.position = Vector3.MoveTowards(transform.position, followTarget.position, step);

            if(followTarget.position.x - transform.position.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            //stop walking/ return to idle
            anim.SetBool(IS_WALK_PARAM, false);
        }

    }

    public void SetFollowDistance(float followDistance)
    {
        followDist = followDistance;
    }
}
