2D Slot Machine

Game Overview
Here is my 2D Slot Machine project built in Unity 6. The game starts you off with 50G. You can change your bet amount and hit spin to roll the three reels. I set up the code to pick a random stopping point for each reel and then check the middle row to see if the symbols match. If you win, it multiplies your bet based on which symbol you landed on and triggers a jackpot screen.

How to Play the WebGL Build
I included a ready-to-play WebGL build right in the repository so you don't have to open the Unity Editor to test it.

Go to the /Build/WebGL/ folder.

A playable WebGL build is included in the /Build/WebGL/ folder. To run the game, host the folder contents using a local web server (such as VS Code Live Server or Python's http.server) or test it via a web hosting platform.

Extra Features I Added
Money Rain Particles: I made a custom particle effect that triggers when you hit a jackpot. I had to tweak the sorting layers to make sure it renders in front of the UI panels.

Audio: Added sound effects for the UI clicks, the spinning reels, cashing out, and a winning chime.

Checkout Button: I added a checkout option that shows your final score in a popup and then resets the game board and your balance back to 50G for a fresh start.

Failsafes: I wrote a check so you can't bet money you don't have (it pops up a warning instead), and I locked the UI buttons during spins so mashing the mouse doesn't break the game state.

Screen Scaling: The UI is set up with the Canvas Scaler on "Expand" mode. I built it using Unity's Default WebGL template set to 720p, so it scales nicely on different monitors without the edges getting chopped off.

My Approach
My main goal with the code was to keep the logic separated and clean. I used one SlotMachineManager script to handle the overarching game rules, UI, and win math, and let the individual ReelController scripts strictly handle the spinning mechanics.

I ended up relying heavily on Unity Coroutines to make the game feel right. They allowed me to sequence the reels so they stop one after the other, delay the popups so you actually have time to read them, and freeze the UI during a spin. For the reels themselves, I built a looping system that physically moves a strip of symbols and teleports it back to the top to create a blur effect, before finally snapping perfectly to the random index I calculated.