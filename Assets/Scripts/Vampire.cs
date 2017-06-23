using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vampire : MonoBehaviour {
    public static Vampire S;
    public Sprite[] move;
    public Sprite[] toBat;
    public Sprite[] bat;
    public Sprite[] attack;
    public Sprite[] death;
    public float walking_velocity;
    public float flying_velocity;
    public float attack_distance;
    public int continuous_damage;
    public int damage_taken;
    public int health_restored;
    public float damageCooldown;
    public int batRegen;
    public int batDeplete;
    public int totalBatPoints;
    public float wolfCooldown;
    public Slider batSlider;
    public Slider wolfSlider;

    public bool ________________________________;

    public StateMachine animation_state_machine;
    public StateMachine movement_state_machine;

    private float _hitCooldownTimer;
    private float _damageCooldownTimer;
    public float _wolfCooldownTimer = 0;
    private float _batPowerTimer = 0;
    private int _batPoints;
    public int batPoints {
        get {
            return _batPoints;
        }
        set {
            if (value > totalBatPoints) {
                _batPoints = totalBatPoints;
            } else {
                _batPoints = value;
            }
        }
    }

    private bool isDead = false;

    private bool inBatState = false;
    public bool isBat {
        get {
            return inBatState;
        }
        set {
            inBatState = value;
            if (inBatState) {
                gameObject.layer = LayerMask.NameToLayer("Bat");
            } else {
                gameObject.layer = LayerMask.NameToLayer("Dracula");
            }
        }
    }

    public int _health = 100;
    public int  Health {
        get {
            return _health;
        }
        set {
            if (_health - value > continuous_damage && value > 0) {
                StartCoroutine(Flasher());
            }

            if (value > 100) {
                _health = 100;
            } else {
                _health = value;
            }

            if (value <= 0 && !isDead) {
                isDead = true;
                KillVampire();
            }

            MainGameController.controller.healthSlider.value = _health;
        }
    }

    IEnumerator Flasher() {
        for (int i = 0; i < 5; i++) {
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(.1f);
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(.1f);
        }
    }

    // Use this for initialization
    void Start () {
        S = this;

        animation_state_machine = new StateMachine();
        animation_state_machine.ChangeState(new StateIdleWithSprite(this, GetComponent<SpriteRenderer>(), move[0]));
        movement_state_machine = new StateMachine();
        movement_state_machine.ChangeState(new StateVampireNormalMovement(this));

        InvokeRepeating("PassiveDamage", 0, 1f);

        batPoints = totalBatPoints;
            
	}
    
    // FixedUpdate is called once per physics engine update	
    void FixedUpdate() {
        movement_state_machine.Update();

        batSlider.value = (float)batPoints / totalBatPoints;
        _batPowerTimer += Time.fixedDeltaTime;
        if (_batPowerTimer > 1f) {
            _batPowerTimer = 0;
            if (isBat) {
                batPoints -= batDeplete;
            } else {
                batPoints += batRegen;
            }
            if (batPoints <= 0) {
                batPoints = 0;
                animation_state_machine.ChangeState(new StateVampireMetamorphosis(this, GetComponent<SpriteRenderer>(), toBat, 10, !isBat));
                isBat = !isBat;
            }
        }

        _wolfCooldownTimer += Time.fixedDeltaTime;
        wolfSlider.value = (_wolfCooldownTimer < wolfCooldown) ? _wolfCooldownTimer / wolfCooldown : 1;
        
        if (_wolfCooldownTimer > wolfCooldown) {
            wolfSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.green;
        }
        else {
            wolfSlider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = Color.red;
        }
    }

	// Update is called once per frame
	void Update () {
        animation_state_machine.Update();

        _hitCooldownTimer += Time.deltaTime;
	}

    void PassiveDamage() {
        Health -= continuous_damage;
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Hunter") {
            //damage vamp
            if (!col.gameObject.GetComponent<Hunter>().hiding && _hitCooldownTimer >= damageCooldown) {
                Health -= damage_taken;
                _hitCooldownTimer = 0;
            }
        }
    }

    void OnTriggerStay(Collider col) {
        if (col.gameObject.tag == "Hunter") {
            _damageCooldownTimer += Time.deltaTime;
            if (_damageCooldownTimer >= damageCooldown && !col.gameObject.GetComponent<Hunter>().hiding) {
                Health -= damage_taken;
                _damageCooldownTimer = 0;
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Hunter") {
            _damageCooldownTimer = 0;
        } 
    }

    public void Attack() {
        //RayCast to look for patient (-y direction)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, attack_distance, 1 << 10)) {
            //patient in front, kill him!
            Patient patient = hit.collider.gameObject.GetComponent<Patient>();
            if (!patient.walking) {
                //only count a bit if the patient is laying in bed
                patient.Bitten();
                MainGameController.controller.score += 1;
                Health += health_restored;
            }
        } 
    }

    public void KillVampire() {
        //kill vampire
        //stop movement
        movement_state_machine.ChangeState(new StateVampireStunned());
        animation_state_machine.ChangeState(new StateVampireDeath(this, death, 5));
    }
}
