Welcome to the Kinect Exercise of the HCI 2015 course at ETH Zurich
===================================================================

This readme gives you a short overview of the exercise template and explains you how to run the project.


Project Structure
-----------------
The GlobIS.Exercise.sln file in the root folder is the Visual Studio Solution file that contains all necessary projects of the Kinect Exercise.

Open "GlobIS.Exercise.sln" to load the solution in Visual Studio.

The solution contains exactly one project: GlobIS.Kinect.Exercise

The project is a Windows WPF application that uses its main window for debug purposes. This debug window is a transparent overlay window that shows a little skeleton (if a user is recognized) in the lower right corner of the screen. Its purpose is to give the user some feedback when operating PowerPoint in fullscreen mode.
The name of the class that you need to edit is "MyKinectApplication". It has two files attched to it. The "MyKinectApplication.xaml" describes the UI of the overlay window in a declarative XML dialect called XAML. You DO NOT need to modify this file. The second file is called "MyKinectApplication.xaml.cs" and contains all our application logic. This is where you will implement your gesture recognition.


Running the application
-----------------------
The application can either be started (in Debug Mode) by pressing F5 or by clicking on the green arrow in the toolbar.


Task: Controlling PowerPoint
----------------------------
Your task is to design suitable (or at least creative) gestures to control a PowerPoint presentation. To implement the recognition of those gestures, you are required to complete the RecognizeGestures(...) method in the MyKinectApplication class. You may use any technique you want, but we recommend to follow a pseudo-gesture approach (as explained in the exercise session) for the sake of simplicity.
All the supported PowerPoint operations are available as methods in the "MyKinectApplication.xaml.cs" file and are grouped in a region called "PowerPoint Operations" at the end of the file. What they do is very simple: They send a keystroke to the currently active window and also "highlight" the overlay skeleton to indicate that an action has been triggered by the user. This means that you will need to have your PowerPoint presentation active in order for the operations to take effect. If another window is currently active, the keystrokes will be sent to that window instead (possibly resulting in some operations there). Please note that the overlay window (MyKinectApplication.xaml) should be the topmost window even if it is not active, otherwise you will not see the overlay skeleton.

You are free to add other PowerPoint operations to your implementation. A list of all PowerPoint shortcuts can be found on the web [1]. Only the "Run a presentation" section is relevant for your task. Please be aware that some shortcuts need certain modifiers (e.g. Shift) or special keys (e.g. "DELETE"). An explanation of the SendKey.SendWait(...) method can be found here [2].

In order to leverage the new hand states (as explained in the exercise session) of the Kinect for Windows SDK, you can read the "HandLeftState" and "HandRightState" properties of the "trackedBody" parameter in the RecognizeGestures(...) method. These properties can be compared against values from the HandState enumeration to determine the current hand state (open, closed, lasso, unknown, not tracked). You can use those properties to better segment your PowerPoint gestures. But beware, recognition of hand state is nowhere near perfect and you should account for accidential state changes!

If you feel really adventurous, you may also change the existing code to further improve your recognition, allow entirely different gestures or even extend the functionality! For example, you might want to exploit multiple bodies. In that case, you also need to adapt the implementation of "bodyReader_FrameArrived" to consider more than one body for your gesture recognition.


Tips & Debugging Hints
----------------------
- You can find the official SDK documentation for body tracking at [3], but it is rather rudimentary. A complete reference of all the methods/properties offered by the current SDK 2.0 can be found at [4].

- In order to close the application gracefully, you can press Escape when the overlay (transparent) window is active. If you only close the debug window, the application will continue to run and you have to close it manually in Visual Studio.

- You can adjust the vertical tilt of the sensor by hand. But please take care of our equipment.

- To assess the visibility of your whole body and the precise location of the joints in 3D space, you can fire up some of the sample applications from the Kinect SDK. In particular, the Color Frame and Body Frame Basics are probably the most useful ones for this exercise. To launch those sample applications, open the Kinect SDK Browser (Windows Key + S to search) and select the C# samples. Body Basics-WPF shows tracked skeleton whereas Color Basics shows you the current stream from the camera. You can launch multiple applications in parallel and also in parallel to your own application.

- The Body Basics sample application has several features to help you determine how well your body is being tracked. Green dots (and thick "bones") indicate that the body tracking pipeline is confident that the joints have been recognized correctly. Yellow dots (and thin "bones") indicate that the corresponding joints have been inferred which means that their position could not been determined reliably but the body tracking pipeline is trying to guess it based on the position of adjacent joints. A red border on either side of the debug window means that the body of the person being tracked at the moment is clipped. In that case make sure your whole body is within the captured view. In addition, coloured circles indicate the currently recognized hand state(s). Green means open, red equals closed and blue corresponds to the lasso state.

- To create breakpoints in Visual Studio, just double click on the grey area to the left of the corresponding code line. Double click again to remove it.

- To output text to a console, you can use the static method "Trace.WriteLine(...)". However, to actually see the output in Visual Studio, you have to make sure that the output view is enabled. You can do that by opening the "View" menu and selecting the "Output" entry.


Copyright Notice
----------------
Most of the code for the overlay skeleton visualisations has been taken from samples of Microsoft's official Developer Toolkit. Therefore, please do not redistribute this exercise.


[1] https://support.office.com/en-us/article/Use-keyboard-shortcuts-to-deliver-your-presentation-1524ffce-bd2a-45f4-9a7f-f18b992b93a0

[2] http://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys(v=vs.110).aspx

[3] https://msdn.microsoft.com/en-us/library/dn799273.aspx

[4] https://msdn.microsoft.com/en-us/library/dn799271.aspx