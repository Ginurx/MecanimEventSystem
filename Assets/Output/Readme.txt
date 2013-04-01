REQUIREMENT:
	Unity 4.1.0f4

HOW TO:

1) Create an empty gameobject in scene, save it as a prefab and delete the gameobject in scene.

2) Drag MecanimEventData script onto the prefab. 
	At this point, you can see an button named Open Event Editor in the inspector.
	
3) Click the button.

3) You'll see a new window be opened. Follow the tips in the window.

4) Drag an AnimatorController which is used by your charator's Animator component
	 into the blank field.
	 
5) Now, you can see layers defined in the AnimatorController 
	following the states defined in the currently selected layer.
	
6) Select the state you want to add event to.

7) Drag the thumbnail on the slider to the time you want to add event at.
	The preview in inspector will be helpful in this step.
	
8) Press the Add Event Button. And a new window will be appeared.

9) Setup message name, type and paramater in the popup window.
	Press save button when you finished these operations.
	If you want to edit an event just click the event on timeline.
	
10) Close event editor windows.

11) Drag a MecanimEventEmitter component onto your character.

12) Drag the charater's Animator comopnent into MecanimEventEmitter's blank field in inspector.

13) Add MecanimEventSetupHelper component to an arbitrary object in scene.
	Or you can create an empty object and attach the component to the empty object.
	
14) Fill the Data Source field of MecanimEventSetupHelper with the prefab previously edited.

15) Done. In game, all MonoBehaviours on charactor with MecanimEventEmitter component
	 will receive the message emitted from event system.
	 
SUPPORT:

	Contact me at Ginurx@Gmail.com