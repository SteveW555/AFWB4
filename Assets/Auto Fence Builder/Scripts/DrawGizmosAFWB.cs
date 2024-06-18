using MeshUtils;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;
using static AFWB.AutoFenceCreator;

namespace AFWB
{
    

    public struct UprightMarker
    {
        /// <summary>The position of the marker.</summary>
        public Vector3 position;
        /// <summary>The radius of the marker.</summary>
        public float radius;
        /// <summary>The height of the marker.</summary>
        public float height;
        /// <summary>The color of the marker.</summary>
        public Color color;
        /// <summary>
        /// Initializes a new instance of the <see cref="UprightMarker"/> struct.
        /// </summary>
        /// <param name="pos">The position of the marker.</param>
        /// <param name="radius">The radius of the marker.</param>
        /// <param name="height">The height of the marker.</param>
        /// <param name="color">The color of the marker.</param>
        public UprightMarker(Vector3 pos, float radius = 0.1f, float height = 2f, Color color = default)
        {
            position = pos;
            this.radius = radius;
            this.height = height;
            this.color = color;
            this.color.a = 0.75f;
        }
        public Vector3 Size()
        {
            Vector3 size = new Vector3(radius * 2, height, radius * 2);
            return size;
        }
        public Vector3 BasePosition()
        {
            Vector3 basePosition = new Vector3(position.x, position.y + height / 2.0f, position.z);
            return basePosition;
        }
    }

    public partial class AutoFenceCreator
    {

        public float postMarkerHeight = 0.8f, extraMarkerHeight = 0.5f;
        public float extraMarkerScale = 0.3f;
        private Mesh mesh = null;
        public List<Vector3> postVizMarkers = new List<Vector3>();
        public List<Vector3> extraVizMarkers = new List<Vector3>();
        public Color lighterRed = new Color(1.0f, 0.4f, 0.4f, 0.9f);
        public Color lighterGreen = new Color(0.5f, 1.0f, 0.5f, 0.8f);
        public Color lighterBlue = new Color(0.1f, 0.1f, 0.9f);

        //-- For Extra grids, the are where the outgoing section would overlap the incoming section
        //-- For Extra grids, the are where a gap will appear at the outer elbow of a corner between the incoming and outgoing sections
        //public List<Quadrilateral2D> fillExtrasZone = new List<Quadrilateral2D>();
        public List<Pentagon2D> fillExtrasZone = new List<Pentagon2D>();

        public Vector3 singleDirectionVector = Vector3.zero; // this is a temp convenience to assign anywhere in code to have it drawn by Gizmos

        public Vector3 marker1 = Vector3.zero, marker2 = Vector3.zero;

        public List<Quadrilateral2D> overlapExtrasZone = new List<Quadrilateral2D>();
        public List<Pentagon2D> overlapExtrasPentagonZone = new List<Pentagon2D>();
        public List<List<Vector2>> arcPointsLists = new List<List<Vector2>>();
        internal List<Triangle2D> triangleZones = new List<Triangle2D>();
        //--------------------------------------------------------------------------------------------


        //  Called from AtuoFenceCreator. OnDrawGizmos()
        //================================================
        private void DrawVisualDebug()
        {
            if (postVectors == null)
                return;
            //return;

            DrawInnerElbowTriangleZones();
            DrawOverlapZonesPentagon();
            DrawFillZones();
            DrawUprightMarkers();

            //============ Extra Grid Overlap Zones ==================
            //DrawOverlapZonesQuad();

            if (Selection.activeGameObject != this.gameObject)
                return;
            List<List<Vector2>> listofArcPointsLists = new List<List<Vector2>>();
            ArcTCT arc = new ArcTCT();

            //===============================================
            //          DrawTCT arcList
            //===============================================
            int numArcPointsLists = arcPointsLists.Count;
            for (int i = 0; i < numArcPointsLists; i++)
            {
                List<Vector2> currList = arcPointsLists[i];
                int numPoints = currList.Count;
                for (int j = 0; j < numPoints; j++)
                {
                    Vector2 pt = currList[j];
                    Vector3 pt3D = new Vector3(pt.x, 0, pt.y);
                    if (j == 0)
                        Gizmos.color = Color.red;
                    else if (j == numPoints - 1)
                        Gizmos.color = Color.blue;
                    else
                        Gizmos.color = Color.green;

                    Gizmos.DrawSphere(pt3D, 0.2f);

                    Gizmos.color = Color.green;
                    if (j < numPoints - 1)
                    {
                        Vector3 nextPt3D = currList[j + 1].To3D();
                        Gizmos.DrawLine(pt3D, nextPt3D);
                    }
                }
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawCube(marker1, Vector3.one * 0.25f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(marker2, Vector3.one * 0.25f);

            for (int i = 0; i < postVectors.Count; i++)
            {
                //DrawArcGizmo(postDirectionVectors.position[i], postDirectionVectors[i], radius, startAngle, endAngle, segments);
            }
            //DrawArcGizmo(postDirectionVectors[0], radius, startAngle, endAngle, segments);

            //return;
            DrawPostDirectionVectors();
        }
        private void DrawUprightMarkers()
        {
            if (showOtherMarkers == true)
            {
                for (int i = 0; i < uprightMarkers.Count; i++)
                {
                    UprightMarker marker = uprightMarkers[i];
                    Vector3 pos = marker.position;
                    Gizmos.color = marker.color;
                    Gizmos.DrawCube(marker.BasePosition(), marker.Size());
                }
            }
        }

        private void DrawFillZones()
        {
            if (showFillZones == true && fillExtrasZone != null && fillExtrasZone.Count > 0)
            {
                bool drawCubesAtVerts = true;
                for (int i = 0; i < fillExtrasZone.Count; i++)
                {
                    //Quadrilateral2D fillQuad = fillExtrasZone[i];
                    Pentagon2D fillPent = fillExtrasZone[i];
                    Vector3[] pts = fillPent.ToVector3Array(yValue: 0.3f);
                    //DrawTCT lines between the corners to form the box
                    Gizmos.color = new Color(.1f, 0.99f, 0.1f); // Set Gizmo color
                    Gizmos.DrawLine(pts[0], pts[1]);
                    Gizmos.DrawLine(pts[1], pts[2]);
                    Gizmos.DrawLine(pts[2], pts[3]);
                    Gizmos.DrawLine(pts[3], pts[4]);
                    Gizmos.DrawLine(pts[4], pts[0]);

                    if (drawCubesAtVerts == true)
                    {
                        //-- DrawTCT a cube to mark which point is pts[0] R
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(pts[0], Vector3.one * 0.15f);

                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(pts[1], Vector3.one * 0.15f); // G

                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(pts[2], Vector3.one * 0.15f); // B

                        Gizmos.color = Color.cyan;
                        Gizmos.DrawCube(pts[3], Vector3.one * 0.15f); // Cyan

                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(pts[4], Vector3.one * 0.15f); // Magenta
                    }
                }
            }
        }
        private void DrawOverlapZonesQuad()
        {
            if (showOverlapZones == true && overlapExtrasZone != null && overlapExtrasZone.Count > 0)
            {
                bool drawCubesAtVerts = true;
                for (int i = 0; i < overlapExtrasZone.Count - 1; i++)
                {
                    Quadrilateral2D overlapQuad = overlapExtrasZone[i];
                    Vector3[] pts = overlapQuad.ToVector3Array(yValue: 0.3f);
                    //DrawTCT lines between the corners to form the box
                    Gizmos.color = new Color(1, 0.6f, 0.2f); // Set Gizmo color
                    Gizmos.DrawLine(pts[0], pts[1]);
                    Gizmos.DrawLine(pts[1], pts[2]);
                    Gizmos.DrawLine(pts[2], pts[3]);
                    Gizmos.DrawLine(pts[3], pts[0]);

                    if (drawCubesAtVerts == true)
                    {
                        //-- DrawTCT a cube to mark which point is pts[0]
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(pts[0], Vector3.one * 0.15f);
                        //-- DrawTCT a cube to mark which point is pts[1]
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(pts[1], Vector3.one * 0.15f);
                    }
                }
            }
        }

        private void DrawOverlapZonesPentagon()
        {
            if (showOverlapZones == true && overlapExtrasPentagonZone != null && overlapExtrasPentagonZone.Count > 0)
            {
                bool drawCubesAtVerts = true;
                for (int i = 0; i < overlapExtrasPentagonZone.Count - 0; i++)
                {
                    Pentagon2D overlapPent = overlapExtrasPentagonZone[i];
                    Vector3[] pts = overlapPent.ToVector3Array(yValue: 0.3f);
                    //DrawTCT lines between the corners to form the box
                    Gizmos.color = new Color(1, 0.6f, 0.2f); // Set Gizmo color
                    Gizmos.DrawLine(pts[0], pts[1]);
                    Gizmos.DrawLine(pts[1], pts[2]);
                    Gizmos.DrawLine(pts[2], pts[3]);
                    Gizmos.DrawLine(pts[3], pts[4]);
                    Gizmos.DrawLine(pts[4], pts[0]);

                    if (drawCubesAtVerts == true)
                    {
                        float size = 0.2f;
                        //-- Draw cubes to mark which point is pts[0]. Color IDs  = R G B C M
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(pts[0], Vector3.one * size);
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(pts[1], Vector3.one * size);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(pts[2], Vector3.one * size);
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawCube(pts[3], Vector3.one * size);
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawCube(pts[4], Vector3.one * size);
                    }
                }
            }
        }

        private void DrawArcGizmo(Vector3 origin, Vector3 forward, float radius, float a, float b, int seg)
        {
            // Ensure clockwise rotation and adjust if end angle is less than start angle
            if (b < a) b += 360f;

            // Calculate the number of points based on 5-degree intervals
            int pointsCount = Mathf.CeilToInt((b - a) / 5f);

            // Previous point, initialized to the start of the arc
            //Vector3 prevPoint = MeshUtilitiesAFWB.SetPointAtCompassAngleAroundPivotYNoQ(origin, a);
            Vector3 prevPoint = MeshUtilitiesAFWB.RotatePointAroundPivotYFromLocalForwardNoQ(forward, origin, a, radius);

            // DrawTCT the arc in segments
            for (int i = 1; i <= pointsCount; i++)
            {
                // Calculate the current angle
                float currentAngle = Mathf.Clamp(a + i * 5f, a, b);

                // Calculate the current point
                //Vector3 currentPoint = MeshUtilitiesAFWB.SetPointAtCompassAngleAroundPivotYNoQ(origin, currentAngle);
                Vector3 currentPoint = MeshUtilitiesAFWB.RotatePointAroundPivotYFromLocalForwardNoQ(forward, origin, currentAngle, radius);

                // DrawTCT line from the previous point to the current point
                Gizmos.DrawLine(prevPoint, currentPoint);

                // Update the previous point
                prevPoint = currentPoint;
            }

            /*Vector3 posA = MeshUtilitiesAFWB.SetPointAtCompassAngleArountPivotY(origin, 45);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(posA, 0.2f);

            //Vector3 posB = MeshUtilitiesAFWB.SetPointAtCompassAngleArountPivotY(origin, 359);
            Vector3 posB = MeshUtilitiesAFWB.SetPointAtCompassAngleAroundPivotYNoQ(origin, 380);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(posB, 0.2f);*/
        }

        /*void DrawArcGizmo(Vector3 origin, float radius, float a, float b, int seg)
        {
            // Convert angles from degrees to radians and adjust for Unity's coordinate system
            a = -a + 90; // Unity's coordinate adjustment for clockwise rotation
            b = -b + 90;
            a *= Mathf.Deg2Rad;
            b *= Mathf.Deg2Rad;

            // Calculate the number of segments based on the angle difference and segments required
            int pointsCount = seg;
            float angleStep = (b - a) / pointsCount;

            // Previous point, initialized to the start of the arc
            Vector3 prevPoint = origin + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Gizmos.DrawSphere(prevPoint, 0.4f);
            // DrawTCT the arc in segments
            for (int i = 1; i <= pointsCount; i++)
            {
                // Calculate the current angle
                float currentAngle = a + i * angleStep;

                // Calculate the current point
                Vector3 currentPoint = origin + new Vector3(Mathf.Cos(currentAngle) * radius, 0f, Mathf.Sin(currentAngle) * radius);

                // DrawTCT line from the previous point to the current point
                Gizmos.DrawLine(prevPoint, currentPoint);

                // Update the previous point
                prevPoint = currentPoint;
            }*/

        //===============================================
        //          DrawTCT Triangle Zones
        //===============================================
        private void DrawInnerElbowTriangleZones()
        {
            /*if (showOverlapZones == true && triangleZones != null && triangleZones.Count > 0)
            {
                bool drawCubesAtVerts = true;
                for (int i = 0; i < triangleZones.Count - 1; i++)
                {
                    Triangle2D tri = triangleZones[i];
                    Vector3[] pts = tri.ToVector3Array(yValue: 0.3f);
                    //-- DrawTCT lines between the corners to form the box
                    Gizmos.color = new Color(.2f, 0.3f, 0.8f); // Set Gizmo color
                    Gizmos.DrawLine(pts[0], pts[1]);
                    Gizmos.DrawLine(pts[1], pts[2]);
                    Gizmos.DrawLine(pts[2], pts[0]);

                    if (drawCubesAtVerts == true)
                    {
                        //-- DrawTCT a cube to mark which point is pts[0]
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(pts[0], Vector3.one * 0.15f);
                        //-- DrawTCT a cube to mark which point is pts[1]
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(pts[1], Vector3.one * 0.15f);
                    }
                }
            }*/
        }

        //===============================================
        //          DrawTCT Post Direction Vectors
        //===============================================
        private void DrawPostDirectionVectors()
        {
            //postVectors.checkForCountExceptions = false; //TODO
            int postVectorsCount = postVectors.Count;
            PostVector postVector;
            if (showPostDirectionVectors == true && postVectorsCount > 0 && postVectors != null)
            {
                Gizmos.color = lighterBlue;
                for (int i = 0; i < postVectorsCount; i++)
                {
                    postVector = postVectors[i];
                    Vector3 postMarkerPos = postVector.Position;
                    postMarkerPos.y += postMarkerHeight / 2;
                    Vector3 dirVec = postVector.Forward;
                    Vector3 to = postMarkerPos + (dirVec * directionVectorLength / 2);
                    Gizmos.DrawLine(postMarkerPos, to);
                    //Debug.Log($"{postMarkerPos}       { to} \n");
                }
                //-- and Right
                if (showPostDirectionVectorsRight == true)
                {
                    Gizmos.color = lighterRed;
                    /*for (int i = 0; i < postVectorsCount; i++)
                    {
                        Vector3 postMarkerPos = allPostPositions[i];
                        postMarkerPos.y += postMarkerHeight / 2;
                        Vector3 dirVec = postDirectionVectors.dirVectorsRight[i];
                        Gizmos.DrawLine(postMarkerPos, postMarkerPos + (dirVec * directionVectorLength / 2));
                    }*/

                    Gizmos.color = Color.black;
                    //TODO
                    int avgRightCount = postVectorsCount;
                    for (int i = 0; i < avgRightCount; i++)
                    {
                        postVector = postVectors[i];
                        Vector3 postMarkerPos = allPostPositions[i];
                        postMarkerPos.y += postMarkerHeight / 2;
                        Vector3 dirVec = postVector.Forward;
                        Gizmos.DrawLine(postMarkerPos, postMarkerPos + (dirVec * directionVectorLength / 2));
                    }
                    /*int avgRightCount = postDirectionVectors.postAvgRight.Count;
                    for (int i = 0; i < avgRightCount; i++)
                    {
                        Vector3 postMarkerPos = allPostPositions[i];
                        postMarkerPos.y += postMarkerHeight / 2;
                        Vector3 dirVec = postDirectionVectors.postAvgRight[i];
                        Gizmos.DrawLine(postMarkerPos, postMarkerPos + (dirVec * directionVectorLength / 2));
                    }*/
                }
            }
        }
    }
    /*public class GizmoDrawManager
    {
        AutoFenceCreator af;
        
        /// <summary> Default color for lines.</summary>
        public static Color defaultLineColor = Color.blue;

        /// <summary>
        /// Default color for cubes.
        /// </summary>
        public static Color defaultCubeColor = new Color(0, 0, 1, 0.3f);

        /// <summary>
        /// Default color for wire cubes.
        /// </summary>
        public static Color defaultWireCubeColor = Color.blue;

        /// <summary>
        /// Default color for spheres.
        /// </summary>
        public static Color defaultSphereColor = new Color(0,0,1,0.3f);

        /// <summary>
        /// Default color for wire spheres.
        /// </summary>
        public static Color defaultWireSphereColor = Color.blue;

        /// <summary>
        /// Default color for meshes.
        /// </summary>
        public static Color defaultMeshColor = Color.blue;

        private List<GizmoLine> gizmoLines = new List<GizmoLine>();
        private List<GizmoCube> gizmoCubes = new List<GizmoCube>();
        private List<GizmoCube> wireCubes = new List<GizmoCube>();
        private List<GizmoSphere> gizmoSpheres = new List<GizmoSphere>();
        private List<GizmoSphere> wireSpheres = new List<GizmoSphere>();
        private List<GizmoMesh> gizmoMeshes = new List<GizmoMesh>();



        public GizmoDrawManager(AutoFenceCreator af)
        {
            this.af = af;
        }

        //public static GizmoDrawManager af.gizmoManager;

        /// <summary>Ensures a single af.gizmoManagerof the GizmoDrawManager exists. </summary>
        void Awake()
        {
            Debug.Log("GizmoDrawManager Awake\n");
        }

        /// <summary>
        /// Draws the gizmos each frame in the scene view.
        /// </summary>
        
        // Called from AtuoFenceCreator. OnDrawGizmos()
        //================================================
        public void DrawGizmoManager()
        {
            return;
            
            //Debug.Log("GizmoDrawManager OnDrawGizmos()\n");

            foreach (var line in gizmoLines)
            {
                Gizmos.color = line.color;
                Gizmos.DrawLine(line.start, line.end);
            }

            foreach (var cube in gizmoCubes)
            {
                Gizmos.color = cube.color;
                Gizmos.DrawCube(cube.centerPos, cube.size);
            }

            foreach (var wireCube in wireCubes)
            {
                Gizmos.color = wireCube.color;
                Gizmos.DrawWireCube(wireCube.centerPos, wireCube.size);
            }

            foreach (var sphere in gizmoSpheres)
            {
                Gizmos.color = sphere.color;
                Gizmos.DrawSphere(sphere.centerPos, sphere.radius);
            }

            foreach (var wireSphere in wireSpheres)
            {
                Gizmos.color = wireSphere.color;
                Gizmos.DrawWireSphere(wireSphere.centerPos, wireSphere.radius);
            }

            foreach (var mesh in gizmoMeshes)
            {
                Gizmos.color = mesh.color;
                if (mesh.isWire)
                {
                    Gizmos.DrawWireMesh(mesh.mesh, mesh.position, mesh.rotation, mesh.scale);
                }
                else
                {
                    Gizmos.DrawMesh(mesh.mesh, mesh.position, mesh.rotation, mesh.scale);
                }
            }
        }

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="start">The start point of the line.</param>
        /// <param name="end">The end point of the line.</param>
        /// <param name="color">The color of the line. Defaults to <see cref="defaultLineColor"/> if not specified.</param>
        public  void DrawLine(Vector3 start, Vector3 end, Color? color = null)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawLine.");
                return;
            }

            af.gizmoManager.gizmoLines.Add(new GizmoLine(start, end, color ?? defaultLineColor));
        }

        public  void DrawCube(Vector3 centerPos, Color? color = null, Vector3 size = default)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawCube.");
                return;
            }
            af.gizmoManager.gizmoCubes.Add(new GizmoCube(centerPos, color ?? defaultCubeColor, size == default ? Vector3.one : size));
        }
        /// <summary>
        /// Draws a virtual post at the specified position. Places base of cube at centerPos
        /// </summary>

        public void DrawPost(Vector3 centerPos, Color? color = null, Vector3 size = default)
        {
            centerPos.y += size.y / 2;
            af.gizmoManager.gizmoCubes.Add(new GizmoCube(centerPos, color ?? defaultCubeColor, size == default ? Vector3.one : size));
        }

        public  void DrawWireCube(Vector3 centerPos, Color? color = null, Vector3 size = default)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawWireCube.");
                return;
            }

            af.gizmoManager.wireCubes.Add(new GizmoCube(centerPos, color ?? defaultWireCubeColor, size == default ? Vector3.one : size));
        }

        public  void DrawSphere(Vector3 centerPos, Color? color = null, float radius = 1.0f)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawSphere.");
                return;
            }

            af.gizmoManager.gizmoSpheres.Add(new GizmoSphere(centerPos, color ?? defaultSphereColor, radius));
        }

        public  void DrawWireSphere(Vector3 centerPos, Color? color = null, float radius = 1.0f)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawWireSphere.");
                return;
            }

            af.gizmoManager.wireSpheres.Add(new GizmoSphere(centerPos, color ?? defaultWireSphereColor, radius));
        }

        public  void DrawMesh(Mesh mesh, Vector3 position, Color? color = null, Vector3 scale = default, Quaternion rotation = default, bool isWire = false)
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in DrawMesh.");
                return;
            }

            af.gizmoManager.gizmoMeshes.Add(new GizmoMesh(mesh, position, color ?? defaultMeshColor, scale == default ? Vector3.one : scale, rotation == default ? Quaternion.identity : rotation, isWire));
        }

        public  void ClearAll()
        {
            if (af.gizmoManager== null)
            {
                Debug.LogWarning("GizmoDrawManager instance is null in ClearAll.");
                return;
            }

            af.gizmoManager.gizmoLines.Clear();
            af.gizmoManager.gizmoCubes.Clear();
            af.gizmoManager.wireCubes.Clear();
            af.gizmoManager.gizmoSpheres.Clear();
            af.gizmoManager.wireSpheres.Clear();
            af.gizmoManager.gizmoMeshes.Clear();
        }
    }



    //===============================================================================
    //          Versions of Unity DrawGizmos for debug use
    //===============================================================================
    /// <summary> Use with GizmoDrawManger. See GizmoDrawManager for useage </summary>
    public struct GizmoLine
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;

        public GizmoLine(Vector3 start, Vector3 end, Color color = default)
        {
            this.start = start;
            this.end = end;
            this.color = color == default ? Color.blue : color;
        }
    }

    public struct GizmoCube
    {
        public Vector3 centerPos;
        public Color color;
        public Vector3 size;

        public GizmoCube(Vector3 centerPos, Color color = default, Vector3 size = default)
        {
            this.centerPos = centerPos;
            this.color = color == default ? Color.blue : color;
            this.size = size == default ? Vector3.one : size;
        }
    }

    public struct GizmoSphere
    {
        public Vector3 centerPos;
        public Color color;
        public float radius;

        public GizmoSphere(Vector3 centerPos, Color color = default, float radius = 1.0f)
        {
            this.centerPos = centerPos;
            this.color = color == default ? Color.blue : color;
            this.radius = radius;
        }
    }

    public struct GizmoMesh
    {
        public Mesh mesh;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Color color;
        public bool isWire;

        public GizmoMesh(Mesh mesh, Vector3 position, Color color = default, Vector3 scale = default, Quaternion rotation = default, bool isWire = false)
        {
            this.mesh = mesh;
            this.position = position;
            this.color = color == default ? Color.blue : color;
            this.scale = scale == default ? Vector3.one : scale;
            this.rotation = rotation == default ? Quaternion.identity : rotation;
            this.isWire = isWire;
        }
    }*/
}