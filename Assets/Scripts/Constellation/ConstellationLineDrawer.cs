using System.Collections.Generic;
using UnityEngine;

namespace StarSower.Constellations
{
    // Ve cac net noi giua cac node, trong khong gian THE GIOI. Moi net la mot SpriteRenderer keo
    // dai dan tu dau nay sang dau kia.
    //
    // CHI ve net ma CA HAI dau da sang. Chom sao khoi phuc dan qua nhieu luot choi, nen mot net
    // noi toi node con toi se lo ra mot dau lo lung giua troi.
    public class ConstellationLineDrawer : MonoBehaviour
    {
        [Tooltip("Sprite dung lam net. Nen la mot vet sang doc, se bi keo gian theo chieu ngang.")]
        [SerializeField] private Sprite lineSprite;

        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color lineColor = new Color(0.62f, 0.82f, 1f, 0.55f);
        [SerializeField] private int sortingOrder = 4;

        [Tooltip("Be day net (world units).")]
        [SerializeField] private float thickness = 0.05f;

        private readonly List<SpriteRenderer> lines = new List<SpriteRenderer>();
        private float glowBoost;

        // Be day net phong theo khi ca chom to len, de net khong bi manh di.
        private float thicknessScale = 1f;
        public void SetThicknessScale(float k) { thicknessScale = Mathf.Max(0.01f, k); }
        private readonly List<Vector3> fromPoints = new List<Vector3>();
        private readonly List<Vector3> toPoints = new List<Vector3>();

        // Dung san toan bo net can co, tat het. Goi mot lan luc dung canh — khong Instantiate
        // giua chung buoi dien.
        public void Prepare(IReadOnlyList<StarConnection> connections, IReadOnlyList<ConstellationNode> nodes)
        {
            EnsureCapacity(connections.Count);

            fromPoints.Clear();
            toPoints.Clear();

            for (int i = 0; i < connections.Count; i++)
            {
                StarConnection c = connections[i];
                bool valid = c.fromIndex >= 0 && c.fromIndex < nodes.Count
                             && c.toIndex >= 0 && c.toIndex < nodes.Count;

                fromPoints.Add(valid ? nodes[c.fromIndex].transform.position : Vector3.zero);
                toPoints.Add(valid ? nodes[c.toIndex].transform.position : Vector3.zero);
                lines[i].gameObject.SetActive(false);
            }

            for (int i = connections.Count; i < lines.Count; i++)
                lines[i].gameObject.SetActive(false);
        }

        // progress 0..1 cho TOAN BO cac net hop le. Net thu k bat dau ve khi progress vuot qua
        // phan cua no, nen ca chom noi dan tung nhip chu khong bung ra cung luc.
        public void SetProgress(IReadOnlyList<StarConnection> connections,
                                IReadOnlyList<ConstellationNode> nodes, float progress)
        {
            int drawable = 0;
            for (int i = 0; i < connections.Count; i++)
                if (IsDrawable(connections[i], nodes))
                    drawable++;

            if (drawable == 0)
                return;

            float span = 1f / drawable;
            int seen = 0;

            for (int i = 0; i < connections.Count; i++)
            {
                if (!IsDrawable(connections[i], nodes))
                {
                    lines[i].gameObject.SetActive(false);
                    continue;
                }

                float start = seen * span;
                float local = Mathf.Clamp01((progress - start) / span);
                seen++;

                if (local <= 0f)
                {
                    lines[i].gameObject.SetActive(false);
                    continue;
                }

                lines[i].gameObject.SetActive(true);
                Stretch(lines[i], fromPoints[i], toPoints[i], local);
            }
        }

        // Doc lai toa do hai dau tu vi tri node HIEN TAI. Goi moi frame khi chom sao dang duoc
        // phong to — neu khong net van nam o cho cu va roi khoi cac ngoi sao.
        public void RefreshPoints(IReadOnlyList<StarConnection> connections,
                                  IReadOnlyList<ConstellationNode> nodes)
        {
            for (int i = 0; i < connections.Count && i < fromPoints.Count; i++)
            {
                StarConnection c = connections[i];
                if (c.fromIndex < 0 || c.fromIndex >= nodes.Count) continue;
                if (c.toIndex < 0 || c.toIndex >= nodes.Count) continue;
                fromPoints[i] = nodes[c.fromIndex].transform.position;
                toPoints[i] = nodes[c.toIndex].transform.position;
            }
        }

        // Man ket: net noi sang bung cung luc voi cac node.
        public void SetGlowBoost(float amount)
        {
            glowBoost = Mathf.Clamp01(amount);
            Color c = Color.Lerp(lineColor, Color.white, glowBoost * 0.6f);
            c.a = Mathf.Lerp(lineColor.a, 1f, glowBoost);
            for (int i = 0; i < lines.Count; i++)
                if (lines[i].gameObject.activeSelf)
                    lines[i].color = c;
        }

        public void HideAll()
        {
            for (int i = 0; i < lines.Count; i++)
                lines[i].gameObject.SetActive(false);
        }

        private static bool IsDrawable(StarConnection c, IReadOnlyList<ConstellationNode> nodes)
        {
            if (c.fromIndex < 0 || c.fromIndex >= nodes.Count) return false;
            if (c.toIndex < 0 || c.toIndex >= nodes.Count) return false;
            return nodes[c.fromIndex].IsLit && nodes[c.toIndex].IsLit;
        }

        private void Stretch(SpriteRenderer sr, Vector3 a, Vector3 b, float t)
        {
            Vector3 delta = b - a;
            float length = delta.magnitude * t;
            Transform tr = sr.transform;
            tr.position = a + delta.normalized * (length * 0.5f);
            tr.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            // Sprite goc rong 1 unit (PPU = be rong anh) nen scale.x chinh la do dai world.
            float w = sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
            float h = sr.sprite != null ? sr.sprite.bounds.size.y : 1f;
            tr.localScale = new Vector3(length / Mathf.Max(w, 0.0001f),
                                        thickness * thicknessScale / Mathf.Max(h, 0.0001f), 1f);
        }

        private void EnsureCapacity(int n)
        {
            while (lines.Count < n)
            {
                var go = new GameObject("Line_" + lines.Count);
                go.transform.SetParent(transform, worldPositionStays: false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = lineSprite;
                if (lineMaterial != null)
                    sr.sharedMaterial = lineMaterial;
                sr.color = lineColor;
                sr.sortingOrder = sortingOrder;
                go.SetActive(false);
                lines.Add(sr);
            }
        }
    }
}
