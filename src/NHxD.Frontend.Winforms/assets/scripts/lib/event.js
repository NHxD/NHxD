{
	// NOTE: IE7 doesn't have EventTarget.

	if (typeof Node.prototype.addEventListener === "undefined")
	{
		Node.prototype.addEventListener = function(eventName, callback, useCapture)
		{
			if (this.attachEvent)
			{
				this.attachEvent("on" + eventName, callback)
			}
			else
			{
				this["on" + eventName] = callback
			}
		}
	}

	if (typeof Node.prototype.removeEventListener === "undefined")
	{
		Node.prototype.removeEventListener = function(eventName, callback)
		{
			if (this.detachEvent)
			{
				this.detachEvent("on" + eventName, callback)
			}
			else if (this.attachEvent)
			{
				this.attachEvent("on" + eventName, null)
			}
			else
			{
				this["on" + eventName] = null
			}
		}
	}

	if (typeof Node.prototype.dispatchEvent === "undefined")
	{
		Node.prototype.dispatchEvent = function(eventName)
		{
			// TODO...
		}
	}

	if (typeof Event.prototype.markAsHandled === "undefined")
	{
		Event.prototype.markAsHandled = function(preventDefault, cancelBubble, stopPropagation)
		{
			preventDefault = preventDefault || true
			cancelBubble = cancelBubble || true
			stopPropagation = stopPropagation || true

			if (this.preventDefault)
			{
				if (preventDefault)
				{
					this.preventDefault()
				}
			}
			else
			{
				this.returnValue = !preventDefault
			}

			this.cancelBubble = cancelBubble

			if (stopPropagation)
			{
				this.stopPropagation()
			}

			return !preventDefault
		}
	}
}
