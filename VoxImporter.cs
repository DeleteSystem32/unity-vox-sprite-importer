using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System;

[ScriptedImporter(1, "vox")]
public class VoxImporter : ScriptedImporter
{

    public enum ImportMode {Sliced, FixedSprite, Mesh};
    public ImportMode importMode;
	//public Shader shader;

    public int pixelsPerUnit = 32;
    public bool animated = false; //if false, only import first frame
    public bool generateShadow = false;
    public Color shadowColor = new Color(.2f,.2f,.2f,.6f);

    public int atlasSize = 2048;

    public bool useOutline;
    public float outlineSize; // relative to pixel size, 1.0 -> size of 1 voxel
    public Color outlineColor = Color.black;
    public float shadowAngle; // see how to use this (for layers, only possible with shader)
    public Shader layeredShader;

    public int startingLayer = 0;
    

    // todo: animation frames
    // todo: 4 different rotations for fixed import
    // todo: outlines

    uint[] palette = {
	0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
	0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
	0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
	0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
	0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
	0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
	0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
	0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
	0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
	0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
	0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
	0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
	0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
	0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
	0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
	0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
    };

    class VoxModel
    {
        int[,,] voxels;

        public int width {get{return voxels.GetLength(0);}}
        public int depth {get{return voxels.GetLength(1);}}
        public int height {get{return voxels.GetLength(2);}}
        public VoxModel(int size_x, int size_y, int size_z){
            voxels = new int[size_x, size_y, size_z];
        }

        public void set_voxels(byte x, byte y, byte z, byte colour_index){
            voxels[x,y,z] = colour_index;
        }

        public int get_colour_index(int x, int y, int z)
        {
            return voxels[x,y,z];
        }
    }

    List<VoxModel> models = null;

    public override void OnImportAsset(AssetImportContext ctx)
    {      
        // parse file
        models = parseFile(ctx.assetPath);
        if(models == null){
            Debug.LogError("model list is invalid");
            return;
        }            

        switch(importMode){
            case ImportMode.Sliced:
                importAsLayers(ctx);
                break;

            case ImportMode.FixedSprite:
                importAsFixedSprite(ctx);
                break;

            case ImportMode.Mesh:
                importAsMesh(ctx);
                break;

            default:
                break;
        }        
    }

    List<VoxModel> parseFile(string filePath){

        List<VoxModel> models = new List<VoxModel>();
        Queue<byte> byteQueue = new Queue<byte>(File.ReadAllBytes(filePath));
        int model_index = 0; // for multiple frames (animation)

        // get file identifier (4 bytes)
        string fileID = System.Text.Encoding.ASCII.GetString(getBytes(byteQueue, 4));

        if(fileID != "VOX "){
            Debug.LogError("invalid .vox file!");
            return null;
        }

        int mvVersion = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);

        // todo: support for multiple MV versions??

        // read chunks
        while(byteQueue.Count > 0){
            string chunkID = System.Text.Encoding.ASCII.GetString(getBytes(byteQueue, 4));
            int chunkSize = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);
            int childrenChunks = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);

            switch (chunkID){
                
                case "SIZE":
                    int x = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);
                    int y = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);
                    int z = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0);
                    models.Add(new VoxModel(x,y,z));
                    //getBytes(chunkSize - 12);
                    break;

                case "XYZI":                    
                    int n = System.BitConverter.ToInt32(getBytes(byteQueue, 4), 0); // n voxels
                    for(int i = 0; i< n; i++){
                        models[model_index].set_voxels(                            
                            getNextByte(byteQueue),
                            getNextByte(byteQueue),
                            getNextByte(byteQueue),
                            getNextByte(byteQueue));                            
                    }
                    ++model_index;
                    //getBytes(chunkSize - 4*n);
                    break;

                case "RGBA":
                    for(int i=1; i<256; i++){
                        palette[i] = System.BitConverter.ToUInt32(getBytes(byteQueue, 4), 0);
                    }
                    getBytes(byteQueue, 4);
                    break;

                case "MATT":
                    //todo
                    getBytes(byteQueue, chunkSize);
                    break;

                default:
                    getBytes(byteQueue, chunkSize);
                    break;
            }
        }

        return models;

    }

    void importAsFixedSprite(AssetImportContext ctx){        
        
        // first, create texture atlas
        Texture2D[] modelTextures = new Texture2D[models.Count];
        for(int i = 0; i< models.Count; i++){

            if(i>0){
                //safeguard until animation is implemented
                break;
            }
            //todo: 4 different textures (1 per rotation)
            //from top to bottom: set pixel once found, then break
            VoxModel model = models[i];
            Texture2D modelTexture = new Texture2D(model.width, startingLayer + model.height + model.depth -1);
            modelTexture.wrapMode = TextureWrapMode.Clamp;
            modelTexture.filterMode = FilterMode.Point;
            clearTexture(modelTexture);
            for(int z = 0; z < model.height; z++){
                for(int x = 0; x<model.width; x++){
                    for(int y = 0; y<model.depth; y++){
                        int adjusted_y = y+z + startingLayer;
                        if(adjusted_y >= 0 && adjusted_y <modelTexture.height + startingLayer){                      
                            int colour_index = model.get_colour_index(x, y, z);
                            if(colour_index != 0){
                                Color pixelColor = toRGBA(palette[model.get_colour_index(x, y, z)]);
                                modelTexture.SetPixel(x, adjusted_y, pixelColor);
                            }                             

                        }
                    }
                }
            }
            modelTextures[i] = modelTexture;
        }
        Texture2D atlas = new Texture2D(atlasSize, atlasSize);
        atlas.wrapMode = TextureWrapMode.Clamp;
        atlas.filterMode = FilterMode.Point;
        Rect[] spriteRects = atlas.PackTextures(modelTextures, 2, atlasSize);
        ctx.AddObjectToAsset("model atlas", atlas);
        ctx.SetMainObject(atlas);

        //then, create sprites
        for(int i= 0; i<models.Count; i++){
            if(i>0){
                break;
            }
            Rect adjustedRect = new Rect(spriteRects[i].x*atlas.width, spriteRects[i].y*atlas.height, spriteRects[i].width*atlas.width, spriteRects[i].height*atlas.height);
            Sprite modelSprite = Sprite.Create(atlas, adjustedRect, new Vector2(.5f,.5f), pixelsPerUnit, 2, SpriteMeshType.FullRect);
            modelSprite.name = "model sprite " + i;
            ctx.AddObjectToAsset("model sprite " + i, modelSprite);
        }
    }

    void importAsLayers(AssetImportContext ctx){
        // parent
        GameObject mainObj = new GameObject();
        mainObj.AddComponent<VoxRotator>();
        ctx.AddObjectToAsset("vox layers", mainObj);
        ctx.SetMainObject(mainObj);       


        // create sprites/layers
        for(int i = 0; i< models.Count; i++){
            if(i > 0){
                // safeguard until animations are implemented
                break;
            }
            
            VoxModel model = models[i];            

            Texture2D shadowTexture = new Texture2D(model.width, model.depth);            
            shadowTexture.wrapMode = TextureWrapMode.Clamp;
            shadowTexture.filterMode = FilterMode.Point;

            // invisible shadow texture
            clearTexture(shadowTexture);

            // texture atlas
            Texture2D[] layerTextures = generateLayerTextures(model);
            Texture2D atlas = new Texture2D(atlasSize, atlasSize);
            atlas.wrapMode = TextureWrapMode.Clamp;
            atlas.filterMode = FilterMode.Point;
            ctx.AddObjectToAsset("layer texture atlas", atlas);
            Rect[] spriteUVs = atlas.PackTextures(layerTextures, 2, atlasSize);

            for(int z = 0; z<model.height; z++){
                Rect spriteRect = new Rect(spriteUVs[z].x * atlas.width, spriteUVs[z].y*atlas.height, spriteUVs[z].width*atlas.width, spriteUVs[z].height*atlas.height);
                Sprite layerSprite = Sprite.Create(atlas, spriteRect, new Vector2(.5f,.5f), pixelsPerUnit);
                //Sprite layerSprite = generateLayer(model, z);
                GameObject layer = new GameObject();
                layer.transform.localPosition += (new Vector3(0, 1f, 0) / pixelsPerUnit) * (z + startingLayer);
                layer.name = "frame " + i + " layer " + z;
                layer.transform.SetParent(mainObj.transform);            
                SpriteRenderer sRend = layer.AddComponent<SpriteRenderer>();
                sRend.sprite = layerSprite;
                sRend.sortingOrder = z;
                sRend.flipY = true;
                ctx.AddObjectToAsset("frame " + i + " layer sprite " + z, layerSprite);

                if(generateShadow){
                    updateShadowTexture(shadowTexture, layerTextures[z]);
                }
            }

            if(generateShadow){
                GameObject shadowLayer = new GameObject();
                shadowLayer.name = "frame " + i + " shadow";
                shadowLayer.transform.SetParent(mainObj.transform);
                SpriteRenderer shadowRend = shadowLayer.AddComponent<SpriteRenderer>();
                shadowRend.sprite = Sprite.Create(shadowTexture, new Rect(0,0, shadowTexture.width, shadowTexture.height), new Vector2(.5f, .5f), pixelsPerUnit);
                shadowRend.sortingOrder = -1;
                shadowRend.flipY = true;
                ctx.AddObjectToAsset("frame " + i + "shadow texture", shadowTexture);
                ctx.AddObjectToAsset("frame " + i + " shadow sprite", shadowRend.sprite);
            }
        }	
    }

    void importAsMesh(AssetImportContext ctx){
        //todo
    }

    Texture2D[] generateLayerTextures(VoxModel model){
        Texture2D[] modelTextures = new Texture2D[model.height];
        for(int z = 0; z<modelTextures.Length; z++){
            modelTextures[z] = generateLayerTexture(model, z);
        }
        return modelTextures;
    }

    void updateShadowTexture(Texture2D shadowTexture, Texture2D layerTexture){
        for(int x = 0; x<layerTexture.width; x++){
            for(int y = 0; y<layerTexture.height; y++){

                if(layerTexture.GetPixel(x,y).a != 0){
                    shadowTexture.SetPixel(x,y, shadowColor);
                }
            }
        }
    }

    Texture2D generateLayerTexture(VoxModel model, int layerIndex){
        Texture2D layerTexture = new Texture2D(model.width, model.depth);
        layerTexture.filterMode = FilterMode.Point;
        layerTexture.wrapMode = TextureWrapMode.Clamp;
        bool textureEmpty = true;
        for(int x = 0; x < model.width; x++){
            for(int y = 0; y < model.depth; y++){
                Color pixelCol = toRGBA(palette[model.get_colour_index(x, y, layerIndex)]);
                layerTexture.SetPixel(x, y, pixelCol);
                if(pixelCol.a != 0){
                    textureEmpty = false;
                }
            }            
        }
        /* if(textureEmpty){
            return null;
        }
        else  */
        return layerTexture;
    }

    byte[] getBytes(Queue<byte> byteQueue, int amount){
        byte[] bytes = new byte[amount];
        for(int i = 0; i < amount; i++){
            bytes[i] = byteQueue.Dequeue();
        }
        return bytes;
    }

    byte getNextByte(Queue<byte> byteQueue){
        return byteQueue.Dequeue();
    }

    Color toRGBA(uint col){
        byte[] bytes = BitConverter.GetBytes(col);
        //Debug.Log(bytes[0] + " " + bytes[1] + " "+ bytes[2] + " " + bytes[3]);
        float r = bytes[0] / 255f;
        float g = bytes[1] / 255f;
        float b = bytes[2] / 255f;
        float a = bytes[3]/ 255f;
        return new Color(r, g, b, a);
    }

    void clearTexture(Texture2D _texture){
        Color[] _pixels = _texture.GetPixels();
        for(int pix = 0; pix<_pixels.Length; pix++){
            _pixels[pix].a = 0;
        }
        _texture.SetPixels(_pixels);
    }
}