using AFWB;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

//============================================================================================================
/// <summary>
/// Settings for a Single variation set to individual fence parts in the Scene
/// <br>    - sourceVariantIndex: -  Index into the List of defined SourceVariants (between 1 and kMaxNumSourceVariants). Not a direct Prefab index.</br>
/// <br>  - Note: Does not reference a GO directly </br>
/// <br>    - Also: pos / svSize / rot / svInvert / svBackToFront / svMirrorZ / enabled </br>
/// </summary>
/// //============================================================================================================
[System.Serializable]
public class SinglesItem
{
    public int sourceVariantIndex; //index into the list of SourceVariants for this layer
    public int sectioinIndex; //the section of the fence it is used by
    public Vector3 pos;
    public Vector3 size = Vector3.one;
    public Vector3 rot;
    public bool invert, backToFront, mirrorZ;
    public bool inUse = false; //whether to use or ignore this variant
    public bool enabled = false; // whether to disable the section in the scene

    public SinglesItem()
    {
        Init();
    }

    public void Init()
    {
        invert = false; backToFront = false; mirrorZ = false;
        sourceVariantIndex = 0;
        pos = Vector3.zero; size = Vector3.one; rot = Vector3.zero;
        inUse = false;
        enabled = false;
    }

    public void ToggleSingleInUse()
    {
        inUse = !inUse;
    }
}

//============================================================================================================
/// <summary>
///         - A list of SinglesItems, where the index refers to a particular panel/post built in the scene
/// <br>    - singleVariants is the List of SinglesItems </br>
/// <br>    - numInUse can never be greater than the number of layer parts that exist. We don't use singleVariants.Count as we don't necessarily clear that list </br>
/// </summary>
/// //============================================================================================================
[System.Serializable]
public class SinglesContainer
{
    public List<SinglesItem> singleVariants;
    public int numInUse = 0; //number of singles where inUse = true;
    public int maxIndexInUse = 0; //highest index of a single where inUse = true;
    private AutoFenceCreator af;

    // Constructor called from AutoFenceCreator for each layer: SinglesContainer[] railSinglesContainer and SinglesContaine postSinglesContainer
    public SinglesContainer(AutoFenceCreator af)
    {
        singleVariants = new List<SinglesItem>();
        singleVariants.Add(new SinglesItem());
        maxIndexInUse = numInUse = 0;
        this.af = af;
    }

    public void SetAllDefaults(bool setNumInUseToZero = true)
    {
        int count = singleVariants.Count;
        for (int i = 0; i < count; i++)
        {
            singleVariants[i].Init();
        }
        if (setNumInUseToZero)
            numInUse = 0;
    }

    public void ClearSingles()
    {
        singleVariants.Clear();
        numInUse = maxIndexInUse = 0;
    }

    public void ToggleSingleInUse(int index)
    {
        int count = singleVariants.Count;
        for (int i = 0; i < count; i++)
        {
            singleVariants[i].inUse = !singleVariants[i].inUse;
        }
    }

    /// <summary>
    /// Gets the SingleVariant at the specified single index.
    /// <br>If the section index is beyond the bounds, return for section [0] </br>
    /// <br><br>singleVariants[sectionIndex].sourceVariantIndex = sourceVariantIndex;</br></br>
    /// <br>numInUse = singleVariants.Count;</br>
    /// </summary>
    public SinglesItem GetSingleAtSectionIndex(int singleIndex)
    {
        if (singleIndex >= singleVariants.Count)
            singleIndex = 0;

        if (singleVariants.Count == 0)
            singleVariants.Add(new SinglesItem());

        SinglesItem variant = singleVariants[singleIndex];
        return variant;
    }

    //======================================================================
    // SetupSingle()[from scene view context menus] -> AddSinglesSourceVariantForLayerWithSectionIndex() -> Here
    // OR... SinglesEditor Randomize Button
    // This should be the only route through which the singles are added to the singles list

    // As this is a class method for SinglesContainer, it is already referencing the correct LayerSet
    // hence we don't need to pass the layerSet as a parameter
    public void SetSingleSourceVariantAtSectionIndex(int sectionIndex, int sourceVariantIndex)
    {
        // There should be enough singles, ** one for each section **, prior to adding the new one
        // If there aren't enough, add some empty ones between the last one assigned and the new one
        if (singleVariants.Count <= sectionIndex)
        {
            int numToAdd = sectionIndex - singleVariants.Count + 1;
            SinglesItem SinglesItem = new SinglesItem();
            singleVariants.AddRange(Enumerable.Repeat(SinglesItem, numToAdd));
        }
        singleVariants[sectionIndex].sourceVariantIndex = sourceVariantIndex;
        singleVariants[sectionIndex].inUse = true;
        singleVariants[sectionIndex].enabled = true;

        // Maybe some of them have been disabled or set back to the main source prefab
        // so find the highest index that is in use as a modified variant
        maxIndexInUse = 0;
        numInUse = 0;
        for (int i = 0; i < singleVariants.Count; i++)
        {
            SinglesItem variant = singleVariants[i];
            if (variant.inUse)
            {
                numInUse++;
                maxIndexInUse = i;
            }
        }
    }

    //--------------------------------------------
    private int[] GetNewSingles(int n)
    {
        int[] newSingles = new int[n];
        for (int i = 0; i < n; i++)
        {
            newSingles[i] = -1;
        }
        return newSingles;
    }

    //-------------------------
    //Ensure the Singles lists grow with the number of post positions
    /*void CheckSinglesLengths()
    {
        int shortage = 0;
        if (allPostPositions.Count > railSinglesIndices[0].Count)
        {
            shortage = allPostPositions.Count - railSinglesIndices[0].Count;
            railSinglesIndices[0].AddRange(GetNewSingles(shortage + 10));
        }
        if (allPostPositions.Count > railSinglesIndices[1].Count)
        {
            shortage = allPostPositions.Count - railSinglesIndices[1].Count;
            railSinglesIndices[1].AddRange(GetNewSingles(shortage + 10));
        }
        if (allPostPositions.Count > postSinglesIndices.Count)
        {
            shortage = allPostPositions.Count - postSinglesIndices.Count;
            postSinglesIndices.AddRange(GetNewSingles(shortage + 10));
        }
    }*/

    //-------------------------------------
    public void ResetAllRailSingles(AutoFenceCreator af)
    {
        af.railSinglesContainer[0].SetAllDefaults();
        af.railSinglesContainer[1].SetAllDefaults();
    }

    //-------------------------------------
    public void ResetAllRailSinglesA(AutoFenceCreator af)
    {
        af.railSinglesContainer[0].SetAllDefaults();
    }

    //-------------------------------------
    public void ResetAllRailSinglesB(AutoFenceCreator af)
    {
        af.railSinglesContainer[1].SetAllDefaults();
    }

    //-------------------------------------
    public void ResetAllPostSingles(AutoFenceCreator af)
    {
        af.postSinglesContainer.SetAllDefaults();
    }

    //-------------------------------------
    public void ResetAllSinglesForLayer(LayerSet layerSet, AutoFenceCreator af)
    {
        if (layerSet == LayerSet.railALayer)
            ResetAllRailSinglesA(af);
        else if (layerSet == LayerSet.railBLayer)
            ResetAllRailSinglesB(af);
        else if (layerSet == LayerSet.postLayer)
            ResetAllPostSingles(af);
    }

    //-------------------------------------
    public void ClearAllSinglesForLayer(LayerSet layerSet, AutoFenceCreator af)
    {
        if (layerSet == LayerSet.railALayer)
            ClearAllRailSinglesA(af);
        else if (layerSet == LayerSet.railBLayer)
            ClearAllRailSinglesB(af);
        else if (layerSet == LayerSet.postLayer)
            ClearAllPostSingles(af);
    }

    //-------------------------------------
    public void ClearAllSingles(AutoFenceCreator af)
    {
        ClearAllRailSinglesA(af);
        ClearAllRailSinglesB(af);
        ClearAllPostSingles(af);
    }

    //-------------------------------------
    public void ClearAllRailSinglesA(AutoFenceCreator af)
    {
        af.railSinglesContainer[0].ClearSingles();
    }

    //-------------------------------------
    public void ClearAllRailSinglesB(AutoFenceCreator af)
    {
        af.railSinglesContainer[1].ClearSingles();
    }

    //-------------------------------------
    public void ClearAllPostSingles(AutoFenceCreator af)
    {
        af.postSinglesContainer.ClearSingles();
    }

    //-------------------------------------
    public void ToggleAllSingleVariants(LayerSet layerSet, bool enabled, AutoFenceCreator af)
    {
        List<SinglesItem> singleVariants = GetSingleVariantsForLayer(layerSet, af);

        foreach (SinglesItem singleVar in singleVariants)
        {
            singleVar.inUse = !singleVar.inUse;
        }
    }

    public int GetNumSinglesInUseForLayer(LayerSet layer, int inSingleSectionIndex, AutoFenceCreator af)
    {
        SinglesContainer singlesContainer = GetSinglesForLayer(layer, af);
        return singlesContainer.numInUse;
    }

    //-----------------------
    public bool GetUseSinglesForLayer(LayerSet layer, AutoFenceCreator af)
    {
        if (af == null)
            Debug.Log("af is null");
        if (af.useRailSingles == null)
            Debug.Log("af.useRailSingles is null");

        if (layer == LayerSet.railALayer)
            return af.useRailSingles[0];
        else if (layer == LayerSet.railBLayer)
            return af.useRailSingles[1];
        else if (layer == LayerSet.postLayer)
            return af.usePostSingles;

        return false;
    }

    public SinglesContainer GetSinglesForLayer(LayerSet layer, AutoFenceCreator af)
    {
        if (af == null)
        {
            //Debug.Log("af is null");
            return null;
        }

        if (layer == LayerSet.railALayer)
        {
            if (af.railSinglesContainer == null)
                Debug.Log("railSinglesContainer is null");
            return af.railSinglesContainer[0];
        }
        else if (layer == LayerSet.railBLayer)
            return af.railSinglesContainer[1];
        else if (layer == LayerSet.postLayer)
        {
            if (af.postSinglesContainer == null)
                Debug.Log("postSinglesContainer is null");
            return af.postSinglesContainer;
        }
        return null;
    }

    public List<SinglesItem> GetSingleVariantsForLayer(LayerSet layer, AutoFenceCreator af, bool warning = true, [CallerMemberName] string caller = null)
    {
        //The singlesContainer contains the list of SinglesItems
        //This List should be kNumSourceVariants long (usually 8+1)
        List<SinglesItem> SinglesItems = null;

        if (layer == LayerSet.railALayer)
            SinglesItems = af.railSinglesContainer[0].singleVariants;
        else if (layer == LayerSet.railBLayer)
            SinglesItems = af.railSinglesContainer[1].singleVariants;
        else if (layer == LayerSet.postLayer)
            SinglesItems = af.postSinglesContainer.singleVariants;

        if (warning)
        {
            int numFenceSections = af.GetNumSectionsBuiltForLayer(layer);
            if (SinglesItems.Count < numFenceSections)
            {
                Debug.LogWarning($"[GetSingleVariantsForLayer:  {af.GetLayerNameAsString(layer)}   called by {caller}() ] " +
                    $"\nSinglesItems.Count {SinglesItems.Count} < numFenceSections {numFenceSections}. \n");
            }
        }
        return SinglesItems;
    }

    public SinglesItem GetSingleItemForLayer(LayerSet layer, int sectionIndex, AutoFenceCreator af)
    {
        SinglesItem SinglesItem = null;
        SinglesContainer singlesContainer = GetSinglesForLayer(layer, af);
        singlesContainer.GetSingleAtSectionIndex(sectionIndex);
        return SinglesItem;
    }

    public int GetNumberOfSinglesInUseForLayer(LayerSet layer, AutoFenceCreator af)
    {
        SinglesContainer singlesContainer = GetSinglesForLayer(layer, af);
        return singlesContainer.numInUse;
    }

    public void PrintSinglesForLayer(LayerSet layer, bool inUseOnly = false)
    {
        SinglesContainer singlesContainer = GetSinglesForLayer(layer, af);
        // print the fileds of the singlesContainer
        Debug.Log($"SinglesContainer for {layer.ToString()} : \n");
        Debug.Log($"numInUse : {singlesContainer.numInUse}  maxIndexInUse : {singlesContainer.maxIndexInUse} \n");

        List<SinglesItem> SinglesItemList = GetSingleVariantsForLayer(layer, af);

        List<SourceVariant> sourceVariants = af.GetSourceVariantsForLayer(layer);
        for (int i = 0; i < SinglesItemList.Count; i++)
        {
            SinglesItem singleVar = SinglesItemList[i];
            int variantIndex = singleVar.sourceVariantIndex;
            //Debug.Log("sourceVariantIndex  " + sourceVariantIndex + "\n");
            //Debug.Log("enabled = " + singleVar.enabled + "\n");
            SourceVariant sourceVariant = sourceVariants[variantIndex];

            string nameStr = "Null";
            if (sourceVariant != null)
                nameStr = sourceVariant.Go.name;
            if (inUseOnly == false || singleVar.inUse == true)
                Debug.Log($"Single {i}:    {nameStr}    [" + variantIndex + "]   inUse: " + singleVar.inUse + "  Enabled:  " + singleVar.enabled + "\n"); ;
        }
    }
}