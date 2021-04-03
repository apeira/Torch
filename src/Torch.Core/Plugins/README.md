### Plugin Package File Layout
* bin/
  * Plugin.dll - must contain the plugin entry point
  * other .NET DLLs required for the plugin
* bin/native/
  * native DLLs required for the plugin
* patch/
  * .NET DLLs containing static patches
* content/
  * Read-only non-executable assets
* plugin.json
  * Version
  * Dependencies
    * Plugin ID
    * Acceptable version range