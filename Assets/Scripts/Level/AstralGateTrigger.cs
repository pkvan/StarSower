using UnityEngine;

namespace StarSower.Level
{
    // Chuyen tiep su kien cham tu child "TriggerCollider" len AstralGateController o node goc.
    //
    // Ton tai vi mot ly do ky thuat: Unity chi goi OnTriggerEnter2D tren dung GameObject dang mang
    // Collider2D. Cau truc yeu cau collider nam o child rieng, nen node goc khong bao gio nhan duoc
    // su kien neu khong co cai cau nay.
    [RequireComponent(typeof(Collider2D))]
    public class AstralGateTrigger : MonoBehaviour
    {
        [Tooltip("De trong thi tu tim nguoc len cay cha.")]
        [SerializeField] private AstralGateController gate;

        private void Awake()
        {
            if (gate == null)
                gate = GetComponentInParent<AstralGateController>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (gate != null)
                gate.NotifyTriggerEnter(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (gate != null)
                gate.NotifyTriggerExit(other);
        }
    }
}
