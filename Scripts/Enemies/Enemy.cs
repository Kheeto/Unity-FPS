using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Combat")]
    [SerializeField] private EnemyType type;
    [SerializeField] private int health;
    [SerializeField] private EnemyGun gun;

    [Header("Path Finding")]
    [SerializeField] private LayerMask whatIsGround;
    private Transform player;
    private NavMeshAgent agent;
    private Vector3 playerDirection;

    [Header("Patroling")]
    [SerializeField] private bool enablePatrol;
    [SerializeField] private float newPointTime = 0f;
    [SerializeField] private float minWalkPointRange = 1f;
    [SerializeField] private float maxWalkPointRange = 5f;
    [SerializeField] private float patrolPointMinDistance = .1f;
    private Vector3 walkPoint;
    private bool walkPointSet;

    [Header("Chasing")]
    [SerializeField] private float sightRange;
    [SerializeField] private float chaseTimeAfterTargetLost = 3f;
    [SerializeField] private float lookSpeed = .5f;
    private float chaseTimer;
    private bool wasChasing;
    private bool timerWasActive;

    [Header("Attacking")]
    [SerializeField] private float attackRange;
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private bool canMoveWhileAttacking;
    private bool alreadyAttacked;
    private bool playerInAttackRange;
    private bool playerInSightRange;
    private bool canDirectlySeePlayer;

    [Header("Hitmarkers")]
    [SerializeField] private TMP_Text hitmarker;
    [SerializeField] private float duration;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;
    [SerializeField] private Vector3 minimumOffset;
    [SerializeField] private Vector3 minimumDistance;
    [SerializeField] private Vector3 maximumDistance;

    public enum EnemyType {
        Default,
        Boss
    }

    private void Awake()
    {
        player = FindObjectOfType<PlayerMovementAdvanced>().transform;
        agent = GetComponent<NavMeshAgent>();
        chaseTimer = 0;
        wasChasing = false;
        timerWasActive = false;
    }

    private void Update()
    {
        playerDirection = player.position - transform.position;

        if (chaseTimer > 0) // Chases the target even if it's out of sight for a given time, until it's lost.
        {
            chaseTimer -= Time.deltaTime;
            Chase();
            timerWasActive = true;
        }
        else
        {
            // Shoots a raycast to checks if he can see the player
            RaycastHit hit;
            Physics.Raycast(transform.position, playerDirection, out hit);

            // The raycast hit the player and there is no object in between
            if (hit.collider.gameObject.GetComponent<PlayerMovementAdvanced>() != null)
            {
                timerWasActive = false;
                if (Vector3.Distance(transform.position, player.position) <= sightRange)
                {
                    // Look towards the player
                    Vector3 lookDirection = new Vector3(playerDirection.x, 0f, playerDirection.z);
                    Quaternion rotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, lookSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.position, player.position) <= attackRange)
                    {
                        Attack(); // Player is in both sight range and attack range
                    }
                    else Chase(); // Player is in sight range but not attack range
                }
                else Patrol(); // Player is too far away
            }
            else // The raycast didn't hit the player
            {
                if (wasChasing && !timerWasActive) chaseTimer = chaseTimeAfterTargetLost;
                else Patrol();
            }
        }
    }

    /// <summary>
    /// Walks around patroling a certain area of the map until a player is seen.
    /// </summary>
    private void Patrol()
    {
        wasChasing = false;
        if (!enablePatrol)
        {
            // Doesn't move
            agent.SetDestination(transform.position);
            return;
        }
        if (!walkPointSet) Invoke(nameof(FindWalkPoint), newPointTime);
        if (walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        // Walkpoint reached
        if (distanceToWalkPoint.magnitude < patrolPointMinDistance)
            walkPointSet = false;
    }

    /// <summary>
    /// Continuously sets the target destination of the enemy to the player position.
    /// </summary>
    private void Chase()
    {
        wasChasing = true;
        agent.SetDestination(player.position);
    }

    /// <summary>
    /// Stops the enemy movement and attacks the player.
    /// </summary>
    private void Attack()
    {
        wasChasing = false;

        if (!canMoveWhileAttacking) agent.SetDestination(transform.position);

        if (!alreadyAttacked) 
        {
            gun.Shoot();

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }
    private void ResetAttack() { alreadyAttacked = false; }

    /// <summary>
    /// Randomly generates a point where the enemy should patrol using a given range.
    /// </summary>
    private void FindWalkPoint()
    {
        float randomXRange = Random.Range(minWalkPointRange, maxWalkPointRange);
        float randomZRange = Random.Range(minWalkPointRange, maxWalkPointRange);
        float randomX = Random.Range(-randomXRange, randomXRange);
        float randomZ = Random.Range(-randomZRange, randomZRange);
        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Makes sure the walk point is't outside of the map
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    /// <summary>
    /// Damages an enemy with a given damage value. Kills it if the remaining health is equal or lower than zero.
    /// </summary>
    /// <param name="damage">How much damage the enemy will take.</param>
    /// <returns>The remaining health of the enemy.</returns>
    public int TakeDamage(int damage, bool isCritical = false)
    {
        health -= damage;
        ShowHitMarker(damage, duration,
            isCritical ? criticalColor : normalColor);

        if(health <= 0)
        {
            Die();
            return 0;
        }

        return health;
    }

    /// <summary>
    /// Kills the enemy and creates the ragdoll.
    /// </summary>
    private void Die()
    {
        Debug.Log("Enemy died");
    }

    /// <returns>The current health of this enemy.</returns>
    public int GetHealth() { return health; }

    #region HitMarkers
    /// <summary>
    /// Activates the hitmarker, changes the color and text depending on the damage, and moves it to a random distance from the enemy.
    /// </summary>
    /// <param name="damage">The damage that will be displayed on the text.</param>
    /// <param name="duration">How much the hitmarker will last.</param>
    /// <param name="color">The text color.</param>
    private void ShowHitMarker(float damage, float duration, Color color)
    {
        if (hitmarker == null) return;

        hitmarker.gameObject.SetActive(true);
        hitmarker.color = color;
        hitmarker.text = damage.ToString();

        // Moves the hitmarker to a random position around the enemy
        hitmarker.transform.localPosition = GetHitMarkerPosition(minimumDistance, maximumDistance);

        Invoke(nameof(HideHitMarker), duration);
    }

    /// <summary>
    /// Disables an hitmarker.
    /// </summary>
    private void HideHitMarker()
    {
        if (hitmarker == null) return;
        hitmarker.gameObject.SetActive(false);
    }

    /// <summary>
    /// Generates a random offset between a minimum and maximum value to change the position of the hitmarker.
    /// Also makes sure the position will be outside of the enemy's body by using a minimumOffset value.
    /// </summary>
    /// <param name="minimum">The minimum distance from the enemy.</param>
    /// <param name="maximum">The maximum distance from the enemy.</param>
    /// <returns>The final offset.</returns>
    private Vector3 GetHitMarkerPosition(Vector3 minimum, Vector3 maximum)
    {
        // Generates a random offset
        float offsetX = Random.Range(minimumDistance.x, maximumDistance.x);
        float offsetY = Random.Range(minimumDistance.y, maximumDistance.y);
        float offsetZ = Random.Range(minimumDistance.z, maximumDistance.z);
        Vector3 offset = new Vector3(offsetX, offsetY, offsetZ);

        // Makes sure the offset is bigger than the minimum offset so the hitmarker isn't inside of the enemy's body
        if (offset.x < minimumOffset.x && offset.x > -minimumOffset.x) offset.x = GetHitMarkerPosition(minimum, maximum).x;
        if (offset.y < minimumOffset.y && offset.y > -minimumOffset.y) offset.y = GetHitMarkerPosition(minimum, maximum).y;
        if (offset.z < minimumOffset.z && offset.z > -minimumOffset.z) offset.z = GetHitMarkerPosition(minimum, maximum).z;

        return offset;
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        // Shows the attack (red) and sight (cyan) range of the enemy.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draws a line between the enemy and the player
        if (player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + playerDirection);
        }
    }
}
