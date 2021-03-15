using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Debug helpers
	/// </summary>

	public static class MMDebug 
	{
		public static MMConsole _console;
        /// whether or not debug logs (MMDebug.DebugLogTime, MMDebug.DebugOnScreen) should be displayed
        public static bool DebugLogsEnabled = true;
        /// whether or not debug draws should be executed
        public static bool DebugDrawEnabled = true;

		/// <summary>
		/// Draws a debug ray in 2D and does the actual raycast
		/// </summary>
		/// <returns>The raycast hit.</returns>
		/// <param name="rayOriginPoint">Ray origin point.</param>
		/// <param name="rayDirection">Ray direction.</param>
		/// <param name="rayDistance">Ray distance.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="debug">If set to <c>true</c> debug.</param>
		/// <param name="color">Color.</param>
		public static RaycastHit2D RayCast(Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);		
		}

        /// <summary>
        /// Does a boxcast and draws a box gizmo
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="size"></param>
        /// <param name="angle"></param>
        /// <param name="direction"></param>
        /// <param name="length"></param>
        /// <param name="mask"></param>
        /// <param name="color"></param>
        /// <param name="drawGizmo"></param>
        /// <returns></returns>
        public static RaycastHit2D BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float length, LayerMask mask, Color color, bool drawGizmo = false)
        {
            if (drawGizmo && DebugDrawEnabled)
            {
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

                Vector3[] points = new Vector3[8];

                float halfSizeX = size.x / 2f;
                float halfSizeY = size.y / 2f;

                points[0] = rotation * (origin + (Vector2.left * halfSizeX) + (Vector2.up * halfSizeY)); // top left
                points[1] = rotation * (origin + (Vector2.right * halfSizeX) + (Vector2.up * halfSizeY)); // top right
                points[2] = rotation * (origin + (Vector2.right * halfSizeX) - (Vector2.up * halfSizeY)); // bottom right
                points[3] = rotation * (origin + (Vector2.left * halfSizeX) - (Vector2.up * halfSizeY)); // bottom left
                
                points[4] = rotation * ((origin + Vector2.left * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top left
                points[5] = rotation * ((origin + Vector2.right * halfSizeX + Vector2.up * halfSizeY) + length * direction); // top right
                points[6] = rotation * ((origin + Vector2.right * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom right
                points[7] = rotation * ((origin + Vector2.left * halfSizeX - Vector2.up * halfSizeY) + length * direction); // bottom left
                                
                Debug.DrawLine(points[0], points[1], color);
                Debug.DrawLine(points[1], points[2], color);
                Debug.DrawLine(points[2], points[3], color);
                Debug.DrawLine(points[3], points[0], color);

                Debug.DrawLine(points[4], points[5], color);
                Debug.DrawLine(points[5], points[6], color);
                Debug.DrawLine(points[6], points[7], color);
                Debug.DrawLine(points[7], points[4], color);
                
                Debug.DrawLine(points[0], points[4], color);
                Debug.DrawLine(points[1], points[5], color);
                Debug.DrawLine(points[2], points[6], color);
                Debug.DrawLine(points[3], points[7], color);

            }
            return Physics2D.BoxCast(origin, size, angle, direction, length, mask);
        }

        /// <summary>
        /// Draws a debug ray without allocating memory
        /// </summary>
        /// <returns>The ray cast non alloc.</returns>
        /// <param name="array">Array.</param>
        /// <param name="rayOriginPoint">Ray origin point.</param>
        /// <param name="rayDirection">Ray direction.</param>
        /// <param name="rayDistance">Ray distance.</param>
        /// <param name="mask">Mask.</param>
        /// <param name="color">Color.</param>
        /// <param name="drawGizmo">If set to <c>true</c> draw gizmo.</param>
        public static RaycastHit2D MonoRayCastNonAlloc(RaycastHit2D[] array, Vector2 rayOriginPoint, Vector2 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{	
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			if (Physics2D.RaycastNonAlloc(rayOriginPoint, rayDirection, array, rayDistance, mask) > 0)
			{
				return array[0];
			}
			return new RaycastHit2D();        	
		}

		/// <summary>
		/// Draws a debug ray in 3D and does the actual raycast
		/// </summary>
		/// <returns>The raycast hit.</returns>
		/// <param name="rayOriginPoint">Ray origin point.</param>
		/// <param name="rayDirection">Ray direction.</param>
		/// <param name="rayDistance">Ray distance.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="debug">If set to <c>true</c> debug.</param>
		/// <param name="color">Color.</param>
		/// <param name="drawGizmo">If set to <c>true</c> draw gizmo.</param>
		public static RaycastHit Raycast3D(Vector3 rayOriginPoint, Vector3 rayDirection, float rayDistance, LayerMask mask, Color color,bool drawGizmo=false)
		{
			if (drawGizmo && DebugDrawEnabled) 
			{
				Debug.DrawRay (rayOriginPoint, rayDirection * rayDistance, color);
			}
			RaycastHit hit;
			Physics.Raycast(rayOriginPoint,rayDirection,out hit,rayDistance,mask);	
			return hit;
		}

		/// <summary>
		/// Outputs the message object to the console, prefixed with the current timestamp
		/// </summary>
		/// <param name="message">Message.</param>
		public static void DebugLogTime(object message, string color="")
		{
            if (!DebugLogsEnabled)
            {
                return;
            }

			string colorPrefix = "";
			string colorSuffix = "";
			if (color != "")
			{
				colorPrefix = "<color="+color+">";
				colorSuffix = "</color>";
			}
			Debug.Log (colorPrefix + Time.time + " " + message + colorSuffix);
		}

		/// <summary>
		/// Instantiates a MMConsole if there isn't one already, and adds the message in parameter to it.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void DebugOnScreen(string message)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole();
			_console.AddMessage(message);
		}

		/// <summary>
		/// Instantiates a MMConsole if there isn't one already, and displays the label in bold and its value next to it.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="value">Value.</param>
		/// <param name="fontSize">The optional font size.</param>
		public static void DebugOnScreen(string label, object value, int fontSize=10)
        {
            if (!DebugLogsEnabled)
            {
                return;
            }

            InstantiateOnScreenConsole(fontSize);
			_console.AddMessage("<b>"+label+"</b> : "+value);
		}

		/// <summary>
		/// Instantiates the on screen console if there isn't one already
		/// </summary>
		public static void InstantiateOnScreenConsole(int fontSize=10)
		{
            if (!DebugLogsEnabled)
            {
                return;
            }

            if (_console == null)
			{
				// we instantiate the console
				GameObject newGameObject = new GameObject();
				newGameObject.name="MMConsole";
				_console = newGameObject.AddComponent<MMConsole>();
				_console.SetFontSize(fontSize);
			}
		}

		/// <summary>
		/// Draws a gizmo arrow going from the origin position and along the direction Vector3
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
	    {
            if (!DebugDrawEnabled)
            {
                return;
            }

	        Gizmos.color = color;
	        Gizmos.DrawRay(origin, direction);
	       
			DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
	    }

	    /// <summary>
		/// Draws a debug arrow going from the origin position and along the direction Vector3
	    /// </summary>
	    /// <param name="origin">Origin.</param>
	    /// <param name="direction">Direction.</param>
	    /// <param name="color">Color.</param>
	    public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 0.2f, float arrowHeadAngle = 35f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction, color);
	       
			DrawArrowEnd(false,origin,direction,color,arrowHeadLength,arrowHeadAngle);
	    }

		/// <summary>
		/// Draws a debug arrow going from the origin position and along the direction Vector3
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		/// <param name="arrowLength">Arrow length.</param>
		/// <param name="arrowHeadLength">Arrow head length.</param>
		/// <param name="arrowHeadAngle">Arrow head angle.</param>
		public static void DebugDrawArrow(Vector3 origin, Vector3 direction, Color color, float arrowLength, float arrowHeadLength = 0.20f, float arrowHeadAngle = 35.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Debug.DrawRay(origin, direction * arrowLength, color);

			DrawArrowEnd(false,origin,direction * arrowLength,color,arrowHeadLength,arrowHeadAngle);
		}

		/// <summary>
		/// Draws a debug cross of the specified size and color at the specified point
		/// </summary>
		/// <param name="spot">Spot.</param>
		/// <param name="crossSize">Cross size.</param>
		/// <param name="color">Color.</param>
		public static void DebugDrawCross (Vector3 spot, float crossSize, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 tempOrigin = Vector3.zero;
			Vector3 tempDirection = Vector3.zero;

			tempOrigin.x = spot.x - crossSize / 2;
			tempOrigin.y = spot.y - crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
			tempDirection.y = 1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);

			tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y + crossSize / 2;
            tempOrigin.z = spot.z ;
            tempDirection.x = 1; 
			tempDirection.y = -1;
            tempDirection.z = 0;
            Debug.DrawRay (tempOrigin, tempDirection * crossSize, color);
		}

		/// <summary>
		/// Draws the arrow end for DebugDrawArrow
		/// </summary>
		/// <param name="drawGizmos">If set to <c>true</c> draw gizmos.</param>
		/// <param name="arrowEndPosition">Arrow end position.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		/// <param name="arrowHeadLength">Arrow head length.</param>
		/// <param name="arrowHeadAngle">Arrow head angle.</param>
		private static void DrawArrowEnd (bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            if (direction == Vector3.zero)
			{
				return;
			}
	        Vector3 right = Quaternion.LookRotation (direction) * Quaternion.Euler (arrowHeadAngle, 0, 0) * Vector3.back;
	        Vector3 left = Quaternion.LookRotation (direction) * Quaternion.Euler (-arrowHeadAngle, 0, 0) * Vector3.back;
	        Vector3 up = Quaternion.LookRotation (direction) * Quaternion.Euler (0, arrowHeadAngle, 0) * Vector3.back;
	        Vector3 down = Quaternion.LookRotation (direction) * Quaternion.Euler (0, -arrowHeadAngle, 0) * Vector3.back;
	        if (drawGizmos) 
	        {
	            Gizmos.color = color;
	            Gizmos.DrawRay (arrowEndPosition + direction, right * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, left * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, up * arrowHeadLength);
	            Gizmos.DrawRay (arrowEndPosition + direction, down * arrowHeadLength);
	        }
	        else
	        {
	            Debug.DrawRay (arrowEndPosition + direction, right * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, left * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, up * arrowHeadLength, color);
	            Debug.DrawRay (arrowEndPosition + direction, down * arrowHeadLength, color);
	        }
	    }

		/// <summary>
		/// Draws handles to materialize the bounds of an object on screen.
		/// </summary>
		/// <param name="bounds">Bounds.</param>
		/// <param name="color">Color.</param>
		public static void DrawHandlesBounds(Bounds bounds, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            #if UNITY_EDITOR
            Vector3 boundsCenter = bounds.center;
		    Vector3 boundsExtents = bounds.extents;
		  
			Vector3 v3FrontTopLeft     = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top left corner
			Vector3 v3FrontTopRight    = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top right corner
			Vector3 v3FrontBottomLeft  = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom left corner
			Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom right corner
			Vector3 v3BackTopLeft      = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top left corner
			Vector3 v3BackTopRight     = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top right corner
			Vector3 v3BackBottomLeft   = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom left corner
			Vector3 v3BackBottomRight  = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom right corner


			Handles.color = color;

			Handles.DrawLine (v3FrontTopLeft, v3FrontTopRight);
			Handles.DrawLine (v3FrontTopRight, v3FrontBottomRight);
			Handles.DrawLine (v3FrontBottomRight, v3FrontBottomLeft);
			Handles.DrawLine (v3FrontBottomLeft, v3FrontTopLeft);
		         
			Handles.DrawLine (v3BackTopLeft, v3BackTopRight);
			Handles.DrawLine (v3BackTopRight, v3BackBottomRight);
			Handles.DrawLine (v3BackBottomRight, v3BackBottomLeft);
			Handles.DrawLine (v3BackBottomLeft, v3BackTopLeft);
		         
			Handles.DrawLine (v3FrontTopLeft, v3BackTopLeft);
			Handles.DrawLine (v3FrontTopRight, v3BackTopRight);
			Handles.DrawLine (v3FrontBottomRight, v3BackBottomRight);
			Handles.DrawLine (v3FrontBottomLeft, v3BackBottomLeft);  
			#endif
		}

        /// <summary>
        /// Draws a solid rectangle at the specified position and size, and of the specified colors
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="borderColor"></param>
        /// <param name="solidColor"></param>
        public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            #if UNITY_EDITOR

            Vector3 halfSize = size / 2f;

            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);
            
            #endif
        }
        
        /// <summary>
        /// Draws a gizmo sphere of the specified size and color at a position
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="size">Size.</param>
        /// <param name="color">Color.</param>
        public static void DrawGizmoPoint(Vector3 position, float size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }
            Gizmos.color = color;
			Gizmos.DrawWireSphere(position,size);
		}

		/// <summary>
		/// Draws a cube at the specified position, and of the specified color and size
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Size.</param>
		public static void DrawCube (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),			
				position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}

        /// <summary>
        /// Draws a cube at the specified position, offset, and of the specified size
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="offset"></param>
        /// <param name="cubeSize"></param>
        /// <param name="wireOnly"></param>
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (wireOnly)
            {
                Gizmos.DrawWireCube(offset, cubeSize);
            }
            else
            {
                Gizmos.DrawCube(offset, cubeSize);
            }
        }

		/// <summary>
		/// Draws a gizmo rectangle
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="size">Size.</param>
		/// <param name="color">Color.</param>
		public static void DrawGizmoRectangle(Vector2 center, Vector2 size, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Gizmos.color = color;

			Vector3 v3TopLeft = new Vector3(center.x - size.x/2, center.y + size.y/2, 0);
			Vector3 v3TopRight = new Vector3(center.x + size.x/2, center.y + size.y/2, 0);;
			Vector3 v3BottomRight = new Vector3(center.x + size.x/2, center.y - size.y/2, 0);;
			Vector3 v3BottomLeft = new Vector3(center.x - size.x/2, center.y - size.y/2, 0);;

			Gizmos.DrawLine(v3TopLeft,v3TopRight);
			Gizmos.DrawLine(v3TopRight,v3BottomRight);
			Gizmos.DrawLine(v3BottomRight,v3BottomLeft);
			Gizmos.DrawLine(v3BottomLeft,v3TopLeft);
		}

		/// <summary>
		/// Draws a rectangle based on a Rect and color
		/// </summary>
		/// <param name="rectangle">Rectangle.</param>
		/// <param name="color">Color.</param>
		public static void DrawRectangle (Rect rectangle, Color color)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 pos = new Vector3( rectangle.x + rectangle.width/2, rectangle.y + rectangle.height/2, 0.0f );
			Vector3 scale = new Vector3 (rectangle.width, rectangle.height, 0.0f );

			MMDebug.DrawRectangle (pos, color, scale); 
		}	

		/// <summary>
		/// Draws a rectangle of the specified color and size at the specified position
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="size">Size.</param>
		public static void DrawRectangle  (Vector3 position, Color color, Vector3 size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3 halfSize = size / 2f; 

			Vector3[] points = new Vector3 []
			{
				position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
				position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
				position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),	
			};

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[3], points[0], color ); 
		}
        
        /// <summary>
        /// Draws a point of the specified color and size at the specified position
        /// </summary>
        /// <param name="pos">Position.</param>
        /// <param name="col">Col.</param>
        /// <param name="scale">Scale.</param>
        public static void DrawPoint (Vector3 position, Color color, float size)
        {
            if (!DebugDrawEnabled)
            {
                return;
            }

            Vector3[] points = new Vector3[] 
			{
				position + (Vector3.up * size), 
				position - (Vector3.up * size), 
				position + (Vector3.right * size), 
				position - (Vector3.right * size), 
				position + (Vector3.forward * size), 
				position - (Vector3.forward * size)
			}; 		

			Debug.DrawLine (points[0], points[1], color ); 
			Debug.DrawLine (points[2], points[3], color ); 
			Debug.DrawLine (points[4], points[5], color ); 
			Debug.DrawLine (points[0], points[2], color ); 
			Debug.DrawLine (points[0], points[3], color ); 
			Debug.DrawLine (points[0], points[4], color ); 
			Debug.DrawLine (points[0], points[5], color ); 
			Debug.DrawLine (points[1], points[2], color ); 
			Debug.DrawLine (points[1], points[3], color ); 
			Debug.DrawLine (points[1], points[4], color ); 
			Debug.DrawLine (points[1], points[5], color ); 
			Debug.DrawLine (points[4], points[2], color ); 
			Debug.DrawLine (points[4], points[3], color ); 
			Debug.DrawLine (points[5], points[2], color ); 
			Debug.DrawLine (points[5], points[3], color ); 
		}
	}
}