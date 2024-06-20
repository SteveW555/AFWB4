using MeshUtils;
using System;
using UnityEngine;

namespace AFWB
{
    public enum SubSpacingMode
    { fixedNumBetweenPosts, dependsOnSectionLength, postPositionsOnly, nodePositionsOnly };

    public partial class AutoFenceCreator
    {
        public SubSpacingMode subsSpacingMode = SubSpacingMode.fixedNumBetweenPosts;

        //=================================================================
        public void BuildSubposts(Vector3 currPostPos, Vector3 nextPostPos, int sectionIndex, bool isLastPost = false)
        {
            //if (isLastPost == true)
            //return;

            PostVector postVector = PostVector.GetPostVectorAtIndex(sectionIndex);
            float distance = Vector3.Distance(currPostPos, nextPostPos);
            Vector3 currDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(nextPostPos, currPostPos);
            Vector3 directionVector = (nextPostPos - currPostPos).normalized;
            Vector3 forward = directionVector;
            Vector3 right = MeshUtilitiesAFWB.RotatePointAroundPivot(directionVector, Vector3.up, new Vector3(0, 90, 0));
            Vector3 currSubpostPos = currPostPos;
            bool isFirstPost = (sectionIndex == 0);

            GameObject thisSubJoiner = null;
            float actualSubSpacing = 1, avgPostVectorHeading = 0, postVectorHeading = 0;

            // Add the incominmg post to the Combined List of strings of Posts + Subposts
            postAndSubpostStringList.Add($"{postAndSubpostStringList.Count.ToString()} Post[{sectionIndex.ToString()}]");
            postsAndSubpostsCombined.Add(postsPool[sectionIndex].transform);

            int currClickPointIndex = IsCloseClickPoint(currPostPos); // -1 on failure
            int nextClickPointIndex = IsCloseClickPoint(nextPostPos);
            bool currIsClickPoint = (currClickPointIndex > -1);
            bool nextIsClickPoint = (nextClickPointIndex > -1);
            numSubpostsPerSection = CalcSubpostsPerSection(isLastPost, distance, ref actualSubSpacing);
            if (isLastPost == true)
                numSubpostsPerSection = 1;
            Vector3 variantScaling = Vector3.one, variantOffset = Vector3.zero, variantRotation = Vector3.zero;

            float radius = 1.0f;
            int ownerPostIndex = sectionIndex;

            for (int i = 0; i < numSubpostsPerSection; i++)
            {
                //int repetitionIndex = (sectionIndex * numSubs) + i;
                GameObject thisSub = RequestSub(subpostsBuiltCount).gameObject;
                if (thisSub == null)
                {
                    print("Missing Sub " + i + " Have you deleted one?");
                    continue;
                }
                thisSub.hideFlags = HideFlags.None;
                thisSub.SetActive(true);
                thisSub.name = "Sub " + subpostsBuiltCount.ToString();
                thisSub.transform.parent = subpostsFolder.transform;

                thisSub.transform.position = currSubpostPos = currPostPos;
                ownerPostIndex = sectionIndex;

                // In stepped mode they take the height position from 'currPostPos' (the previous post, rather than the nextPos)
                if (slopeMode[kRailALayerInt] == SlopeMode.step)
                {
                    Vector3 stepPos = currPostPos;
                    stepPos.y = currPostPos.y;
                    currSubpostPos = thisSub.transform.position = stepPos;
                }

                //================= Subpost Variations ================================
                int currSeqIndex = -1;
                SeqItem currSeqItem = null;

                //Debug.Log("sectionIndex " + sectionIndex + ":" + i  + "      currSeqIndex " + currSeqIndex + "       stepEnabled " + currSeqItem.stepEnabled + "   " + thisSub.name +
                //"    "  + variantScaling + "\n");

                if (currSeqItem != null && currSeqItem.stepEnabled == false)
                {
                    thisSub.hideFlags = HideFlags.HideInHierarchy;
                    thisSub.SetActive(false);
                    subpostsBuiltCount++;
                    continue;
                }
                //=================================
                //            Position
                //=================================
                // Interpolate the subpostsPool position between currPostPos & nextPostPos, but keep Y fixed in Stepped Mode
                //Vector3 right = thisSub.transform.right;
                Vector3 offset = Vector3.zero, varOffset = Vector3.zero;
                int indexOffset = i + 1;
                if (addSubpostAtPostPointAlso)
                    indexOffset = i;
                Vector3 move = directionVector * actualSubSpacing * (indexOffset);
                float moveY = move.y;
                float subFinalHeight = subpostScale.y * globalScale.y;
                // Modes for subsSpacingMode: 0 "Fixed Number Between Posts",  1 "Depends on Section Length",  2 "Post Positions Only",  3 "Post Positions Only"

                //      Fixed or DistanceTCT Dependent
                //====================================
                if ((subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.dependsOnSectionLength) && i > -1)
                {
                    if (slopeMode[kRailALayerInt] == SlopeMode.step)
                        moveY = 0;
                    thisSub.transform.position += new Vector3(move.x, moveY, move.z);
                    thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                    offset = right * subpostPositionOffset.z;
                    thisSub.transform.position += offset;

                    offset = forward * subpostPositionOffset.x;
                    thisSub.transform.position += offset;
                }
                //      Post Positions Only
                //====================================
                else if (subsSpacingMode == SubSpacingMode.postPositionsOnly || i == -1)
                {
                    thisSub.transform.position = currSubpostPos;
                    thisSub.name += "_P"; // denotes that it's at a Post Position. Useful for some Extra modes
                    thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                    offset = right * subpostPositionOffset.z;
                    thisSub.transform.position += offset;

                    offset = forward * subpostPositionOffset.x;
                    thisSub.transform.position += offset;
                }
                //      Node Positions Only
                //====================================
                else if (subsSpacingMode == SubSpacingMode.nodePositionsOnly && currIsClickPoint == true || i == -1)
                {
                    thisSub.transform.position = currSubpostPos;
                    thisSub.name += "_P"; // denotes that it'i at a Post Position. Useful for some Extra modes
                    thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                    offset = right * subpostPositionOffset.z;
                    thisSub.transform.position += offset;

                    offset = forward * subpostPositionOffset.x;
                    thisSub.transform.position += offset;
                }
                //-- We only want node positions and this isn't one, so continue
                else if ((subsSpacingMode == SubSpacingMode.nodePositionsOnly && currIsClickPoint == false))
                    continue;

                if (useSubpostVariations == true /*&& variantOffset != Vector3.zero*/)
                {
                    varOffset = right * variantOffset.z;
                    //Debug.Log("var right  " + right + "         z  " + variantOffset.z + "\n");
                    thisSub.transform.position += varOffset;

                    varOffset = forward * variantOffset.x;
                    //Debug.Log("forward  " + thisSub.transform.forward + "         x  " + subpostPositionOffset.x + "\n");
                    thisSub.transform.position += varOffset;
                }

                //=================================
                //            Rotation
                //=================================
                thisSub.transform.rotation = Quaternion.identity;
                AlignSubpostToSurfaceNormal(ref thisSub);

                //      Rotate Avg Corner Direction
                //=====================================
                if (subsSpacingMode == SubSpacingMode.nodePositionsOnly || subsSpacingMode == SubSpacingMode.postPositionsOnly)
                {
                    avgPostVectorHeading = postVector.GetDirAvgHeading();
                    thisSub.transform.Rotate(new Vector3(0, avgPostVectorHeading+180, 0), Space.Self);
                }
                else
                {

                    //      Rotate from Fence Direction
                    //=====================================
                    //thisSub.transform.Rotate(new Vector3(0, currDirection.y, 0), Space.Self);

                    postVectorHeading = postVector.GetHeading();
                    thisSub.transform.Rotate(new Vector3(0, postVectorHeading+180, 0), Space.Self);

                    //if (subsSpacingMode == SubSpacingMode.postPositionsOnly || subsSpacingMode == SubSpacingMode.nodePositionsOnly && isLastPost == true) // using 'replicate postsPool only' mode
                    //thisSub.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
                }

                //      Rotate from Transform Box
                //======================================
                thisSub.transform.Rotate(new Vector3(subpostRotation.x, subpostRotation.y, subpostRotation.z), Space.Self);
                if (useSubpostVariations == true && variantRotation != Vector3.zero)
                    thisSub.transform.Rotate(variantRotation);

                //===================== Apply sine to height of subpostsPool =======================
                if (useSubWave && (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts))
                {
                    float realMoveForward = move.magnitude;
                    float sinValue = Mathf.Sin((((realMoveForward / distance) * Mathf.PI * 2) + subWavePosition) * subWaveFreq);
                    sinValue *= subWaveAmp * globalScale.y;
                    subFinalHeight = (subpostScale.y) + sinValue + (subWaveAmp * globalScale.y);
                    //==== CreateMergedPrefabs Sub Joiners ====
                    if (i > 0 && useSubJoiners)
                    {
                        thisSubJoiner = RequestSubJoiner(subJoinersBuiltSoFarCount++);
                        if (thisSubJoiner != null)
                        {
                            thisSubJoiner.transform.position = thisSub.transform.position + new Vector3(0, (subFinalHeight) - .01f, 0);
                            thisSubJoiner.transform.rotation = Quaternion.identity;
                        }
                    }
                }
                //=========== Scale ==============
                Vector3 scale = Vector3.one;
                scale.x *= subpostScale.x * globalScale.x;
                scale.y *= subFinalHeight;
                scale.z *= subpostScale.z * globalScale.z;

                if (useSubpostVariations == true && variantScaling != Vector3.one)
                    scale = Vector3.Scale(scale, variantScaling);

                //if(i==0) { scale.y *= 2; } //??? remove this

                thisSub.transform.localScale = scale;

                //=============== SubPost Spreading ================
                // currPostPos quick and dirty spreading/bunching. TODO add a choice of spreading algorithms
                if (Math.Abs(subPostSpread) >= 0.1f)
                {
                    float spread = subPostSpread;
                    spread *= interPostDist / distance;

                    float halfDist = distance / 2;
                    Vector3 vecFromA = currSubpostPos - thisSub.transform.localPosition;
                    float distFromA = Math.Abs(vecFromA.magnitude);
                    Vector3 vecFromB = nextPostPos - thisSub.transform.localPosition;
                    float distFromB = Math.Abs(vecFromB.magnitude);
                    Vector3 moveSpread = Vector3.zero;
                    if (Math.Abs(distFromA - distFromB) < 0.1f) { } //TODO
                    else if (distFromA < distFromB)
                    {
                        moveSpread = distFromA * (distFromA / 2) * directionVector * spread;
                    }
                    else if (distFromA > distFromB)
                    {
                        moveSpread = distFromB * (distFromB / 2) * -directionVector * spread;
                    }
                    moveSpread.y = 0;

                    moveSpread = -thisSub.transform.InverseTransformVector(moveSpread);
                    thisSub.transform.Translate(moveSpread);

                    Vector3 flatA = new Vector3(currSubpostPos.x, 0, currSubpostPos.z);
                    Vector3 flatB = new Vector3(nextPostPos.x, 0, nextPostPos.z);
                    Vector3 flatPos = new Vector3(thisSub.transform.localPosition.x, 0, thisSub.transform.localPosition.z);

                    Vector3 flatDeltaVec = flatB - flatA;

                    float finalFlatDistFromA = (flatPos - flatA).magnitude;
                    float distProp = finalFlatDistFromA / flatDeltaVec.magnitude;
                    float spanHeightDelta = (nextPostPos - currSubpostPos).y;
                    float yPos = currSubpostPos.y + (spanHeightDelta * distProp);
                    thisSub.transform.localPosition = new Vector3(thisSub.transform.localPosition.x, yPos, thisSub.transform.localPosition.z);
                }

                //=============== Sub Joiners ================
                if (i > 0 && useSubJoiners && thisSubJoiner != null) // need to do this after the final sub calculations
                {
                    Vector3 a = subpostsPool[subpostsBuiltCount].transform.position + new Vector3(0, subpostsPool[subpostsBuiltCount].transform.localScale.y, 0);
                    Vector3 b = subpostsPool[subpostsBuiltCount - 1].transform.position + new Vector3(0, subpostsPool[subpostsBuiltCount - 1].transform.localScale.y, 0);
                    float joinerDist = Vector3.Distance(b, a);
                    thisSubJoiner.transform.localScale = new Vector3(joinerDist, thisSubJoiner.transform.localScale.y, thisSubJoiner.transform.localScale.z);
                    Vector3 subJoinerDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(a, b);
                    thisSubJoiner.transform.Rotate(new Vector3(0, subJoinerDirection.y - 90, -subJoinerDirection.x + 180));
                    thisSubJoiner.GetComponent<Renderer>().sharedMaterial = thisSub.GetComponent<Renderer>().sharedMaterial;
                }
                //=============== Force Subs to Ground ================
                if (forceSubsToGroundContour)
                {
                    SetIgnorePartsColliders(true); // temporarily ignore other fence colliders to find distanceToNextPost to ground
                    Vector3 currPos = thisSub.transform.position;
                    float rayStartHeight = globalScale.y * 3.0f;
                    //float margin = 10;
                    currPos.y += rayStartHeight;
                    RaycastHit hit;
                    if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
                    {
                        if (hit.collider.gameObject != null)
                        {
                            float distToGround = hit.distance + 0.0f - subsGroundBurial; //in the ground a little
                            thisSub.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                            scale.y += (distToGround - rayStartHeight) / 2;
                            thisSub.transform.localScale = scale;
                            //Debug.Log(hit.distanceToNextPost + "  " + rayStartHeight + "\n");
                        }
                    }
                    SetIgnorePartsColliders(false);
                }
                else if (keepSubsAboveGround)
                {
                    //If not forced DOWN to ground we need to make sure that at least they are at not below fround
                    SetIgnorePartsColliders(true); // temporarily ignore other fence colliders to find distanceToNextPost to ground
                    Vector3 currPos = thisSub.transform.position;
                    float currY = currPos.y;
                    float rayStartHeight = globalScale.y * 3.0f;
                    currPos.y += rayStartHeight;
                    RaycastHit hit;
                    if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
                    {
                        if (hit.collider.gameObject != null)
                        {
                            float distToGround = hit.distance + 0.0f;
                            if (hit.distance < rayStartHeight)
                            {
                                thisSub.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                            }
                            //Debug.Log(hit.distanceToNextPost + "  " + rayStartHeight + "    " + currY + "\n");
                        }
                    }
                    SetIgnorePartsColliders(false);
                }
                //== Random Variation ==

                if (allowSubpostRandomization)
                {
                    if (allowHeightVariationSubpost)
                    {
                        float randHeightScale = UnityEngine.Random.Range(minRandHeightSubpost, maxRandHeightSubpost);
                        thisSub.transform.localScale = Vector3.Scale(thisSub.transform.localScale, new Vector3(1, randHeightScale, 1));
                    }
                    //================= Add Random Rotations ===========================
                    if (allowRandSubpostSmallRotationVariation)
                    {
                        float xRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.x, smallRotationAmountSubpost.x);
                        float yRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.y, smallRotationAmountSubpost.y);
                        float zRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.z, smallRotationAmountSubpost.z);
                        thisSub.transform.Rotate(new Vector3(xRot, yRot, zRot));
                    }
                    //================= Add Random Quantized Rotations ===========================
                    if (allowQuantizedRandomSubpostRotation)
                    {
                        float totalRotAmount = 0;
                        if (quantizeRotAngleSubpost == -180)
                        {
                            if (isLastPost == true)
                                totalRotAmount = 0;

                            if (isLastPost == false && quantizeRotProbSubpost == 0 && i % 2 == 1)
                                totalRotAmount = 180;
                        }
                        else
                        {
                            int num = UnityEngine.Random.Range(0, 24);
                            totalRotAmount = num * quantizeRotAngleSubpost;
                            totalRotAmount = totalRotAmount % 360;
                        }
                        thisSub.transform.Rotate(new Vector3(0, totalRotAmount, 0));
                    }
                    //- Because the direction is reversed for the last post, we need to correct the rotation
                    //if (isLastPost == true)
                    //thisSub.transform.Rotate(new Vector3(0, 180, 0), Space.Self);
                    //========= Chance of Missing SubPost ==============
                    bool disableSub = false;
                    if ((chanceOfMissingSubpost == -1 && i % 2 == 1))
                        disableSub = true;
                    else if (chanceOfMissingSubpost > 0 && UnityEngine.Random.value < chanceOfMissingSubpost)
                        disableSub = true;
                    if (disableSub == true)
                    {
                        thisSub.gameObject.SetActive(false); // deleted when finalized
                        thisSub.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                subpostsBuiltCount++;
                thisSub.isStatic = usingStaticBatching;
                //====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
                int numSubsFolders = (subpostsBuiltCount / objectsPerFolder) + 1;
                string subsDividedFolderName = "SubsGroupedFolder" + (numSubsFolders - 1);
                GameObject subsDividedFolder = GameObject.Find("Current Fences Folder/Subs/" + subsDividedFolderName);
                if (subsDividedFolder == null)
                {
                    subsDividedFolder = new GameObject(subsDividedFolderName);
                    subsDividedFolder.transform.parent = subpostsFolder.transform;
                    if (addCombineScripts)
                    {
                        CombineChildrenPlus combineChildren = subsDividedFolder.AddComponent<CombineChildrenPlus>();
                        if (combineChildren != null)
                            combineChildren.combineAtStart = true;
                    }
                }

                CreateColliderForLayer(thisSub, thisSub.transform.localPosition, LayerSet.subpostLayer);

                postAndSubpostStringList.Add($"{postAndSubpostStringList.Count.ToString()} - Sub[{sectionIndex.ToString()} {i.ToString()}]");
                postsAndSubpostsCombined.Add(thisSub.transform);

                thisSub.transform.parent = subsDividedFolder.transform;

                if (countTriangles)
                    subPostsTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(thisSub);
            }
        }

        private int CalcSubpostsPerSection(bool isLastPost, float distance, ref float actualSubSpacing)
        {
            int numSubs = 1;
            //      DistanceTCT Dependent
            //====================================
            if (subsSpacingMode == SubSpacingMode.dependsOnSectionLength) // depends on istanceToNextPost between postsPool
            {
                float idealSubSpacing = subSpacing;
                numSubs = (int)Mathf.Round(distance / idealSubSpacing);
                if (idealSubSpacing > distance)
                    numSubs = 1;
                actualSubSpacing = distance / (numSubs + 1);
            }
            //      Fixed NUmber Between Posts
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts)
            {
                numSubs = (int)subSpacing;
                actualSubSpacing = distance / (numSubs + 1);
            }
            //      Post Positions Only
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.postPositionsOnly) // replicate post position
            {
                numSubs = 1;
            }
            //      Clickpoint Post Positions Only
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.nodePositionsOnly) // replicate post position
            {
                numSubs = 1;
            }

            //-- Add an extra one if we're duplicating the post at the post point
            if (addSubpostAtPostPointAlso == true && isLastPost == false && (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.dependsOnSectionLength))
            {
                numSubs += 1;
                //start = forceSubAtPost;
            }
            return numSubs;
        }

        //==================================================================================================================
        // Uncomment below for v4.1
        //==================================================================================================================
        /*public void BuildSubposts(Vector3 currPostPos, Vector3 nextPostPos, int sectionIndex, bool isLastPost = false, DuplicateMode subpostDuplicateMode = DuplicateMode.single, float dualWidth = 1.0f)
        {
            if (subpostDuplicateMode == DuplicateMode.single || subpostDuplicateMode == DuplicateMode.dualWithCentre)
                BuildSubposts(currPostPos, nextPostPos, sectionIndex, isLastPost);
            else if (subpostDuplicateMode == DuplicateMode.dual)
            {
                BuildSubposts(currPostPos, nextPostPos, sectionIndex, isLastPost);
            }
        }*/

        /*public void BuildSubposts(Vector3 currPostPos, Vector3 nextPostPos, int sectionIndex, bool isLastPost = false)
        {
            float distance = Vector3.DistanceTCT(currPostPos, nextPostPos);
            Vector3 currDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(nextPostPos, currPostPos);
            Vector3 directionVector = (nextPostPos - currPostPos).normalized;
            Vector3 forward = directionVector;
            Vector3 right = MeshUtilitiesAFWB.RotatePointAroundPivot(directionVector, Vector3.up, new Vector3(0, 90, 0));
            Vector3 currSubpostPos = currPostPos;
            bool isFirstPost = (sectionIndex == 0);

            int numSubs = 1;
            GameObject thisSubJoiner = null;
            float actualSubSpacing = 1;

            // Add the incominmg post to the Combined List of strings of Posts + Subposts
            postAndSubpostStringList.Add($"{postAndSubpostStringList.Count.ToString()} Post[{sectionIndex.ToString()}]");
            postsAndSubpostsCombined.Add(postsPool[sectionIndex].transform);

            int currClickPointIndex = IsCloseClickPoint(currPostPos); // -1 on failure
            int nextClickPointIndex = IsCloseClickPoint(nextPostPos);
            bool currIsClickPoint = (currClickPointIndex > -1);
            bool nextIsClickPoint = (nextClickPointIndex > -1);

            //      DistanceTCT Dependent
            //====================================
            if (subsSpacingMode == SubSpacingMode.dependsOnSectionLength) // depends on istanceToNextPost between postsPool
            {
                float idealSubSpacing = subSpacing;
                numSubs = (int)Mathf.Round(distance / idealSubSpacing);
                if (idealSubSpacing > distance)
                    numSubs = 1;
                actualSubSpacing = distance / (numSubs + 1);
            }
            //      Fixed NUmber Between Posts
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts)
            {
                numSubs = (int)subSpacing;
                actualSubSpacing = distance / (numSubs + 1);
            }
            //      Post Positions Only
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.postPositionsOnly) // replicate post position
            {
                numSubs = 1;
            }
            //      Clickpoint Post Positions Only
            //=====================================
            else if (subsSpacingMode == SubSpacingMode.nodePositionsOnly) // replicate post position
            {
                numSubs = 1;
            }

            //int start = 0, forceSubAtPost = -1;
            if (addSubpostAtPostPointAlso == true && (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.dependsOnSectionLength))
            {
                numSubs += 1;
                //start = forceSubAtPost;
            }

            Vector3 variantScaling = Vector3.one, variantOffset = Vector3.zero, variantRotation = Vector3.zero;

            float radius = 1.0f;
            int ownerPostIndex = sectionIndex;
            int numParallelSubLines = 1;

            //-- For v4.1
            Vector3 currPostPosLeft = postDirectionVectors.CalculatePositionToLeft(ownerPostIndex, radius);
            Vector3 currPostPosRight = postDirectionVectors.CalculatePositionToRight(ownerPostIndex, radius);
            Vector3 nextPostPosLeft = currPostPosLeft, nextPostPosRight = currPostPosRight;

            if (isLastPost == false)
            {
                nextPostPosLeft = postDirectionVectors.CalculatePositionToLeft(ownerPostIndex + 1, radius);
                nextPostPosRight = postDirectionVectors.CalculatePositionToRight(ownerPostIndex + 1, radius);
            }

            //For intermediate posts we need to set the start position at the elbow point
            if (currIsClickPoint == true && isLastPost == false && isFirstPost == false)
            {
                Vector3 innerOffset, outerOffset;// postDirectionVectors.CalculateOuterElbow(ownerPostIndex, radius).To3D();

                (innerOffset, outerOffset) = postDirectionVectors.GetElbowPoints3D(ownerPostIndex, radius, useWorldSpace:true);

                currPostPosLeft = currPostPos + outerOffset;
                currPostPosRight = currPostPos + innerOffset;
            }

            if (subpostDuplicateMode == DuplicateMode.dual)
            {
                numParallelSubLines = 2;
                currSubpostPos = currPostPosLeft;
            }
            else if(subpostDuplicateMode == DuplicateMode.dualWithCentre)
            {
                numParallelSubLines = 3;
                currSubpostPos = currPostPosLeft;
            }

            for (int k = 0; k < numParallelSubLines; k++)
            {
                //-- For v4.1
                if (subpostDuplicateMode == DuplicateMode.dual)
                {
                    if (k == 0)
                    {
                        currSubpostPos = currPostPosLeft;
                        nextPostPos = nextPostPosLeft;
                    }
                    else if (k == 1)
                    {
                        currSubpostPos = currPostPosRight;
                        nextPostPos = nextPostPosRight;
                    }
                    if (currIsClickPoint == true || nextIsClickPoint == true)
                    {
                        distance = Vector3.DistanceTCT(currSubpostPos, nextPostPos);
                        actualSubSpacing = distance / numSubs;
                    }
                }
                else if (subpostDuplicateMode == DuplicateMode.dualWithCentre)
                {
                    if (k == 0)
                    {
                        currSubpostPos = currPostPosLeft;
                        nextPostPos = nextPostPosLeft;
                    }
                    else if (k == 1)
                    {
                        currSubpostPos = currPostPos;
                        nextPostPos = nextPostPos;
                    }
                    else if (k == 2)
                    {
                        currSubpostPos = currPostPosRight;
                        nextPostPos = nextPostPosRight;
                    }
                    if (nextIsClickPoint == true)
                    {
                        distance = Vector3.DistanceTCT(currSubpostPos, nextPostPos);
                        actualSubSpacing = distance / numSubs ;
                    }
                }

                for (int i = 0; i < numSubs; i++)
                {
                    //int repetitionIndex = (sectionIndex * numSubs) + i;
                    GameObject thisSub = RequestSub(subpostsBuiltCount).gameObject;
                    if (thisSub == null)
                    {
                        print("Missing Sub " + i + " Have you deleted one?");
                        continue;
                    }
                    thisSub.hideFlags = HideFlags.None;
                    thisSub.SetActive(true);
                    thisSub.name = "Sub " + subpostsBuiltCount.ToString();
                    thisSub.transform.parent = subpostsFolder.transform;

                    thisSub.transform.position = currSubpostPos = currPostPos;
                    ownerPostIndex = sectionIndex;

                    //-- For v4.1

                    if (subpostDuplicateMode == DuplicateMode.dual)
                    {
                        if (k == 0)
                            currSubpostPos = thisSub.transform.position = currPostPosLeft;
                        else if (k == 1)
                            currSubpostPos = thisSub.transform.position = currPostPosRight;
                    }
                    else if (subpostDuplicateMode == DuplicateMode.dualWithCentre)
                    {
                        if (k == 0)
                            currSubpostPos = thisSub.transform.position = currPostPosLeft;
                        else if (k == 1)
                            currSubpostPos = thisSub.transform.position = currPostPos;
                        else if (k == 2)
                            currSubpostPos = thisSub.transform.position = currPostPosRight;
                    }

                    // In stepped mode they take the height position from 'currPostPos' (the previous post, rather than the nextPos)
                    if (slopeMode[kRailALayerInt] == SlopeMode.step)
                    {
                        Vector3 stepPos = currPostPos;
                        stepPos.y = currPostPos.y;
                        currSubpostPos = thisSub.transform.position = stepPos;
                    }

                    //================= Subpost Variations ================================
                    int currSeqIndex = -1;
                    SeqItem currSeqItem = null;
                    //-- For v4.1
                    if (useSubpostVariations && subseqNumSteps[kPostLayerInt] > 1)
                    {
                        currSeqItem = new SeqItem();

                        currSeqIndex = subpostsBuiltCount % subseqNumSteps[kPostLayerInt];
                        if (currSeqIndex != -1)
                            currSeqItem = userSequenceSubpost[currSeqIndex];
                        variantOffset = currSeqItem.pos;
                        variantScaling = currSeqItem.svSize;
                        variantRotation = currSeqItem.rot;
                    }
                    //Debug.Log("sectionIndex " + sectionIndex + ":" + i  + "      currSeqIndex " + currSeqIndex + "       stepEnabled " + currSeqItem.stepEnabled + "   " + thisSub.name +
                    //"    "  + variantScaling + "\n");

                    if (currSeqItem != null && currSeqItem.stepEnabled == false)
                    {
                        thisSub.hideFlags = HideFlags.HideInHierarchy;
                        thisSub.SetActive(false);
                        subpostsBuiltCount++;
                        continue;
                    }
                    //=================================
                    //            Position
                    //=================================
                    // Interpolate the subpostsPool position between currPostPos & nextPostPos, but keep Y fixed in Stepped Mode
                    //Vector3 right = thisSub.transform.right;
                    Vector3 offset = Vector3.zero, varOffset = Vector3.zero;

                    Vector3 move = directionVector * actualSubSpacing * (i + 1);
                    float moveY = move.y;
                    float subFinalHeight = subpostScale.y * globalScale.y;
                    // Modes for subsSpacingMode: 0 "Fixed Number Between Posts",  1 "Depends on Section Length",  2 "Post Positions Only",  3 "Post Positions Only"

                    //      Fixed or DistanceTCT Dependent
                    //====================================
                    if ((subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.dependsOnSectionLength) && i > -1)
                    {
                        if (slopeMode[kRailALayerInt] == SlopeMode.step)
                            moveY = 0;
                        thisSub.transform.position += new Vector3(move.x, moveY, move.z);
                        thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                        offset = right * subpostPositionOffset.z;
                        thisSub.transform.position += offset;

                        offset = forward * subpostPositionOffset.x;
                        thisSub.transform.position += offset;
                    }
                    //      Post Positions Only
                    //====================================
                    else if (subsSpacingMode == SubSpacingMode.postPositionsOnly || i == -1)
                    {
                        thisSub.transform.position = currSubpostPos;
                        thisSub.name += "_P"; // denotes that it's at a Post Position. Useful for some Extra modes
                        thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                        offset = right * subpostPositionOffset.z;
                        thisSub.transform.position += offset;

                        offset = forward * subpostPositionOffset.x;
                        thisSub.transform.position += offset;
                    }
                    //      Node Positions Only
                    //====================================
                    else if (subsSpacingMode == SubSpacingMode.nodePositionsOnly && currIsClickPoint == true || i == -1)
                    {
                        thisSub.transform.position = currSubpostPos;
                        thisSub.name += "_P"; // denotes that it'i at a Post Position. Useful for some Extra modes
                        thisSub.transform.position += new Vector3(0, subpostPositionOffset.y * globalScale.y, 0);

                        offset = right * subpostPositionOffset.z;
                        thisSub.transform.position += offset;

                        offset = forward * subpostPositionOffset.x;
                        thisSub.transform.position += offset;
                    }
                    //-- We only want node positions and this isn't one, so continue
                    else if ((subsSpacingMode == SubSpacingMode.nodePositionsOnly && currIsClickPoint == false))
                        continue;

                    if (useSubpostVariations == true )
                    {
                        varOffset = right * variantOffset.z;
                        //Debug.Log("var right  " + right + "         z  " + variantOffset.z + "\n");
                        thisSub.transform.position += varOffset;

                        varOffset = forward * variantOffset.x;
                        //Debug.Log("forward  " + thisSub.transform.forward + "         x  " + subpostPositionOffset.x + "\n");
                        thisSub.transform.position += varOffset;
                    }

                    //=================================
                    //            Rotation
                    //=================================
                    thisSub.transform.rotation = Quaternion.identity;
                    AlignSubpostToSurfaceNormal(ref thisSub);
                    thisSub.transform.Rotate(new Vector3(0, currDirection.y, 0), Space.Self);
                    if (subsSpacingMode == SubSpacingMode.postPositionsOnly || subsSpacingMode == SubSpacingMode.nodePositionsOnly && isLastPost == true) // using 'replicate postsPool only' mode
                        thisSub.transform.Rotate(new Vector3(0, 180, 0), Space.Self);

                    thisSub.transform.Rotate(new Vector3(subpostRotation.x, subpostRotation.y, subpostRotation.z), Space.Self);
                    if (useSubpostVariations == true && variantRotation != Vector3.zero)
                        thisSub.transform.Rotate(variantRotation);

                    //===================== Apply sine to height of subpostsPool =======================
                    if (useSubWave && (subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts || subsSpacingMode == SubSpacingMode.fixedNumBetweenPosts))
                    {
                        float realMoveForward = move.magnitude;
                        float sinValue = Mathf.Sin((((realMoveForward / distance) * Mathf.PI * 2) + subWavePosition) * subWaveFreq);
                        sinValue *= subWaveAmp * globalScale.y;
                        subFinalHeight = (subpostScale.y) + sinValue + (subWaveAmp * globalScale.y);
                        //==== CreateMergedPrefabs Sub Joiners ====
                        if (i > 0 && useSubJoiners)
                        {
                            thisSubJoiner = RequestSubJoiner(subJoinersBuiltSoFarCount++);
                            if (thisSubJoiner != null)
                            {
                                thisSubJoiner.transform.position = thisSub.transform.position + new Vector3(0, (subFinalHeight) - .01f, 0);
                                thisSubJoiner.transform.rotation = Quaternion.identity;
                            }
                        }
                    }
                    //=========== Scale ==============
                    Vector3 scale = Vector3.one;
                    scale.x *= subpostScale.x * globalScale.x;
                    scale.y *= subFinalHeight;
                    scale.z *= subpostScale.z * globalScale.z;

                    if (useSubpostVariations == true && variantScaling != Vector3.one)
                        scale = Vector3.Scale(scale, variantScaling);

                    //if(i==0) { scale.y *= 2; } //??? remove this

                    thisSub.transform.localScale = scale;

                    //=============== SubPost Spreading ================
                    // currPostPos quick and dirty spreading/bunching. TODO add a choice of spreading algorithms
                    if (Math.Abs(subPostSpread) >= 0.1f)
                    {
                        float spread = subPostSpread;
                        spread *= interPostDist / distance;

                        float halfDist = distance / 2;
                        Vector3 vecFromA = currSubpostPos - thisSub.transform.localPosition;
                        float distFromA = Math.Abs(vecFromA.magnitude);
                        Vector3 vecFromB = nextPostPos - thisSub.transform.localPosition;
                        float distFromB = Math.Abs(vecFromB.magnitude);
                        Vector3 moveSpread = Vector3.zero;
                        if (Math.Abs(distFromA - distFromB) < 0.1f) { } //TODO
                        else if (distFromA < distFromB)
                        {
                            moveSpread = distFromA * (distFromA / 2) * directionVector * spread;
                        }
                        else if (distFromA > distFromB)
                        {
                            moveSpread = distFromB * (distFromB / 2) * -directionVector * spread;
                        }
                        moveSpread.y = 0;

                        moveSpread = -thisSub.transform.InverseTransformVector(moveSpread);
                        thisSub.transform.Translate(moveSpread);

                        Vector3 flatA = new Vector3(currSubpostPos.x, 0, currSubpostPos.z);
                        Vector3 flatB = new Vector3(nextPostPos.x, 0, nextPostPos.z);
                        Vector3 flatPos = new Vector3(thisSub.transform.localPosition.x, 0, thisSub.transform.localPosition.z);

                        Vector3 flatDeltaVec = flatB - flatA;

                        float finalFlatDistFromA = (flatPos - flatA).magnitude;
                        float distProp = finalFlatDistFromA / flatDeltaVec.magnitude;
                        float spanHeightDelta = (nextPostPos - currSubpostPos).y;
                        float yPos = currSubpostPos.y + (spanHeightDelta * distProp);
                        thisSub.transform.localPosition = new Vector3(thisSub.transform.localPosition.x, yPos, thisSub.transform.localPosition.z);
                    }

                    //=============== Sub Joiners ================
                    if (i > 0 && useSubJoiners && thisSubJoiner != null) // need to do this after the final sub calculations
                    {
                        Vector3 a = subpostsPool[subpostsBuiltCount].transform.position + new Vector3(0, subpostsPool[subpostsBuiltCount].transform.localScale.y, 0);
                        Vector3 b = subpostsPool[subpostsBuiltCount - 1].transform.position + new Vector3(0, subpostsPool[subpostsBuiltCount - 1].transform.localScale.y, 0);
                        float joinerDist = Vector3.DistanceTCT(b, a);
                        thisSubJoiner.transform.localScale = new Vector3(joinerDist, thisSubJoiner.transform.localScale.y, thisSubJoiner.transform.localScale.z);
                        Vector3 subJoinerDirection = VectorUtilitiesTCT.GetRotationAnglesFromDirection(a, b);
                        thisSubJoiner.transform.Rotate(new Vector3(0, subJoinerDirection.y - 90, -subJoinerDirection.x + 180));
                        thisSubJoiner.GetComponent<Renderer>().sharedMaterial = thisSub.GetComponent<Renderer>().sharedMaterial;
                    }
                    //=============== Force Subs to Ground ================
                    if (forceSubsToGroundContour)
                    {
                        SetIgnorePartsColliders(true); // temporarily ignore other fence colliders to find distanceToNextPost to ground
                        Vector3 currPos = thisSub.transform.position;
                        float rayStartHeight = globalScale.y * 3.0f;
                        //float margin = 10;
                        currPos.y += rayStartHeight;
                        RaycastHit hit;
                        if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
                        {
                            if (hit.collider.gameObject != null)
                            {
                                float distToGround = hit.distance + 0.0f - subsGroundBurial; //in the ground a little
                                thisSub.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                                scale.y += (distToGround - rayStartHeight) / 2;
                                thisSub.transform.localScale = scale;
                                //Debug.Log(hit.distanceToNextPost + "  " + rayStartHeight + "\n");
                            }
                        }
                        SetIgnorePartsColliders(false);
                    }
                    else if (keepSubsAboveGround)
                    {
                        //If not forced DOWN to ground we need to make sure that at least they are at not below fround
                        SetIgnorePartsColliders(true); // temporarily ignore other fence colliders to find distanceToNextPost to ground
                        Vector3 currPos = thisSub.transform.position;
                        float currY = currPos.y;
                        float rayStartHeight = globalScale.y * 3.0f;
                        currPos.y += rayStartHeight;
                        RaycastHit hit;
                        if (Physics.Raycast(currPos, Vector3.down, out hit, 500))
                        {
                            if (hit.collider.gameObject != null)
                            {
                                float distToGround = hit.distance + 0.0f;
                                if (hit.distance < rayStartHeight)
                                {
                                    thisSub.transform.Translate(0, -(distToGround - rayStartHeight), 0);
                                }
                                //Debug.Log(hit.distanceToNextPost + "  " + rayStartHeight + "    " + currY + "\n");
                            }
                        }
                        SetIgnorePartsColliders(false);
                    }
                    //== Random Variation ==
                    if (allowSubpostRandomization)
                    {
                        if (allowHeightVariationSubpost)
                        {
                            float randHeightScale = UnityEngine.Random.Range(minRandHeightSubpost, maxRandHeightSubpost);
                            thisSub.transform.localScale = Vector3.Scale(thisSub.transform.localScale, new Vector3(1, randHeightScale, 1));
                        }
                        //================= Add Random Rotations ===========================
                        if (allowRandSubpostSmallRotationVariation)
                        {
                            float xRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.x, smallRotationAmountSubpost.x);
                            float yRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.y, smallRotationAmountSubpost.y);
                            float zRot = UnityEngine.Random.Range(-smallRotationAmountSubpost.z, smallRotationAmountSubpost.z);
                            thisSub.transform.Rotate(new Vector3(xRot, yRot, zRot));
                        }
                        //================= Add Random Quantized Rotations ===========================
                        if (allowQuantizedRandomSubpostRotation)
                        {
                            int num = UnityEngine.Random.Range(0, 24);
                            float totalRotAmount = num * quantizeRotAngleSubpost;
                            totalRotAmount = totalRotAmount % 360;
                            thisSub.transform.Rotate(new Vector3(0, totalRotAmount, 0));
                        }
                        //========= Chance of Missing SubPost ==============
                        bool disableSub = false;
                        if ((chanceOfMissingSubpost == -1 && i % 2 == 1))
                            disableSub = true;
                        else if (chanceOfMissingSubpost > 0 && UnityEngine.Random.value < chanceOfMissingSubpost)
                            disableSub = true;
                        if (disableSub == true)
                        {
                            thisSub.gameObject.SetActive(false); // deleted when finalized
                            thisSub.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        }
                    }

                    subpostsBuiltCount++;
                    thisSub.isStatic = usingStaticBatching;
                    //====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
                    int numSubsFolders = (subpostsBuiltCount / objectsPerFolder) + 1;
                    string subsDividedFolderName = "SubsGroupedFolder" + (numSubsFolders - 1);
                    GameObject subsDividedFolder = GameObject.Find("Current Fences Folder/Subs/" + subsDividedFolderName);
                    if (subsDividedFolder == null)
                    {
                        subsDividedFolder = new GameObject(subsDividedFolderName);
                        subsDividedFolder.transform.parent = subpostsFolder.transform;
                        if (addCombineScripts)
                        {
                            CombineChildrenPlus combineChildren = subsDividedFolder.AddComponent<CombineChildrenPlus>();
                            if (combineChildren != null)
                                combineChildren.combineAtStart = true;
                        }
                    }

                    CreateColliderForLayer(thisSub, thisSub.transform.localPosition, LayerSet.subpostLayer);

                    postAndSubpostStringList.Add($"{postAndSubpostStringList.Count.ToString()} - Sub[{sectionIndex.ToString()} {i.ToString()}]");
                    postsAndSubpostsCombined.Add(thisSub.transform);

                    thisSub.transform.parent = subsDividedFolder.transform;

                    subPostsTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(thisSub);
                }
            }
        }*/
    }
}