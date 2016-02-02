using UnityEngine;
using System.Collections;
using UnityEditor;

public static class BoardFunctions {
	
	[MenuItem("Monopoly/RefreshBoardNames")]
	static void RefreshBoardNames () {
		BoardTile[] tiles = Object.FindObjectsOfType<BoardTile>();
		Debug.Log("found tiles amount "+tiles.Length);
		for(int i=0;i<tiles.Length;i++){
			tiles[i].SetTileDetails();
		}
	}
}
