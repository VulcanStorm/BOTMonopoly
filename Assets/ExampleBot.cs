using UnityEngine;
using System.Collections;

public class ExampleBot : GamePlayer {

	public override void OnTurnHasStarted () {
		DoneTurnStart();
	}
	
	public override void OnLandedOnUnownedTile(BoardTile landedTile){
		DoneProcessingUnOwnedTile();
	}
	
	public override void OnMustPayRent(int amountToPay){
		PayRent(amountToPay);
	}
	
	public override void OnTradeOfferRecieved(TradeOffer offer){
		ResolveTradeOffer(false);
	}
	
	public override void OnEndOfTurn(){
		DoneTurnEnd();
	}
	
	
}
