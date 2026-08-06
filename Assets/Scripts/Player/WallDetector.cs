using UnityEngine;

namespace StarSower.Player
{
    // Phat hien dang ap vao tuong ben trai hay ben phai (S3-000).
    //
    // Dung contact normal giong GroundChecker chu khong overlap mot vung co dinh: dung o mep hay
    // goc thi vung co dinh co the truot ra ngoai tuong du nguoi choi van dang ap vao that.
    //
    // Tuong = be mat co normal gan nam ngang. Nguong minWallNormalX loai bo mai doc, thu von phai
    // truot xuong duoc chu khong phai bam vao.
    [RequireComponent(typeof(Collider2D))]
    public class WallDetector : MonoBehaviour
    {
        [Tooltip("Layer duoc tinh la tuong. Thuong dat bang chinh layer mat dat.")]
        [SerializeField] private LayerMask wallLayer;

        [Tooltip("Do lon toi thieu cua normal.x de coi la tuong dung. 1 = tuong thang dung tuyet doi.")]
        [Range(0.3f, 1f)]
        [SerializeField] private float minWallNormalX = 0.75f;

        // -1 = tuong ben TRAI, +1 = tuong ben PHAI, 0 = khong cham tuong nao.
        public int WallDirection { get; private set; }
        public bool IsTouchingWall => WallDirection != 0;

        private int directionThisStep;

        private void FixedUpdate()
        {
            WallDirection = directionThisStep;
            directionThisStep = 0;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if ((wallLayer.value & (1 << collision.gameObject.layer)) == 0)
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector2 n = collision.GetContact(i).normal;
                if (Mathf.Abs(n.x) < minWallNormalX)
                    continue;

                // normal huong RA khoi tuong, nen normal.x > 0 nghia la tuong nam ben TRAI nguoi choi.
                directionThisStep = n.x > 0f ? -1 : 1;
                return;
            }
        }
    }
}
