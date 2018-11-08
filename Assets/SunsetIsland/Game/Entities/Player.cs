using UnityEngine;

namespace Assets.SunsetIsland.Game.Entities
{
    public interface IChunkLoadingEntity
    {
        Vector3 Position { get; }
    }
    
    public class Player : MonoBehaviour, IChunkLoadingEntity
    {
        public Camera Camera;
        public float maximumY = 60F;

        public float minimumY = -60F;
        public Rigidbody Rigidbody;

        public float sensitivityX = 0.001F;
        public float sensitivityY = 0.001F;

        private void Start()
        {
            Rigidbody.freezeRotation = true;
        }

        public void Initialize(Vector3 initialPosition)
        {
            Rigidbody.MovePosition(initialPosition);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                var rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
                var rotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * sensitivityY;
                if (rotationY < 360 + minimumY && rotationY > maximumY)
                    if (Mathf.Abs(rotationY - (360 + minimumY)) < Mathf.Abs(rotationY - maximumY))
                        rotationY = 360 + minimumY;
                    else
                        rotationY = maximumY;

                transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);

                var hozMove = Input.GetAxis("Horizontal");
                var verMove = Input.GetAxis("Vertical");
                var move = new Vector3(hozMove, 0, verMove).normalized;
                move = Quaternion.Euler(transform.localEulerAngles) * move;
                Rigidbody.AddForce(move * 10);
                if (Input.GetKeyDown(KeyCode.Space))
                    Rigidbody.AddForce(Vector3.up * 1000);
            }
        }

        public Vector3 Position => transform.position;
    }
}