# AssetIndexGenerator
A simple tool to generate AssetIndexes for Codex-Ipsa or BetaCraft launchers.
## Usage
You can either set launch arguments, or fill in the info when running the tool.

### Arguments
-help         Shows a help menu<br>
-name=        Name the manifest will be saved as [REQUIRED]<br>
-url=         URL to where you upload your assets [REQUIRED]

### Example usage
`AssetIndexGenerator.exe -name=my_cool_manifest -url=http://example.com/assets/resources`<br>
Then upload the contents of `.\out` to the folder on your webserver you specified above
