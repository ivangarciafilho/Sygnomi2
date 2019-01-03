using UnityEngine;
using System.Collections;

namespace NGS.PolygonalCulling
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private float _moveSpeed = 1f;

        [SerializeField]
        private float _rotationSpeed = 1f;


        private void Start()
        {
            _rigidbody = _rigidbody == null ? GetComponentInChildren<Rigidbody>() : _rigidbody;
            _camera = _camera == null ? GetComponentInChildren<Camera>() : _camera;

            if (_camera == null || _rigidbody == null)
                enabled = false;
        }

        private void FixedUpdate()
        {
            PerformPosition();

            PerformRotation();
        }

        private void PerformPosition()
        {
            Vector3 velocity = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                velocity += transform.forward;

            if (Input.GetKey(KeyCode.S))
                velocity -= transform.forward;

            if (Input.GetKey(KeyCode.D))
                velocity += transform.right;

            if (Input.GetKey(KeyCode.A))
                velocity -= transform.right;

            _rigidbody.velocity = velocity.normalized * _moveSpeed;
        }

        private void PerformRotation()
        {
            Vector3 rigidbodyRotation = _rigidbody.rotation.eulerAngles;
            Vector3 cameraRotation = _camera.transform.eulerAngles;

            float rotationX = rigidbodyRotation.x - (Input.GetAxis("Mouse Y") * _rotationSpeed);
            float rotationY = rigidbodyRotation.y + (Input.GetAxis("Mouse X") * _rotationSpeed);

            _rigidbody.MoveRotation(Quaternion.Euler(rotationX, rotationY, rigidbodyRotation.z));
        }
    }
}
