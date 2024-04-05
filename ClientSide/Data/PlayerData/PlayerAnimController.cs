using System.Linq;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    [RegisterTypeInIl2Cpp]
    public class PlayerAnimController : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Animator _animator;

        private float speed = 100;
        private float maxDistanceToTarget = 0.1f;

        private Player player;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _rigidbody.useGravity = false;
            _rigidbody.GetComponent<CapsuleCollider>().enabled = false;
        }

        public void UpdatePlayer()
        {
            SpeedControl();
            if (player != null)
            {
                Vector3 targetPosition = player.desiredPosition.toVector3();
                Vector3 currentPosition = transform.position;
                Vector3 moveDirection = (targetPosition - currentPosition).normalized;

                float distanceToTarget = Vector3.Distance(currentPosition, targetPosition);

                // Calculer la vitesse proportionnelle à la distance
                float desiredSpeed = Mathf.Lerp(0f, speed, Mathf.InverseLerp(0f, maxDistanceToTarget, distanceToTarget));

                // Appliquer la force de mouvement
                _rigidbody.AddForce(moveDirection * desiredSpeed, ForceMode.Force);

                // Vérifier si on est arrivé à destination
                if (distanceToTarget <= maxDistanceToTarget)
                {
                    _rigidbody.velocity = Vector3.zero;
                }

                // Mise à jour des paramètres de l'Animator
                float verticalSpeed = Vector3.Dot(transform.forward, _rigidbody.velocity);
                float horizontalSpeed = Vector3.Dot(transform.right, _rigidbody.velocity);
                _animator.SetFloat("Vertical", verticalSpeed);
                _animator.SetFloat("Horizontal", horizontalSpeed);
            }
            else
            {
                // Réinitialiser les paramètres de l'Animator
                _animator.SetFloat("Vertical", 0);
                _animator.SetFloat("Horizontal", 0);
                player = ClientData.players.FirstOrDefault(s => s.Value.username == gameObject.name).Value;
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            if(flatVel.magnitude > speed)
            {
                Vector3 limitedVel = flatVel.normalized * speed;
                _rigidbody.velocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
            }
        }
    }
}