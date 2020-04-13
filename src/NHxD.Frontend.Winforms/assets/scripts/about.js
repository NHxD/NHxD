//
// Cover Load
//

function __onCoverReady(imageSource)
{
	var html = document.querySelector("HTML")

	if (html)
	{
		html.style.backgroundImage = "url(\"" + encodeURI(imageSource) + "\")" 
	}
}

//
// Initialization
//

(function()
{
	console.create()
})()
