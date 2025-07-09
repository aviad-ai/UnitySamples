# The Tell-Tale Heart

Inspired by Edgar Allan Poe's The Tell-Tale Heart, this short experience is a demonstration of on-device language models in games.

You play as The Author, creating characters and shaping their story. Choose game scenarios with potential action choices. A custom small language model (SLM) selects the most in-character response and explains why. Make different choices from your characters too often… and discover the consequences.

### Support

The game should build for Windows, macOS, and WebGL. There are some graphical inconsistencies across the builds still.

### Setup

The game is dependent on the paid asset from the unity store [EndlessBook](https://assetstore.unity.com/packages/3d/props/endlessbook-134213). To build the game you'll have to own this asset too. After adding the EndlessBook asset to the game you should be able to run `git apply EndlessBook.patch` to apply our minor changes to the asset that help support WebGL builds.
