//
// Initialization
//

function resizeCover()
{
	var img = document.getElementById("cover")

	if (!img)
	{
		return
	}

	//
	// FIXME: some cover are still too tall and clips outside the client area
	//

	var cover = metadata.images.cover
	var adjustedHeight = cover.h * img.width / cover.w

	img.style.height = adjustedHeight + "px"
}

function highlightTitles()
{
	var englishTitle = document.getElementById("title-english")

	englishTitle.innerHTML = titleFormatter.format(englishTitle.innerText, "english")
}

(function()
{
	highlightTitles()
	resizeCover()
})()
