
# Oxide-Rust-CodeLock2FA

A [Rust](http://playrust.com) [Oxide](http://oxidemod.org) plugin to enable 2FA (two factor auth) for your codelock doors in game.

## Installation

+ Copy [CodeLock2FA.cs](./CodeLock2FA.cs) to your Oxide server's `oxide/plugins` folder
+ Visit [rust2fa.azurewebsites.net](https://rust2fa.azurewebsites.net) and sign up
+ Start Rust
+ Make a door with a CodeLock
+ Set code to 0000

## Usage

Once installed, any `0000` coded door that is locked and hasn't been 2FA unlocked
for the requestor in the last 10 minutes will require 2FA.

## License

MIT