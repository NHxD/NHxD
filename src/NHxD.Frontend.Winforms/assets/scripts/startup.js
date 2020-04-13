var totalProgress

//
// Program load
//

function __onLoadProgressChanged(progressPercentage, status)
{
	totalProgress.setValue(progressPercentage)
}

//
// Initialization
//

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
	initProgress()
})()
