using System;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    [RegisterTypeInIl2Cpp]
    public class ModCharacterController : MonoBehaviour
    {
        public float moveSpeed = 26f;
        public float acceleration = 50f;
        public float deceleration = 50f;
        public float arrivalThreshold = 0.1f; // Seuil de distance pour considérer que le personnage est arrivé

        private Vector3 velocity = Vector3.zero;
        private Vector3 targetVelocity = Vector3.zero;
        private Vector3 targetPosition;
        private bool isMovingToTarget;
        
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void UpdatePlayer()
        {
            // Si le personnage doit se déplacer vers une cible
            if (isMovingToTarget)
            {
                // Calculer la direction et la distance jusqu'à la cible
                Vector3 directionToTarget = (targetPosition - transform.position).normalized;
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

                // Si le personnage est assez proche de la cible, s'arrêter
                if (distanceToTarget <= arrivalThreshold)
                {
                    velocity = Vector3.zero;
                    targetVelocity = Vector3.zero;
                    isMovingToTarget = false;
                    return;
                }

                // Calculer la vitesse cible pour se déplacer vers la cible
                targetVelocity = directionToTarget * moveSpeed;
            }

            // Appliquer l'accélération/décélération
            velocity = Vector3.Lerp(velocity, targetVelocity, (targetVelocity.magnitude > 0.1f) ? acceleration * Time.deltaTime : deceleration * Time.deltaTime);

            // Déplacer le personnage
           // transform.position += velocity * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, transform.position + velocity * Time.deltaTime, Time.deltaTime * 10f);
            
            /*float verticalSpeed = Vector3.Dot(transform.forward, velocity);
            float horizontalSpeed = Vector3.Dot(transform.right, velocity);
            
            _animator.SetFloat("Vertical", verticalSpeed);
            _animator.SetFloat("Horizontal", horizontalSpeed);*/
            
            float verticalSpeed = Vector3.Dot(transform.forward, velocity);
            float horizontalSpeed = Vector3.Dot(transform.right, velocity);

            // Lissage des paramètres de l'animateur pour une transition plus fluide
            _animator.SetFloat("Vertical", Mathf.Lerp(_animator.GetFloat("Vertical"), verticalSpeed, Time.deltaTime * 10f));
            _animator.SetFloat("Horizontal", Mathf.Lerp(_animator.GetFloat("Horizontal"), horizontalSpeed, Time.deltaTime * 10f));
        }

        public void MoveToPosition(Vector3 position)
        {
            targetPosition = position;
            isMovingToTarget = true;
        }
    }
}