{
	// require HTML element with id <elementId> with two nested div element (i.e., progress > container > fill)
	// require CSS "display-none" to be defined
	// require htmlelement.js
	
	var Progress = {}

	Progress.element = null
	Progress.containerElement = null
	Progress.fillElement = null
	Progress.autoHide = false
	Progress.value = 0

	Progress.init = function(props)
	{
		var propNames =
		[
			"element"
			, "containerElement"
			, "fillElement"
			, "autoHide"
			, "animationFrameRate"
			, "animationScrollSpeed"
		]

		for (var i = 0; i < propNames.length; ++i)
		{
			var propName = propNames[i]

			if (typeof props[propName] !== "undefined")
			{
				this[propName] = props[propName]
			}
		}

		if (!this.element)
		{
			return
		}

		this.containerElement = this.element.getElementsByTagName("DIV")[0]
		this.fillElement = this.containerElement.getElementsByTagName("DIV")[0]
	}

	Progress.show = function()
	{
		if (!this.element)
		{
			return
		}

		this.element.removeClass("display-none")

		return this
	}

	Progress.hide = function()
	{
		if (!this.element)
		{
			return
		}

		this.element.addClass("display-none")

		return this
	}

	Progress.setValue = function(percentageValue)
	{
		percentageValue = parseInt(percentageValue)

		if (!this.fillElement
			|| isNaN(percentageValue))
		{
			return
		}

		var isCompleted = percentageValue === 100

		this.value = percentageValue
		this.fillElement.style.width = percentageValue + "%"
		this.doAutoHide(isCompleted)
		this.element.addOrRemoveClass("progress-complete", isCompleted)

		return this
	}

	Progress.doAutoHide = function(shouldHide)
	{
		if (!this.element)
		{
			return
		}

		if (this.autoHide)
		{
			if (shouldHide)
			{
				this.element.addClass("display-none")
			}
			else if (this.element.hasClass("display-none"))
			{
				this.element.removeClass("display-none")
			}
		}
	}

	// WORKAROUND: until I find a nice animated GIF, let's just animate manually in JS.

	Progress.animationTimer = null
	Progress.animationFrameRate = 24
	Progress.animationScrollSpeed = [ 1, 0 ]

	Progress.enableAnimation = function()
	{
		if (this.animationTimer)
		{
			return
		}

		var that = this

		this.animationTimer = self.setInterval(function()
		{
			var sz = that.containerElement.style.backgroundPosition ? that.containerElement.style.backgroundPosition.split(" ") : ["0px","0px"]

			that.containerElement.style.backgroundPosition = (parseInt(sz[0]) + that.animationScrollSpeed[0]) + "px " + (parseInt(sz[1]) + that.animationScrollSpeed[1]) + "px"
		},
		this.animationFrameRate)

		this.element.addClass("progress-download-active")
	}

	Progress.disableAnimation = function()
	{
		if (!this.animationTimer)
		{
			return
		}

		self.clearInterval(this.animationTimer)

		this.animationTimer = null

		this.element.removeClass("progress-download-active")
	}
}
