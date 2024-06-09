
To use the HandTrackedScene example you will need to add the Ultraleap hand tracking "UnityPlugin" 6.0+ from the Package Manager.

See the instructions under the "UPM Package via OpenUPM" section:
https://github.com/ultraleap/UnityPlugin#consumer-workflows

=== Instructions ===

In Edit -> Project Settings -> Package Manager, add a new scoped registry with the following details:

Name: Ultraleap
URL: https://package.openupm.com
Scope(s): com.ultraleap

Open the Package Manager (Window -> Package Manager) and navigate to "My Registries" in the dropdown at the top left of the window.
After fetching, Ultraleap UPM packages should be available in the list.

Click on "Ultraleap Tracking", make sure it is version 6.0+, then click the "Install" button in the bottom right of the window.