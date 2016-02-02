using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BoardTile : MonoBehaviour {

	// house data
	public string actualTileName = "UNNAMED";
	public TileGroup tileGroup = TileGroup.other;
	public TileType tileType = TileType.property;
	public uint buyPrice = 0;
	
	public readonly uint mortgagePrice = 0;
	
	public readonly uint mortgageCost = 0;
	public int currentRentCost = 0;
	public int utilityMultiplier;
	public int houseBuyCost = 0;
	public int[] housePrice = new int[6];
	public HouseImprovement improvement = HouseImprovement.None;
	// if owner = -1, house is unowned
	public int owner = -1;
	public GamePlayer owningPlayer;
	public bool isOwned = false;
	public bool fullSet = false;
	public bool canBuildHouses = false;


	public BoardTile nextTile;
	public BoardTile previousTile;
	public Text uiText;
	public Text priceText;
	public Text houseCostUiText;
	public Transform gamePiecePos;
	public Transform housePos;
	public Image uiImage;
	public Sprite unOwnedImage;
	public GameObject houseObj;
	
	public BoardTile(){
		mortgagePrice = buyPrice / 2;
		mortgageCost = (uint)(mortgagePrice * 1.1f);
	}
	
	// Use this for initialization
	void Start () {
		if(gamePiecePos == null){
			gamePiecePos = this.transform;
		}
		SetTileDetails();
	}
	
	public void SetTileDetails () {
		SetRentCost();
		gameObject.name = actualTileName;
		uiText.text = actualTileName;
		houseCostUiText.text = "Houses: £"+houseBuyCost;
		
	}
	
	public void SetRentCost(){
		if(tileType == TileType.property && isOwned == true){
			
			
			if(tileGroup == TileGroup.station){
				// work out how many stations owned
				currentRentCost = 25 * ((int)Mathf.Pow(2,owningPlayer.numOfStationsOwned-1));
			}
			else if(tileGroup == TileGroup.utility){
				if(owningPlayer.numOfUtilitiesOwned == 2){
					utilityMultiplier = 10;
				}
				else{
					utilityMultiplier = 4;
				}
			}
			else{
				currentRentCost = housePrice [(int)improvement];
				if (improvement == HouseImprovement.None && fullSet == true) {
					currentRentCost *=2;
				}
			}
			priceText.text = "£"+currentRentCost;
			
		}
		else if(tileType == TileType.property){
			priceText.text = "£"+buyPrice;
		}
		else{
			priceText.text = "£"+currentRentCost;
		}

	}
	
	public void OnTilePassed(GamePlayer player){
		if(tileType == TileType.start){
			player.money += 200;
		}
	}
	
	public void OnTileLanded(GamePlayer player){
		
		switch (tileType){
			case TileType.property:
				if(isOwned == false){
					player.StartCoroutine(player.ProcessUnownedPropertyTile(this));
				}
				else{
					if(tileGroup == TileGroup.utility){
						// calculate rent cost
						currentRentCost = player.moveDiceScore.number * utilityMultiplier;
						SetTileDetails();
						player.StartCoroutine(player.ProcessOwnedPropertyTile(this));
					}
					else{
						player.StartCoroutine(player.ProcessOwnedPropertyTile(this));
					}
				}
			break;
			
			case TileType.chance:
				player.StartCoroutine(player.ProcessChanceCard());
			break;
			
			case TileType.chest:
				player.StartCoroutine(player.ProcessCommunityChestCard());
			break;
		
			case TileType.goJail:
				player.GoToJail();
			break;
			
			case TileType.inJail:
				player.DoneMoveAction();
			break;
			
			case TileType.start:
				player.DoneMoveAction();
			break;
			
			case TileType.tax:
				player.StartCoroutine(player.PayTax(currentRentCost));
			break;
			
			case TileType.free:
				// do nothing
				player.DoneMoveAction();
			break;
		}
		
	}
	
	public void SetOwner (GamePlayer player){
		owner = player.playerIndex;
		owningPlayer = player;
		isOwned = true;
		uiImage.sprite = player.myPlayerSprite;
		SetTileDetails();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void AddHouseUpgrade () {
		if(fullSet == false){
			Debug.Log (actualTileName+ " is not part of a full set, Cannot add house");
			return;
		}
	
		// increase our improvement by 1
		if(improvement != HouseImprovement.Hotel){
			improvement = (HouseImprovement)((int)improvement +1);
		}
		else{
			Debug.Log (actualTileName + " already has a hotel, cannot improve it any more!");
			return;
		}
		// destroy the house object if it exists
		if(houseObj != null){
			Destroy(houseObj);
		}
		
		switch(improvement){
			case HouseImprovement.House1:
				houseObj = (GameObject)Instantiate(HouseManager.singleton.house1Prefab, housePos.position, housePos.rotation);
				houseObj.transform.parent = housePos;
			break;
			
			case HouseImprovement.House2:
				houseObj = (GameObject)Instantiate(HouseManager.singleton.house2Prefab, housePos.position, housePos.rotation);
				houseObj.transform.parent = housePos;
			break;
			
			case HouseImprovement.House3:
				houseObj = (GameObject)Instantiate(HouseManager.singleton.house3Prefab, housePos.position, housePos.rotation);
				houseObj.transform.parent = housePos;
			break;
			
			case HouseImprovement.House4:
				houseObj = (GameObject)Instantiate(HouseManager.singleton.house4Prefab, housePos.position, housePos.rotation);
				houseObj.transform.parent = housePos;
			break;
			
			case HouseImprovement.Hotel:
				houseObj = (GameObject)Instantiate(HouseManager.singleton.hotelPrefab, housePos.position, housePos.rotation);
				houseObj.transform.parent = housePos;
			break;
		}
		// update the rent cost
		SetRentCost();
		Debug.Log ("Just built a "+improvement+" on "+actualTileName);
		
	}
	
	public void RemoveHouseUpgrade (int numOfUpgrades) {
		for(int i=0;i<numOfUpgrades;i++){
			if(improvement != HouseImprovement.None){
				improvement = (HouseImprovement)((int)improvement -1);
			}
			else{
				// no more upgrades to remove
				Debug.Log (actualTileName + " already has no upgrades");
				// stop the loop
				i = numOfUpgrades;
			}
		}
		// destroy the house object if it exists
		if(houseObj != null){
			Destroy(houseObj);
		}
		// update the rent cost
		SetRentCost();
		// check for no improvement
		if(improvement == HouseImprovement.None){
			return;
		}	
		
		switch(improvement){
		case HouseImprovement.House1:
			houseObj = (GameObject)Instantiate(HouseManager.singleton.house1Prefab, housePos.position, housePos.rotation);
			houseObj.transform.parent = housePos;
			break;
			
		case HouseImprovement.House2:
			houseObj = (GameObject)Instantiate(HouseManager.singleton.house2Prefab, housePos.position, housePos.rotation);
			houseObj.transform.parent = housePos;
			break;
			
		case HouseImprovement.House3:
			houseObj = (GameObject)Instantiate(HouseManager.singleton.house3Prefab, housePos.position, housePos.rotation);
			houseObj.transform.parent = housePos;
			break;
			
		case HouseImprovement.House4:
			houseObj = (GameObject)Instantiate(HouseManager.singleton.house4Prefab, housePos.position, housePos.rotation);
			houseObj.transform.parent = housePos;
			break;
			
		}
		
		Debug.Log ("Just removed a house from "+actualTileName+": current house status is "+improvement);
	}
	
	public void ResetProperty() {
		// remove all house upgrades
		RemoveHouseUpgrade(999);
		// no longer part of a set
		fullSet = false;
		// not owned by anyone
		owner = -1;
		owningPlayer = null;
		isOwned = false;
		uiImage.sprite = unOwnedImage;
		SetTileDetails();
		
	}
}
