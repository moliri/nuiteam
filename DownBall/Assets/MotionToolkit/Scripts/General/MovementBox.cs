using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.General
{
	/// <summary>
	/// A data structure of box in space meant to be placed relatively to another position.
	/// The box dimensions and offset are in arbitrary units meant to be used from the outside.
	/// </summary>
	[Serializable]
	public class MovementBox
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MovementBox() { }

		/// <summary>
		/// Constructor from a framework movement box
		/// </summary>
		/// <param name="movementBox">Framework movement box</param>
		public MovementBox(OmekFramework.Common.BasicTypes.MovementBox movementBox)
		{
			CenterOffset = UnityConverter.ToUnity(movementBox.CenterOffset);
			Dimensions = UnityConverter.ToUnity(movementBox.Dimensions);
		}

		/// <summary>
		/// The center position offset
		/// </summary>
		public Vector3 CenterOffset;

		/// <summary>
		/// The dimensions of the box.
		/// </summary>
		public Vector3 Dimensions;

		/// <summary>
		/// Returns an instance of a framework Movement box from this box's data
		/// </summary>
		/// <returns></returns>
		public OmekFramework.Common.BasicTypes.MovementBox ToFrameworkMovementBox()
		{
			return new OmekFramework.Common.BasicTypes.MovementBox(UnityConverter.ToFramework(CenterOffset), UnityConverter.ToFramework(Dimensions));
		}
	}
}