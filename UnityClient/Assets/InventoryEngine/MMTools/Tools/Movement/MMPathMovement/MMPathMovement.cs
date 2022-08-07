using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	[AddComponentMenu("MMTools/Environment/Path Movement")]
	/// <summary>
	/// Add this component to an object and it'll be able to move along a path defined from its inspector.
	/// </summary>
	public class MMPathMovement : MonoBehaviour 
	{
		/// the possible movement types
		public enum PossibleAccelerationType
		{
			ConstantSpeed,
			EaseOut,
			AnimationCurve
		}

		/// the possible cycle options
		public enum CycleOptions
		{
			BackAndForth,
			Loop,
			OnlyOnce,
            StopAtBounds
		}

		/// the possible movement directions
		public enum MovementDirection
		{
			Ascending,
			Descending
		}

        public enum UpdateModes
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

		[Header("Path")]
		[MMInformation("Here you can select the '<b>Cycle Option</b>'. Back and Forth will have your object follow the path until its end, and go back to the original point. If you select Loop, the path will be closed and the object will move along it until told otherwise. If you select Only Once, the object will move along the path from the first to the last point, and remain there forever.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public CycleOptions CycleOption;

		[MMInformation("Add points to the <b>Path</b> (set the size of the path first), then position the points using either the inspector or by moving the handles directly in scene view. For each path element you can specify a delay (in seconds). The order of the points will be the order the object follows.\nFor looping paths, you can then decide if the object will go through the points in the Path in Ascending (1, 2, 3...) or Descending (Last, Last-1, Last-2...) order.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial movement direction : ascending > will go from the points 0 to 1, 2, etc ; descending > will go from the last point to last-1, last-2, etc
		public MovementDirection LoopInitialMovementDirection = MovementDirection.Ascending;
		/// the points that make up the path the object will follow
		public List<MMPathMovementElement> PathElements;

		[Header("Movement")]
		[MMInformation("Set the <b>speed</b> at which the path will be crawled, and if the movement should be constant or eased.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the movement speed
		public float MovementSpeed = 1;
		/// returns the current speed at which the object is traveling
		public Vector3 CurrentSpeed { get; protected set; }
		/// the movement type of the object
		public PossibleAccelerationType AccelerationType = PossibleAccelerationType.ConstantSpeed;
		/// the acceleration to apply to an object traveling between two points of the path. 
		public AnimationCurve Acceleration = new AnimationCurve(new Keyframe(0,1f),new Keyframe(1f,0f));
        /// the chosen update mode (update, fixed update, late update)
        public UpdateModes UpdateMode = UpdateModes.Update;

        [Header("Settings")]
		[MMInformation("The <b>MinDistanceToGoal</b> is used to check if we've (almost) reached a point in the Path. The 2 other settings here are for debug only, don't change them.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the minimum distance to a point at which we'll arbitrarily decide the point's been reached
		public float MinDistanceToGoal = .1f;
		/// the original position of the transform, hidden and shouldn't be accessed
		protected Vector3 _originalTransformPosition;
		/// internal flag, hidden and shouldn't be accessed
		protected bool _originalTransformPositionStatus=false;
        /// if this is true, the object can move along the path
        public virtual bool CanMove { get; set; }

		protected bool _active=false;
	    protected IEnumerator<Vector3> _currentPoint;
		protected int _direction = 1;
		protected Vector3 _initialPosition;
	    protected Vector3 _finalPosition;
		protected Vector3 _previousPoint = Vector3.zero;
	    protected float _waiting=0;
	    protected int _currentIndex;
		protected float _distanceToNextPoint;
		protected bool _endReached = false;

		/// <summary>
	    /// Initialization
	    /// </summary>
	    protected virtual void Awake ()
		{
			Initialization ();
		}

        protected virtual void Start()
        {
            _originalTransformPosition = transform.position;
        }

		/// <summary>
		/// Flag inits, initial movement determination, and object positioning
		/// </summary>
		protected virtual void Initialization()
		{
			// on Start, we set our active flag to true
			_active=true;
			_endReached = false;
            CanMove = true;

			// if the path is null we exit
			if(PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			// we set our initial direction based on the settings
			if (LoopInitialMovementDirection == MovementDirection.Ascending)
			{
				_direction=1;
			}
			else
			{
				_direction=-1;
			}

			// we initialize our path enumerator
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();

			// initial positioning
			if (!_originalTransformPositionStatus)
			{
				_originalTransformPositionStatus = true;
				_originalTransformPosition = transform.position;
			}
			transform.position = _originalTransformPosition + _currentPoint.Current;
		}

        protected virtual void FixedUpdate()
        {
            if (UpdateMode == UpdateModes.FixedUpdate)
            {
                ExecuteUpdate();
            }
        }

        protected virtual void LateUpdate()
        {
            if (UpdateMode == UpdateModes.LateUpdate)
            {
                ExecuteUpdate();
            }
        }

        protected virtual void Update()
        {
            if (UpdateMode == UpdateModes.Update)
            {
                ExecuteUpdate();
            }
        }

		/// <summary>
		/// On update we keep moving along the path
		/// </summary>
		protected virtual void ExecuteUpdate () 
		{
			// if the path is null we exit, if we only go once and have reached the end we exit, if we can't move we exit
			if(PathElements == null 
				|| PathElements.Count < 1
				|| _endReached
				|| !CanMove
				)
			{
				return;
			}

			Move ();
		}

		/// <summary>
		/// Moves the object and determines when a point has been reached
		/// </summary>
		protected virtual void Move()
		{
			// we wait until we can proceed
			_waiting -= Time.deltaTime;
			if (_waiting > 0)
			{
				CurrentSpeed = Vector3.zero;
				return;
			}

			// we store our initial position to compute the current speed at the end of the udpate	
			_initialPosition=transform.position;

			// we move our object
			MoveAlongThePath();

			// we decide if we've reached our next destination or not, if yes, we move our destination to the next point 
			_distanceToNextPoint = (transform.position - (_originalTransformPosition + _currentPoint.Current)).magnitude;
			if(_distanceToNextPoint < MinDistanceToGoal)
			{
				//we check if we need to wait
				if (PathElements.Count > _currentIndex)
				{
					_waiting = PathElements[_currentIndex].Delay;				 
				}

				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}

			// we determine the current speed		
			_finalPosition = transform.position;
			CurrentSpeed = (_finalPosition-_initialPosition) / Time.deltaTime;

			if (_endReached) 
			{
				CurrentSpeed = Vector3.zero;
			}
		}

		/// <summary>
		/// Moves the object along the path according to the specified movement type.
		/// </summary>
		public virtual void MoveAlongThePath()
		{
			switch (AccelerationType)
			{
				case PossibleAccelerationType.ConstantSpeed:
					transform.position = Vector3.MoveTowards (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed);
					break;
				
				case PossibleAccelerationType.EaseOut:
					transform.position = Vector3.Lerp (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed);
					break;

				case PossibleAccelerationType.AnimationCurve:
					float distanceBetweenPoints = Vector3.Distance (_previousPoint, _currentPoint.Current);

					if (distanceBetweenPoints <= 0)
					{
						return;
					}

					float remappedDistance = 1 - MMMaths.Remap (_distanceToNextPoint, 0f, distanceBetweenPoints, 0f, 1f);
					float speedFactor = Acceleration.Evaluate (remappedDistance);

					transform.position = Vector3.MoveTowards (transform.position, _originalTransformPosition + _currentPoint.Current, Time.deltaTime * MovementSpeed * speedFactor);
					break;
			}
		}

		/// <summary>
		/// Returns the current target point in the path
		/// </summary>
		/// <returns>The path enumerator.</returns>
		public virtual IEnumerator<Vector3> GetPathEnumerator()
		{

			// if the path is null we exit
			if(PathElements == null || PathElements.Count < 1)
			{
				yield break;
			}

			int index = 0;
			_currentIndex = index;
			while (true)
			{
				_currentIndex = index;
				yield return PathElements[index].PathElementPosition;
				
				if(PathElements.Count <= 1)
				{
					continue;
				}

				// if the path is looping
                switch(CycleOption)
                {
                    case CycleOptions.Loop:
                        index = index + _direction;
                        if (index < 0)
                        {
                            index = PathElements.Count - 1;
                        }
                        else if (index > PathElements.Count - 1)
                        {
                            index = 0;
                        }
                        break;

                    case CycleOptions.BackAndForth:
                        if (index <= 0)
                        {
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            _direction = -1;
                        }
                        index = index + _direction;
                        break;

                    case CycleOptions.OnlyOnce:
                        if (index <= 0)
                        {
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            _direction = 0;
                            CurrentSpeed = Vector3.zero;
                            _endReached = true;
                        }
                        index = index + _direction;
                        break;

                    case CycleOptions.StopAtBounds:
                        if (index <= 0)
                        {
                            if (_direction == -1)
                            {
                                CurrentSpeed = Vector3.zero;
                                _endReached = true;
                            }
                            _direction = 1;
                        }
                        else if (index >= PathElements.Count - 1)
                        {
                            if (_direction == 1)
                            {
                                CurrentSpeed = Vector3.zero;
                                _endReached = true;
                            }
                            _direction = -1;
                        }
                        index = index + _direction;
                        break;
                }
            }
		}

		/// <summary>
		/// Call this method to force a change in direction at any time
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = - _direction;
			_currentPoint.MoveNext();
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (PathElements==null)
			{
				return;
			}

			if (PathElements.Count==0)
			{
				return;
			}
							
			// if we haven't stored the object's original position yet, we do it
			if (_originalTransformPositionStatus==false)
			{
		    	_originalTransformPosition=transform.position;
				_originalTransformPositionStatus=true;
			}
			// if we're not in runtime mode and the transform has changed, we update our position
			if (transform.hasChanged && _active==false)
			{
				_originalTransformPosition=transform.position;
			}
			// for each point in the path
			for (int i=0;i<PathElements.Count;i++)
			{
				// we draw a green point 
				MMDebug.DrawGizmoPoint(_originalTransformPosition+PathElements[i].PathElementPosition,0.2f,Color.green);

				// we draw a line towards the next point in the path
				if ((i+1)<PathElements.Count)
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[i].PathElementPosition,_originalTransformPosition+PathElements[i+1].PathElementPosition);
				}
				// we draw a line from the first to the last point if we're looping
				if ( (i == PathElements.Count-1) && (CycleOption == CycleOptions.Loop) )
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[0].PathElementPosition,_originalTransformPosition+PathElements[i].PathElementPosition);
				}
			}

			// if the game is playing, we add a blue point to the destination, and a red point to the last visited point
			if (Application.isPlaying)
			{
				MMDebug.DrawGizmoPoint(_originalTransformPosition + _currentPoint.Current,0.2f,Color.blue);
				MMDebug.DrawGizmoPoint(_originalTransformPosition + _previousPoint,0.2f,Color.red);
			}
			#endif


		}

		/// <summary>
		/// Updates the original transform position.
		/// </summary>
		/// <param name="newOriginalTransformPosition">New original transform position.</param>
		public virtual void UpdateOriginalTransformPosition(Vector3 newOriginalTransformPosition)
		{
			_originalTransformPosition = newOriginalTransformPosition;
		}

		/// <summary>
		/// Gets the original transform position.
		/// </summary>
		/// <returns>The original transform position.</returns>
		public virtual Vector3 GetOriginalTransformPosition()
		{
			return _originalTransformPosition;
		}

		/// <summary>
		/// Sets the original transform position status.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void SetOriginalTransformPositionStatus(bool status)
		{
			_originalTransformPositionStatus = status;
		}

		/// <summary>
		/// Gets the original transform position status.
		/// </summary>
		/// <returns><c>true</c>, if original transform position status was gotten, <c>false</c> otherwise.</returns>
		public virtual bool GetOriginalTransformPositionStatus()
		{
			return _originalTransformPositionStatus ;
		}
	}
}