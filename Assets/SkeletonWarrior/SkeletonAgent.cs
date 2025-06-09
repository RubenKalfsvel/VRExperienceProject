using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class SkeletonAgent : Agent, IDamageable
{
    [Header("Agent Stats")]
    public float maxHealth = 3f;
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;
    public float attackCooldown = 2f;

    [Header("Rewards")]
    public float hitOpponentReward = 1f;
    public float tookDamagePenalty = -0.2f;
    public float deathPenalty = -1.0f;
    public float killReward = 2.0f;
    public float missedAttackPenalty = -0.05f;
    public float distancePenalty = -0.02f;
    public float tooFarDistance = 20f;
    public float combatProximityReward = 0.01f;
    public float orientationReward = 0.005f;
    
    [Header("References")]
    private Animator animator;
    public GameObject weapon;
    public Transform opponentTransform;
    public ScoreManager scoreManager;

    private float currentHealth;
    private bool attackReady;
    private bool attackLanded;
    private bool isDead = false;

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        attackReady = true;
        currentHealth = maxHealth;
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
        opponentTransform = GameObject.FindWithTag("Player").transform;
    }

    public void ResetAgent()
    {
        isDead = false;
        currentHealth = maxHealth;
        transform.localPosition = GetRandomSpawnPosition();
        transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

        animator.applyRootMotion = false;
        animator.Rebind();
        animator.Update(0f);
        animator.SetFloat("speed", 0f);
        animator.ResetTrigger("damage");
        animator.ResetTrigger("attack");
        animator.ResetTrigger("die");
        animator.SetInteger("Death", 0);

        attackReady = true;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(currentHealth / maxHealth);
        sensor.AddObservation(attackReady ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isDead) return;
        float moveInput = actions.ContinuousActions[0];
        float rotateInput = actions.ContinuousActions[1];
        int combatAction = actions.DiscreteActions[0];

        float effectiveMoveInput = attackReady ? moveInput : 0f;
        float effectiveRotateInput = attackReady ? rotateInput : 0f;

        float speedMultiplier = effectiveMoveInput >= 0 ? 1f : 0.60f;
        transform.Translate(transform.forward * effectiveMoveInput * moveSpeed * speedMultiplier * Time.deltaTime, Space.World);
        transform.Rotate(transform.up, effectiveRotateInput * rotationSpeed * Time.deltaTime);
        animator.SetFloat("speed", effectiveMoveInput);

        if (combatAction == 1 && attackReady)
        {
            AttemptAttack();
            StartCoroutine(AttackCooldownCoroutine(attackCooldown));
        }

        if (opponentTransform != null)
        {
            float distance = Vector3.Distance(transform.position, opponentTransform.position);
            if (distance > tooFarDistance)
            {
                Debug.Log("Too far apart! Applied penalty and reset.");
                AddReward(distancePenalty);
                EndEpisode();
            }
            float combatDistance = 2.5f;
            if (distance < combatDistance)
            {
                AddReward(combatProximityReward);
            }

            Vector3 toOpponent = opponentTransform.position - transform.position;
            float forwardDot = Vector3.Dot(transform.forward, toOpponent.normalized);
            AddReward(forwardDot * orientationReward);
        }

        AddReward(-0.001f / MaxStep);
    }

    IEnumerator AttackCooldownCoroutine(float cooldown)
    {
        attackReady = false;
        yield return new WaitForSeconds(cooldown);
        attackReady = true;
    }

    void AttemptAttack()
    {
        attackLanded = false;
        animator.SetTrigger("attack");
        StartCoroutine(CheckAttackMissedAfterDelay(attackCooldown));
    }

    IEnumerator CheckAttackMissedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!attackLanded)
        {
            AddReward(missedAttackPenalty);
            Debug.Log("Missed attack! Applied penalty.");
        }
    }

    public void StartDealDamage()
    {
        if (isDead) return;
        weapon.GetComponentInChildren<EnemyDamageDealer>()?.StartDealDamage();
    }

    public void EndDealDamage()
    {
        if (isDead) return;
        weapon.GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
    }


    public void ReportSuccessfulHit()
    {
        attackLanded = true;
        Debug.Log("Skeleton hit player.");
        AddReward(hitOpponentReward);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        AddReward(tookDamagePenalty);
        Debug.Log($"{name} took {damage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("damage");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        transform.position -= new Vector3(0f, .5f, 0f);

        int deathType = Random.Range(1, 4);
        animator.SetInteger("Death", deathType);
        animator.applyRootMotion = false;
        AddReward(deathPenalty);
        scoreManager.IncreaseScore();

        StartCoroutine(DelayedEpisodeEnd(5f));
    }


    private IEnumerator DelayedEpisodeEnd(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndEpisode();
    }


    Vector3 GetRandomSpawnPosition()
    {
        float range = 8f;
        return new Vector3(Random.Range(-range, range), 0f, Random.Range(-range, range));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public void OpponentDied()
    {
        AddReward(killReward);
        EndEpisode();
    }
}