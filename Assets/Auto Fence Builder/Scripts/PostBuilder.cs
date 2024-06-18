using MeshUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AFWB
{
    public partial class AutoFenceCreator
    {
        void CopyPostSeqInfoToPostBuildInfo()
        {
            //Get smaller of PostVector.GetParentListCount  and postSequencer.Length()
            int count = Math.Min(PostVector.GetParentListCount(), postSequencer.Length());

            for (int i = 0; i < count; i++)
            {
                SeqItem seqItem = postSequencer.GetSeqItem(i);
                PostBuildInfo postBuildInfo = PostVector.GetPostVectorAtIndex(i).postBuildInfo;
                postBuildInfo.varSizeScaling = seqItem.size;
                postBuildInfo.varRotation = seqItem.rot;
                postBuildInfo.varPositionOffset = seqItem.pos;
                //postBuildInfo.stepEnabled = seqItem.stepEnabled;
            }
        }
        //==========================================================================
        //                          Setup Post
        //==========================================================================

        // Sets the post in the pool with all the correct attributes, and show a click-nodeMarker if they are enabled

        void SetupPost(int n, Vector3 postPoint)
        {
            Timer t = new Timer("Post Build Time");

            Vector3 unRandomizedPoint = allPostPositions[n];
            if (allPostsPositionsUnrandomized.Count > n)
                unRandomizedPoint = allPostsPositionsUnrandomized[n];

            PostVector postVector = PostVector.GetPostVectorAtIndex(n);
            PostBuildInfo postBuildInfo = postVector.postBuildInfo;
            //List<SeqItem> postSeqList = postSequencer.SeqList;
            LayerSet layer = LayerSet.postLayerSet;

            int clickpointIndex = -1;
            bool isClickPoint = false;
            if (n == 0)
            {
                clickpointIndex = 0;
            }
            else if (n == allPostPositions.Count - 1)
            {
                clickpointIndex = clickPoints.Count - 1;
            }
            else
            {
                clickpointIndex = IsCloseClickPoint(unRandomizedPoint);
            }
            if (clickpointIndex != -1)
                isClickPoint = true;

            bool isGap = false;
            for (int i = 0; i < gaps.Count(); i += 1)
            {
                if (gaps[i] == postPoint)
                {
                    isGap = true;
                }
            }

            if (postsPool == null)
                Debug.LogWarning("SetupPost(): postsPool is null");
            else if (postsPool.Count == 0)
                Debug.LogWarning("SetupPost(): postsPool is empty");
            else if (postsPool[0] == null)
                Debug.LogWarning("SetupPost(): First item (transform) of postsPool is null");
            else if (postsPool[0].gameObject == null)
                Debug.LogWarning("SetupPost(): First Game Object of postsPool is null");

            else
            {
                GameObject thisPost = postsPool[n].gameObject;
                bool isMainVariant = thisPost.name.Contains("v0]") || usePostVariations == false;
                thisPost.SetActive(true);
                thisPost.hideFlags = HideFlags.None;

                if (thisPost.name.Contains("_sq"))
                {
                    thisPost.name = thisPost.name.Remove(thisPost.name.IndexOf("_sq"), 5);
                }

                // Name it if it is a click point, remove old name first
                bool nameContainsClick = thisPost.name.Contains("_click");
                if (nameContainsClick)
                    thisPost.name = thisPost.name.Remove(thisPost.name.IndexOf("_click"), 6);
                bool nameContainsGap = thisPost.name.Contains("_gap");
                if (nameContainsGap)
                    thisPost.name = thisPost.name.Remove(thisPost.name.IndexOf("_gap"), 4);

                if (isClickPoint == true)
                    thisPost.name += "_click";
                if (isGap == true)
                    thisPost.name += "_gap";

                //Set not to interfere with the picking of the control posts which coincide with Posts. v2.3 removed after Finalize. Editable in Setting Window
                thisPost.layer = ignoreControlNodesLayerNum;

                //====================================
                //          Post Variations
                //====================================
                Vector3 variantScaling = Vector3.one, variantOffset = Vector3.zero;
                bool usePostSequencer = GetUseSequencerForLayer(layer);
                int numSeqSteps = postSequencer.Length();
                if (usePostVariations && usePostSequencer && variationModePost == VariationMode.sequenced && numSeqSteps > 1)
                {
                    SeqItem currSeqItem = postSequencer.GetSeqItemForSectionIndex(n);
                    variantScaling = currSeqItem.size;

                    //-- Name the seq step if needed --
                    if (thisPost.name.EndsWith("]") || thisPost.name.EndsWith("]_click") || thisPost.name.EndsWith("]_click_gap")) //ending with ']' means haven't named the seq yet 
                    {
                        int currSeqIndex = postSequencer.GetSeqStepNumFromSectionIndex(n);
                        string seqIndexString = "_sq" + currSeqIndex.ToString("00");
                        thisPost.name += seqIndexString;
                    }
                }

                //============================================================================
                //  Note: Override prefab for Main and End Post Nodes is set during Pooling
                //============================================================================


                //=========== Position [ Rotation is handled in RotateAndModifyPostsFinal() ] ==============
                thisPost.transform.position = postPoint;
                thisPost.transform.position += new Vector3(0, postHeightOffset * globalScale.y, 0);
                //=========== Scale ==============
                thisPost.transform.localScale = Vector3.Scale(nativePostScale, new Vector3(postScale.x * globalScale.x, postScale.y * globalScale.y, postScale.z * globalScale.z));
                //======= Variation Scale ========
                if (usePostVariations == true && variantScaling != Vector3.one)
                    thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, variantScaling);

                //======= Main Boost Scale ========
                if (isClickPoint == true /*&& allowNodePostsPrefabOverride*/)
                    //thisPost.transform.localScale = mainPostsSizeBoost;
                    thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, mainPostsSizeBoost);

                //======= Ends Boost Scale ========
                if ((n == 0 || n == GetLastPostIndex()) /*&& allowEndPostsPrefabOverride*/)
                    //thisPost.transform.localScale =  endPostsSizeBoost;
                    thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, endPostsSizeBoost);


                if (allowPostRandomization)
                {
                    //====================================
                    //          Random Height
                    //====================================
                    if (allowHeightVariationPost)
                    {
                        if (postRandomScope == RandomScope.all || (postRandomScope == RandomScope.main && isMainVariant)
                            || (postRandomScope == RandomScope.variations && isMainVariant == false))
                        {
                            if (postAndGlobalSeeds == null || postAndGlobalSeeds.rHeight.Count <= n)
                                Debug.LogError("postAndGlobalSeeds");
                            float randHeightScale = postAndGlobalSeeds.rHeight[n];
                            postVector.postBuildInfo.varSizeScaling.y = randHeightScale;
                            thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, new Vector3(1, randHeightScale, 1));
                        }
                    }
                    //== Random Size (XZ) Variation ==
                    if (allowPostSizeVariation)
                    {
                        if (postRandomScope == RandomScope.all || (postRandomScope == RandomScope.main && isMainVariant)
                                || (postRandomScope == RandomScope.variations && isMainVariant == false))
                        {
                            float randX = postAndGlobalSeeds.rPostSizeX[n];
                            float randZ = postAndGlobalSeeds.rPostSizeZ[n];
                            thisPost.transform.localScale = Vector3.Scale(thisPost.transform.localScale, new Vector3(randX, 1, randZ));
                        }
                    }
                }

                if (postMenuNames[currentPost_PrefabIndex] == "_None_Post" || usePostsLayer == false || (isClickPoint == false && hideInterpolated == true)) // don't show it if it'stackIdx a none post, but it'stackIdx still built as a reference for other objects
                {
                    thisPost.SetActive(false);
                    thisPost.hideFlags = HideFlags.HideInHierarchy;
                }
                else
                {
                    thisPost.SetActive(true);
                    thisPost.hideFlags = HideFlags.None;
                }

                thisPost.isStatic = usingStaticBatching;

                if (usePostsLayer == true && countTriangles == true)
                    postsTotalTriCount += MeshUtilitiesAFWB.CountAllTrianglesInGameObject(thisPost);


                //====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
                int numPostsFolders = (postsBuiltCount / objectsPerFolder) + 1;
                string postsDividedFolderName = "PostsGroupedFolder" + (numPostsFolders - 1);
                GameObject postsDividedFolder = GameObject.Find("Current Fences Folder/Posts/" + postsDividedFolderName);
                if (postsDividedFolder == null)
                {
                    postsDividedFolder = new GameObject(postsDividedFolderName);
                    postsDividedFolder.transform.parent = postsFolder.transform;
                    if (addCombineScripts)
                    {
                        CombineChildrenPlus combineChildren = postsDividedFolder.AddComponent<CombineChildrenPlus>();
                        if (combineChildren != null)
                            combineChildren.combineAtStart = true;
                    }
                }

                thisPost.transform.parent = postsDividedFolder.transform;

                //====================================
                //             Colliders  
                //====================================
                Vector3 centreColliderPos = thisPost.transform.localPosition;
                Collider collider = CreateColliderForLayer(thisPost, centreColliderPos, LayerSet.postLayerSet);


            }
            //====== Set Up Debug Visualisation Markers =======


            postBuildTime = t.End(print: false);
        }

        //-------------------------------------------------
        // this is done at the end because depending on the settings the post rotation/direction need updating
        void RotateAndModifyPostsFinal()
        {
            Timer t = new Timer("RotateAndModifyPostsFinal");

            //return;

            if (postsPool == null || postsPool.Count == 0 || postsPool[0] == null)
            {
                Debug.LogWarning("Missing Post Instance in RotateAndModifyPostsFinal()");
                return;
            }
            bool disablePostRandom = false, enablePostVariation = true;
            Vector3 normLerp;
            PostVector postVector = PostVector.GetPostVectorAtIndex(0);

            PostBuildInfo postBuildInfo = postVector.postBuildInfo;
            List<SeqItem> postSeqList = postSequencer.SeqList;
            int numSeqSteps = postSequencer.Length();


            if (postsBuiltCount >= 2)
            {
                Vector3 A = Vector3.zero, B = Vector3.zero;
                Vector3 eulerDirectionFromPrevPost = Vector3.zero;
                Vector3 eulerDirectionNext = Vector3.zero;
                Vector3 variantRotation = Vector3.zero, variantOffset = Vector3.zero, variantScale = Vector3.one;
                Transform firstPost = postsPool[0], thisPost, nextPost, prevPost;
                if (firstPost == null) return;

                //-- FIRST post is angled straight in the direction of the outgoing singleVarGO
                A = firstPost.position;
                B = postsPool[1].position;
                eulerDirectionFromPrevPost = VectorUtilitiesTCT.GetRotationAnglesFromDirection(A, B);
                firstPost.rotation = Quaternion.identity;

                //===========  Rotate To Surface Normal ================
                //AlignPostToSurfaceNormal(postsPool, 0);

                firstPost.Rotate(0, eulerDirectionFromPrevPost.y + 180, 0);
                firstPost.Rotate(postRotation.x, postRotation.y, postRotation.z);

                //================= Stretch Mitre Joints ===========================
                // main, but not first and last which are handled separately
                if (stretchPostWidthAtMitreJoint == true)
                {
                    for (int i = 1; i < postsBuiltCount - 1; i++)
                    {
                        //-- Miter Resize
                        float mitreWidth = 1;
                        if (i > 0 && i < postsPool.Count - 1)
                        {
                            Vector3 pt1 = postsPool[i - 1].position;
                            Vector3 pt2 = postsPool[i].position;
                            Vector3 pt3 = postsPool[i + 1].position;
                            mitreWidth = VectorUtilitiesTCT.GetWidthAtElbow(pt1, pt2, pt3, 1.0f);
                            //Debug.Log(" stackIdx = " + stackIdx + "  Angle = " + VectorUtilitiesTCT.GetCornerAngle(pt1, pt2, pt3) + "  mitreWidth =" + mitreWidth + "\n");

                            //TODO for now limit it to double width as it can blow up on acute angles
                            //Debug.Log("mitreWidth " + mitreWidth + "\n");
                            if (mitreWidth > 2.5f)
                                mitreWidth = 2.5f;
                            //TODO adjustedNativeScale Z at high values
                            float mitreZ = 1.0f;
                            if (mitreWidth > 1.4f)
                                mitreZ += (mitreWidth - 1.3f) / 2;

                            postsPool[i].transform.localScale = Vector3.Scale(postsPool[i].transform.localScale, new Vector3(mitreWidth, 1, mitreZ));
                        }
                    }
                }
                //================= Add Random Rotations ===========================
                // main, but not first and last which are handled separately
                if (postsBuiltCount >= postAndGlobalSeeds.rQuantRot.Count)
                {
                    postAndGlobalSeeds.GenerateRandomSmallRotValues();
                }
                //================================================
                //                  Main Loop
                //================================================
                t.Lap("pre loop");
                for (int i = 1; i < postsBuiltCount - 1; i++)
                {
                    postVector = PostVector.GetPostVectorAtIndex(i);
                    bool isClickPoint = postVector.IsClickPointNode;
                    PostVector prevPostVector = postVector.GetPrevious();
                    postBuildInfo = postVector.postBuildInfo;

                    thisPost = postsPool[i];
                    nextPost = postsPool[i + 1];
                    prevPost = postsPool[i - 1];

                    A = thisPost.position;
                    B = prevPost.position;
                    if (A != B)
                        eulerDirectionFromPrevPost = VectorUtilitiesTCT.GetRotationAnglesFromDirection(A, B);

                    //if(isClickPoint)
                    //Debug.Log($"euler.y {eulerDirectionFromPrevPost.y}   \n");

                    //if (postsPool[i].name.EndsWith("click"))
                    //isClickPoint = true;

                    //-- It's a clickpoint, and Interpolate rotation at corners
                    if (isClickPoint == true && lerpPostRotationAtCorners == true)
                    {
                        if (i + 1 >= postsPool.Count)
                            continue;
                        // interpolare the svRotation bewteen the direction of incoming & outgoing rails (always do for interpolated)
                        thisPost.rotation = Quaternion.identity;
                        thisPost.Rotate(0, eulerDirectionFromPrevPost.y, 0);

                        
                        float angle = GetUnsignedAngleToNextPost(thisPost, nextPost);
                        float avgRot = angle / 2 - 90;
                        postsPool[i].transform.Rotate(0, avgRot, 0);
                        
                        
                        //postsPool[i].transform.Rotate(0, postVector.GetDirAvgHeading(), 0);
                        //Debug.Log($"Avg Heading {i} : {postVector.GetDirAvgHeading()}   \n");

                    }
                    else
                    {
                        thisPost.rotation = Quaternion.identity;

                        //-- Rotate By Fence Direction
                        A = nextPost.position;
                        B = thisPost.position;
                        eulerDirectionNext = VectorUtilitiesTCT.GetRotationAnglesFromDirection(A, B);
                        thisPost.Rotate(0, eulerDirectionNext.y, 0);
                    }

                    //      Apply Transform Box Rotation
                    //=======================================
                    thisPost.Rotate(postRotation.x, postRotation.y, postRotation.z);

                    //===========  Rotate To Surface Normal ================
                    //AlignPostToSurfaceNormal(postsPool, i);

                    //========= Chance of Random Missing Post ==============
                    disablePostRandom = ChanceOfMissingPost(thisPost, i);
                    if (disablePostRandom)
                        continue;

                    //================= Post Variations ================================
                    variantRotation = Vector3.zero; variantOffset = Vector3.zero; variantScale = Vector3.one; enablePostVariation = true;
                    ApplyVariation(ref enablePostVariation, numSeqSteps, ref variantRotation, ref variantOffset, ref variantScale, thisPost, i);

                    //      Add Small Random Rotations
                    //=======================================
                    SmallRandomRotations(thisPost, i);

                    //      Quantized Random Rotations
                    //=======================================
                    QuantizedRandomRotations(thisPost, i, center: thisPost.position);


                    //-- We have to do this last so that it doesn't confuse the rotations
                    if (usePostVariations && numSeqSteps > 1)
                        thisPost.position += variantOffset;

                }
                t.Lap("post loop");
                int lastPostIndex = postsBuiltCount - 1;

                //      Last Post
                //=======================
                Transform lastPost = postsPool[lastPostIndex];
                Vector3 lastPostPos = lastPost.position;

                //-- Rotate By Fence Direction
                Vector3 prevPostPos = postsPool[lastPostIndex - 1].position;
                lastPost.rotation = Quaternion.identity;
                eulerDirectionFromPrevPost = VectorUtilitiesTCT.GetRotationAnglesFromDirection(lastPostPos, prevPostPos);
                lastPost.Rotate(0, eulerDirectionFromPrevPost.y, 0);

                //-- Apply Transform Box Rotation
                lastPost.Rotate(postRotation.x, postRotation.y, postRotation.z);

                //AlignPostToSurfaceNormal(postsPool, lastPostIndex);

                //-- We have to handle Variations and Random the first and last separately
                if (usePostVariations && numSeqSteps > 1)
                {
                    //Last
                    ApplyVariation(ref enablePostVariation, numSeqSteps, ref variantRotation, ref variantOffset, ref variantScale, lastPost, lastPostIndex);
                    //AlignPostToSurfaceNormal(postsPool, lastPostIndex);

                    //SmallRandomRotations(lastPost, lastPostIndex);
                    //QuantizedRandomRotations(lastPost, lastPostIndex, center: firstPost.position);

                    //-- First
                    ApplyVariation(ref enablePostVariation, numSeqSteps, ref variantRotation, ref variantOffset, ref variantScale, firstPost, 0);
                    //AlignPostToSurfaceNormal(postsPool, 0);

                }
            }
            t.Lap("final");
            postBuildTime += t.End(print: false);
        }
        //--------------------------------
        /// <summary>
        /// Applies variations to a post based on sequence information, but only if the post variations are enabled.
        /// </summary>
        /// <param name="disablePost">Reference to a boolean indicating if the post should be disabled.</param>
        /// <param name="numSeqSteps">The number of sequence steps.</param>
        /// <param name="varRotation">Reference to the variant rotation to be applied to the post.</param>
        /// <param name="varOffset">Reference to the variant offset to be applied to the post.</param>
        /// <param name="varScale">Reference to the variant scale to be applied to the post.</param>
        /// <param name="thisPost">The Transform of the post being modified.</param>
        /// <param name="i">The index of the current post in the sequence.</param>
        private void ApplyVariation(ref bool enablePost, int numSeqSteps, ref Vector3 varRotation, ref Vector3 varOffset, ref Vector3 varScale, Transform thisPost, int i)
        {
            if (usePostVariations && numSeqSteps > 1)
            {
                SeqItem currSeqItem = postSequencer.GetSeqItemForSectionIndex(i);
                varOffset = postSequencer.GetSeqOffsetForSectionIndex(i);
                varScale = postSequencer.GetSeqScaleForSectionIndex(i);
                varRotation = postSequencer.GetSeqRotationForSectionIndex(i);
                enablePost = postSequencer.GetSeqStepEnabledForSectionIndex(i);

                thisPost.localPosition += varOffset;
                thisPost.localScale = Vector3.Scale(thisPost.localScale, varScale);
                thisPost.Rotate(varRotation);

                if (enablePost == false)
                {
                    Debug.Log("Disabling post " + (i));
                    thisPost.gameObject.SetActive(false); // deleted when finalized
                }
            }
        }

        //------------------
        /// <summary>
        /// Post is deactivated and hidden based on the random probability
        /// </summary>
        /// <param name="thisPost"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private bool ChanceOfMissingPost(Transform thisPost, int i)
        {
            bool disablePost = false;

            if (allowPostRandomization && chanceOfMissingPost > 0)
            {
                disablePost = false;
                if ((chanceOfMissingPost == -1 && i % 2 == 1))
                    disablePost = true;
                else if (chanceOfMissingPost > 0 && UnityEngine.Random.value < chanceOfMissingPost)
                    disablePost = true;
                if (disablePost == true)
                {
                    thisPost.gameObject.SetActive(false); // deleted when finalized
                    thisPost.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
            return disablePost;
        }

        //----------
        private void QuantizedRandomRotations(Transform thisPost, int i, Vector3 center)
        {
            if (allowPostRandomization && allowQuantizedRandomPostRotation)
            {
                Vector3 totalQuantRot = Vector3.zero; // only so we can calculate the total rotations later if needed or for debug
                int numChunks = (int)(360 / quantizeRotAnglePost) - 1;//eg, for 90 will return 3 (90,180,270)
                int multipleRots = 1, totalQuantRotAmount = 0;
                if (quantizeRotAnglePost > 0) // -90, or -180 denote that we should do exactly 1 of that angle, not multiples
                {
                    if (i >= postAndGlobalSeeds.rQuantRot.Count)
                        Debug.LogWarning("postAndGlobalSeeds  " + postAndGlobalSeeds.rQuantRot.Count);
                    multipleRots = (int)(postAndGlobalSeeds.rQuantRot[i] * (numChunks + 1));
                    totalQuantRotAmount = (int)(multipleRots * quantizeRotAnglePost);
                }
                else
                    totalQuantRotAmount = (int)(multipleRots * quantizeRotAnglePost);
                bool rot = false;
                if (quantizeRotProbPost == 0 && i % 2 == 1)
                    rot = true;
                else if (quantizeRotProbPost > 0 && postAndGlobalSeeds.rQuantRot[i] <= quantizeRotProbPost)
                    rot = true;
                if (quantizeRotAnglePost == -1) // -1 denotes consecutive 90 deg rotations
                {
                    totalQuantRotAmount = i % 4 * 90;
                    rot = true;
                }
                if (rot == true && allowQuantizedRandomPostRotation)
                {
                    //Debug.Log("  " + quantizeRotAxisPost + "   " + postsPool[stackIdx].transform.eulerAngles + "\n");

                    if (quantizeRotAxisPost == 0) //x
                    {
                        thisPost.RotateAround(center, thisPost.right, totalQuantRotAmount);
                    }
                    else if (quantizeRotAxisPost == 1)//y
                    {
                        thisPost.RotateAround(center, thisPost.up, totalQuantRotAmount);
                    }
                    else if (quantizeRotAxisPost == 2)//z
                    {
                        thisPost.RotateAround(center, thisPost.forward, totalQuantRotAmount);
                    }
                    //Debug.Log("  " + quantizeRotAxisPost + "   " + postsPool[stackIdx].transform.eulerAngles + "\n");
                }
            }
        }

        //----------
        private void SmallRandomRotations(Transform thisPost, int i)
        {
            if (allowPostRandomization && allowRandPostSmallRotationVariation)
            {
                RandomSeededValuesAF seedValues = postAndGlobalSeeds;
                List<Vector3> smallRotList = seedValues.rSmallRot;
                Vector3 smallRandRot = smallRotList[i];
                thisPost.Rotate(new Vector3(smallRandRot.x, smallRandRot.y, smallRandRot.z));
            }
        }
    }
}