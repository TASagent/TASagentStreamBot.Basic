# TASagent Stream Bot Basic

A basic version of my C# [twitch bot development framework](https://github.com/TASagent/TASagentTwitchBotCore), with alert-handling stripped out.

## How do I run this?

You can either build it yourself, or download the binaries

### Build It Yourself (MacOS)

To build the application yourself, you'll need [the latest DotNet Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).  Then:

* Clone the repository: `git clone https://github.com/TASagent/TASagentStreamBot.Basic`
* Navigate to the project directory: `cd TASagentStreamBot.Basic/TASagentTwitchBot.Basic`
* Build the DotNet project: `dotnet publish -c Release -r osx-x64`
* Navigate to the build directory: `cd bin/Release/net6.0/osx-x64/publish`
* Set execute permission on the binary: `chmod a+x TASagentTwitchBot.Basic`
* Run the application: `./TASagentTwitchBot.Basic`

### Build It Yourself (Win)

To build the application yourself, you'll need [the latest DotNet Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).  Then:

* Clone the repository: `git clone https://github.com/TASagent/TASagentStreamBot.Basic`
* Navigate to the project directory: `cd TASagentStreamBot.Basic/TASagentTwitchBot.Basic`
* Build the DotNet project: `dotnet publish -c Release -r win-x64`
* Navigate to the build directory: `cd bin/Release/net6.0/win-x64/publish`
* Run the application: `./TASagentTwitchBot.Basic`

## How do I use this?

The first time you run the application, it's going to prompt you for a password you want to use on the control page.  If you need to change this in the future, just clear out the password hash in the config file `~/TASagentStreamBotBasic/Config/Config.json`. (All of the configuration files will be written to `~/TASagentStreamBotBasic/`)

Once the program is running, open up your browser and go to http://localhost:5000/API/ControlPage.html  enter the password you chose and click "Log In".

If you are using Input Capture, then go to the "Input Capture" tab of the Settings section and select the com port.

The Timer can be controlled by the "Timer" tab in the Tools section, and layout options controlled in the "Timer" tab of the Settings section.

### Adding Overlays into OBS

In OBS, add the following BrowserSources

`http://localhost:5000/BrowserSource/timer.html` - Timer overlay (example size: 450px wide x 150px tall)  
`http://localhost:5000/BrowserSource/controllerSpy.html` - SNES NintendoSpy Overlay (example size: 400px wide x 100px tall)  

### Controlling the Timer

The timer is primarily designed for tracking playtime over longer sessions, and isn't a great tool for splits.

Clicking on the **Timer** tab button will refresh the contents in the tab.

* **Start** will unpase the timer if it's paused, and otherwise start it.  
* **Stop** will pause the timer if it's going.  
* **Reset** will clear all information about the current timer, setting it back to 0.  
* **Lap** will start a new lap.  A marker is inerted into the cumulative time that indicates an event of interest just happened.  The cumulative time is unaffected, but the Current Lap Time resets to zero, and the LapStart time freezes at the current Cumulative Time. What does this mean? **Press Lap when you start a new stage**.  
* **Unlap** will unmark the latest lap without affecting your cumulative time.  What does this mean? **Press Unlap if you accidentally press Lap when you didn't mean to.**  
* **Reset Lap** will clear the time spent on the current lap (or stage) back to zero and pause the timer.  This _does_ affect the cumulative time.  

* Use **Set Time** to specify the _current lap time_ in seconds.  To restore several laps, you could enter the first lap's time in seconds, hit **Set**, then **Lap**, enter the 2nd lap's time, etc.  
* To **Save** the current timer and lap values to file persistently, enter a name in the "Save" input box and click **Save**.  
* To **Load** a saved timer, select it in the "Load" dropdown and press **Load**. This will destroy your current timer if it's not already saved.  

The timer will autosave the current time value every few minutes under the name "Autosave", to facilitate recovery if you screw up.  But the autosave will be overwritten if you load save it.

### Customizing the Timer

In the "Timer" tab in the "Settings" panel, you can customize how the timer values are displayed.  By default (until you start customizing it), there is a large main timer display and a smaller secondary one.  Each timer display can be given a label and set to show the "Cumulative" time, the "Current" time, the "Lap Start" time, or no time.

* "Cumulative" time is the total time across all laps of the timer.  In other words, your Total time.
* "Current" time is the elapsed time since the start of the last lap.  In other words, it's the time you've spent on the current level.
* "Lap Start" time is frozen at the Cumulative Time value when you started the latest lap.  In other words, it's the cumulative time when you began the current level.


Ta Da!