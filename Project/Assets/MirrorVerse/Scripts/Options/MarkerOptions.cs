using UnityEngine;

namespace MirrorVerse.Options
{
	[CreateAssetMenu(fileName = "MarkerOptions", menuName = "MirrorVerse/Marker Options")]
	public class MarkerOptions : ScriptableObject
	{
		// Physical Side length of QR Code marker. In meters.
		public float sideLength = 0.05f;
		
		// Side length of QR Code marker. In pixels in device screen resolution.
		// This will override the side length value in meters if set to any positive number.
		public float sideLengthPixelOverride = 0;
	}
}
