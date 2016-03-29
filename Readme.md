# SoftFluent.DiacriticsMappingToJavascript

This repository contains a tool to build a mapping array of unicode characters and there full compatibility decomposition. The output is a JavaScript file that allow you to remove Diacritics from a string.

## How to use

```js
<script type="application/javascript" src="NormalizationFormKDMap.js" />
<script type="application/javascript">
    String.prototype.removeDiacritics = function() {
	    return this.replace(/[^A-Za-z0-9]/g, function(x) { return NormalizationFormKDMap[x] || x; })
    };
</script>
```