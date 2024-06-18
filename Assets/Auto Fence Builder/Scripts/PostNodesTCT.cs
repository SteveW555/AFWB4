using System.Collections.Generic;
using UnityEngine;

namespace PostNodesTCT
{
    public class Post
    {
        public Vector3 pos = Vector3.zero; // The middle of 3 points that create a corner: previous, pos, nextPos
        public Vector3 prevPos = Vector3.zero, nextPos = Vector3.zero; // The previous and nextPos posts at this pos post's joint
        public Vector3 inVec = Vector3.zero; // The incoming vector to this point, from prevPos to pos
        public Vector3 outVec = Vector3.zero; // The outgoing vector from pos to nextPos
        public Vector3 inVecRaw = Vector3.zero; // Unnormalized incoming vector
        public Vector3 outVecRaw = Vector3.zero; // Unnormalized outgoing vector
        public float width = 1; // The original width of the source path
        public float halfWidth; // Half of the path width, for convenience
        public float miterWidth; // The calculated length of the miter edge
        public float angle = 0; // The relative angle between inVec and outVec. Clockwise and where continuing straight = 0
        public Vector3 prevLeft = Vector3.zero, prevRight = Vector3.zero; // Left and right points of prevPos
        public Vector3 pivotLeft = Vector3.zero, pivotRight = Vector3.zero; // Left and right points of pos
        public Vector3 nextLeft = Vector3.zero, nextRight = Vector3.zero; // Left and right points of nextPos
        public Vector3 outerElbowPt = Vector3.zero, innerElbowPt = Vector3.zero; // The outside and inside corners, can be diff to left/right
        public Vector3 edgeVec = Vector3.zero, edgeVecNormal = Vector3.zero; // The dir of miter edge, always from perspective of the left boundary point
        public bool isNode = false; // Indicates if the post is a corner
        public float cornerAngleThreshold = 1; // Angle in degrees that defines a corner

        public Post(Vector3 prev, Vector3 postPos, Vector3 next, float width = 1, bool isNode = false)
        {
            InitializePost(prev, postPos, next, width, isNode);
        }

        // This method updates the post position with new values and recalculates dependent properties.
        public void UpdatePost(Vector3 newPos, bool isNodePost = default)
        {
            this.pos = newPos; // Update the pos position
            // Only update isNode status if isNodePost is explicitly provided
            if (isNodePost != default)
                this.isNode = isNodePost;
            CalculateProperties();
        }

        // This also updates prev & next, e.g. if a post has been inserted or deleted
        public void UpdatePostLinks(Vector3 prevPos, Vector3 postPos, Vector3 nextPos)
        {
            this.pos = postPos;
            this.prevPos = prevPos;
            this.nextPos = nextPos;
            CalculateProperties();
        }

        private void InitializePost(Vector3 prev, Vector3 postPos, Vector3 next, float width = 1, bool isNode = false)
        {
            this.prevPos = prev;
            this.pos = postPos;
            this.nextPos = next;
            this.width = width;
            this.halfWidth = width / 2;
            this.isNode = isNode;
            CalculateProperties();
        }

        private void CalculateProperties()
        {
            CalculateVectors();
            CalculateAngle();
            CalculateSidePoints();
        }

        private void CalculateVectors()
        {
            inVecRaw = pos - prevPos;
            outVecRaw = nextPos - pos;

            inVec = inVecRaw.normalized;
            outVec = outVecRaw.normalized;
        }

        private void CalculateAngle()
        {
            // Calculate the angle and determine if this post is a corner
            angle = Vector3.SignedAngle(inVec, outVec, Vector3.up);
            if (angle < 0)
            {
                angle += 360;
            }

            // Use cornerAngleThreshold to determine if the post is a corner
            isNode = Mathf.Abs(angle) > cornerAngleThreshold;
        }

        private void CalculateSidePoints()
        {
            Vector3 leftVectorPrev = Vector3.Cross(inVec, Vector3.up).normalized * halfWidth;
            Vector3 rightVectorPrev = -leftVectorPrev; // Opposite direction for prevPos

            prevLeft = prevPos + leftVectorPrev;
            prevRight = prevPos + rightVectorPrev;

            Vector3 leftVectorNext = Vector3.Cross(outVec, Vector3.up).normalized * halfWidth;
            Vector3 rightVectorNext = -leftVectorNext; // Opposite direction for nextPos

            pivotLeft = pos + leftVectorPrev; // Or adjust based on context
            pivotRight = pos + rightVectorPrev; // Or adjust

            nextLeft = nextPos + leftVectorNext;
            nextRight = nextPos + rightVectorNext;
        }
    }

    //==================================================================================================
    //                                  PostContainer
    //==================================================================================================
    public class PostContainer : IEnumerable<Post>
    {
        private List<Post> posts = new List<Post>();

        // Method to add a post at the end of the list.
        public void AddPost(Vector3 pivot, float width = 1, bool isNode = false)
        {
            Post newPost = new Post(Vector3.zero, pivot, Vector3.zero, width, isNode); // Initialize with temporary vectors
            if (posts.Count > 0)
            {
                // Set 'prevPos' to the last post's 'pos'
                newPost.prevPos = posts[posts.Count - 1].pos;
            }
            posts.Add(newPost);
            UpdatePostLinks(); // Update all post links
        }

        // Method to insert a post after a specified index.
        public void InsertPostAfter(int index, Vector3 pivot, float width = 1, bool isNode = false)
        {
            if (index < 0 || index >= posts.Count)
            {
                // Optionally handle the invalid index (e.g., throw an exception or add the post at the end)
                return;
            }

            Post newPost = new Post(posts[index].pos, pivot, Vector3.zero, width, isNode); // Initialize with temporary vectors
            posts.Insert(index + 1, newPost);
            UpdatePostLinks(); // Recalculate links for all posts
        }

        // Updates 'prevPos' and 'nextPos' for each post in the list based on their positions.
        private void UpdatePostLinks()
        {
            for (int i = 0; i < posts.Count; i++)
            {
                Vector3 prevPos = i > 0 ? posts[i - 1].pos : Vector3.zero;
                Vector3 nextPos = i < posts.Count - 1 ? posts[i + 1].pos : Vector3.zero;

                // Assuming UpdatePost is a method within Post that updates its properties based on new 'prevPos' and 'nextPos'
                posts[i].UpdatePostLinks(prevPos, posts[i].pos, nextPos);
            }
        }

        private void UpdatePost(int index, Vector3 postPos, bool isNodePost = default)
        {
            posts[index].UpdatePost(postPos, isNodePost);
        }

        public Post FindPostByPosition(Vector3 pivot, float tolerance = 0.001f)
        {
            foreach (Post post in posts)
            {
                if (Vector3.Distance(post.pos, pivot) <= tolerance)
                {
                    return post;
                }
            }
            return null; // Return null if no matching post is found
        }

        // Implementation of IEnumerable<Post> to enable iteration over the post list.
        public IEnumerator<Post> GetEnumerator() => posts.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}