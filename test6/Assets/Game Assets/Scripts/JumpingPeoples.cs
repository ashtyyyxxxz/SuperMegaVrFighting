using UnityEngine;

public class JumpingPeoples : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Animator animator;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        animator = GetComponentInChildren<Animator>();
        animator.speed = Random.Range(0.5f, 2f);
        Material newMaterial = meshRenderer.material;

        newMaterial.color = new Color(
                    Random.Range(0, 1f),
                    Random.Range(0, 1f),
                    Random.Range(0, 1f));
        meshRenderer.material = newMaterial;
    }
}