{
	var Browser = {}

	Browser.cancelDelay = 1500
	Browser.cancelTimer = null

	Browser.init = function()
	{
		var that = this

		document.ondocumentcancel = null
		document.onpagerefresh = null
		document.onnavigateback = null
		document.onnavigateforward = null

		document.addEventListener("keydown", function(event)
		{
			event = event || window.event
			var keyCode = event.keyCode || event.which

			if (event.keyCode === 116)	// F5
			{
				if (document.onpagerefresh)
				{
					document.onpagerefresh(event)
				}

				return event.markAsHandled()
			}
			else if (event.keyCode === 37	// left arrow
					&& event.altKey)
			{
				//window.history.back()
				if (document.onnavigateback)
				{
					document.onnavigateback(event)
				}

				return event.markAsHandled()
			}
			else if (event.keyCode === 39	// right arrow
					&& event.altKey)
			{
				//window.history.forward()
				if (document.onnavigateforward)
				{
					document.onnavigateforward(event)
				}

				return event.markAsHandled()
			}
			else if (event.keyCode === 27)	// escape
			{
				if (this.cancelTimer)
				{
					return
				}

				if (this.cancelTimer)
				{
					self.clearTimeout(this.cancelTimer)
				}

				this.cancelTimer = self.setTimeout(function()
				{
					if (document.ondocumentcancel)
					{
						document.ondocumentcancel(event)
					}
				}, this.cancelDelay)

				return event.markAsHandled()
			}
		})

		document.addEventListener("keyup", function(event)
		{
			event = event || window.event
			var keyCode = event.keyCode || event.which

			if (event.keyCode === 27)	// escape
			{
				if (this.cancelTimer)
				{
					self.clearTimeout(this.cancelTimer)
				}
			}
		})
	}
}
