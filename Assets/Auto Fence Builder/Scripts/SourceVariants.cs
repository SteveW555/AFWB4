using AFWB;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//============================================================================================================
//- Holds details about a fixed number (kNumRailVariations) of  source objects that can be used as Variants
//- SeqItems are almost identical but used only to hold info about each step of a Sequence
//- Generally only the go filed is used, but there may be other uses in updates
//-- Both seqs and singles reference these, mainly to know which GameObject is used
//============================================================================================================

public interface ISourceVariantObserver
{
    void OnSourceVariantGoChanged(GameObject newGo);
}

[System.Serializable]
public class SourceVariant
{
    private static List<ISourceVariantObserver> observers = new List<ISourceVariantObserver>();
    public static bool stopComplainingAboutNullGos = false; //For deugging, natch.
    public bool enabled = false, inUse = true;
    public float probability = 1;
    public string goName = ""; //for backward compatibility only when loading old presets
    [SerializeField] private GameObject go = null;

    public GameObject Go
    {
        get
        {
            //if (go == null)
            //Debug.LogWarning("Getting sourceVariant.Go is null."); // Debug Only.
            return go;
        }
        set
        {
            if (value == null && stopComplainingAboutNullGos == false)
                Debug.LogWarning("SourceVariant().set Attempted to set sourceVariant.Go to null.\n");

            go = value;
            goName = go != null ? go.name : "null";

            AutoFenceCreator af = FindAutoFenceCreatorObserver(); // Don't assume this is valid
            NotifyObservers();
        }
    }

    //------------------
    public void Subscribe(ISourceVariantObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
        //PrintAllObservers();
    }

    public void Unsubscribe(ISourceVariantObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    private void NotifyObservers()
    {
        AutoFenceCreator af = FindAutoFenceCreatorObserver(); // Don't assume this is valid
        string varsStr = "";
        if (af)
        {
            varsStr = af.GetSourceVariantGOsForLayerAsString(LayerSet.railALayer);
        }
        foreach (var observer in observers)
        {
            observer.OnSourceVariantGoChanged(go);
        }
    }

    public void PrintAllObservers()
    {
        Debug.Log("Listing all observers of SourceVariant: \n");
        foreach (var observer in observers)
            Debug.Log(observer.ToString());
    }

    //---------
    // Not really necessary, but useful for debugging
    public AutoFenceCreator FindAutoFenceCreatorObserver()
    {
        foreach (var observer in observers)
        {
            if (observer is AutoFenceCreator autoFenceCreator)
                return autoFenceCreator; // Return the found AutoFenceCreator
        }
        return null; // Return null if no AutoFenceCreator is found
    }

    //---------
    // Basic Constructor
    public SourceVariant()
    {
        stopComplainingAboutNullGos = true;
        Initialize();
        stopComplainingAboutNullGos = false;
    }

    public void Initialize(GameObject inGO = null)
    {
        Go = inGO;
        if (go != null)
            goName = go.name; //this should have been done by the setter
        else
            goName = ""; //Needed for backward compatibility with older presets
        enabled = true;
        inUse = true;
        probability = 0.51f;

        /*svPositionOffset = Vector3.zero;
        svSize = Vector3.one;
        svRotation = Vector3.zero;
        svInvert = false;
        svBackToFront = false;
        svMirrorZ = false;*/
    }

    public void UpdateSourceVariant(SourceVariant inSourceVar, AutoFenceCreator af, LayerSet layer)
    {
        if (inSourceVar.Go != null)
            Go = inSourceVar.Go;
        else if (inSourceVar.goName != "")
        {
            List<GameObject> prefabs = af.GetPrefabsForLayer(layer);
            Go = af.FindPrefabByName(layer, inSourceVar.goName);
        }
        if (Go == null && af != null)
        {
            Go = af.GetMainPrefabForLayer(layer);
        }

        enabled = inSourceVar.enabled;
        if (enabled)
            inUse = true;

        probability = inSourceVar.probability;
        /*svPositionOffset = inSourceVar.svPositionOffset;
        svSize = inSourceVar.svSize;
        svRotation = inSourceVar.svRotation;
        svInvert = inSourceVar.svInvert;
        svBackToFront = inSourceVar.svBackToFront;
        svMirrorZ = inSourceVar.svMirrorZ;*/
    }

    public SourceVariant(GameObject inGO)
    {
        Initialize(inGO);
    }

    public SourceVariant(string inGOName)
    {
        Initialize();
        goName = inGOName;
    }

    // if copyGo = true we Instantiate a new clone copy of the go, if not get it from the name
    // otherwise we just copy its name and set go to the first
    // prefab in the prebs List. Useful when saving presets.
    // You only need to pass in 'AutoFenceCreator af' if you want it to locate the prefab by name instead (required when loading presets)
    public SourceVariant(SourceVariant inSourceVar, bool copyGo, AutoFenceCreator af, LayerSet layer)
    {
        List<GameObject> prefabs = af.GetPrefabsForLayer(layer);

        //Major change in v4.0 dealing with Gos instead of names
        if (copyGo == true)
        {
            GameObject newGo = null;
            if (inSourceVar.Go != null)
                newGo = inSourceVar.Go;
            else if (inSourceVar.goName != "")
            {
                newGo = af.FindPrefabByName(layer, inSourceVar.goName);
            }
            if (newGo == null && af != null)
            {
                newGo = prefabs[0];
            }
            Go = newGo; // Use the property to set the value
        }
        else
        {
            Go = prefabs[0]; // Use the property to set the value
        }
        enabled = inSourceVar.enabled;
        probability = inSourceVar.probability;

        /*svPositionOffset = inSourceVar.svPositionOffset;
        svSize = inSourceVar.svSize;
        svRotation = inSourceVar.svRotation;
        svInvert = inSourceVar.svInvert;
        svBackToFront = inSourceVar.svBackToFront;
        svMirrorZ = inSourceVar.svMirrorZ;*/
    }

    //============================================================================================================
    //
    //                      Static functions for SourceVariants
    //
    //============================================================================================================
    /// <summary>
    /// Creates a new List of SourceVariants. size or kMaxNumSourceVariants. Optionally pass a GameObject to set them all to.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="go"></param>
    /// <returns></returns>
    public static List<SourceVariant> CreateInitialisedSourceVariantList(GameObject go = null, int size = 0)
    {
        if (size == 0)
            size = AutoFenceCreator.kMaxNumSourceVariants;
        SourceVariant[] newVariants = CreateSourceVariantArray(size);
        List<SourceVariant> variantList = new List<SourceVariant>(newVariants);
        if (go != null)
            SetAllSourceVariantsToGo(variantList, go);
        return variantList;
    }

    public static void FindGoNameInSourceVariantsForLayer(string goName, LayerSet layer, AutoFenceCreator af)
    {
        af.GetSourceVariantsForLayer(layer);

        List<string> goNames = af.GetSourceVariantGoNamesForLayer(layer);

        for (int i = 0; i < goNames.Count; i++)
        {
            if (goNames[i] == goName)
            {
                Debug.Log($"Found {goName} in SourceVariants for layer {layer} at index {i}");
                return;
            }
        }
    }
    public static void ReplaceGosByNameInSourceVariantsForLayer(string goName, GameObject newGo,  LayerSet layer, AutoFenceCreator af)
    {
        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
        for (int i = 0; i < sourceVariants.Count; i++)
        {
            SourceVariant sv = sourceVariants[i];
            CheckSourceVariant(sv, layer, i, true, af);
            if (sv.go.name == goName)
            {
                Debug.Log($"Found {goName} in SourceVariants for layer {layer} at index {i}");
                sv.go = newGo;
                return;
            }
        }
    }
    public static void CheckSourceVariant(SourceVariant sv, LayerSet layer, int index, bool fix, AutoFenceCreator af, [CallerMemberNameAttribute]string caller = null)
    {
        if (sv != null && sv.Go != null)
            return;
                
        GameObject mainGo = af.GetMainPrefabForLayer(layer);
        string fixStr = "";
        if (fix)
            fixStr = $"Fixing with Main prefab: {mainGo.name}";
        if (sv == null)
        {
            Debug.Log($"SourceVariant[{index}] for {layer.String()} is null in {caller}.  {fixStr}");
            if (fix)
                sv = new SourceVariant(mainGo);
        }
        else if(sv.Go == null)
        {
            Debug.Log($"SourceVariant[{index}] for {layer.String()} has null Go in {caller}.  {fixStr}");
            if (fix)
                sv.Go = mainGo;
        }
 
    }



    public static SourceVariant[] CreateSourceVariantArray(int size)
    {
        SourceVariant[] array = new SourceVariant[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = new SourceVariant();
        }
        return array;
    }

    public static void UpdateGoNames(List<SourceVariant> sourceVariants)
    {
        foreach (SourceVariant sourceVariant in sourceVariants)
        {
            if (sourceVariant.Go != null)
            {
                sourceVariant.goName = sourceVariant.Go.name;
            }
        }
    }

    public static void SetAllSourceVariantsToGo(List<SourceVariant> sourceVariants, GameObject go)
    {
        foreach (SourceVariant sourceVariant in sourceVariants)
        {
            sourceVariant.Go = go;
            if (go != null)
                sourceVariant.goName = go.name;
        }
    }

    public static List<string> GetGoNamesFromSourceVariantList(List<SourceVariant> SourceVariants, bool strip = true)
    {
        List<string> goNameList = new List<string>();
        foreach (SourceVariant variant in SourceVariants)
        {
            if (variant != null && variant.Go != null)
            {
                string name = variant.Go.name;
                if (strip)
                    name = AutoFenceCreator.StripPanelRailFromNameStatic(name);
                goNameList.Add(name);
            }
            else
                goNameList.Add("nullGO");
        }
        return goNameList;
    }

    public static List<GameObject> GetAllGameObjectsFromVariations(List<SourceVariant> variants)
    {
        List<GameObject> goList = new List<GameObject>();

        int count = variants.Count;
        for (int i = 0; i < count; i++)
        {
            goList.Add(variants[i].Go);
        }
        return goList;
    }

    //------------
    public static void PrintSourceVariantList(List<SourceVariant> SourceVariantList, bool nameOnly = true)
    {
        Debug.Log("\n_____  Fence Variants  _____\n");
        for (int i = 0; i < SourceVariantList.Count; i++)
        {
            SourceVariant fenceVar = SourceVariantList[i];
            PrintSourceVariant(fenceVar, i.ToString(), nameOnly);
        }
    }

    private static void PrintSourceVariant(SourceVariant sourceVariant, string prefix = "", bool nameOnly = true)
    {
        string nameStr = "null go";
        if (sourceVariant.Go != null)
            nameStr = sourceVariant.Go.name;

        if (prefix != "")
        {
            if (sourceVariant.Go != null)
                Debug.Log("------  Variant " + prefix + "  ------\n" + nameStr + "\n");
            else
                Debug.Log("------  Variant " + prefix + ":  " + nameStr + "  ------\n");
        }
        if (nameOnly)
            return;
        //Debug.Log(sourceVariant.Go + "\n");
        //Debug.Log("sectionIndex = " + sourceVariant.sectionIndex + "\n");
        //Debug.Log("svBackToFront = " + sourceVariant.svBackToFront + "     svInvert = " + sourceVariant.svInvert + "     svMirrorZ = " + sourceVariant.svMirrorZ + "\n");
        //Debug.Log("pos = " + sourceVariant.svPositionOffset + "      svSize = " + sourceVariant.svSize + "      rot = " + sourceVariant.svRotation + "\n");
        Debug.Log("stepEnabled = " + sourceVariant.enabled + "        probability = " + sourceVariant.probability + "\n");
    }
}


/*public static List<GameObject> GetAllGOsFromSourceVariantList(List<SourceVariant> SourceVariants)
{
    List<GameObject> gameObjectList = new List<GameObject>();
    foreach (SourceVariant sourceVariant in SourceVariants)
    {
        gameObjectList.Add(sourceVariant.Go);
    }

    return gameObjectList;
}*/

public class SourceVariations
{
    public int numObjects = 1, numFlips = 1;
}