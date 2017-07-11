using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Characters
{
    [SelectionBase]
	public class CharacterControl : MonoBehaviour
	{
        [SerializeField] float movingTurnSpeed = 1000;
        [SerializeField] float stationaryTurnSpeed = 800;
        [SerializeField] float rotationSpeed = 300f;
        [SerializeField] float moveSpeedMultiplier = .7f;
        [SerializeField] float animSpeedMultiplier = 1.5f;
        [SerializeField] float moveThreshold = 1f;
        [SerializeField] float animatorDampingTime = 0.1f;
		[SerializeField] float stoppingDistance = 1f;

        Rigidbody myRigidbody;
        Animator animator;
        float turnAmount;
        float forwardAmount;
		NavMeshAgent agent = null;
        WeaponSystem weaponSystem;

		const string ATTACK_TRIGGER = "Attack";

		void Start()
		{
			animator = GetComponent<Animator>();
			myRigidbody = GetComponent<Rigidbody>();
			myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            weaponSystem = GetComponent<WeaponSystem>();

			agent = GetComponentInChildren<NavMeshAgent>();
			agent.updateRotation = false;
			agent.updatePosition = true;
			agent.stoppingDistance = stoppingDistance;
        }

		void Update()
		{
			if (agent.remainingDistance > agent.stoppingDistance)
			{
				Move(agent.desiredVelocity);
			}
			else
			{
				Move(Vector3.zero);
			}
		}

		public void SetDestination(Vector3 worldPos)
		{
			agent.destination = worldPos;
		}

        public float GetAnimSpeedMultiplier()
        {
            return animSpeedMultiplier;
        }

		public void AttackTarget(GameObject target)
		{
            transform.LookAt(target.transform);
			animator.SetTrigger(ATTACK_TRIGGER);
            float hitTime = weaponSystem.GetCurrentWeapon().GetAnimHitTime();
            StartCoroutine(DamageTargetAfterSeconds(target, hitTime));
		}

        IEnumerator DamageTargetAfterSeconds(GameObject target, float seconds)
        {
			yield return new WaitForSecondsRealtime(seconds);
			HealthSystem enemyDamageSystem = target.GetComponent<HealthSystem>();
			enemyDamageSystem.AdjustHealth(weaponSystem.GetTotalDamagePerHit());
        }

        private void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator(); // send input and other state parameters to the animator
        }

        private void SetForwardAndTurn(Vector3 movement)
        {
            // convert the world relative moveInput vector into a local-relative
            // turn amount and forward amount required to head in the desired direction
            if (movement.magnitude > moveThreshold)
            {
                movement.Normalize();
            }
            var localMove = transform.InverseTransformDirection(movement);
            turnAmount = Mathf.Atan2(localMove.x, localMove.z);
            forwardAmount = localMove.z;
        }

        void UpdateAnimator()
		{
			animator.SetFloat("Forward", forwardAmount, animatorDampingTime, Time.deltaTime);
			animator.SetFloat("Turn", turnAmount, animatorDampingTime, Time.deltaTime); 
			animator.speed = animSpeedMultiplier;
		}

		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}

		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (Time.deltaTime > 0)
			{
				Vector3 v = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;

				// we preserve the existing y part of the current velocity.
				v.y = myRigidbody.velocity.y;
				myRigidbody.velocity = v;
			}
		}
	}
}