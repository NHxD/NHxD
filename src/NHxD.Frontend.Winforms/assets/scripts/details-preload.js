var totalProgress

//
// Search download
//

function __onSearchError(pageIndex, tagId, query, error)
{
	if (search.page_index !== pageIndex
		|| (tagId !== -1 && search.tag_id !== tagId)
		|| (query && search.query !== query))
	{
		return
	}

	var mainContainer = document.getElementById("main-container")

	mainContainer.addClass("error")

	var status = document.getElementById("status-message")

	status.innerText = error
}

function __onSearchResultLoaded(pageIndex, tagId, query, progressPercentage, current, total, metadata)
{
	if (search.page_index !== pageIndex
		|| (tagId !== -1 && search.tag_id !== tagId)
		|| (query && search.query !== query))
	{
		return
	}

	totalProgress.setValue(progressPercentage)
}

//
// Initialization
//

function initDocument()
{
	document.onpagerefresh = function(event)
	{
	}
}

function initZoom()
{
	// TODO: preload is now reused between the search and library views so a context should be provided.

	zoom.init(window.external.Browsers.DetailsPreload.GetZoomRatio())

	zoom.onzoomchanged = function(ratio)
	{
		window.external.Browsers.DetailsPreload.SetZoomRatio(ratio)
	}
}

function initProgress()
{
	totalProgress = Object.create(Progress)
	totalProgress.init({
		"element": document.getElementById("progress-total"),
		autoHide: true
	})
}

(function()
{
	console.create()
	Browser.init()
	initZoom()
	initDocument()
	initProgress()
})()
