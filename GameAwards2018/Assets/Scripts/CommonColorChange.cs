using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CommonColorChange : MonoBehaviour
{
    [SerializeField]
    Color commonColor = Color.white;

    private void OnValidate()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        Mesh copy = new Mesh();
        // 頂点コピー
        copy.vertices = mesh.vertices;
        // 法線コピー
        copy.normals = mesh.normals;
        // インデックスコピー
        copy.triangles = mesh.triangles;
        // テクスチャ座標コピー
        copy.uv = mesh.uv;
        // 頂点カラーコピー
        copy.colors = mesh.colors;
        // タンジェントコピー
        copy.tangents = mesh.tangents;

        List<Color> colorList = new List<Color>();
        for (int vtxCnt = 0; vtxCnt < copy.vertexCount; vtxCnt++)
        {
            colorList.Add(commonColor);
        }
        copy.SetColors(colorList);

        GetComponent<MeshFilter>().sharedMesh = copy;
    }
}
