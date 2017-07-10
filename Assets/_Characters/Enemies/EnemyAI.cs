﻿﻿﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Characters
{
    [SelectionBase]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] float chaseRadius = 6f;
        [SerializeField] float attackRadius = 4f;

		float lastHitTime = 0f;
        bool isAttacking = false;
        Animator animator;
        WeaponSystem weaponSystem;
        PlayerControl player;
        CharacterMovement characterMovement = null;

        void Start()
        {
            animator = GetComponent<Animator>();
			weaponSystem = GetComponent<WeaponSystem>();
            characterMovement = GetComponent<CharacterMovement>();
            player = FindObjectOfType<PlayerControl>();
        }

        void Update()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            float currentWeaponRange = weaponSystem.GetCurrentWeapon().GetMaxAttackRange();

			float weaponHitPeriod = weaponSystem.GetCurrentWeapon().GetMinTimeBetweenHits();
			bool timeToHitAgain = Time.time - lastHitTime > weaponHitPeriod;

            if (distanceToPlayer < currentWeaponRange && timeToHitAgain)
            {
                characterMovement.AttackTarget(player.gameObject);
                lastHitTime = Time.time;
            }
        }

        void OnDrawGizmos()
        {
            // Draw attack sphere 
            Gizmos.color = new Color(255f, 0, 0, .5f);
            Gizmos.DrawWireSphere(transform.position, attackRadius);

            // Draw chase sphere 
            Gizmos.color = new Color(0, 0, 255, .5f);
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }
}