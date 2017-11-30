# MonoInjection
Replaces Unity's magic Update method with a high performance, high control subscription model that allows any object (not just MonoBehaviours) to tie itself into the game loop, and allows multi-threaded scripts to "inject" code into the main Unity thread.

## Why use this script?
### This script fixes a variety of problems with the default Unity system:
- Performance - Calls to the Update method from the Unity engine require crossing the C++/C# barrier, which has costs that are not associated with C#/C# calls. If you have a large amount of in-scene objects with Update calls, you will get a significant boost in performance from using this script.

- Control - You have little control over whether Update is called on an object. With the MonoInjector, objects can selectively remove themselves from the game loop without side effects on other systems (like collisions). The entire MonoInjector can be disabled to instantly stop ALL subscribers from updating. With multiple MonoInjectors, you can group subscriptions to selectively enable and disable the behaviour of a large group of objects.

- Flexibility - With Unity's default method, an object must be a MonoBehaviour to receive Update calls. This method allows any object to tie itself into the game loop.

Using the script is simple. Attach the MonoInjector component to an empty object in your scene. Any object that wants to subscribe to the Update loop needs to implement the IUpdate interface, and be registered with the MonoInjector using the Subscribe method. The Unsubscribe method will remove an object from the loop. For calls from multi-threaded scripts, the Inject method can schedule a delegate to be called once on the next frame (this creates a certain amount of unavoidable garbage). If your scripts need to use FixedUpdate or LateUpdate, set the Injection Mode of the MonoInjector to the relevant mode.
