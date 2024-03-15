using System.Linq;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    [RegisterTypeInIl2Cpp]
    public class PlayerAnimController : MonoBehaviour
    {
        private Rigidbody _characterController;
        private Animator _animator;

        public float speedSmoothTime = 0.05f; // Temps lissage de la transition

        private float speed = 7500;

        private Player player;

        private void Awake()
        {
            _characterController = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            
            _characterController.freezeRotation = true;
        }

        public void UpdatePlayer()
        {
            if (player != null)
            {
                if (Vector3.Distance(player.desiredPosition.toVector3(), gameObject.transform.position) > 0.1f)
                {
                    /*Vector3 direction = (player.desiredPosition.toVector3() - gameObject.transform.position).normalized;
                    // Déplacer dans cette direction 
                    gameObject.GetComponent<CharacterController>().Move(direction * speed * Time.deltaTime);*/
                    Vector3 moveDirection = (player.desiredPosition.toVector3() - gameObject.transform.position).normalized;

                    // Apply the movement direction to the GameObject
                    _characterController.AddForce(moveDirection * (speed * Time.deltaTime), ForceMode.Force);
                }
                else
                {
                    _characterController.velocity = Vector3.zero;
                }
                    
                float verticalSpeed = Vector3.Dot(transform.forward, _characterController.velocity); 
                float horizontalSpeed = Vector3.Dot(transform.right, _characterController.velocity);
                
        
                _animator.SetFloat("Vertical", verticalSpeed);
                _animator.SetFloat("Horizontal", horizontalSpeed);
                // Vérifier si on est arrivé à destination
                /*if (Vector3.Distance(gameObject.transform.position, player.desiredPosition.toVector3()) < 0.1f)
                    gameObject.GetComponent<CharacterController>().Move(Vector3.zero);*/
                /*if (Vector3.Distance(gameObject.transform.position, player.desiredPosition.toVector3()) < 0.1f)
                {
                    _characterController.Move(Vector3.zero);
                }*/
            }
            else
            {
                _animator.SetFloat("Vertical", 0);
                _animator.SetFloat("Horizontal", 0);
                player = ClientData.players.FirstOrDefault(s => s.Value.username == gameObject.name).Value;
            }
            
            
        }
    }
}