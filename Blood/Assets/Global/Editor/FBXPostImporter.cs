using UnityEditor;
using UnityEngine;

class FBXPostprocessor : AssetPostprocessor
{
    // This method is called just before importing an FBX.
    void OnPreprocessModel()
    {
        ModelImporter mi = (ModelImporter)assetImporter;
        mi.globalScale = 1;

        // Materials for characters are created using the GenerateMaterials script.
        //mi.generateMaterials = 0;
		mi.importMaterials = false;
		
		mi.generateSecondaryUV = true;

		string assetName = "";
		int index = mi.assetPath.LastIndexOf("/");
		if( index != -1 )
		{
			assetName = mi.assetPath.Substring( index + 1 ); // MAC
		}
		else
		{
			index = mi.assetPath.LastIndexOf("\\");
			assetName = mi.assetPath.Substring( index + 1 ); // Windows
		}


		//Debug.LogError("NAME import : " + assetName);

		if( !assetName.Contains("Character") && 
		    !assetName.Contains("@") &&
		    !assetName.Contains("Mask")
		   )
		{
			mi.animationType = ModelImporterAnimationType.None;
		}
		
		Debug.Log ("Model post processed " + mi.assetPath);
    }

}
