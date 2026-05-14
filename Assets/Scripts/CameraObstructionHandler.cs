using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionHandler : MonoBehaviour
{
    #region Variables
    [SerializeField] private Transform target;
    [SerializeField] private LayerMask obstructionLayer;
    [SerializeField] private float fadeAmount = 0.3f; // 0 is invisible, 1 is solid
    private List<MeshRenderer> currentlyHidden = new List<MeshRenderer>();
    #endregion

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float distance = Vector3.Distance(transform.position, target.position);

        foreach (var renderer in currentlyHidden)
        {
            SetMaterialAlpha(renderer, 1.0f);
        }
        currentlyHidden.Clear();

        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, obstructionLayer);

        foreach (var hit in hits)
        {
            MeshRenderer mesh = hit.collider.GetComponent<MeshRenderer>();
            if (mesh != null)
            {
                SetMaterialAlpha(mesh, fadeAmount);
                currentlyHidden.Add(mesh);
            }
        }
    }
    void SetMaterialAlpha(MeshRenderer renderer, float alpha)
    {
        Color color = renderer.material.color;
        color.a = alpha;
        renderer.material.color = color;
    }
}
