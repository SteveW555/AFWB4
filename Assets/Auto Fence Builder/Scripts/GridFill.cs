#pragma warning disable 0219 // disbale unused variables warnings. Most of them needed ready for updates
#pragma warning disable 0414 // variable assigned but not used.

using AFWB;
using System.Collections.Generic;
using UnityEngine;

public enum GridLayout
{ evenGrid, cornerDistributed };

public class GridFill : MonoBehaviour
{
    private const int kLeft = 0, kRight = 1;
    //===================================================================
    // For v4.0 Creates a grid (x,z) of V3 positions that follow a path of posts
    //===================================================================

    public void CreateGrid(List<NodeInfo> nodeList, int numX, int numZ, float width, GridLayout gridLayout)
    {
        int nodeCount = nodeList.Count;
        float distance = 0, prevDistance = 0, halfWidth = width * 0.5f, leftDistance = 0, rightDistance = 0;
        NodeInfo prevNode, node, nextNode;
        Vector3 pos = Vector3.zero, prevPos = Vector3.zero, nextPos = Vector3.zero;
        Vector3 currPosL = Vector3.zero, currPosR = Vector3.zero;
        Vector3[] elbowPointsStart = { Vector3.zero, Vector3.zero }; // the outer Left & Right points at the current node
        Vector3[] elbowPointsEnd = { Vector3.zero, Vector3.zero }; // the outer Left & Right points at the nextPos node
        Vector3[] elbowPointsStartPrev = { Vector3.zero, Vector3.zero };
        Vector3[] elbowPointsEndPrev = { Vector3.zero, Vector3.zero };

        float strideX = width / (numX - 1), strideZ = 0;

        elbowPointsEndPrev[kLeft] = elbowPointsStart[kLeft] = pos + nodeList[0].dirLeft * halfWidth;
        elbowPointsEndPrev[kRight] = elbowPointsStart[kRight] = pos + nodeList[0].dirRight * halfWidth;

        for (int i = 1; i < nodeCount; i++)
        {
            node = nodeList[i];
            pos = node.position;

            prevNode = nodeList[i - 1];
            prevPos = prevNode.position;
            prevDistance = (pos - prevPos).magnitude;

            if (i < nodeCount - 1)
            {
                nextNode = nodeList[i + 1];
                nextPos = nextNode.position;
                distance = (nextPos - pos).magnitude;

                Vector3 forward = (nextPos - pos).normalized;

                elbowPointsStart = elbowPointsEndPrev;

                if (gridLayout == GridLayout.cornerDistributed)
                {
                    //-- Next
                    if (i < nodeCount - 2)
                        elbowPointsEnd = VectorUtilitiesTCT.GetMiterPoints(pos, nextPos, nodeList[i + 2].position, width);
                    //-- Next is the last node
                    else
                    {
                        elbowPointsEnd[kLeft] = nextPos + nextNode.avgLeft * halfWidth;
                        elbowPointsEnd[kRight] = nextPos + nextNode.avgRight * halfWidth;
                    }
                }
                if (gridLayout == GridLayout.evenGrid)
                {
                    elbowPointsEnd[kLeft] = nextPos + nextNode.dirLeft * halfWidth;
                    elbowPointsEnd[kRight] = nextPos + nextNode.dirRight * halfWidth;
                }

                strideZ = distance / (numZ);

                Vector3 currPos = Vector3.zero;
                for (int z = 0; z < numZ; z++)
                {
                    float a = 0;
                    for (int x = 0; x < numX; x++)
                    {
                        currPos.x += x * strideX;
                    }
                }
                elbowPointsStartPrev = elbowPointsStart;
                elbowPointsEndPrev = elbowPointsEnd;
            }
        }
    }
}