//    ExampleScript - LowHealth


using UnityEngine;
using Leguar.LowHealth;

namespace Leguar.LowHealth.Example {

	public class ExampleScript_LowHealth : MonoBehaviour {

		// This script is attached to camera in the scene
		public LowHealthController shaderControllerScript;

		// Keep actual value of player health in own classes
		// In this example player health is integer between 0 and 100
		private int currentPlayerHealth;

		void Start() {

			// Set full health to player
			currentPlayerHealth = 100;

			// By default 'LowHealthController' starts from full health, but no harm to call this is start
			shaderControllerScript.SetPlayerHealthInstantly(currentPlayerHealth);

		}

		public void UIButtonClicked(int newPlayerHealth) {

			// Set effects
			bool healthGoingDown = (newPlayerHealth<currentPlayerHealth);
			setNewPlayerHealth(newPlayerHealth/100f, healthGoingDown);

			// Remember the new health
			currentPlayerHealth = newPlayerHealth;

		}

		private void setNewPlayerHealth(float newPlayerHealthPercent, bool healthGoingDown) {

			if (healthGoingDown) {

				// Player took damage, make transition faster (1 second)
				shaderControllerScript.SetPlayerHealthSmoothly(newPlayerHealthPercent, 1f);

			} else {

				// Player gained health (medikit?), make transition slightly slower (2 seconds)
				shaderControllerScript.SetPlayerHealthSmoothly(newPlayerHealthPercent, 2f);

			}

		}

	}

}
