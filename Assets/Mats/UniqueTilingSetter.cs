using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class UniqueTilingSetter : MonoBehaviour
{
    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);

        mpb.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, offset.x, offset.y));

        renderer.SetPropertyBlock(mpb);
    }
}
