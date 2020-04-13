{
	// FIXME: the computed zoom factor isn't always accurate.

	var zoom = {}

	zoom.pollInterval = 500
	zoom.deviceXDPI = 0
	zoom.pollTimer = null
	zoom.onzoomchanged = null

	zoom.init = function(initialZoomFactor)
	{
		var that = this

		this.deviceXDPI = initialZoomFactor * screen.logicalXDPI

		this.pollTimer = self.setInterval(function()
		{
			if (that.deviceXDPI === screen.deviceXDPI)
			{
				return
			}

			if (screen.logicalXDPI === 0)
			{
				return
			}

			var ratio = (that.deviceXDPI / screen.logicalXDPI)

			if (isNaN(ratio))
			{
				return
			}

			if (that.onzoomchanged)
			{
				that.onzoomchanged(ratio)
			}

			that.deviceXDPI = screen.deviceXDPI
		}, this.pollInterval)
	}

	zoom.stop = function()
	{
		if (this.pollTimer)
		{
			self.clearInterval(this.pollTimer)
			this.pollTimer = null
		}
	}
}
