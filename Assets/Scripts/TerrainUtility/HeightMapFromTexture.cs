#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;

public class HeightMapFromTexture : MonoBehaviour {

    public Texture2D texture;

    [ContextMenu ("Height Map From Texture")]
    void DoHeightMappingFromTexture ()
    {
        if(texture != null)
            ApplyHeightMap(texture);
    }

    public void ApplyHeightMapFromRenderTexture(RenderTexture rt) {
        RenderTexture temp = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D allocTex = new Texture2D(rt.width, rt.height);
        allocTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        allocTex.Apply();
        RenderTexture.active = temp;

        ApplyHeightMap(allocTex);
    }

    public void ApplyHeightMap(Texture2D inputTexture) {
        int texWidth = inputTexture.width;
        int texHeight = inputTexture.height;
        var texPixels = inputTexture.GetPixels();

        Terrain terrain = GetComponent<Terrain>();
        if(terrain == null) 
            return;

        TerrainData terrainData = terrain.terrainData;
        if(terrainData == null) 
            return;

        Undo.RegisterCompleteObjectUndo (terrainData, "Heightmap From Texture");

        int heightMapWidth = terrainData.heightmapWidth;
        int heightMapHeight = terrainData.heightmapHeight;
        
        Color[] colorMap = null;

        if(texWidth != heightMapWidth || texHeight != heightMapHeight) {
            colorMap = new Color[heightMapWidth * heightMapHeight];

            //sampling

            //if texture has no filtering, use nearest neighbor scaling
            if(inputTexture.filterMode == FilterMode.Point) {
                var xScale = ((float)texWidth) / heightMapWidth;
                var yScale = ((float)texHeight) / heightMapHeight;

                for(int y = 0;y < heightMapHeight;y++) {
                    var yAccIndex = y * heightMapWidth;
                    var scaledYAccIndex = ((int)(y * yScale)) * texWidth;
                    for(int x = 0;x < heightMapWidth;x++) {
                        colorMap[yAccIndex + x] = texPixels[scaledYAccIndex + (int)(x * xScale)];
                    }
                }
            }
            else {
                 //Bilinear filtering
                 for(int y = 0;y < heightMapHeight;y++) {
                    var yAccIndex = y * heightMapWidth;
                    var yRatio = (float)y/heightMapHeight;
                    for(int x = 0;x < heightMapWidth;x++) {
                        var xRatio = (float)x/heightMapWidth;
                        colorMap[yAccIndex + x] = inputTexture.GetPixelBilinear(xRatio, yRatio);
                    }
                 }
            }
            
        }
        else {
            colorMap = texPixels;
        }

        var heightData = terrainData.GetHeights(0, 0, heightMapWidth, heightMapHeight);
        for(int y = 0;y < heightMapHeight;y++) {
            for(int x = 0;x < heightMapWidth;x++) {
                heightData[y,x] = colorMap[y * heightMapWidth + x].grayscale;
            }
        }

        terrainData.SetHeights(0,0,heightData);

    }

}


#endif