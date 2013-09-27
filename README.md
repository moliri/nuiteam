nuiteam
=======
Project Description
The game, currently dubbed as DownBall, is a two-player interactive racing game using a PC or Kinect platform.


Project Proposal-NUITeam
Jordan Geltner, Leesha Maliakal, Zach Straight, Jeannie Tran

Introduction:
The game, currently dubbed as DownBall, is a two-player interactive racing game using a PC or Kinect platform where the players alternate between two roles: navigating down a hill and placing obstacles down for their opponent. 
Background: 
To design and implement the game, we will primarily be using the Unity3D game engine. Pre-built assets such as textures will be taken from the public assets available through the Unity3D Store. As we work on developing the game, we will integrate Kinect functionality with the Kinect SDK which is provided by Microsoft. 

Description:
You start at the top of a hill and the timer goes off, Gravity pulls you forward as you accelerate down the hill. As obstacles appear before you, your goal as Player 1 is to maneuver the ball around or through the obstacles to reach the bottom of the hill as fast as possible. Maneuvering through the obstacles requires a large amount of skill and dexterity to avoid even the smallest reduction of speed. You race down the hill, with the ability to move left, right, jump, and brake. You approach both helpful and harmful obstacles along the way. There are slick patches that increase your speed, and friction patches that slow you down. Other obstacles will directly affect your gameplay status. For example, you may hit an object that freezes your motion with a three second delay. With the implementation of Player 2 and the Kinect, you would be able to play the role of defense. As Player 1 moves down the hill, you can drop a limited amount of obstacles before the player to slow down their run time. The end-goal for this game is to have either another player using the Kinect to deploy obstacles in a restricted manner in front of the ball, or to integrate the first player’s controls with the Kinect. Time permitted, both goals may be realized. 


Key Features: 
As Player 1, I want to navigate down the hill by moving left, right, jumping, and braking.
As Player 1, I want to decrease my time by hitting helpful obstacles.
As Player 1, I want to decrease my time by avoiding harmful obstacles.
As Player 2, I want to increase the time of my opponent by placing obstacles in their path.
As Player 2, I want to carefully use up my obstacle resources by selectively choosing optimal plays.
As Player 2, I want to surprise my opponent by strategically placing obstacles in unexpected areas.
As a player, I want to win by having the fastest track time in a given round. 

Genre:
Two player racing simulation strategy game, a modern flavor with interactive aspects and intuitive gameplay.

Platform:
The target platform is the Kinect by Microsoft. The game can also be played as a PC game, or a hybrid of the two platforms, depending on stages of implementation. 

Technical Analysis:
Because of this 1v1 dynamic, we expect there to be a great deal of thought put into balance details of the game. Basically, how can we restrict the controls of the ball-player and the resources of the obstacle-player so that it is fun to play both roles? Furthermore, we want the meta-game to constantly evolve based upon each run-through of the game. The interest here lies in making a simple racing game into an iterative 1v1 game.
There are a few main implementation details for us: control of the ball, creation of the tracks (we want more than one so there is variation in slope/length), useable Kinect controls for the obstacle-placer, useable Kinect controls for the first player, different objects with resource restriction to create unique obstacle sets. Once we have all of these things created and tested, we can focus on making tracks more fun and implementing better graphics or other features. Some of the other features we’ve considered so far include varying the quality of the track so that the movement of the ball changes, varying the quality of the ball, dynamic obstacles in a single-player scenario that adapt to the changes of the player, increasing gravity to increase the speed of the ball as to increase difficulty, and etc. We hope to implement one major aspect of the game at a time in order to fit with the limited time constraint of the quarter. Our development will require the following resources:
Kinect (Provided by Ian Horswill)
Unity3D (Free download)
Playtesters (victim students)
4 group members:
Unity can be difficult to integrate with the Kinect, so we think that there will be a great deal of work simply unifying the development environments.
We are all in Ken Forbus’ 370, so we hope to make one strong project with aspects relating to both courses.
Our vision for implementation of this game is also very broad, so in order to implement enough of this game to make it fun, we need more manpower. 

Regarding the learning curve, we plan to learn how to use the Kinect SDK in combination with Unity. Since most of us have not used Unity or the Kinect SDK before, it will be a great opportunity to learn how to use Unity as well as the Kinect SDK. Furthermore, making this game fun will require a great deal of balancing with the multiplayer components, which we will also think will take time to figure out properly. We expect to spend a lot of time after implementing the game simply changing values of certain restrictions and attributes to change the game dynamics. 

Assets:
Assets will primarily come from the Unity3D store. Assets include:
Player (Ball)
Player (Obstacle)
Dynamic maps for varied game experience
Timed obstacles
Stationary obstacles
Obstacles that directly affect player status
Checkpoints
Start/FInish points
3rd person main camera
Cursor for 2nd player to indicate where to place objects


Codex Repository: 
https://nuiteam.codeplex.com/team/view
