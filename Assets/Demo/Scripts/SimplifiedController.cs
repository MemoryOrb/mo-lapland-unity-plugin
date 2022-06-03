using UnityEngine;

public class SimplifiedController : MonoBehaviour
{
    private Renderer pRenderer;

    void Start()
    {
        pRenderer = this.GetComponent<Renderer>();
    }

    public void onSelectStarted()
    {
        pRenderer.materials[1].SetColor("_Color", Color.cyan);
    }

    public void onSelectedEnded()
    {
        pRenderer.materials[1].SetColor("_Color", Color.black);
    }
}
