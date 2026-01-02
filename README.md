🎧 The Blind Labyrinth: Audio-Augmented Navigation
A navigation experiment in 3D environments based entirely on procedural audio synthesis and game physics.

📖 Project Description
The Blind Labyrinth is a Unity project that explores the role of audio as an active Navigational Aid.

Unlike traditional video games, this project adopts a "Zero-Asset" philosophy: no pre-recorded audio files (.wav, .mp3) are used. The entire soundscape—from player footsteps and ambience to guidance signals—is generated in real-time by the CPU using DSP (Digital Signal Processing) algorithms.

The goal is to demonstrate how sound synthesis, dynamically integrated with physics Raycasting, can guide the player towards non-visible (occluded) objectives and provide precise feedback on the spatial geometry.

✨ Key Features
🔊 1. Pure Procedural Audio (Sample-Free)
All sounds are mathematically synthesized via OnAudioFilterRead:

Sonar Beacon: Additive synthesis using sine waves. The pulsation frequency (LFO) is directly controlled by the Euclidean distance between the Player and the Goal.

Footsteps: Subtractive synthesis. White Noise shaped by short ADSR envelopes and Low-Pass Filters, featuring stochastic pitch variation to eliminate repetition.

Ambience: Generation of Brownian Noise (integrated White Noise) to create a low-frequency "rumble" that simulates an underground environment.

Victory Event: Procedural generation of an ascending arpeggio with specific harmonics (Perfect Fifth/Octave) and dynamic mixing (immediate muting of other sources).

🧱 2. Physics & Spatial Audio
Sonar Navigation: The audio tempo acts as a guide (Slow = Far, Fast = Near).

Dynamic Occlusion: A Raycasting system traces the line of sight between the Source and the Listener. If a wall interrupts the ray, a Low Pass Filter is applied in real-time, simulating sound diffraction and obstacle absorption.

3D Spatial Blend: Full binaural localization allowing the player to identify the source direction.

🧩 3. Procedural Level Generation
Implementation of the Recursive Backtracker algorithm to generate perfect mazes (no loops) that are unique on every run.

Automatic "safe placement" of the Player and Goal at opposite ends of the map to ensure playability.

🛠️ Scripts & Architecture
The core components of the project are:

ProceduralBeacon.cs: Manages the sine oscillator and proximity logic (Distance → LFO Speed).

ProceduralFootsteps.cs: Detects CharacterController movement and triggers footstep synthesis.

OcclusionHandler.cs: Handles Raycasts and modulates the audio filter's Cutoff Frequency.

MazeGenerator.cs: Generates the grid, builds meshes (Walls/Ceiling), and handles safe player spawning (physics fix).

WinHandler.cs: Manages the victory trigger, pauses gameplay, and synthesizes the final audio reward.

🎮 Controls
W, A, S, D: Move.

Mouse: Look around.

Objective: Follow the pulsing sound (Beacon) to find the glowing exit.

🚀 Installation & Usage
Clone or download this repository.

Open the project with Unity Hub (Recommended version: 2021.3 or higher).

Open the MainScene located in the Scenes folder.

Press Play.

🐛 Technical Notes / Troubleshooting
CPU Cost: Since audio is generated via software, CPU usage is slightly higher compared to playing static samples. However, RAM and Disk I/O impact is virtually zero.

Respawn Physics: The generator includes a Safe Start system that momentarily disables the CharacterController during respawn to prevent physics pass-through bugs (falling through the floor).

👨‍💻 Author
Andrea Sicignano Course: Procedural and Spatial Sound Academic Year: 2025/2026 University: University of Milan
