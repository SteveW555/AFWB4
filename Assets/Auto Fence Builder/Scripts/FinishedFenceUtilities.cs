using AFWB;
using UnityEngine;

public class FinishedFenceUtilities : MonoBehaviour
{
    public string presetID;
    public AutoFenceCreator af = null;
    public Transform finishedFolderRoot;

    private void Awake()
    {
        af = GameObject.FindObjectOfType<AutoFenceCreator>();
        finishedFolderRoot = transform.root;
    }

    private void Reset()
    {
        af = GameObject.FindObjectOfType<AutoFenceCreator>();
        finishedFolderRoot = transform.root;
    }
}