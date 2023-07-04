using UnityEngine;

namespace MirrorVerse.Options
{
	[CreateAssetMenu(fileName = "MarkerOptions", menuName = "MirrorVerse/Marker Options")]
	public class MarkerOptions : ScriptableObject
	{
		// Physical Side length of QR Code marker. In meters.
		public float sideLength = 0.05f;
	}
}
