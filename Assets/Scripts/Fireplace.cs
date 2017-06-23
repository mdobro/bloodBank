using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireplace : MonoBehaviour {

    StateMachine animationMachine;
    public Sprite[] fireplace;

	// Use this for initialization
	void Start () {
        animationMachine = new StateMachine();
        animationMachine.ChangeState(new AI_PlayAnimation(GetComponent<SpriteRenderer>(), fireplace, 10));
	}
	
	// Update is called once per frame
	void Update () {
        animationMachine.Update();
		
	}
}
