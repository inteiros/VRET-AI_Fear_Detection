using UnityEngine;

namespace DefaultNamespace
{
    public class Walk : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float speed = 1;

        private void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            var movement = (transform.right * horizontal + transform.forward * vertical) * speed;

            characterController.Move(movement * Time.deltaTime);
        }
    }
}
